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

public class RegisterHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<IJwtTokenService> _mockTokenService;
    private readonly Mock<ILogger<RegisterHandler>> _mockLogger;
    private readonly RegisterHandler _handler;

    public RegisterHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockTokenService = new Mock<IJwtTokenService>();
        _mockLogger = new Mock<ILogger<RegisterHandler>>();
        _handler = new RegisterHandler(_context, _mockPasswordHasher.Object, _mockTokenService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_CreatesUserAndReturnsAuthResponse()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "newuser",
            Password = "password123"
        };

        _mockPasswordHasher.Setup(x => x.HashPassword("password123"))
            .Returns("hashedPassword123");

        _mockTokenService.Setup(x => x.GenerateToken(It.IsAny<Guid>(), "newuser"))
            .Returns("fake-jwt-token");

        // Act
        var (response, error) = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        response!.Token.Should().Be("fake-jwt-token");
        response.Username.Should().Be("newuser");
        error.Should().BeNull();

        var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Username == "newuser");
        userInDb.Should().NotBeNull();
        userInDb!.PasswordHash.Should().Be("hashedPassword123");
    }

    [Fact]
    public async Task HandleAsync_DuplicateUsername_ReturnsError()
    {
        // Arrange
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "existinguser",
            PasswordHash = "hashedPassword"
        };
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var request = new RegisterRequest
        {
            Username = "existinguser",
            Password = "password123"
        };

        // Act
        var (response, error) = await _handler.HandleAsync(request);

        // Assert
        response.Should().BeNull();
        error.Should().Be("Username already exists");
    }

    [Fact]
    public async Task HandleAsync_InvalidUsername_ReturnsValidationError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "ab", // Too short
            Password = "password123"
        };

        // Act
        var (response, error) = await _handler.HandleAsync(request);

        // Assert
        response.Should().BeNull();
        error.Should().NotBeNull();
        error.Should().Contain("Username must be at least 3 characters");
    }

    [Fact]
    public async Task HandleAsync_InvalidUsernameFormat_ReturnsValidationError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "user@test", // Invalid format
            Password = "password123"
        };

        // Act
        var (response, error) = await _handler.HandleAsync(request);

        // Assert
        response.Should().BeNull();
        error.Should().NotBeNull();
        error.Should().Contain("Username can only contain letters, numbers, and underscores");
    }

    [Fact]
    public async Task HandleAsync_ShortPassword_ReturnsValidationError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Password = "12345" // Too short
        };

        // Act
        var (response, error) = await _handler.HandleAsync(request);

        // Assert
        response.Should().BeNull();
        error.Should().NotBeNull();
        error.Should().Contain("Password must be at least 6 characters");
    }

    [Fact]
    public async Task HandleAsync_ValidUsername_WithUnderscoreAndNumbers()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "test_user_123",
            Password = "password123"
        };

        _mockPasswordHasher.Setup(x => x.HashPassword("password123"))
            .Returns("hashedPassword123");

        _mockTokenService.Setup(x => x.GenerateToken(It.IsAny<Guid>(), "test_user_123"))
            .Returns("fake-jwt-token");

        // Act
        var (response, error) = await _handler.HandleAsync(request);

        // Assert
        response.Should().NotBeNull();
        error.Should().BeNull();
    }
}
