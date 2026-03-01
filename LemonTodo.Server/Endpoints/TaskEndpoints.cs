using System.Security.Claims;
using LemonTodo.Server.Data;
using LemonTodo.Server.DTOs;
using LemonTodo.Server.Handlers.Tasks;
using LemonTodo.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LemonTodo.Server.Endpoints;

public static class TaskEndpoints
{
    public static RouteGroupBuilder MapTaskEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/tasks")
            .RequireAuthorization()
            .WithTags("Tasks");

        group.MapGet("/", GetTasks)
            .WithName("GetTasks")
            .WithOpenApi();

        group.MapGet("/{id:guid}", GetTask)
            .WithName("GetTask")
            .WithOpenApi();

        group.MapPost("/", CreateTask)
            .WithName("CreateTask")
            .WithOpenApi();

        group.MapPut("/{id:guid}", UpdateTask)
            .WithName("UpdateTask")
            .WithOpenApi();

        group.MapDelete("/{id:guid}", DeleteTask)
            .WithName("DeleteTask")
            .WithOpenApi();

        group.MapPost("/bulk-delete", BulkDeleteTasks)
            .WithName("BulkDeleteTasks")
            .WithOpenApi();

        group.MapGet("/export", ExportTasks)
            .WithName("ExportTasks")
            .WithOpenApi();

        group.MapPost("/import", ImportTasks)
            .WithName("ImportTasks")
            .WithOpenApi();

