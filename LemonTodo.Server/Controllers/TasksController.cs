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

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
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
    public async Task<ActionResult<TaskResponse>> GetTask(Guid id)
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
    public async Task<ActionResult<TaskResponse>> UpdateTask(Guid id, [FromBody] UpdateTaskRequest request)
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
    public async Task<IActionResult> DeleteTask(Guid id)
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
    public async Task<IActionResult> BulkDeleteTasks([FromBody] Guid[] taskIds)
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

    [HttpGet("export")]
    public async Task<IActionResult> ExportTasks([FromQuery] string format = "json")
    {
        try
        {
            var userId = GetUserId();
            var tasks = await _context.Tasks
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            if (format.ToLower() == "csv")
            {
                var csv = new System.Text.StringBuilder();
                csv.AppendLine("Id,Title,Description,Priority,IsCompleted,DueDate,CreatedAt,CompletedAt");

                foreach (var task in tasks)
                {
                    csv.AppendLine($"\"{task.Id}\",\"{task.Title}\",\"{task.Description ?? ""}\",\"{task.Priority}\",\"{task.IsCompleted}\",\"{task.DueDate?.ToString("yyyy-MM-dd") ?? ""}\",\"{task.CreatedAt:yyyy-MM-dd HH:mm:ss}\",\"{task.CompletedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? ""}\"");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", $"tasks_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            }
            else // JSON
            {
                var jsonTasks = tasks.Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    Priority = t.Priority.ToString(),
                    t.IsCompleted,
                    DueDate = t.DueDate?.ToString("yyyy-MM-dd"),
                    CreatedAt = t.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    CompletedAt = t.CompletedAt?.ToString("yyyy-MM-dd HH:mm:ss")
                }).ToList();

                var json = System.Text.Json.JsonSerializer.Serialize(jsonTasks, new System.Text.Json.JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });

                var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                return File(bytes, "application/json", $"tasks_export_{DateTime.Now:yyyyMMdd_HHmmss}.json");
            }
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting tasks");
            return StatusCode(500, new { message = "An error occurred while exporting tasks" });
        }
    }

    [HttpPost("import")]
    public async Task<IActionResult> ImportTasks([FromBody] ImportTasksRequest request)
    {
        try
        {
            var userId = GetUserId();
            var importedCount = 0;
            var skippedCount = 0;
            var errors = new List<string>();

            // Get existing task IDs for this user to check for duplicates
            var existingIds = (await _context.Tasks
                .Where(t => t.UserId == userId)
                .Select(t => t.Id)
                .ToListAsync())
                .ToHashSet();

            foreach (var taskData in request.Tasks)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(taskData.Title))
                    {
                        errors.Add($"Skipped task: Title is required");
                        continue;
                    }

                    // Check for duplicates by Id
                    if (taskData.Id.HasValue && existingIds.Contains(taskData.Id.Value))
                    {
                        skippedCount++;
                        _logger.LogInformation("Skipped duplicate task with Id: {Id} - {Title}", taskData.Id, taskData.Title);
                        continue;
                    }

                    var taskId = taskData.Id ?? Guid.NewGuid();
                    var task = new TodoTask
                    {
                        Id = taskId,
                        UserId = userId,
                        Title = taskData.Title,
                        Description = taskData.Description,
                        Priority = taskData.Priority,
                        IsCompleted = taskData.IsCompleted ?? false,
                        DueDate = taskData.DueDate,
                        CreatedAt = DateTime.UtcNow
                    };

                    if (task.IsCompleted)
                    {
                        task.CompletedAt = DateTime.UtcNow;
                    }

                    _context.Tasks.Add(task);
                    existingIds.Add(taskId); // Add to set for subsequent duplicate checks within the same import
                    importedCount++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Error importing task '{taskData.Title}': {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Imported {Count} tasks, skipped {Skipped} duplicates for user {UserId}", importedCount, skippedCount, userId);

            return Ok(new 
            { 
                importedCount,
                skippedCount,
                errors = errors.Count > 0 ? errors : null,
                message = $"Successfully imported {importedCount} task(s), skipped {skippedCount} duplicate(s)"
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing tasks");
            return StatusCode(500, new { message = "An error occurred while importing tasks" });
        }
    }
}
