using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using LemonTodo.Server.Data;
using LemonTodo.Server.DTOs;
using LemonTodo.Server.Endpoints;
using LemonTodo.Server.Handlers.Tasks;
using LemonTodo.Server.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace LemonTodo.Server.Tests.Endpoints;

public class TaskEndpointsTests : IDisposable
{
    private readonly WebApplication _app;
    private readonly HttpClient _client;
    private readonly ApplicationDbContext _context;
    private readonly Guid _testUserId;
    private readonly string _testToken = "test-token";

    public TaskEndpointsTests()
    {
        _testUserId = Guid.NewGuid();

        var builder = WebApplication.CreateBuilder();

        // Setup in-memory database
        var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(dbOptions);

        builder.Services.AddSingleton(_context);
        builder.Services.AddScoped<GetTasksHandler>();
        builder.Services.AddScoped<CreateTaskHandler>();
        builder.Services.AddScoped<UpdateTaskHandler>();
        builder.Services.AddScoped<DeleteTaskHandler>();
        builder.Services.AddScoped<ExportTasksHandler>();
        builder.Services.AddScoped<ImportTasksHandler>();

        builder.Services.AddLogging();

        // Mock authentication
        builder.Services.AddAuthentication("Test")
            .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
        builder.Services.AddAuthorization();

        builder.WebHost.UseTestServer();

        _app = builder.Build();

        _app.UseAuthentication();
        _app.UseAuthorization();

        _app.MapTaskEndpoints();

        _app.StartAsync().Wait();

        var server = _app.Services.GetRequiredService<IServer>();
        _client = ((IHost)_app).GetTestClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", _testToken);

        // Inject test user ID into the authentication
        TestAuthHandler.TestUserId = _testUserId;
    }

    [Fact]
    public async Task GetTasks_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var clientWithoutAuth = ((IHost)_app).GetTestClient();