        return group;
    }

    private static async Task<IResult> GetTasks(
        HttpContext httpContext,
        [FromServices] GetTasksHandler handler,
        [FromServices] ILogger<GetTasksHandler> logger,
        bool? isCompleted = null,
        TaskPriority? priority = null,
        string? sortBy = "createdAt",
        bool descending = false)
    {
        try
        {
            var userId = GetUserId(httpContext);
            logger.LogInformation("Getting tasks for user {UserId}", userId);
            var response = await handler.HandleAsync(userId, isCompleted, priority, sortBy, descending);
            return Results.Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Unauthorized access attempt");
            return Results.Unauthorized();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving tasks");
            return Results.Problem("An error occurred while retrieving tasks");
        }
    }

    private static async Task<IResult> GetTask(
        HttpContext httpContext,
        [FromServices] ILogger<GetTasksHandler> logger,
        [FromServices] ApplicationDbContext context,
        Guid id)
    {
        try
        {
            var userId = GetUserId(httpContext);
            logger.LogInformation("Getting task {TaskId} for user {UserId}", id, userId);

            var task = await context.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
            {
                logger.LogWarning("Task {TaskId} not found for user {UserId}", id, userId);
                return Results.NotFound(new { message = "Task not found" });
            }

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

            return Results.Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Unauthorized access attempt for task {TaskId}", id);
            return Results.Unauthorized();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving task {TaskId}", id);
            return Results.Problem("An error occurred while retrieving the task");
        }
    }

    private static async Task<IResult> CreateTask(
        HttpContext httpContext,
        [FromServices] CreateTaskHandler handler,
        [FromServices] ILogger<CreateTaskHandler> logger,
        [FromBody] CreateTaskRequest request)
    {
        try
        {
            var userId = GetUserId(httpContext);
            logger.LogInformation("Creating task for user {UserId}", userId);
            var response = await handler.HandleAsync(userId, request);
            logger.LogInformation("Task {TaskId} created successfully", response.Id);
            return Results.Created($"/api/tasks/{response.Id}", response);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Unauthorized task creation attempt");
            return Results.Unauthorized();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Invalid operation while creating task");
            return Results.BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating task");
            return Results.Problem("An error occurred while creating the task");
        }
    }

    private static async Task<IResult> UpdateTask(
        HttpContext httpContext,
        [FromServices] UpdateTaskHandler handler,
        [FromServices] ILogger<UpdateTaskHandler> logger,
        Guid id,
        [FromBody] UpdateTaskRequest request)
    {
        try
        {
            var userId = GetUserId(httpContext);
            logger.LogInformation("Updating task {TaskId} for user {UserId}", id, userId);
            var response = await handler.HandleAsync(userId, id, request);

            if (response == null)
            {
                logger.LogWarning("Task {TaskId} not found for user {UserId}", id, userId);
                return Results.NotFound(new { message = "Task not found" });
            }

            logger.LogInformation("Task {TaskId} updated successfully", id);
            return Results.Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Unauthorized update attempt for task {TaskId}", id);
            return Results.Unauthorized();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Invalid operation while updating task {TaskId}", id);
            return Results.BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating task {TaskId}", id);
            return Results.Problem("An error occurred while updating the task");
        }
    }

    private static async Task<IResult> DeleteTask(
        HttpContext httpContext,
        [FromServices] DeleteTaskHandler handler,
        [FromServices] ILogger<DeleteTaskHandler> logger,
        Guid id)
    {
        try
        {
            var userId = GetUserId(httpContext);
            logger.LogInformation("Deleting task {TaskId} for user {UserId}", id, userId);
            var deleted = await handler.HandleAsync(userId, id);

            if (!deleted)
            {
                logger.LogWarning("Task {TaskId} not found for user {UserId}", id, userId);
                return Results.NotFound(new { message = "Task not found" });
            }

            logger.LogInformation("Task {TaskId} deleted successfully", id);
            return Results.NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Unauthorized delete attempt for task {TaskId}", id);
            return Results.Unauthorized();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting task {TaskId}", id);
            return Results.Problem("An error occurred while deleting the task");
        }
    }

    private static async Task<IResult> BulkDeleteTasks(
        HttpContext httpContext,
        [FromServices] ApplicationDbContext context,
        [FromServices] ILogger<DeleteTaskHandler> logger,
        [FromBody] Guid[] taskIds)
    {
        try
        {
            var userId = GetUserId(httpContext);
            logger.LogInformation("Bulk deleting {Count} tasks for user {UserId}", taskIds.Length, userId);

            var tasks = await context.Tasks
                .Where(t => taskIds.Contains(t.Id) && t.UserId == userId)
                .ToListAsync();

            if (tasks.Count == 0)
            {
                logger.LogWarning("No tasks found for bulk delete for user {UserId}", userId);
                return Results.NotFound(new { message = "No tasks found" });
            }

            context.Tasks.RemoveRange(tasks);
            await context.SaveChangesAsync();

            logger.LogInformation("Bulk deleted {Count} tasks for user {UserId}", tasks.Count, userId);
            return Results.Ok(new { deletedCount = tasks.Count });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Unauthorized bulk delete attempt");
            return Results.Unauthorized();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error bulk deleting tasks");
            return Results.Problem("An error occurred while deleting tasks");
        }
    }

    private static async Task<IResult> ExportTasks(
        HttpContext httpContext,
        [FromServices] ExportTasksHandler handler,
        [FromServices] ILogger<ExportTasksHandler> logger,
        string format = "json")
    {
        try
        {
            var userId = GetUserId(httpContext);
            logger.LogInformation("Exporting tasks for user {UserId} in {Format} format", userId, format);
            var result = await handler.HandleAsync(userId, format);
            logger.LogInformation("Tasks exported successfully for user {UserId}", userId);
            return Results.File(result.Content, result.ContentType, result.FileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Unauthorized export attempt");
            return Results.Unauthorized();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error exporting tasks");
            return Results.Problem("An error occurred while exporting tasks");
        }
    }

    private static async Task<IResult> ImportTasks(
        HttpContext httpContext,
        [FromServices] ImportTasksHandler handler,
        [FromServices] ILogger<ImportTasksHandler> logger,
        [FromBody] ImportTasksRequest? request)
    {
        try
        {
            if (request == null || request.Tasks == null || request.Tasks.Count == 0)
            {
                logger.LogWarning("Import request is null or empty");
                return Results.BadRequest(new { message = "No tasks provided for import" });
            }

            var userId = GetUserId(httpContext);
            logger.LogInformation("Importing {Count} tasks for user {UserId}", request.Tasks.Count, userId);
            var result = await handler.HandleAsync(userId, request);
            logger.LogInformation("Import completed: {Imported} imported, {Skipped} skipped for user {UserId}", 
                result.ImportedCount, result.SkippedCount, userId);

            return Results.Ok(new 
            { 
                result.ImportedCount,
                result.SkippedCount,
                result.Errors,
                result.Message
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Unauthorized import attempt");
            return Results.Unauthorized();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error importing tasks");
            return Results.Problem("An error occurred while importing tasks");
        }
    }

    private static Guid GetUserId(HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            throw new UnauthorizedAccessException("Invalid user claims");
        }
        return userId;
    }
}
