using LemonTodo.Server.Data;
using LemonTodo.Server.DTOs;
using LemonTodo.Server.Models;
using LemonTodo.Server.Services;
using LemonTodo.Server.Handlers.Auth.Validators;
using Microsoft.EntityFrameworkCore;

namespace LemonTodo.Server.Handlers.Auth;

public class RegisterHandler
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _tokenService;
    private readonly ILogger<RegisterHandler> _logger;

    public RegisterHandler(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenService tokenService,
        ILogger<RegisterHandler> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<(AuthResponse? Response, string? Error)> HandleAsync(RegisterRequest request)
    {
        // Validate request
        var validationResult = RegisterValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Registration validation failed: {Errors}", validationResult.ErrorMessage);
            return (null, validationResult.ErrorMessage);
        }

        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
        {
            _logger.LogWarning("Registration failed: Username {Username} already exists", request.Username);
            return (null, "Username already exists");
        }

        var user = new User
        {
            Username = request.Username,
            PasswordHash = _passwordHasher.HashPassword(request.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user.Id, user.Username);

        _logger.LogInformation("User registered successfully: {Username}", user.Username);

        return (new AuthResponse
        {
            Token = token,
            Username = user.Username
        }, null);
    }
}