        // Act
        var response = await clientWithoutAuth.GetAsync("/api/tasks/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTasks_WithAuthentication_ReturnsOk()
    {
        // Arrange
        var task = new TodoTask
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            Title = "Test Task",
            Priority = TaskPriority.Medium,
            IsCompleted = false
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/tasks/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<TaskListResponse>();
        content.Should().NotBeNull();
        content!.Tasks.Should().HaveCount(1);
        content.Tasks.First().Title.Should().Be("Test Task");
    }

    [Fact]
    public async Task GetTasks_WithFilters_ReturnsFilteredTasks()
    {
        // Arrange
        var tasks = new[]
        {
            new TodoTask { Id = Guid.NewGuid(), UserId = _testUserId, Title = "Task 1", Priority = TaskPriority.High, IsCompleted = false },
            new TodoTask { Id = Guid.NewGuid(), UserId = _testUserId, Title = "Task 2", Priority = TaskPriority.Low, IsCompleted = true },
            new TodoTask { Id = Guid.NewGuid(), UserId = _testUserId, Title = "Task 3", Priority = TaskPriority.High, IsCompleted = false }
        };
        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/tasks/?isCompleted=false&priority=High");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<TaskListResponse>();
        content!.Tasks.Should().HaveCount(2);
        content.Tasks.Should().AllSatisfy(t =>
        {
            t.IsCompleted.Should().BeFalse();
            t.Priority.Should().Be(TaskPriority.High);
        });
    }

    [Fact]
    public async Task GetTask_ExistingTask_ReturnsTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new TodoTask
        {
            Id = taskId,
            UserId = _testUserId,
            Title = "Test Task",
            Description = "Test Description",
            Priority = TaskPriority.High,
            IsCompleted = false
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/tasks/{taskId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<TaskResponse>();
        content.Should().NotBeNull();
        content!.Id.Should().Be(taskId);
        content.Title.Should().Be("Test Task");
        content.Description.Should().Be("Test Description");
    }

    [Fact]
    public async Task GetTask_NonExistingTask_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/tasks/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTask_OtherUsersTask_ReturnsNotFound()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var task = new TodoTask
        {
            Id = taskId,
            UserId = otherUserId,
            Title = "Other User's Task",
            Priority = TaskPriority.Low,
            IsCompleted = false
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/tasks/{taskId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateTask_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "New Task",
            Description = "New Description",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(7)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks/", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<TaskResponse>();
        content.Should().NotBeNull();
        content!.Title.Should().Be("New Task");
        content.Description.Should().Be("New Description");
        content.Priority.Should().Be(TaskPriority.High);

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/api/tasks/{content.Id}");
    }

    [Fact]
    public async Task UpdateTask_ExistingTask_ReturnsOk()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new TodoTask
        {
            Id = taskId,
            UserId = _testUserId,
            Title = "Old Title",
            Priority = TaskPriority.Low,
            IsCompleted = false
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var updateRequest = new UpdateTaskRequest
        {
            Title = "Updated Title",
            Description = "Updated Description",
            Priority = TaskPriority.High,
            IsCompleted = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/tasks/{taskId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<TaskResponse>();
        content.Should().NotBeNull();
        content!.Title.Should().Be("Updated Title");
        content.Description.Should().Be("Updated Description");
        content.Priority.Should().Be(TaskPriority.High);
        content.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateTask_NonExistingTask_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        var updateRequest = new UpdateTaskRequest
        {
            Title = "Updated Title",
            Priority = TaskPriority.Medium
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/tasks/{nonExistingId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTask_ExistingTask_ReturnsNoContent()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new TodoTask
        {
            Id = taskId,
            UserId = _testUserId,
            Title = "Task to Delete",
            Priority = TaskPriority.Low,
            IsCompleted = false
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/tasks/{taskId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var deletedTask = await _context.Tasks.FindAsync(taskId);
        deletedTask.Should().BeNull();
    }

    [Fact]
    public async Task DeleteTask_NonExistingTask_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/tasks/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task BulkDeleteTasks_MultipleTasks_ReturnsOk()
    {
        // Arrange
        var taskIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var tasks = taskIds.Select(id => new TodoTask
        {
            Id = id,
            UserId = _testUserId,
            Title = $"Task {id}",
            Priority = TaskPriority.Medium,
            IsCompleted = false
        }).ToArray();

        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks/bulk-delete", taskIds);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        content.GetProperty("deletedCount").GetInt32().Should().Be(3);

        var remainingTasks = await _context.Tasks.Where(t => taskIds.Contains(t.Id)).ToListAsync();
        remainingTasks.Should().BeEmpty();
    }

    [Fact]
    public async Task BulkDeleteTasks_NoTasksFound_ReturnsNotFound()
    {
        // Arrange
        var nonExistingIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks/bulk-delete", nonExistingIds);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ExportTasks_JsonFormat_ReturnsFile()
    {
        // Arrange
        var task = new TodoTask
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            Title = "Export Task",
            Priority = TaskPriority.High,
            IsCompleted = false
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/tasks/export?format=json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Export Task");
    }

    [Fact]
    public async Task ExportTasks_CsvFormat_ReturnsFile()
    {
        // Arrange
        var task = new TodoTask
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            Title = "CSV Export Task",
            Priority = TaskPriority.Medium,
            IsCompleted = true
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/tasks/export?format=csv");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
    }

    [Fact]
    public async Task ImportTasks_ValidRequest_ReturnsOk()
    {
        // Arrange
        var importRequest = new ImportTasksRequest
        {
            Tasks =
            [
                new ImportTaskData
                {
                    Title = "Imported Task 1",
                    Description = "Description 1",
                    Priority = TaskPriority.High,
                    IsCompleted = false
                },
                new ImportTaskData
                {
                    Title = "Imported Task 2",
                    Description = "Description 2",
                    Priority = TaskPriority.Low,
                    IsCompleted = true
                }
            ]
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks/import", importRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        content.GetProperty("importedCount").GetInt32().Should().Be(2);
        content.GetProperty("skippedCount").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task ImportTasks_EmptyRequest_ReturnsBadRequest()
    {
        // Arrange
        var emptyRequest = new ImportTasksRequest
        {
            Tasks = []
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks/import", emptyRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ImportTasks_NullRequest_ReturnsBadRequest()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks/import", (ImportTasksRequest?)null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    public void Dispose()
    {
        _client?.Dispose();
        _app?.DisposeAsync().AsTask().Wait();
        _context?.Dispose();
    }
}

// Test Authentication Handler
public class TestAuthHandler : Microsoft.AspNetCore.Authentication.AuthenticationHandler<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions>
{
    public static Guid TestUserId { get; set; }

    public TestAuthHandler(
        Microsoft.Extensions.Options.IOptionsMonitor<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions> options,
        Microsoft.Extensions.Logging.ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<Microsoft.AspNetCore.Authentication.AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return Task.FromResult(Microsoft.AspNetCore.Authentication.AuthenticateResult.Fail("Missing Authorization Header"));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, TestUserId.ToString()),
            new Claim(ClaimTypes.Name, "TestUser")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new Microsoft.AspNetCore.Authentication.AuthenticationTicket(principal, "Test");

        return Task.FromResult(Microsoft.AspNetCore.Authentication.AuthenticateResult.Success(ticket));
    }
}
