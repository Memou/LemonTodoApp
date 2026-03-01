using System.Text;
using LemonTodo.Server.Data;
using LemonTodo.Server.Services;
using LemonTodo.Server.Handlers.Tasks;
using LemonTodo.Server.Handlers.Auth;
using LemonTodo.Server.Endpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("LemonTodoDB"));

// Services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Task Handlers
builder.Services.AddScoped<GetTasksHandler>();
builder.Services.AddScoped<CreateTaskHandler>();
builder.Services.AddScoped<UpdateTaskHandler>();
builder.Services.AddScoped<DeleteTaskHandler>();
builder.Services.AddScoped<ImportTasksHandler>();
builder.Services.AddScoped<ExportTasksHandler>();

// Auth Handlers
builder.Services.AddScoped<RegisterHandler>();
builder.Services.AddScoped<LoginHandler>();

var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrEmpty(jwtSecret))
{
    if (builder.Environment.IsProduction())
    {
        throw new InvalidOperationException(
            "JWT Secret is required in production. Set via environment variable 'Jwt__Secret' or User Secrets.");
    }

    jwtSecret = GenerateSecureSecret();
    builder.Configuration["Jwt:Secret"] = jwtSecret;
    Console.WriteLine("⚠️  WARNING: Using temporary JWT secret (not persistent)");
    Console.WriteLine("   For persistent tokens, run: dotnet user-secrets set \"Jwt:Secret\" \"your-secret\" --project LemonTodo.Server");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "LemonTodoAPI",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "LemonTodoClient",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            // Security: Don't log exception details - may contain sensitive token information
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("Authentication failed for request from {IP}", 
                context.HttpContext.Connection.RemoteIpAddress);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var userId = context.Principal?.FindFirst("sub")?.Value;
            logger.LogDebug("Token validated for user {UserId}", userId);
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            // Security: Only log error type, not description which may contain sensitive info
            logger.LogWarning("Authentication challenge: {Error}", context.Error);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Minimal APIs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                builder.Configuration["Frontend:Url"] ?? "https://localhost:58900",
                "http://localhost:58900",
                "https://localhost:5173",
                "http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// Map Minimal API endpoints
app.MapAuthEndpoints();
app.MapTaskEndpoints();

app.MapFallbackToFile("/index.html");

app.Run();

static string GenerateSecureSecret()
{
    var key = new byte[32];
    using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
    rng.GetBytes(key);
    return Convert.ToBase64String(key);
}
