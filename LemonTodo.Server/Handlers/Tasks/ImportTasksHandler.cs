using LemonTodo.Server.Data;
using LemonTodo.Server.DTOs;
using LemonTodo.Server.Models;
using LemonTodo.Server.Handlers.Tasks.Validators;
using Microsoft.EntityFrameworkCore;

namespace LemonTodo.Server.Handlers.Tasks;

public class ImportTasksHandler
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ImportTasksHandler> _logger;

    public ImportTasksHandler(ApplicationDbContext context, ILogger<ImportTasksHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ImportResult> HandleAsync(Guid userId, ImportTasksRequest request)
    {
        var importedCount = 0;
        var skippedCount = 0;
        var errors = new List<string>();

        // Get existing tasks for this user to check for content-based duplicates
        var existingTasks = await _context.Tasks
            .Where(t => t.UserId == userId)
            .Select(t => new { t.Title, t.Description, t.DueDate, t.Priority })
            .ToListAsync();

        var existingTaskSet = existingTasks
            .Select(t => $"{t.Title}|{t.Description ?? ""}|{t.DueDate?.ToString("yyyy-MM-dd") ?? ""}|{(int)t.Priority}")
            .ToHashSet();

        foreach (var taskData in request.Tasks)
        {
            try
            {
                // Validate each task
                var validationResult = ImportTaskValidator.Validate(taskData);
                if (!validationResult.IsValid)
                {
                    skippedCount++;
                    errors.Add($"Skipped task '{taskData.Title ?? "unnamed"}': {validationResult.ErrorMessage}");
                    _logger.LogWarning("Skipped invalid task: {Errors}", validationResult.ErrorMessage);
                    continue;
                }

                // Check for content-based duplicates (same title, description, due date, priority)
                var taskFingerprint = $"{taskData.Title}|{taskData.Description ?? ""}|{taskData.DueDate?.ToString("yyyy-MM-dd") ?? ""}|{(int)taskData.Priority}";

                if (existingTaskSet.Contains(taskFingerprint))
                {
                    skippedCount++;
                    _logger.LogInformation("Skipped duplicate task: {Title}", taskData.Title);
                    continue;
                }

                // Create new task with new ID
                var task = new TodoTask
                {
                    Id = Guid.NewGuid(), // Always new ID for cross-user imports
                    UserId = userId, // Assign to importing user
                    Title = taskData.Title,
                    Description = taskData.Description,
                    Priority = taskData.Priority,
                    IsCompleted = taskData.IsCompleted ?? false,
                    DueDate = taskData.DueDate,
                    CreatedAt = DateTime.UtcNow // New creation date
                };

                if (task.IsCompleted)
                {
                    task.CompletedAt = DateTime.UtcNow;
                }

                _context.Tasks.Add(task);
                existingTaskSet.Add(taskFingerprint); // Add to set to detect duplicates within this import
                importedCount++;
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error importing task '{taskData.Title}': {ex.Message}";
                errors.Add(errorMsg);
                _logger.LogError(ex, "Error importing task '{Title}'", taskData.Title);
            }
        }

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully saved {Count} imported tasks for user {UserId}", importedCount, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving imported tasks to database");
            throw;
        }

        _logger.LogInformation("Import completed: {Imported} imported, {Skipped} skipped for user {UserId}", importedCount, skippedCount, userId);

        return new ImportResult
        {
            ImportedCount = importedCount,
            SkippedCount = skippedCount,
            Errors = errors.Count > 0 ? errors : null,
            Message = $"Successfully imported {importedCount} task(s), skipped {skippedCount} duplicate(s)"
        };
    }
}

public class ImportResult
{
    public int ImportedCount { get; set; }
    public int SkippedCount { get; set; }
    public List<string>? Errors { get; set; }
    public string Message { get; set; } = string.Empty;
}
