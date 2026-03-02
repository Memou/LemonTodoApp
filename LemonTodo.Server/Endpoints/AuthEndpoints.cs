using LemonTodo.Server.DTOs;
using LemonTodo.Server.Handlers.Auth;
using Microsoft.AspNetCore.Mvc;

namespace LemonTodo.Server.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost("/register", Register)
            .WithName("Register")
            .WithOpenApi()
            .AllowAnonymous();

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithOpenApi()
            .AllowAnonymous();

        return group;
    }

    private static async Task<IResult> Register(
        [FromServices] RegisterHandler handler,
        [FromServices] ILogger<RegisterHandler> logger,
        [FromBody] RegisterRequest request)
    {
        logger.LogInformation("Registration attempt for username: {Username}", request.Username);

        var (response, error) = await handler.HandleAsync(request);

        if (response == null)
        {
            return Results.BadRequest(new { message = error ?? "Registration failed" });
        }

        return Results.Ok(response);
    }

    private static async Task<IResult> Login(
        [FromServices] LoginHandler handler,
        [FromServices] ILogger<LoginHandler> logger,
        [FromBody] LoginRequest request)
    {
        logger.LogInformation("Login attempt for username: {Username}", request.Username);

        var (response, error) = await handler.HandleAsync(request);

        if (response == null)
        {
            // Security: Same message for both user not found and invalid password
            return Results.Json(new { message = "Invalid username or password" }, statusCode: 401);
        }

        return Results.Ok(response);
    }
}
