using FluentAssertions;
using LemonTodo.Server.Data;
using LemonTodo.Server.DTOs;
using LemonTodo.Server.Handlers.Auth;
using LemonTodo.Server.Models;
using LemonTodo.Server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LemonTodo.Server.Tests.Handlers.Auth;

public class LoginHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<IJwtTokenService> _mockTokenService;
    private readonly Mock<ILogger<LoginHandler>> _mockLogger;
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockTokenService = new Mock<IJwtTokenService>();
        _mockLogger = new Mock<ILogger<LoginHandler>>();
        _handler = new LoginHandler(_context, _mockPasswordHasher.Object, _mockTokenService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            PasswordHash = "hashedPassword123"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "password123"
        };

        _mockPasswordHasher.Setup(x => x.VerifyPassword("password123", "hashedPassword123"))
            .Returns(true);

        _mockTokenService.Setup(x => x.GenerateToken(user.Id, user.Username))
            .Returns("fake-jwt-token");

        // Act
        var (response, error) = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response!.Token.Should().Be("fake-jwt-token");
        response.Username.Should().Be("testuser");
        error.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ReturnsInvalidCredentialsError()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "nonexistent",
            Password = "password123"
        };

        // Act
        var (response, error) = await _handler.HandleAsync(request);

        // Assert
        response.Should().BeNull();
        error.Should().Be(LoginError.InvalidCredentials);
    }

    [Fact]
    public async Task HandleAsync_InvalidPassword_ReturnsInvalidCredentialsError()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            PasswordHash = "hashedPassword123"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "wrongpassword"
        };

        _mockPasswordHasher.Setup(x => x.VerifyPassword("wrongpassword", "hashedPassword123"))
            .Returns(false);

        // Act
        var (response, error) = await _handler.HandleAsync(request);

        // Assert
        response.Should().BeNull();
        error.Should().Be(LoginError.InvalidCredentials);
    }

    [Fact]
    public async Task HandleAsync_InvalidRequest_ReturnsValidationFailedError()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "ab", // Too short
            Password = "password123"
        };

        // Act
        var (response, error) = await _handler.HandleAsync(request);

        // Assert
        response.Should().BeNull();
        error.Should().Be(LoginError.ValidationFailed);
    }

    [Fact]
    public async Task HandleAsync_CaseSensitiveUsername_ReturnsUserNotFoundError()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "TestUser",
            PasswordHash = "hashedPassword123"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest
        {
            Username = "testuser", // Different case
            Password = "password123"
        };

        // Act
        var (response, error) = await _handler.HandleAsync(request);

        // Assert
        response.Should().BeNull();
        error.Should().Be(LoginError.InvalidCredentials);
    }
}
