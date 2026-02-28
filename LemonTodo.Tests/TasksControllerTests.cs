using System.Security.Claims;
using LemonTodo.Server.Controllers;
using LemonTodo.Server.Data;
using LemonTodo.Server.DTOs;
using LemonTodo.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace LemonTodo.Tests;

public class TasksControllerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<TasksController>> _mockLogger;
    private readonly TasksController _controller;
    private readonly int _testUserId = 1;

    public TasksControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _mockLogger = new Mock<ILogger<TasksController>>();

        _controller = new TasksController(_context, _mockLogger.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString())
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task CreateTask_WithValidData_ReturnsCreatedTask()
    {
        var request = new CreateTaskRequest
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = TaskPriority.High
        };

        var result = await _controller.CreateTask(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<TaskResponse>(createdResult.Value);
        Assert.Equal("Test Task", response.Title);
        Assert.Equal(TaskPriority.High, response.Priority);
    }

    [Fact]
    public async Task GetTasks_ReturnsUserTasksOnly()
    {
        var task1 = new TodoTask
        {
            Title = "User 1 Task",
            UserId = _testUserId,
            User = null!
        };
        var task2 = new TodoTask
        {
            Title = "User 2 Task",
            UserId = 2,
            User = null!
        };

        _context.Tasks.AddRange(task1, task2);
        await _context.SaveChangesAsync();

        var result = await _controller.GetTasks(null, null, "createdAt", false);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<TaskListResponse>(okResult.Value);
        Assert.Single(response.Tasks);
        Assert.Equal("User 1 Task", response.Tasks.First().Title);
    }

    [Fact]
    public async Task GetTask_WithValidId_ReturnsTask()
    {
        var task = new TodoTask
        {
            Title = "Test Task",
            UserId = _testUserId,
            User = null!
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var result = await _controller.GetTask(task.Id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<TaskResponse>(okResult.Value);
        Assert.Equal("Test Task", response.Title);
    }

    [Fact]
    public async Task GetTask_WithInvalidId_ReturnsNotFound()
    {
        var result = await _controller.GetTask(999);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetTask_FromDifferentUser_ReturnsNotFound()
    {
        var task = new TodoTask
        {
            Title = "Other User Task",
            UserId = 2,
            User = null!
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var result = await _controller.GetTask(task.Id);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateTask_WithValidData_ReturnsUpdatedTask()
    {
        var task = new TodoTask
        {
            Title = "Original Title",
            UserId = _testUserId,
            User = null!
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var updateRequest = new UpdateTaskRequest
        {
            Title = "Updated Title",
            IsCompleted = true
        };

        var result = await _controller.UpdateTask(task.Id, updateRequest);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<TaskResponse>(okResult.Value);
        Assert.Equal("Updated Title", response.Title);
        Assert.True(response.IsCompleted);
        Assert.NotNull(response.CompletedAt);
    }

    [Fact]
    public async Task DeleteTask_WithValidId_ReturnsNoContent()
    {
        var task = new TodoTask
        {
            Title = "Task to Delete",
            UserId = _testUserId,
            User = null!
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var result = await _controller.DeleteTask(task.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.False(await _context.Tasks.AnyAsync(t => t.Id == task.Id));
    }

    [Fact]
    public async Task GetTasks_WithStatistics_ReturnsCorrectCounts()
    {
        var tasks = new[]
        {
            new TodoTask { Title = "Task 1", UserId = _testUserId, IsCompleted = false, User = null! },
            new TodoTask { Title = "Task 2", UserId = _testUserId, IsCompleted = true, User = null! },
            new TodoTask { Title = "Task 3", UserId = _testUserId, IsCompleted = false, DueDate = DateTime.UtcNow.AddDays(-1), User = null! }
        };
        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        var result = await _controller.GetTasks(null, null, "createdAt", false);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<TaskListResponse>(okResult.Value);
        Assert.Equal(3, response.Statistics.TotalTasks);
        Assert.Equal(1, response.Statistics.CompletedTasks);
        Assert.Equal(2, response.Statistics.PendingTasks);
        Assert.Equal(1, response.Statistics.OverdueTasks);
    }

    [Fact]
    public async Task BulkDeleteTasks_WithValidIds_DeletesMultipleTasks()
    {
        var tasks = new[]
        {
            new TodoTask { Title = "Task 1", UserId = _testUserId, User = null! },
            new TodoTask { Title = "Task 2", UserId = _testUserId, User = null! },
            new TodoTask { Title = "Task 3", UserId = _testUserId, User = null! }
        };
        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        var taskIds = tasks.Select(t => t.Id).ToArray();

        var result = await _controller.BulkDeleteTasks(taskIds);

        Assert.IsType<OkObjectResult>(result);
        Assert.Empty(await _context.Tasks.Where(t => t.UserId == _testUserId).ToListAsync());
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
