using FluentAssertions;
using LemonTodo.Server.DTOs;
using LemonTodo.Server.Handlers.Auth.Validators;
using Xunit;

namespace LemonTodo.Server.Tests.Validators.Auth;

public class RegisterValidatorTests
{
    [Fact]
    public void Validate_ValidRequest_ReturnsValid()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Password = "password123"
        };

        // Act
        var result = RegisterValidator.Validate(request);

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
        var request = new RegisterRequest
        {
            Username = username!,
            Password = "password123"
        };

        // Act
        var result = RegisterValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Username is required");
    }

    [Fact]
    public void Validate_ShortUsername_ReturnsError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "ab",
            Password = "password123"
        };

        // Act
        var result = RegisterValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Username must be at least 3 characters");
    }

    [Fact]
    public void Validate_LongUsername_ReturnsError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = new string('a', 51),
            Password = "password123"
        };

        // Act
        var result = RegisterValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Username cannot exceed 50 characters");
    }

    [Theory]
    [InlineData("user@test")]
    [InlineData("user-test")]
    [InlineData("user test")]
    [InlineData("user.test")]
    [InlineData("user#test")]
    [InlineData("user'; DROP TABLE--")]
    public void Validate_InvalidUsernameFormat_ReturnsError(string username)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = username,
            Password = "password123"
        };

        // Act
        var result = RegisterValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Username can only contain letters, numbers, and underscores");
    }

    [Theory]
    [InlineData("user123")]
    [InlineData("User_Test")]
    [InlineData("test_user_123")]
    [InlineData("USER")]
    public void Validate_ValidUsernameFormat_ReturnsValid(string username)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = username,
            Password = "password123"
        };

        // Act
        var result = RegisterValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyPassword_ReturnsError(string password)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Password = password!
        };

        // Act
        var result = RegisterValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Password is required");
    }

    [Fact]
    public void Validate_ShortPassword_ReturnsError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Password = "12345"
        };

        // Act
        var result = RegisterValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Password must be at least 6 characters");
    }

    [Fact]
    public void Validate_LongPassword_ReturnsError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Password = new string('a', 101)
        };

        // Act
        var result = RegisterValidator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Password cannot exceed 100 characters");
    }
}
