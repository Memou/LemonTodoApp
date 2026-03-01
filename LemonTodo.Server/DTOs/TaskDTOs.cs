using System.ComponentModel.DataAnnotations;
using LemonTodo.Server.Models;

namespace LemonTodo.Server.DTOs;

public class CreateTaskRequest
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
    public required string Title { get; set; }

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public DateTime? DueDate { get; set; }
}

public class UpdateTaskRequest
{
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
    public string? Title { get; set; }

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    public bool? IsCompleted { get; set; }

    public TaskPriority? Priority { get; set; }

    public DateTime? DueDate { get; set; }
}

public class TaskResponse
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class TaskListResponse
{
    public IEnumerable<TaskResponse> Tasks { get; set; } = [];
    public int TotalCount { get; set; }
    public TaskStatistics Statistics { get; set; } = null!;
}

public class TaskStatistics
{
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
    public int OverdueTasks { get; set; }
}

public class ImportTaskData
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    public required string Title { get; set; }
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public bool? IsCompleted { get; set; }
    public DateTime? DueDate { get; set; }
}

public class ImportTasksRequest
{
    [Required]
    public required List<ImportTaskData> Tasks { get; set; }
}
