namespace LemonTodo.Server.Models;

public class User
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<TodoTask> Tasks { get; set; } = new List<TodoTask>();
}
