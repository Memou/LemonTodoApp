using LemonTodo.Server.Data;
using LemonTodo.Server.DTOs;
using LemonTodo.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace LemonTodo.Server.Handlers.Tasks;

public class GetTasksHandler
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GetTasksHandler> _logger;

    public GetTasksHandler(ApplicationDbContext context, ILogger<GetTasksHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TaskListResponse> HandleAsync(
        Guid userId,
        bool? isCompleted,
        TaskPriority? priority,
        string? sortBy,
        bool descending)
    {
        var query = _context.Tasks.Where(t => t.UserId == userId);

        if (isCompleted.HasValue)
        {
            query = query.Where(t => t.IsCompleted == isCompleted.Value);
        }

        if (priority.HasValue)
        {
            query = query.Where(t => t.Priority == priority.Value);
        }

        query = sortBy?.ToLower() switch
        {
            "duedate" => descending ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate),
            "priority" => descending ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
            "title" => descending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
            _ => descending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt)
        };

        var tasks = await query.ToListAsync();

        var totalCount = tasks.Count;
        var completedCount = tasks.Count(t => t.IsCompleted);
        var pendingCount = tasks.Count(t => !t.IsCompleted);
        var overdueCount = tasks.Count(t => !t.IsCompleted && t.DueDate.HasValue && t.DueDate.Value < DateTime.UtcNow);

        return new TaskListResponse
        {
            Tasks = tasks.Select(t => new TaskResponse
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                IsCompleted = t.IsCompleted,
                Priority = t.Priority,
                DueDate = t.DueDate,
                CreatedAt = t.CreatedAt,
                CompletedAt = t.CompletedAt
            }),
            TotalCount = totalCount,
            Statistics = new TaskStatistics
            {
                TotalTasks = totalCount,
                CompletedTasks = completedCount,
                PendingTasks = pendingCount,
                OverdueTasks = overdueCount
            }
        };
    }
}
