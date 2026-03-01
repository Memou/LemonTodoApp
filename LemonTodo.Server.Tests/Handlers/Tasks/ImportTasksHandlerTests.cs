using FluentAssertions;
using LemonTodo.Server.Data;
using LemonTodo.Server.DTOs;
using LemonTodo.Server.Handlers.Tasks;
using LemonTodo.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LemonTodo.Server.Tests.Handlers.Tasks;

public class ImportTasksHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<ImportTasksHandler>> _mockLogger;
    private readonly ImportTasksHandler _handler;
    private readonly Guid _testUserId;

    public ImportTasksHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _mockLogger = new Mock<ILogger<ImportTasksHandler>>();
        _handler = new ImportTasksHandler(_context, _mockLogger.Object);
        _testUserId = Guid.NewGuid();
    }

    [Fact]
    public async Task HandleAsync_ValidTasks_ImportsSuccessfully()
    {
        // Arrange
        var request = new ImportTasksRequest
        {
            Tasks = new List<ImportTaskData>
            {
                new() { Title = "Task 1", Priority = TaskPriority.Low },
                new() { Title = "Task 2", Priority = TaskPriority.High }
            }
        };

        // Act
        var result = await _handler.HandleAsync(_testUserId, request);

        // Assert
        result.ImportedCount.Should().Be(2);
        result.SkippedCount.Should().Be(0);
        result.Errors.Should().BeNullOrEmpty();

        var tasksInDb = await _context.Tasks.Where(t => t.UserId == _testUserId).ToListAsync();
        tasksInDb.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_DuplicateTask_SkipsDuplicate()
    {
        // Arrange
        var existingTask = new TodoTask
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            Title = "Existing Task",
            Description = "Same description",
            Priority = TaskPriority.Medium,
            DueDate = new DateTime(2026, 3, 1),
            IsCompleted = false
        };
        _context.Tasks.Add(existingTask);
        await _context.SaveChangesAsync();

        var request = new ImportTasksRequest
        {
            Tasks = new List<ImportTaskData>
            {
                new() 
                { 
                    Title = "Existing Task", 
                    Description = "Same description",
                    Priority = TaskPriority.Medium,
                    DueDate = new DateTime(2026, 3, 1)
                }
            }
        };

        // Act
        var result = await _handler.HandleAsync(_testUserId, request);

        // Assert
        result.ImportedCount.Should().Be(0);
        result.SkippedCount.Should().Be(1);

        var tasksInDb = await _context.Tasks.Where(t => t.UserId == _testUserId).ToListAsync();
        tasksInDb.Should().HaveCount(1); // Still only the original task
    }

    [Fact]
    public async Task HandleAsync_SimilarTaskDifferentDescription_Imports()
    {
        // Arrange
        var existingTask = new TodoTask
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            Title = "Task",
            Description = "Old description",
            Priority = TaskPriority.Medium,
            IsCompleted = false
        };
        _context.Tasks.Add(existingTask);
        await _context.SaveChangesAsync();

        var request = new ImportTasksRequest
        {
            Tasks = new List<ImportTaskData>
            {
                new() 
                { 
                    Title = "Task", 
                    Description = "New description", // Different description
                    Priority = TaskPriority.Medium
                }
            }
        };

        // Act
        var result = await _handler.HandleAsync(_testUserId, request);

        // Assert
        result.ImportedCount.Should().Be(1);
        result.SkippedCount.Should().Be(0);

        var tasksInDb = await _context.Tasks.Where(t => t.UserId == _testUserId).ToListAsync();
        tasksInDb.Should().HaveCount(2); // Both tasks exist
    }

    [Fact]
    public async Task HandleAsync_InvalidTask_SkipsAndAddsError()
    {
        // Arrange
        var request = new ImportTasksRequest
        {
            Tasks = new List<ImportTaskData>
            {
                new() { Title = "", Priority = TaskPriority.Low }, // Invalid: empty title
                new() { Title = "Valid Task", Priority = TaskPriority.Medium }
            }
        };

        // Act
        var result = await _handler.HandleAsync(_testUserId, request);

        // Assert
        result.ImportedCount.Should().Be(1);
        result.SkippedCount.Should().Be(1);
        result.Errors.Should().NotBeNull();
        result.Errors.Should().Contain(e => e.Contains("Title is required"));
    }

    [Fact]
    public async Task HandleAsync_AssignsToCurrentUser()
    {
        // Arrange
        var request = new ImportTasksRequest
        {
            Tasks = new List<ImportTaskData>
            {
                new() { Title = "Imported Task", Priority = TaskPriority.Low }
            }
        };

        // Act
        var result = await _handler.HandleAsync(_testUserId, request);

        // Assert
        var taskInDb = await _context.Tasks.FirstOrDefaultAsync();
        taskInDb.Should().NotBeNull();
        taskInDb!.UserId.Should().Be(_testUserId);
    }

    [Fact]
    public async Task HandleAsync_GeneratesNewIds()
    {
        // Arrange
        var originalId = Guid.NewGuid();
        var request = new ImportTasksRequest
        {
            Tasks = new List<ImportTaskData>
            {
                new() { Id = originalId, Title = "Task", Priority = TaskPriority.Low }
            }
        };

        // Act
        var result = await _handler.HandleAsync(_testUserId, request);

        // Assert
        var taskInDb = await _context.Tasks.FirstOrDefaultAsync();
        taskInDb.Should().NotBeNull();
        taskInDb!.Id.Should().NotBe(originalId); // New ID generated
    }

    [Fact]
    public async Task HandleAsync_SetsCompletedAt_ForCompletedTasks()
    {
        // Arrange
        var request = new ImportTasksRequest
        {
            Tasks = new List<ImportTaskData>
            {
                new() { Title = "Completed Task", Priority = TaskPriority.Low, IsCompleted = true }
            }
        };

        var beforeImport = DateTime.UtcNow;

        // Act
        var result = await _handler.HandleAsync(_testUserId, request);

        // Assert
        var taskInDb = await _context.Tasks.FirstOrDefaultAsync();
        taskInDb.Should().NotBeNull();
        taskInDb!.IsCompleted.Should().BeTrue();
        taskInDb.CompletedAt.Should().NotBeNull();
        taskInDb.CompletedAt.Should().BeCloseTo(beforeImport, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task HandleAsync_DetectsDuplicatesWithinSameImport()
    {
        // Arrange
        var request = new ImportTasksRequest
        {
            Tasks = new List<ImportTaskData>
            {
                new() { Title = "Same Task", Priority = TaskPriority.Low },
                new() { Title = "Same Task", Priority = TaskPriority.Low } // Duplicate in same import
            }
        };

        // Act
        var result = await _handler.HandleAsync(_testUserId, request);

        // Assert
        result.ImportedCount.Should().Be(1);
        result.SkippedCount.Should().Be(1);

        var tasksInDb = await _context.Tasks.Where(t => t.UserId == _testUserId).ToListAsync();
        tasksInDb.Should().HaveCount(1); // Only one task imported
    }

    [Fact]
    public async Task HandleAsync_AllowsHistoricalDueDates()
    {
        // Arrange
        var request = new ImportTasksRequest
        {
            Tasks = new List<ImportTaskData>
            {
                new() 
                { 
                    Title = "Historical Task", 
                    Priority = TaskPriority.Low,
                    DueDate = DateTime.UtcNow.AddDays(-30) // Past date
                }
            }
        };

        // Act
        var result = await _handler.HandleAsync(_testUserId, request);

        // Assert
        result.ImportedCount.Should().Be(1);
        result.SkippedCount.Should().Be(0);
    }
}
