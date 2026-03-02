using LemonTodo.Server.DTOs;
using LemonTodo.Server.Models;

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

        if (task.Description?.Length > 1000)
        {
            errors.Add("Description cannot exceed 1000 characters");
        }

        // Validate priority enum is within valid range
        if (!Enum.IsDefined(typeof(TaskPriority), task.Priority))
        {
            errors.Add("Invalid priority value");
        }

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
}
