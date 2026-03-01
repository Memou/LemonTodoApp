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

public class CreateTaskHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<CreateTaskHandler>> _mockLogger;
    private readonly CreateTaskHandler _handler;
    private readonly Guid _testUserId;

    public CreateTaskHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _mockLogger = new Mock<ILogger<CreateTaskHandler>>();
        _handler = new CreateTaskHandler(_context, _mockLogger.Object);
        _testUserId = Guid.NewGuid();
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_CreatesTask()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(5)
        };

        // Act
        var response = await _handler.HandleAsync(_testUserId, request);

        // Assert
        response.Should().NotBeNull();
        response.Title.Should().Be(request.Title);
        response.Description.Should().Be(request.Description);
        response.Priority.Should().Be(request.Priority);
        response.DueDate.Should().Be(request.DueDate);
        response.IsCompleted.Should().BeFalse();
        response.CompletedAt.Should().BeNull();

        var taskInDb = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == response.Id);
        taskInDb.Should().NotBeNull();
        taskInDb!.UserId.Should().Be(_testUserId);
    }

    [Fact]
    public async Task HandleAsync_EmptyTitle_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "",
            Priority = TaskPriority.Medium
        };

        // Act
        Func<Task> act = async () => await _handler.HandleAsync(_testUserId, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Title is required");
    }

    [Fact]
    public async Task HandleAsync_PastDueDate_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "Test Task",
            Priority = TaskPriority.Medium,
            DueDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        Func<Task> act = async () => await _handler.HandleAsync(_testUserId, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Due date cannot be in the past*");
    }

    [Fact]
    public async Task HandleAsync_NullDescription_CreatesTaskSuccessfully()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "Test Task",
            Description = null,
            Priority = TaskPriority.Low
        };

        // Act
        var response = await _handler.HandleAsync(_testUserId, request);

        // Assert
        response.Should().NotBeNull();
        response.Description.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_CreatesTaskWithCurrentTimestamp()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "Test Task",
            Priority = TaskPriority.Medium
        };

        var beforeCreation = DateTime.UtcNow;

        // Act
        var response = await _handler.HandleAsync(_testUserId, request);

        // Assert
        response.CreatedAt.Should().BeCloseTo(beforeCreation, TimeSpan.FromSeconds(5));
    }
}
