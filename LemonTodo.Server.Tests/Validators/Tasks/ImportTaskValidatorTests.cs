using FluentAssertions;
using LemonTodo.Server.DTOs;
using LemonTodo.Server.Handlers.Tasks.Validators;
using LemonTodo.Server.Models;
using Xunit;

namespace LemonTodo.Server.Tests.Validators.Tasks;

public class ImportTaskValidatorTests
{
    [Fact]
    public void Validate_ValidTask_ReturnsValid()
    {
        // Arrange
        var task = new ImportTaskData
        {
            Title = "Test Task",
            Description = "Test Description",
            Priority = TaskPriority.Medium
        };

        // Act
        var result = ImportTaskValidator.Validate(task);

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
        var task = new ImportTaskData
        {
            Title = title!,
            Priority = TaskPriority.Medium
        };

        // Act
        var result = ImportTaskValidator.Validate(task);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Title is required");
    }

    [Fact]
    public void Validate_LongTitle_ReturnsError()
    {
        // Arrange
        var task = new ImportTaskData
        {
            Title = new string('a', 201),
            Priority = TaskPriority.Medium
        };

        // Act
        var result = ImportTaskValidator.Validate(task);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Title cannot exceed 200 characters");
    }

    [Fact]
    public void Validate_PastDueDate_ReturnsValid()
    {
        // Arrange - Import should allow historical data
        var task = new ImportTaskData
        {
            Title = "Historical Task",
            Priority = TaskPriority.Medium,
            DueDate = DateTime.UtcNow.AddDays(-30)
        };

        // Act
        var result = ImportTaskValidator.Validate(task);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
