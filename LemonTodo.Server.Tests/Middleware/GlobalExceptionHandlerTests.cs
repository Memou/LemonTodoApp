using LemonTodo.Server.Middleware;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace LemonTodo.Server.Tests.Middleware;

public class GlobalExceptionHandlerTests
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> _loggerMock;
    private readonly Mock<IHostEnvironment> _environmentMock;
    private readonly GlobalExceptionHandler _handler;
    private readonly DefaultHttpContext _httpContext;

    public GlobalExceptionHandlerTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        _environmentMock = new Mock<IHostEnvironment>();
        _handler = new GlobalExceptionHandler(_loggerMock.Object, _environmentMock.Object);
        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();
    }

    [Fact]
    public async Task TryHandleAsync_UnauthorizedAccessException_Returns401()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Production");
        var exception = new UnauthorizedAccessException("Not authorized");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, _httpContext.Response.StatusCode);
        Assert.StartsWith("application/json", _httpContext.Response.ContentType);
    }

    [Fact]
    public async Task TryHandleAsync_ArgumentException_Returns400()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Production");
        var exception = new ArgumentException("Invalid argument");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(StatusCodes.Status400BadRequest, _httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_ArgumentNullException_Returns400()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Production");
        var exception = new ArgumentNullException("parameter");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(StatusCodes.Status400BadRequest, _httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_KeyNotFoundException_Returns404()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Production");
        var exception = new KeyNotFoundException("Resource not found");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(StatusCodes.Status404NotFound, _httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_InvalidOperationException_Returns409()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Production");
        var exception = new InvalidOperationException("Operation failed");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(StatusCodes.Status409Conflict, _httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_GenericException_Returns500()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Production");
        var exception = new Exception("Unexpected error");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, _httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_LogsError()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Production");
        var exception = new Exception("Test error");

        // Act
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_ProductionMode_HidesExceptionDetails()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Production");
        var exception = new Exception("Sensitive error details");

        // Act
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<JsonElement>(responseBody);

        // Should not contain error or stackTrace properties in production
        Assert.False(response.TryGetProperty("error", out var errorProp) && errorProp.ValueKind != JsonValueKind.Null);
        Assert.False(response.TryGetProperty("stackTrace", out var stackProp) && stackProp.ValueKind != JsonValueKind.Null);
    }

    [Fact]
    public async Task TryHandleAsync_DevelopmentMode_ShowsExceptionDetails()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Development");
        var exception = new Exception("Detailed error message");

        // Act
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<JsonElement>(responseBody);

        // Should contain error and stackTrace properties in development
        Assert.True(response.TryGetProperty("error", out var errorProp));
        Assert.Equal("Detailed error message", errorProp.GetString());
        Assert.True(response.TryGetProperty("stackTrace", out _));
    }

    [Fact]
    public async Task TryHandleAsync_IncludesTraceId()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Production");
        _httpContext.TraceIdentifier = "test-trace-id-123";
        var exception = new Exception("Test error");

        // Act
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<JsonElement>(responseBody);

        Assert.True(response.TryGetProperty("traceId", out var traceId));
        Assert.Equal("test-trace-id-123", traceId.GetString());
    }

    [Fact]
    public async Task TryHandleAsync_ReturnsJsonResponse()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Production");
        var exception = new Exception("Test error");

        // Act
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.StartsWith("application/json", _httpContext.Response.ContentType);
    }

    [Fact]
    public async Task TryHandleAsync_ResponseContainsRequiredFields()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Production");
        var exception = new Exception("Test error");

        // Act
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<JsonElement>(responseBody);

        // Verify required fields
        Assert.True(response.TryGetProperty("type", out _));
        Assert.True(response.TryGetProperty("title", out _));
        Assert.True(response.TryGetProperty("status", out _));
        Assert.True(response.TryGetProperty("detail", out _));
        Assert.True(response.TryGetProperty("traceId", out _));
    }

    [Fact]
    public async Task TryHandleAsync_LogsRequestContext()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Production");
        _httpContext.Request.Method = "POST";
        _httpContext.Request.Path = "/api/test";
        var exception = new Exception("Test error");

        // Act
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert - Verify that logging includes request method and path
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("POST") && v.ToString()!.Contains("/api/test")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_AlwaysReturnsTrue()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns("Production");
        var exception = new Exception("Test error");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert - Handler should always return true indicating it handled the exception
        Assert.True(result);
    }
}
