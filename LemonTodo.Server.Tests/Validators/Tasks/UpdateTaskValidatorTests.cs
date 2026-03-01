using FluentAssertions;
using LemonTodo.Server.DTOs;
using LemonTodo.Server.Handlers.Tasks.Validators;
using LemonTodo.Server.Models;
using Xunit;

namespace LemonTodo.Server.Tests.Validators.Tasks;

public class UpdateTaskValidatorTests
{
    [Fact]
    public void Validate_ValidRequest_ReturnsValid()
    {
        // Arrange
        var existingTask = new TodoTask
        {
            Id = Guid.NewGuid(),
            Title = "Old Title",
            IsCompleted = false,
            DueDate = DateTime.UtcNow.AddDays(5)
        };

        var request = new UpdateTaskRequest
        {
            Title = "New Title",
            Priority = TaskPriority.High
        };

        // Act
        var result = UpdateTaskValidator.Validate(request, existingTask);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyTitle_ReturnsError(string title)
    {
        // Arrange
        var existingTask = new TodoTask
        {
            Id = Guid.NewGuid(),
            Title = "Old Title",
            IsCompleted = false
        };

        var request = new UpdateTaskRequest
        {
            Title = title
        };

        // Act
        var result = UpdateTaskValidator.Validate(request, existingTask);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Title cannot be empty");
    }

    [Fact]
    public void Validate_LongTitle_ReturnsError()
    {
        // Arrange
        var existingTask = new TodoTask
        {
            Id = Guid.NewGuid(),
            Title = "Old Title",
            IsCompleted = false
        };

        var request = new UpdateTaskRequest
        {
            Title = new string('a', 201)
        };

        // Act
        var result = UpdateTaskValidator.Validate(request, existingTask);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Title cannot exceed 200 characters");
    }

    [Fact]
    public void Validate_ChangingDueDateToPast_IncompleteTask_ReturnsError()
    {
        // Arrange
        var existingTask = new TodoTask
        {
            Id = Guid.NewGuid(),
            Title = "Task",
            IsCompleted = false,
            DueDate = DateTime.UtcNow.AddDays(5)
        };

        var request = new UpdateTaskRequest
        {
            DueDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = UpdateTaskValidator.Validate(request, existingTask);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Due date cannot be in the past for incomplete tasks");
    }

    [Fact]
    public void Validate_NotChangingPastDueDate_ReturnsValid()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-5);
        var existingTask = new TodoTask
        {
            Id = Guid.NewGuid(),
            Title = "Task",
            IsCompleted = false,
            DueDate = pastDate
        };

        var request = new UpdateTaskRequest
        {
            Title = "New Title",
            DueDate = pastDate // Same date, not changing
        };

        // Act
        var result = UpdateTaskValidator.Validate(request, existingTask);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ChangingDueDateToPast_CompletingTask_ReturnsValid()
    {
        // Arrange
        var existingTask = new TodoTask
        {
            Id = Guid.NewGuid(),
            Title = "Task",
            IsCompleted = false,
            DueDate = DateTime.UtcNow.AddDays(5)
        };

        var request = new UpdateTaskRequest
        {
            IsCompleted = true,
            DueDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = UpdateTaskValidator.Validate(request, existingTask);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ChangingDueDateToPast_AlreadyCompleted_ReturnsValid()
    {
        // Arrange
        var existingTask = new TodoTask
        {
            Id = Guid.NewGuid(),
            Title = "Task",
            IsCompleted = true,
            DueDate = DateTime.UtcNow.AddDays(5)
        };

        var request = new UpdateTaskRequest
        {
            DueDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = UpdateTaskValidator.Validate(request, existingTask);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
