using FluentAssertions;
using LemonTodo.Server.DTOs;
using LemonTodo.Server.Handlers.Auth.Validators;
using Xunit;

namespace LemonTodo.Server.Tests.Validators.Auth;

public class LoginValidatorTests
{
    [Fact]
    public void Validate_ValidRequest_ReturnsValid()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "password123"
        };

        // Act
        var result = LoginValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyUsername_ReturnsError(string username)
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = username!,
            Password = "password123"
        };

        // Act
        var result = LoginValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Username is required");
    }

    [Fact]
    public void Validate_ShortUsername_ReturnsError()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "ab",
            Password = "password123"
        };

        // Act
        var result = LoginValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Username must be at least 3 characters");
    }

    [Fact]
    public void Validate_LongUsername_ReturnsError()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = new string('a', 51),
            Password = "password123"
        };

        // Act
        var result = LoginValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Username cannot exceed 50 characters");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyPassword_ReturnsError(string password)
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = password!
        };

        // Act
        var result = LoginValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Password is required");
    }

    [Fact]
    public void Validate_ShortPassword_ReturnsError()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "12345"
        };

        // Act
        var result = LoginValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Password must be at least 6 characters");
    }

    [Fact]
    public void Validate_MultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "ab",
            Password = "12345"
        };

        // Act
        var result = LoginValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain("Username must be at least 3 characters");
        result.Errors.Should().Contain("Password must be at least 6 characters");
    }
}
