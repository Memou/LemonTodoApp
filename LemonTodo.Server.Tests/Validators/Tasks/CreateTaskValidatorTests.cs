using FluentAssertions;
using LemonTodo.Server.DTOs;
using LemonTodo.Server.Handlers.Tasks.Validators;
using LemonTodo.Server.Models;
using Xunit;

namespace LemonTodo.Server.Tests.Validators.Tasks;

public class CreateTaskValidatorTests
{
    [Fact]
    public void Validate_ValidRequest_ReturnsValid()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = TaskPriority.Medium,
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var result = CreateTaskValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyTitle_ReturnsError(string title)
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = title!,
            Priority = TaskPriority.Medium
        };

        // Act
        var result = CreateTaskValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Title is required");
    }

    [Fact]
    public void Validate_LongTitle_ReturnsError()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = new string('a', 201),
            Priority = TaskPriority.Medium
        };

        // Act
        var result = CreateTaskValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Title cannot exceed 200 characters");
    }

    [Fact]
    public void Validate_PastDueDate_ReturnsError()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "Test Task",
            Priority = TaskPriority.Medium,
            DueDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = CreateTaskValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Due date cannot be in the past");
    }

    [Fact]
    public void Validate_TodayDueDate_ReturnsValid()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "Test Task",
            Priority = TaskPriority.Medium,
            DueDate = DateTime.UtcNow
        };

        // Act
        var result = CreateTaskValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_NoDueDate_ReturnsValid()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "Test Task",
            Priority = TaskPriority.Medium,
            DueDate = null
        };

        // Act
        var result = CreateTaskValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
