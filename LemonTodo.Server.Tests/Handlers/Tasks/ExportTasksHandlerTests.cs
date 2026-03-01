using FluentAssertions;
using LemonTodo.Server.Data;
using LemonTodo.Server.Handlers.Tasks;
using LemonTodo.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Text.Json;
using Xunit;

namespace LemonTodo.Server.Tests.Handlers.Tasks;

public class ExportTasksHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<ExportTasksHandler>> _mockLogger;
    private readonly ExportTasksHandler _handler;
    private readonly Guid _testUserId;

    public ExportTasksHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _mockLogger = new Mock<ILogger<ExportTasksHandler>>();
        _handler = new ExportTasksHandler(_context, _mockLogger.Object);
        _testUserId = Guid.NewGuid();
    }

    [Fact]
    public async Task HandleAsync_JsonFormat_ReturnsValidJson()
    {
        // Arrange
        var task = new TodoTask
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            Title = "Test Task",
            Description = "Test Description",
            Priority = TaskPriority.High,
            IsCompleted = false,
            DueDate = new DateTime(2026, 3, 1),
            CreatedAt = DateTime.UtcNow
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.HandleAsync(_testUserId, "json");

        // Assert
        result.ContentType.Should().Be("application/json");
        result.FileName.Should().EndWith(".json");
        result.Content.Should().NotBeEmpty();

        var json = Encoding.UTF8.GetString(result.Content);
        var tasks = JsonSerializer.Deserialize<List<JsonElement>>(json);
        tasks.Should().HaveCount(1);
    }

    [Fact]
    public async Task HandleAsync_JsonFormat_ContainsCorrectFields()
    {
        // Arrange
        var task = new TodoTask
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            Title = "Test Task",
            Description = "Test Description",
            Priority = TaskPriority.Medium,
            IsCompleted = false,
            DueDate = new DateTime(2026, 3, 1),
            CreatedAt = DateTime.UtcNow
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.HandleAsync(_testUserId, "json");

        // Assert
        var json = Encoding.UTF8.GetString(result.Content);
        json.Should().Contain("Test Task");
        json.Should().Contain("Test Description");
        json.Should().Contain("\"Priority\": 1"); // Medium = 1 (with space after colon in formatted JSON)
        json.Should().Contain("\"IsCompleted\": false");
    }

    [Fact]
    public async Task HandleAsync_CsvFormat_ReturnsValidCsv()
    {
        // Arrange
        var task = new TodoTask
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            Title = "Test Task",
            Description = "Test Description",
            Priority = TaskPriority.Low,
            IsCompleted = false,
            DueDate = new DateTime(2026, 3, 1),
            CreatedAt = DateTime.UtcNow
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.HandleAsync(_testUserId, "csv");

        // Assert
        result.ContentType.Should().Be("text/csv");
        result.FileName.Should().EndWith(".csv");
        result.Content.Should().NotBeEmpty();

        var csv = Encoding.UTF8.GetString(result.Content);
        csv.Should().Contain("Id,Title,Description,Priority,IsCompleted,DueDate,CreatedAt,CompletedAt");
        csv.Should().Contain("Test Task");
    }

    [Fact]
    public async Task HandleAsync_NoTasks_ReturnsEmptyExport()
    {
        // Act
        var result = await _handler.HandleAsync(_testUserId, "json");

        // Assert
        var json = Encoding.UTF8.GetString(result.Content);
        json.Should().Be("[]");
    }

    [Fact]
    public async Task HandleAsync_OnlyExportsUsersTasks()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var tasks = new[]
        {
            new TodoTask { Id = Guid.NewGuid(), UserId = _testUserId, Title = "My Task", Priority = TaskPriority.Low, IsCompleted = false },
            new TodoTask { Id = Guid.NewGuid(), UserId = otherUserId, Title = "Other Task", Priority = TaskPriority.Medium, IsCompleted = false }
        };
        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.HandleAsync(_testUserId, "json");

        // Assert
        var json = Encoding.UTF8.GetString(result.Content);
        json.Should().Contain("My Task");
        json.Should().NotContain("Other Task");
    }

    [Fact]
    public async Task HandleAsync_ExportsPriorityAsNumber()
    {
        // Arrange
        var task = new TodoTask
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            Title = "Test Task",
            Priority = TaskPriority.Urgent, // 3
            IsCompleted = false
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.HandleAsync(_testUserId, "json");

        // Assert
        var json = Encoding.UTF8.GetString(result.Content);
        json.Should().Contain("\"Priority\": 3"); // Urgent = 3 (with space in formatted JSON)
    }

    [Fact]
    public async Task HandleAsync_JsonFormat_OrdersByCreatedAtDescending()
    {
        // Arrange
        var tasks = new[]
        {
            new TodoTask { Id = Guid.NewGuid(), UserId = _testUserId, Title = "Old Task", Priority = TaskPriority.Low, IsCompleted = false, CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new TodoTask { Id = Guid.NewGuid(), UserId = _testUserId, Title = "New Task", Priority = TaskPriority.Medium, IsCompleted = false, CreatedAt = DateTime.UtcNow }
        };
        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.HandleAsync(_testUserId, "json");

        // Assert
        var json = Encoding.UTF8.GetString(result.Content);
        var indexNew = json.IndexOf("New Task");
        var indexOld = json.IndexOf("Old Task");
        indexNew.Should().BeLessThan(indexOld); // New task should come first
    }

    [Fact]
    public async Task HandleAsync_CsvFormat_EscapesQuotes()
    {
        // Arrange
        var task = new TodoTask
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            Title = "Task with \"quotes\"",
            Description = "Description with \"quotes\"",
            Priority = TaskPriority.Low,
            IsCompleted = false
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.HandleAsync(_testUserId, "csv");

        // Assert
        var csv = Encoding.UTF8.GetString(result.Content);
        csv.Should().Contain("Task with \"quotes\""); // CSV handles quotes
        csv.Should().Contain("Description with \"quotes\"");
    }
}
