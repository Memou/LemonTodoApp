using LemonTodo.Server.Middleware;
using Microsoft.AspNetCore.Http;

namespace LemonTodo.Server.Tests.Middleware;

public class SecurityHeadersMiddlewareTests
{
    private readonly DefaultHttpContext _httpContext;
    private bool _nextCalled;

    public SecurityHeadersMiddlewareTests()
    {
        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();
        _nextCalled = false;
    }

    private Task NextDelegate(HttpContext context)
    {
        _nextCalled = true;
        return Task.CompletedTask;
    }

    [Fact]
    public async Task InvokeAsync_AddsXFrameOptionsHeader()
    {
        // Arrange
        var middleware = new SecurityHeadersMiddleware(NextDelegate);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.True(_httpContext.Response.Headers.ContainsKey("X-Frame-Options"));
        Assert.Equal("DENY", _httpContext.Response.Headers["X-Frame-Options"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_AddsXContentTypeOptionsHeader()
    {
        // Arrange
        var middleware = new SecurityHeadersMiddleware(NextDelegate);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.True(_httpContext.Response.Headers.ContainsKey("X-Content-Type-Options"));
        Assert.Equal("nosniff", _httpContext.Response.Headers["X-Content-Type-Options"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_AddsXXSSProtectionHeader()
    {
        // Arrange
        var middleware = new SecurityHeadersMiddleware(NextDelegate);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.True(_httpContext.Response.Headers.ContainsKey("X-XSS-Protection"));
        Assert.Equal("1; mode=block", _httpContext.Response.Headers["X-XSS-Protection"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_AddsContentSecurityPolicyHeader()
    {
        // Arrange
        var middleware = new SecurityHeadersMiddleware(NextDelegate);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.True(_httpContext.Response.Headers.ContainsKey("Content-Security-Policy"));
        var csp = _httpContext.Response.Headers["Content-Security-Policy"].ToString();
        Assert.Contains("default-src 'self'", csp);
        Assert.Contains("script-src 'self' 'unsafe-inline' 'unsafe-eval'", csp);
        Assert.Contains("style-src 'self' 'unsafe-inline'", csp);
        Assert.Contains("img-src 'self' data: https:", csp);
        Assert.Contains("font-src 'self' data:", csp);
        Assert.Contains("connect-src 'self'", csp);
        Assert.Contains("frame-ancestors 'none'", csp);
    }

    [Fact]
    public async Task InvokeAsync_AddsReferrerPolicyHeader()
    {
        // Arrange
        var middleware = new SecurityHeadersMiddleware(NextDelegate);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.True(_httpContext.Response.Headers.ContainsKey("Referrer-Policy"));
        Assert.Equal("strict-origin-when-cross-origin", _httpContext.Response.Headers["Referrer-Policy"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_AddsPermissionsPolicyHeader()
    {
        // Arrange
        var middleware = new SecurityHeadersMiddleware(NextDelegate);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.True(_httpContext.Response.Headers.ContainsKey("Permissions-Policy"));
        var policy = _httpContext.Response.Headers["Permissions-Policy"].ToString();
        Assert.Contains("accelerometer=()", policy);
        Assert.Contains("camera=()", policy);
        Assert.Contains("geolocation=()", policy);
        Assert.Contains("gyroscope=()", policy);
        Assert.Contains("magnetometer=()", policy);
        Assert.Contains("microphone=()", policy);
        Assert.Contains("payment=()", policy);
        Assert.Contains("usb=()", policy);
    }

    [Fact]
    public async Task InvokeAsync_CallsNextMiddleware()
    {
        // Arrange
        var middleware = new SecurityHeadersMiddleware(NextDelegate);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.True(_nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_AddsAllSecurityHeadersInSingleRequest()
    {
        // Arrange
        var middleware = new SecurityHeadersMiddleware(NextDelegate);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert - Verify all security headers are present
        Assert.True(_httpContext.Response.Headers.ContainsKey("X-Frame-Options"));
        Assert.True(_httpContext.Response.Headers.ContainsKey("X-Content-Type-Options"));
        Assert.True(_httpContext.Response.Headers.ContainsKey("X-XSS-Protection"));
        Assert.True(_httpContext.Response.Headers.ContainsKey("Content-Security-Policy"));
        Assert.True(_httpContext.Response.Headers.ContainsKey("Referrer-Policy"));
        Assert.True(_httpContext.Response.Headers.ContainsKey("Permissions-Policy"));
        Assert.Equal(6, _httpContext.Response.Headers.Count);
    }

    [Fact]
    public async Task InvokeAsync_DoesNotModifyRequestHeaders()
    {
        // Arrange
        var middleware = new SecurityHeadersMiddleware(NextDelegate);
        _httpContext.Request.Headers.Append("Custom-Header", "test-value");

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        Assert.Single(_httpContext.Request.Headers);
        Assert.Equal("test-value", _httpContext.Request.Headers["Custom-Header"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_WorksWithMultipleRequests()
    {
        // Arrange
        var middleware = new SecurityHeadersMiddleware(NextDelegate);
        var context1 = new DefaultHttpContext();
        var context2 = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context1);
        await middleware.InvokeAsync(context2);

        // Assert
        Assert.True(context1.Response.Headers.ContainsKey("X-Frame-Options"));
        Assert.True(context2.Response.Headers.ContainsKey("X-Frame-Options"));
    }
}
