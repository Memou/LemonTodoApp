using LemonTodo.Server.DTOs;

namespace LemonTodo.Server.Handlers.Auth.Validators;

public class LoginValidator
{
    public static ValidationResult Validate(LoginRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            errors.Add("Username is required");
        }

        if (request.Username?.Length < 3)
        {
            errors.Add("Username must be at least 3 characters");
        }

        if (request.Username?.Length > 50)
        {
            errors.Add("Username cannot exceed 50 characters");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors.Add("Password is required");
        }

        if (request.Password?.Length < 6)
        {
            errors.Add("Password must be at least 6 characters");
        }

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
}
