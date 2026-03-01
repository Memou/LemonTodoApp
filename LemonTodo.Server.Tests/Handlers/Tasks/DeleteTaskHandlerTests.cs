using FluentAssertions;
using LemonTodo.Server.Data;
using LemonTodo.Server.Handlers.Tasks;
using LemonTodo.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LemonTodo.Server.Tests.Handlers.Tasks;

public class DeleteTaskHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<DeleteTaskHandler>> _mockLogger;
    private readonly DeleteTaskHandler _handler;
    private readonly Guid _testUserId;

    public DeleteTaskHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _mockLogger = new Mock<ILogger<DeleteTaskHandler>>();
        _handler = new DeleteTaskHandler(_context, _mockLogger.Object);
        _testUserId = Guid.NewGuid();
    }

    [Fact]
    public async Task HandleAsync_TaskExists_DeletesAndReturnsTrue()
    {
        // Arrange
        var task = new TodoTask
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            Title = "Task to Delete",
            Priority = TaskPriority.Medium,
            IsCompleted = false
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.HandleAsync(_testUserId, task.Id);

        // Assert
        result.Should().BeTrue();

        var deletedTask = await _context.Tasks.FindAsync(task.Id);
        deletedTask.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_TaskDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var nonExistentTaskId = Guid.NewGuid();

        // Act
        var result = await _handler.HandleAsync(_testUserId, nonExistentTaskId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WrongUserId_ReturnsFalse()
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

        // Act
        var result = await _handler.HandleAsync(_testUserId, task.Id);

        // Assert
        result.Should().BeFalse();

        var taskStillExists = await _context.Tasks.FindAsync(task.Id);
        taskStillExists.Should().NotBeNull();
    }
}
