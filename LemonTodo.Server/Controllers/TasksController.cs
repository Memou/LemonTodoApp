using System.Security.Claims;
using LemonTodo.Server.Data;
using LemonTodo.Server.DTOs;
using LemonTodo.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LemonTodo.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ApplicationDbContext context, ILogger<TasksController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            throw new UnauthorizedAccessException("Invalid user claims");
        }
        return userId;
    }

    [HttpGet]
    public async Task<ActionResult<TaskListResponse>> GetTasks(
        [FromQuery] bool? isCompleted,
        [FromQuery] TaskPriority? priority,
        [FromQuery] string? sortBy = "createdAt",
        [FromQuery] bool descending = false)
    {
        try
        {
            var userId = GetUserId();

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

            var response = new TaskListResponse
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

            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks");
            return StatusCode(500, new { message = "An error occurred while retrieving tasks" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskResponse>> GetTask(int id)
    {
        try
        {
            var userId = GetUserId();
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
            {
                return NotFound(new { message = "Task not found" });
            }

            return Ok(new TaskResponse
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                IsCompleted = task.IsCompleted,
                Priority = task.Priority,
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt,
                CompletedAt = task.CompletedAt
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task {TaskId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the task" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<TaskResponse>> CreateTask([FromBody] CreateTaskRequest request)
    {
        try
        {
            var userId = GetUserId();

            if (request.DueDate.HasValue && request.DueDate.Value.Date < DateTime.UtcNow.Date)
            {
                return BadRequest(new { message = "Due date cannot be in the past" });
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

            var response = new TaskResponse
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

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, response);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return StatusCode(500, new { message = "An error occurred while creating the task" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TaskResponse>> UpdateTask(int id, [FromBody] UpdateTaskRequest request)
    {
        try
        {
            var userId = GetUserId();
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
            {
                return NotFound(new { message = "Task not found" });
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
                if (request.DueDate.Value < DateTime.UtcNow && !task.IsCompleted)
                {
                    return BadRequest(new { message = "Due date cannot be in the past for incomplete tasks" });
                }
                task.DueDate = request.DueDate.Value;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Task updated successfully: {TaskId}", task.Id);

            return Ok(new TaskResponse
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                IsCompleted = task.IsCompleted,
                Priority = task.Priority,
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt,
                CompletedAt = task.CompletedAt
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {TaskId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the task" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        try
        {
            var userId = GetUserId();
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
            {
                return NotFound(new { message = "Task not found" });
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Task deleted successfully: {TaskId}", task.Id);

            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {TaskId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the task" });
        }
    }

    [HttpPost("bulk-delete")]
    public async Task<IActionResult> BulkDeleteTasks([FromBody] int[] taskIds)
    {
        try
        {
            var userId = GetUserId();
            var tasks = await _context.Tasks
                .Where(t => taskIds.Contains(t.Id) && t.UserId == userId)
                .ToListAsync();

            if (tasks.Count == 0)
            {
                return NotFound(new { message = "No tasks found" });
            }

            _context.Tasks.RemoveRange(tasks);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Bulk deleted {Count} tasks for user {UserId}", tasks.Count, userId);

            return Ok(new { deletedCount = tasks.Count });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk deleting tasks");
            return StatusCode(500, new { message = "An error occurred while deleting tasks" });
        }
    }
}
