using LemonTodo.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace LemonTodo.Server.Handlers.Tasks;

public class DeleteTaskHandler
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DeleteTaskHandler> _logger;

    public DeleteTaskHandler(ApplicationDbContext context, ILogger<DeleteTaskHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> HandleAsync(Guid userId, Guid taskId)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);

        if (task == null)
        {
            return false;
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Task deleted successfully: {TaskId}", task.Id);

        return true;
    }
}
