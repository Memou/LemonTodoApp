using LemonTodo.Server.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace LemonTodo.Server.Handlers.Tasks;

public class ExportTasksHandler
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ExportTasksHandler> _logger;

    public ExportTasksHandler(ApplicationDbContext context, ILogger<ExportTasksHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ExportResult> HandleAsync(Guid userId, string format)
    {
        var tasks = await _context.Tasks
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        if (format.ToLower() == "csv")
        {
            var csv = new StringBuilder();
            csv.AppendLine("Id,Title,Description,Priority,IsCompleted,DueDate,CreatedAt,CompletedAt");

            foreach (var task in tasks)
            {
                csv.AppendLine($"\"{task.Id}\",\"{task.Title}\",\"{task.Description ?? ""}\",\"{task.Priority}\",\"{task.IsCompleted}\",\"{task.DueDate?.ToString("yyyy-MM-dd") ?? ""}\",\"{task.CreatedAt:yyyy-MM-dd HH:mm:ss}\",\"{task.CompletedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? ""}\"");
            }

            return new ExportResult
            {
                Content = Encoding.UTF8.GetBytes(csv.ToString()),
                ContentType = "text/csv",
                FileName = $"tasks_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };
        }
        else // JSON
        {
            var jsonTasks = tasks.Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                Priority = (int)t.Priority,  // Export as number for round-trip compatibility
                t.IsCompleted,
                DueDate = t.DueDate?.ToString("yyyy-MM-dd"),
                CreatedAt = t.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                CompletedAt = t.CompletedAt?.ToString("yyyy-MM-dd HH:mm:ss")
            }).ToList();

            var json = System.Text.Json.JsonSerializer.Serialize(jsonTasks, new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true 
            });

            return new ExportResult
            {
                Content = Encoding.UTF8.GetBytes(json),
                ContentType = "application/json",
                FileName = $"tasks_export_{DateTime.Now:yyyyMMdd_HHmmss}.json"
            };
        }
    }
}

public class ExportResult
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}
