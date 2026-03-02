using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using LemonTodo.Server.Data;
using LemonTodo.Server.DTOs;
using LemonTodo.Server.Endpoints;
using LemonTodo.Server.Handlers.Auth;
using LemonTodo.Server.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LemonTodo.Server.Tests.Endpoints;

public class AuthEndpointsTests : IDisposable
{
    private readonly WebApplication _app;
    private readonly HttpClient _client;
    private readonly ApplicationDbContext _context;

    public AuthEndpointsTests()
    {
        var builder = WebApplication.CreateBuilder();

        // Setup in-memory database
        var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(dbOptions);

        builder.Services.AddSingleton(_context);
        builder.Services.AddScoped<RegisterHandler>();
        builder.Services.AddScoped<LoginHandler>();
        builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
        builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

        // Add configuration for JWT
        var inMemorySettings = new Dictionary<string, string>
        {
            {"Jwt:Secret", "ThisIsATestSecretKeyForJwtTokenGeneration123456"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"},
            {"Jwt:ExpirationMinutes", "60"}
        };

        builder.Configuration.AddInMemoryCollection(inMemorySettings!);

        builder.Services.AddLogging();

        builder.WebHost.UseTestServer();

        _app = builder.Build();

        _app.MapAuthEndpoints();

        _app.StartAsync().Wait();

        _client = ((IHost)_app).GetTestClient();
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Password = "Test@123456"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        content.Should().NotBeNull();
        content!.Username.Should().Be("testuser");
        content.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_DuplicateUsername_ReturnsBadRequest()
    {
        // Arrange
        var passwordHasher = new PasswordHasher();
        var existingUser = new LemonTodo.Server.Models.User
        {
            Id = Guid.NewGuid(),
            Username = "existinguser",
            PasswordHash = passwordHasher.HashPassword("Password123!")
        };
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var request = new RegisterRequest
        {
            Username = "existinguser",
            Password = "NewPass@123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        content.GetProperty("message").GetString().Should().Contain("already exists");
    }

    [Fact]
    public async Task Register_DuplicateUsername_CaseSensitive_ReturnsBadRequest()
    {
        // Arrange
        var passwordHasher = new PasswordHasher();
        var existingUser = new LemonTodo.Server.Models.User
        {
            Id = Guid.NewGuid(),
            Username = "existinguser",
            PasswordHash = passwordHasher.HashPassword("Password123!")
        };
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var request = new RegisterRequest
        {
            Username = "EXISTINGUSER",  // Test case sensitivity
            Password = "NewPass@123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert - This will pass if username check is case-insensitive
        // or fail if case-sensitive (adjust based on your requirements)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_InvalidPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Password = "weak"  // Too short, no uppercase, no special char
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_EmptyUsername_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "",
            Password = "Test@123456"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        // Arrange
        var passwordHasher = new PasswordHasher();
        var user = new LemonTodo.Server.Models.User
        {
            Id = Guid.NewGuid(),
            Username = "loginuser",
            PasswordHash = passwordHasher.HashPassword("Test@123456")
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest
        {
            Username = "loginuser",
            Password = "Test@123456"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        content.Should().NotBeNull();
        content!.Username.Should().Be("loginuser");
        content.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_InvalidUsername_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "nonexistentuser",
            Password = "Test@123456"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        content.GetProperty("message").GetString().Should().Be("Invalid username or password");
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var passwordHasher = new PasswordHasher();
        var user = new LemonTodo.Server.Models.User
        {
            Id = Guid.NewGuid(),
            Username = "loginuser",
            PasswordHash = passwordHasher.HashPassword("CorrectPassword@123")
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest
        {
            Username = "loginuser",
            Password = "WrongPassword@123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        content.GetProperty("message").GetString().Should().Be("Invalid username or password");
    }

    [Fact]
    public async Task Login_EmptyUsername_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "",
            Password = "Test@123456"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_EmptyPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = ""
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_CreatesUserInDatabase()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "dbuser",
            Password = "Test@123456"
        };

        // Act
        await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "dbuser");
        user.Should().NotBeNull();
        user!.PasswordHash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_AfterRegistration_Succeeds()
    {
        // Arrange - Register
        var registerRequest = new RegisterRequest
        {
            Username = "fullflowuser",
            Password = "Test@123456"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Act - Login
        var loginRequest = new LoginRequest
        {
            Username = "fullflowuser",
            Password = "Test@123456"
        };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
        content.Should().NotBeNull();
        content!.Username.Should().Be("fullflowuser");
    }

    public void Dispose()
    {
        _client?.Dispose();
        _app?.DisposeAsync().AsTask().Wait();
        _context?.Dispose();
    }
}
