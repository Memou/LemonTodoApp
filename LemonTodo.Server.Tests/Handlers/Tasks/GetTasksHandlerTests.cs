using FluentAssertions;
using LemonTodo.Server.Data;
using LemonTodo.Server.Handlers.Tasks;
using LemonTodo.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LemonTodo.Server.Tests.Handlers.Tasks;

public class GetTasksHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<GetTasksHandler>> _mockLogger;
    private readonly GetTasksHandler _handler;
    private readonly Guid _testUserId;

    public GetTasksHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _mockLogger = new Mock<ILogger<GetTasksHandler>>();
        _handler = new GetTasksHandler(_context, _mockLogger.Object);
        _testUserId = Guid.NewGuid();
    }

    [Fact]
    public async Task HandleAsync_NoTasks_ReturnsEmptyList()
    {
        // Act
        var response = await _handler.HandleAsync(_testUserId, null, null, null, false);

        // Assert
        response.Should().NotBeNull();
        response.Tasks.Should().BeEmpty();
        response.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_MultipleTasks_ReturnsAllTasks()
    {
        // Arrange
        var tasks = new[]
        {
            new TodoTask { Id = Guid.NewGuid(), UserId = _testUserId, Title = "Task 1", Priority = TaskPriority.Low, IsCompleted = false },
            new TodoTask { Id = Guid.NewGuid(), UserId = _testUserId, Title = "Task 2", Priority = TaskPriority.Medium, IsCompleted = true },
            new TodoTask { Id = Guid.NewGuid(), UserId = _testUserId, Title = "Task 3", Priority = TaskPriority.High, IsCompleted = false }
        };
        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        // Act
        var response = await _handler.HandleAsync(_testUserId, null, null, null, false);

        // Assert
        response.Tasks.Should().HaveCount(3);
        response.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task HandleAsync_FilterByCompleted_ReturnsOnlyCompletedTasks()
    {
        // Arrange
        var tasks = new[]
        {
            new TodoTask { Id = Guid.NewGuid(), UserId = _testUserId, Title = "Task 1", Priority = TaskPriority.Low, IsCompleted = false },
            new TodoTask { Id = Guid.NewGuid(), UserId = _testUserId, Title = "Task 2", Priority = TaskPriority.Medium, IsCompleted = true },
            new TodoTask { Id = Guid.NewGuid(), UserId = _testUserId, Title = "Task 3", Priority = TaskPriority.High, IsCompleted = true }
        };
        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        // Act
        var response = await _handler.HandleAsync(_testUserId, isCompleted: true, null, null, false);

        // Assert
        response.Tasks.Should().HaveCount(2);
        response.Tasks.Should().OnlyContain(t => t.IsCompleted);
    }

    [Fact]
    public async Task HandleAsync_FilterByPriority_ReturnsTasksWithPriority()
    {
        // Arrange
        var tasks = new[]
        {
            new TodoTask { Id = Guid.NewGuid(), UserId = _testUserId, Title = "Task 1", Priority = TaskPriority.Low, IsCompleted = false },
            new TodoTask { Id = Guid.NewGuid(), UserId = _testUserId, Title = "Task 2", Priority = TaskPriority.High, IsCompleted = false },
            new TodoTask { Id = Guid.NewGuid(), UserId = _testUserId, Title = "Task 3", Priority = TaskPriority.High, IsCompleted = false }
        };
        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        // Act
        var response = await _handler.HandleAsync(_testUserId, null, TaskPriority.High, null, false);

        // Assert
        response.Tasks.Should().HaveCount(2);
        response.Tasks.Should().OnlyContain(t => t.Priority == TaskPriority.High);
    }

    [Fact]
    public async Task HandleAsync_SortByTitle_SortsCorrectly()
    {
        // Arrange
        var tasks = new[]
        {
            new TodoTask { Id = Guid.NewGuid(), UserId = _testUserId, Title = "C Task", Priority = TaskPriority.Low, IsCompleted = false },
            new TodoTask { Id = Guid.NewGuid(), UserId = _testUserId, Title = "A Task", Priority = TaskPriority.Medium, IsCompleted = false },
            new TodoTask { Id = Guid.NewGuid(), UserId = _testUserId, Title = "B Task", Priority = TaskPriority.High, IsCompleted = false }
        };
        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        // Act
        var response = await _handler.HandleAsync(_testUserId, null, null, "title", false);

        // Assert
        response.Tasks.Should().HaveCount(3);
        response.Tasks.ElementAt(0).Title.Should().Be("A Task");
        response.Tasks.ElementAt(1).Title.Should().Be("B Task");
        response.Tasks.ElementAt(2).Title.Should().Be("C Task");
    }

    [Fact]
    public async Task HandleAsync_SortByTitleDescending_SortsCorrectly()
    {
        // Arrange
        var tasks = new[]
        {
            new TodoTask { Id = Guid.NewGuid(), UserId = _testUserId, Title = "A Task", Priority = TaskPriority.Low, IsCompleted = false },
            new TodoTask { Id = Guid.NewGuid(), UserId = _testUserId, Title = "C Task", Priority = TaskPriority.Medium, IsCompleted = false },
            new TodoTask { Id = Guid.NewGuid(), UserId = _testUserId, Title = "B Task", Priority = TaskPriority.High, IsCompleted = false }
        };
        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        // Act
        var response = await _handler.HandleAsync(_testUserId, null, null, "title", true);

        // Assert
        response.Tasks.Should().HaveCount(3);
        response.Tasks.ElementAt(0).Title.Should().Be("C Task");
        response.Tasks.ElementAt(1).Title.Should().Be("B Task");
        response.Tasks.ElementAt(2).Title.Should().Be("A Task");
    }

    [Fact]
    public async Task HandleAsync_SortByPriority_SortsCorrectly()
    {
        // Arrange
        var tasks = new[]
        {
            new TodoTask { Id = Guid.NewGuid(), UserId = _testUserId, Title = "Task 1", Priority = TaskPriority.High, IsCompleted = false },
            new TodoTask { Id = Guid.NewGuid(), UserId = _testUserId, Title = "Task 2", Priority = TaskPriority.Low, IsCompleted = false },
            new TodoTask { Id = Guid.NewGuid(), UserId = _testUserId, Title = "Task 3", Priority = TaskPriority.Urgent, IsCompleted = false }
        };
        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        // Act
        var response = await _handler.HandleAsync(_testUserId, null, null, "priority", false);

        // Assert
        response.Tasks.Should().HaveCount(3);
        response.Tasks.ElementAt(0).Priority.Should().Be(TaskPriority.Low);
        response.Tasks.ElementAt(1).Priority.Should().Be(TaskPriority.High);
        response.Tasks.ElementAt(2).Priority.Should().Be(TaskPriority.Urgent);
    }

    [Fact]
    public async Task HandleAsync_OnlyReturnsUsersTasks()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var tasks = new[]
        {
            new TodoTask { Id = Guid.NewGuid(), UserId = _testUserId, Title = "My Task", Priority = TaskPriority.Low, IsCompleted = false },
            new TodoTask { Id = Guid.NewGuid(), UserId = otherUserId, Title = "Other User Task", Priority = TaskPriority.Medium, IsCompleted = false }
        };
        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        // Act
        var response = await _handler.HandleAsync(_testUserId, null, null, null, false);

        // Assert
        response.Tasks.Should().HaveCount(1);
        response.Tasks.Should().OnlyContain(t => t.Title == "My Task");
    }
}
