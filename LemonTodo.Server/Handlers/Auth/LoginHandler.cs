using LemonTodo.Server.Data;
using LemonTodo.Server.DTOs;
using LemonTodo.Server.Services;
using LemonTodo.Server.Handlers.Auth.Validators;
using Microsoft.EntityFrameworkCore;

namespace LemonTodo.Server.Handlers.Auth;

public class LoginHandler
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _tokenService;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenService tokenService,
        ILogger<LoginHandler> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<(AuthResponse? Response, LoginError? Error)> HandleAsync(LoginRequest request)
    {
        // Validate request
        var validationResult = LoginValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Login validation failed: {Errors}", validationResult.ErrorMessage);
            return (null, LoginError.ValidationFailed);
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

        // Security: Combine user not found and invalid password into single error to prevent user enumeration
        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed for username: {Username}", request.Username);
            return (null, LoginError.InvalidCredentials);
        }

        var token = _tokenService.GenerateToken(user.Id, user.Username);

        _logger.LogInformation("User logged in successfully: {Username}", user.Username);

        return (new AuthResponse
        {
            Token = token,
            Username = user.Username
        }, null);
    }
}

public enum LoginError
{
    ValidationFailed,
    InvalidCredentials  // Combines UserNotFound and InvalidPassword to prevent user enumeration
}
