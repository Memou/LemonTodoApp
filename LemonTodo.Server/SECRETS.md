# Secrets Management

This document explains how secrets are managed in the LemonTodo API project.

## Development (User Secrets)

The project uses .NET User Secrets for local development. This keeps sensitive data out of source control.

### Initial Setup

The project is already configured with User Secrets. The `UserSecretsId` is stored in `LemonTodo.Server.csproj`:

```xml
<UserSecretsId>a37b2e4a-cbe2-4039-9140-a671d55e0830</UserSecretsId>
```

### Setting Your JWT Secret

If you're setting up the project for the first time, configure your JWT secret:

```sh
# Set JWT secret (minimum 32 characters recommended)
dotnet user-secrets set "Jwt:Secret" "your-secure-secret-key-here-min-32-chars" --project LemonTodo.Server

# Verify it was saved
dotnet user-secrets list --project LemonTodo.Server
```

### Managing Secrets

```sh
# View all secrets
dotnet user-secrets list --project LemonTodo.Server

# Add/update a secret
dotnet user-secrets set "Key:Name" "value" --project LemonTodo.Server

# Remove a secret
dotnet user-secrets remove "Key:Name" --project LemonTodo.Server

# Clear all secrets
dotnet user-secrets clear --project LemonTodo.Server
```

### Storage Location

Secrets are stored outside your project directory:

- **Windows**: `%APPDATA%\Microsoft\UserSecrets\a37b2e4a-cbe2-4039-9140-a671d55e0830\secrets.json`
- **macOS/Linux**: `~/.microsoft/usersecrets/a37b2e4a-cbe2-4039-9140-a671d55e0830/secrets.json`

## Configuration Hierarchy

The application loads configuration in this order (later sources override earlier ones):

1. `appsettings.json` (committed to Git)
2. `appsettings.Development.json` (excluded from Git)
3. User Secrets (Development environment only)
4. Environment variables
5. Command-line arguments

## Fallback Behavior

If no JWT secret is configured, the application generates a random secret per session:

```csharp
var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrEmpty(jwtSecret))
{
    jwtSecret = GenerateSecureSecret();
    Console.WriteLine("Generated JWT Secret for this session (not for production use)");
}
```

⚠️ **Warning**: Random secrets invalidate all existing JWT tokens when the application restarts.

## Production Deployment

For production environments, **DO NOT** use User Secrets. Instead, use:

### Option 1: Environment Variables

```sh
# Set environment variable
export Jwt__Secret="production-secret-key-here"

# Or in Docker
docker run -e Jwt__Secret="production-secret-key-here" ...
```

### Option 2: Azure Key Vault (Recommended for Azure)

```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

### Option 3: AWS Secrets Manager (for AWS)

```csharp
builder.Configuration.AddSecretsManager();
```

### Option 4: Kubernetes Secrets

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: lemontodo-secrets
type: Opaque
stringData:
  jwt-secret: "production-secret-key-here"
```

## Security Best Practices

1. **Never commit secrets to Git**
   - User Secrets are stored outside the repo
   - `appsettings.Development.json` and `appsettings.Production.json` are in `.gitignore`

2. **Use strong secrets**
   - JWT secrets should be at least 32 characters
   - Use cryptographically secure random values

3. **Different secrets per environment**
   - Development: User Secrets
   - Staging: Environment variables or cloud secret manager
   - Production: Cloud secret manager (Azure Key Vault, AWS Secrets Manager, etc.)

4. **Rotate secrets regularly**
   - Update production secrets periodically
   - Have a plan for secret rotation without downtime

## Troubleshooting

### "Generated JWT Secret for this session" message appears

This means no persistent secret is configured. Set one using:

```sh
dotnet user-secrets set "Jwt:Secret" "your-secret-here" --project LemonTodo.Server
```

### Tokens become invalid after restart

You're using the generated random secret. Configure a persistent secret as shown above.

### "UserSecretsId not found" error

Re-initialize user secrets:

```sh
dotnet user-secrets init --project LemonTodo.Server
```

### Secrets not loading on Mac/Linux

Verify the secrets file exists:

```sh
# macOS/Linux
cat ~/.microsoft/usersecrets/a37b2e4a-cbe2-4039-9140-a671d55e0830/secrets.json

# Windows (PowerShell)
Get-Content "$env:APPDATA\Microsoft\UserSecrets\a37b2e4a-cbe2-4039-9140-a671d55e0830\secrets.json"
```

## Current Configuration

The following secrets are expected:

| Key | Description | Required |
|-----|-------------|----------|
| `Jwt:Secret` | Secret key for signing JWT tokens | Yes (falls back to random) |
| `Jwt:Issuer` | JWT token issuer | No (defaults to "LemonTodoAPI") |
| `Jwt:Audience` | JWT token audience | No (defaults to "LemonTodoClient") |
| `Frontend:Url` | Frontend URL for CORS | No (defaults to "https://localhost:58900") |

## Team Onboarding

When new developers join:

1. Clone the repository
2. Run `dotnet user-secrets list --project LemonTodo.Server` to see expected secrets
3. Set their own JWT secret using `dotnet user-secrets set`
4. Each developer can use their own secret locally without conflicts
