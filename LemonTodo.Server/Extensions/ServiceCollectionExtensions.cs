using LemonTodo.Server.Data;
using LemonTodo.Server.Handlers.Auth;
using LemonTodo.Server.Handlers.Tasks;
using LemonTodo.Server.Services;
using Microsoft.EntityFrameworkCore;

namespace LemonTodo.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("LemonTodoDB"));

        // Core Services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }

    public static IServiceCollection AddHandlers(this IServiceCollection services)
    {
        // Task Handlers
        services.AddScoped<GetTasksHandler>();
        services.AddScoped<CreateTaskHandler>();
        services.AddScoped<UpdateTaskHandler>();
        services.AddScoped<DeleteTaskHandler>();
        services.AddScoped<ImportTasksHandler>();
        services.AddScoped<ExportTasksHandler>();

        // Auth Handlers
        services.AddScoped<RegisterHandler>();
        services.AddScoped<LoginHandler>();

        return services;
    }
}
