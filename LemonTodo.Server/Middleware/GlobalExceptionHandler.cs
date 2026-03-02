using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;

namespace LemonTodo.Server.Middleware;

/// <summary>
/// Global exception handler that catches all unhandled exceptions and returns appropriate responses
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Log the exception with full details
        _logger.LogError(
            exception,
            "Unhandled exception occurred. Request: {Method} {Path}, User: {User}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            httpContext.User?.Identity?.Name ?? "Anonymous");

        // Determine status code based on exception type
        var (statusCode, title, detail) = exception switch
        {
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized", "You are not authorized to access this resource."),
            ArgumentException or ArgumentNullException => (StatusCodes.Status400BadRequest, "Bad Request", "Invalid request parameters."),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found", "The requested resource was not found."),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Operation Failed", "The operation could not be completed."),
            _ => (StatusCodes.Status500InternalServerError, "Server Error", "An unexpected error occurred.")
        };

        // Create problem details response
        var problemDetails = new
        {
            type = $"https://httpstatuses.com/{statusCode}",
            title,
            status = statusCode,
            detail,
            // Only include exception details in development
            error = _environment.IsDevelopment() ? exception.Message : null,
            stackTrace = _environment.IsDevelopment() ? exception.StackTrace : null,
            traceId = httpContext.TraceIdentifier
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; // Exception handled
    }
}
