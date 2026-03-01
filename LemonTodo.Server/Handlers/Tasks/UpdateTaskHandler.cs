using LemonTodo.Server.Data;
using LemonTodo.Server.DTOs;
using LemonTodo.Server.Handlers.Tasks.Validators;
using Microsoft.EntityFrameworkCore;

namespace LemonTodo.Server.Handlers.Tasks;

public class UpdateTaskHandler
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UpdateTaskHandler> _logger;

    public UpdateTaskHandler(ApplicationDbContext context, ILogger<UpdateTaskHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TaskResponse?> HandleAsync(Guid userId, Guid taskId, UpdateTaskRequest request)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);

        if (task == null)
        {
            return null;
        }

        // Validate request against existing task
        var validationResult = UpdateTaskValidator.Validate(request, task);
        if (!validationResult.IsValid)
        {
            throw new InvalidOperationException(validationResult.ErrorMessage);
        }

        if (request.Title != null)
        {
            task.Title = request.Title;
        }

        if (request.Description != null)
        {
            task.Description = request.Description;
        }

        if (request.IsCompleted.HasValue)
        {
            task.IsCompleted = request.IsCompleted.Value;
            task.CompletedAt = request.IsCompleted.Value ? DateTime.UtcNow : null;
        }

        if (request.Priority.HasValue)
        {
            task.Priority = request.Priority.Value;
        }

        if (request.DueDate.HasValue)
        {
            task.DueDate = request.DueDate.Value;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Task updated successfully: {TaskId}", task.Id);

        return new TaskResponse
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            IsCompleted = task.IsCompleted,
            Priority = task.Priority,
            DueDate = task.DueDate,
            CreatedAt = task.CreatedAt,
            CompletedAt = task.CompletedAt
        };
    }
}
