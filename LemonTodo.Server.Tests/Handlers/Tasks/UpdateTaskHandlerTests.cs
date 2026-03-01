using FluentAssertions;
using LemonTodo.Server.Data;
using LemonTodo.Server.DTOs;
using LemonTodo.Server.Handlers.Tasks;
using LemonTodo.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LemonTodo.Server.Tests.Handlers.Tasks;

public class UpdateTaskHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<UpdateTaskHandler>> _mockLogger;
    private readonly UpdateTaskHandler _handler;
    private readonly Guid _testUserId;

    public UpdateTaskHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _mockLogger = new Mock<ILogger<UpdateTaskHandler>>();
        _handler = new UpdateTaskHandler(_context, _mockLogger.Object);
        _testUserId = Guid.NewGuid();
    }

    [Fact]
    public async Task HandleAsync_TaskNotFound_ReturnsNull()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var request = new UpdateTaskRequest { Title = "New Title" };

        // Act
        var response = await _handler.HandleAsync(_testUserId, taskId, request);

        // Assert
        response.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_UpdateTitle_UpdatesSuccessfully()
    {
        // Arrange
        var task = new TodoTask
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            Title = "Old Title",
            Priority = TaskPriority.Medium,
            IsCompleted = false
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var request = new UpdateTaskRequest { Title = "New Title" };

        // Act
        var response = await _handler.HandleAsync(_testUserId, task.Id, request);

        // Assert
        response.Should().NotBeNull();
        response!.Title.Should().Be("New Title");
    }

    [Fact]
    public async Task HandleAsync_UpdateDescription_UpdatesSuccessfully()
    {
        // Arrange
        var task = new TodoTask
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            Title = "Task",
            Priority = TaskPriority.Medium,
            IsCompleted = false
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var request = new UpdateTaskRequest { Description = "New Description" };

        // Act
        var response = await _handler.HandleAsync(_testUserId, task.Id, request);

        // Assert
        response.Should().NotBeNull();
        response!.Description.Should().Be("New Description");
    }

    [Fact]
    public async Task HandleAsync_CompleteTask_SetsCompletedAt()
    {
        // Arrange
        var task = new TodoTask
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            Title = "Task",
            Priority = TaskPriority.Medium,
            IsCompleted = false,
            CompletedAt = null
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var request = new UpdateTaskRequest { IsCompleted = true };

        // Act
        var response = await _handler.HandleAsync(_testUserId, task.Id, request);

        // Assert
        response.Should().NotBeNull();
        response!.IsCompleted.Should().BeTrue();
        response.CompletedAt.Should().NotBeNull();
        response.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task HandleAsync_UncompleteTask_ClearsCompletedAt()
    {
        // Arrange
        var task = new TodoTask
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            Title = "Task",
            Priority = TaskPriority.Medium,
            IsCompleted = true,
            CompletedAt = DateTime.UtcNow
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var request = new UpdateTaskRequest { IsCompleted = false };

        // Act
        var response = await _handler.HandleAsync(_testUserId, task.Id, request);

        // Assert
        response.Should().NotBeNull();
        response!.IsCompleted.Should().BeFalse();
        response.CompletedAt.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_UpdatePriority_UpdatesSuccessfully()
    {
        // Arrange
        var task = new TodoTask
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            Title = "Task",
            Priority = TaskPriority.Low,
            IsCompleted = false
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var request = new UpdateTaskRequest { Priority = TaskPriority.Urgent };

        // Act
        var response = await _handler.HandleAsync(_testUserId, task.Id, request);

        // Assert
        response.Should().NotBeNull();
        response!.Priority.Should().Be(TaskPriority.Urgent);
    }

    [Fact]
    public async Task HandleAsync_ChangeDueDateToPast_IncompleteTask_ThrowsException()
    {
        // Arrange
        var task = new TodoTask
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            Title = "Task",
            Priority = TaskPriority.Medium,
            IsCompleted = false,
            DueDate = DateTime.UtcNow.AddDays(5)
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var request = new UpdateTaskRequest { DueDate = DateTime.UtcNow.AddDays(-1) };

        // Act
        Func<Task> act = async () => await _handler.HandleAsync(_testUserId, task.Id, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Due date cannot be in the past*");
    }

    [Fact]
    public async Task HandleAsync_WrongUserId_ReturnsNull()
    {
        // Arrange
        var differentUserId = Guid.NewGuid();
        var task = new TodoTask
        {
            Id = Guid.NewGuid(),
            UserId = differentUserId,
            Title = "Task",
            Priority = TaskPriority.Medium,
            IsCompleted = false
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var request = new UpdateTaskRequest { Title = "New Title" };

        // Act
        var response = await _handler.HandleAsync(_testUserId, task.Id, request);

        // Assert
        response.Should().BeNull();
    }
}
