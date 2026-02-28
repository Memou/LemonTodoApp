using LemonTodo.Server.Controllers;
using LemonTodo.Server.Data;
using LemonTodo.Server.DTOs;
using LemonTodo.Server.Models;
using LemonTodo.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace LemonTodo.Tests;

public class AuthControllerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly Mock<IJwtTokenService> _mockTokenService;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _passwordHasher = new PasswordHasher();
        _mockTokenService = new Mock<IJwtTokenService>();
        _mockLogger = new Mock<ILogger<AuthController>>();

        _controller = new AuthController(_context, _passwordHasher, _mockTokenService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsAuthResponse()
    {
        var request = new RegisterRequest
        {
            Username = "testuser",
            Password = "Password123!"
        };

        _mockTokenService.Setup(x => x.GenerateToken(It.IsAny<int>(), It.IsAny<string>()))
            .Returns("test-token");

        var result = await _controller.Register(request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuthResponse>(okResult.Value);
        Assert.Equal("test-token", response.Token);
        Assert.Equal("testuser", response.Username);
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ReturnsBadRequest()
    {
        var existingUser = new User
        {
            Username = "testuser",
            PasswordHash = _passwordHasher.HashPassword("Password123!")
        };
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var request = new RegisterRequest
        {
            Username = "testuser",
            Password = "Password456!"
        };

        var result = await _controller.Register(request);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsAuthResponse()
    {
        var user = new User
        {
            Username = "testuser",
            PasswordHash = _passwordHasher.HashPassword("Password123!")
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _mockTokenService.Setup(x => x.GenerateToken(It.IsAny<int>(), It.IsAny<string>()))
            .Returns("test-token");

        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "Password123!"
        };

        var result = await _controller.Login(request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuthResponse>(okResult.Value);
        Assert.Equal("test-token", response.Token);
    }

    [Fact]
    public async Task Login_WithInvalidUsername_ReturnsUnauthorized()
    {
        var request = new LoginRequest
        {
            Username = "nonexistent",
            Password = "Password123!"
        };

        var result = await _controller.Login(request);

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        var user = new User
        {
            Username = "testuser",
            PasswordHash = _passwordHasher.HashPassword("Password123!")
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "WrongPassword!"
        };

        var result = await _controller.Login(request);

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
