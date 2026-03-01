using LemonTodo.Server.DTOs;
using LemonTodo.Server.Models;

namespace LemonTodo.Server.Handlers.Tasks.Validators;

public class UpdateTaskValidator
{
    public static ValidationResult Validate(UpdateTaskRequest request, TodoTask existingTask)
    {
        var errors = new List<string>();

        if (request.Title != null)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                errors.Add("Title cannot be empty");
            }

            if (request.Title.Length > 200)
            {
                errors.Add("Title cannot exceed 200 characters");
            }
        }

        if (request.Description != null && request.Description.Length > 1000)
        {
            errors.Add("Description cannot exceed 1000 characters");
        }

        // Validate priority enum if provided
        if (request.Priority.HasValue && !Enum.IsDefined(typeof(TaskPriority), request.Priority.Value))
        {
            errors.Add("Invalid priority value");
        }

        if (request.DueDate.HasValue)
        {
            // Check if we're setting a past due date for an incomplete task
            var willBeCompleted = request.IsCompleted ?? existingTask.IsCompleted;

            // Only validate if the due date is actually being changed
            var isDueDateChanging = request.DueDate.Value.Date != existingTask.DueDate?.Date;

            // Compare only the date part, not the time
            if (isDueDateChanging && 
                request.DueDate.Value.Date < DateTime.UtcNow.Date && 
                !willBeCompleted)
            {
                errors.Add("Due date cannot be in the past for incomplete tasks");
            }
        }

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
}
