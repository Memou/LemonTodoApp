using LemonTodo.Server.DTOs;

namespace LemonTodo.Server.Handlers.Tasks.Validators;

public class ImportTaskValidator
{
    public static ValidationResult Validate(ImportTaskData task)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(task.Title))
        {
            errors.Add("Title is required");
        }

        if (task.Title?.Length > 200)
        {
            errors.Add("Title cannot exceed 200 characters");
        }

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
}
