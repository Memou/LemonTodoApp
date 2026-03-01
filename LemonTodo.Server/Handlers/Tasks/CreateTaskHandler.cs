using LemonTodo.Server.Data;
using LemonTodo.Server.DTOs;
using LemonTodo.Server.Models;
using LemonTodo.Server.Handlers.Tasks.Validators;

namespace LemonTodo.Server.Handlers.Tasks;

public class CreateTaskHandler
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CreateTaskHandler> _logger;

    public CreateTaskHandler(ApplicationDbContext context, ILogger<CreateTaskHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TaskResponse> HandleAsync(Guid userId, CreateTaskRequest request)
    {
        // Validate request
        var validationResult = CreateTaskValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            throw new InvalidOperationException(validationResult.ErrorMessage);
        }

        var task = new TodoTask
        {
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            DueDate = request.DueDate,
            UserId = userId
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Task created successfully: {TaskId} by user {UserId}", task.Id, userId);

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
