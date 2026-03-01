using LemonTodo.Server.DTOs;

namespace LemonTodo.Server.Handlers.Tasks.Validators;

public class CreateTaskValidator
{
    public static ValidationResult Validate(CreateTaskRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            errors.Add("Title is required");
        }

        if (request.Title?.Length > 200)
        {
            errors.Add("Title cannot exceed 200 characters");
        }

        if (request.DueDate.HasValue && request.DueDate.Value.Date < DateTime.UtcNow.Date)
        {
            errors.Add("Due date cannot be in the past");
        }

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
}
