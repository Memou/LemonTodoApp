# Quick Start Guide

## Running the Application

### Option 1: Visual Studio
1. Open the solution in Visual Studio 2025
2. Set `LemonTodo.Server` as the startup project
3. Press F5 to run
4. The application will open in your browser at https://localhost:5001 (or similar)

### Option 2: Command Line
```bash
# From the root directory
dotnet run --project LemonTodo.Server
```

The frontend will be served through the backend using the SPA proxy.

## First Time Setup

1. **Register a new user**
   - Open the application
   - Click on "Register" tab
   - Enter a username (min 3 characters) and password (min 6 characters)
   - Click Register

2. **Create your first task**
   - Enter a task title
   - Optionally add description, priority, and due date
   - Click "Create Task"

## Testing the API

### Using the browser
Once the app is running, you can test the API endpoints directly:
- OpenAPI/Swagger UI will be available at: https://localhost:5001/openapi/v1.json

### Using curl or Postman
1. Register/Login to get a JWT token
2. Use the token in the Authorization header for task endpoints

Example:
```bash
# Register
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","password":"password123"}'

# Login
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","password":"password123"}'

# Create Task (replace YOUR_TOKEN with actual token)
curl -X POST https://localhost:5001/api/tasks \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title":"My First Task","priority":1}'
```

## Running Tests

```bash
# Run all tests
dotnet test

# Run tests with verbose output
dotnet test --verbosity normal

# Run specific test class
dotnet test --filter "FullyQualifiedName~TasksControllerTests"
```

## Troubleshooting

### Port Already in Use
If you get a port conflict error, you can change the port in `LemonTodo.Server/Properties/launchSettings.json`

### Frontend Not Loading
- Make sure Node.js is installed (v18+)
- Run `npm install` in the `lemontodo.client` directory
- Clear browser cache and reload

### Database Issues
Since we're using InMemory database:
- Data is lost when the application restarts
- Each test run uses a fresh database
- No migrations needed

## Development Tips

### Hot Reload
Both backend and frontend support hot reload:
- Backend: C# code changes are automatically compiled (in some cases)
- Frontend: React components reload automatically when saved

### Debugging
- Backend: Set breakpoints in Visual Studio or VS Code
- Frontend: Use browser DevTools (F12)
- API calls: Check the Network tab in DevTools

## Configuration

### JWT Secret
For development, a secret is auto-generated. For production:
1. Set an environment variable: `Jwt__Secret=your-secure-secret-here`
2. Or update `appsettings.json` (not recommended for production)

### CORS
CORS is configured for localhost by default. Update `Program.cs` for production URLs.

## Next Steps

1. ✅ Application is running
2. ✅ Tests are passing
3. Create some tasks to test functionality
4. Review the README.md for architecture details
5. Check the code for implementation patterns

## Production Deployment

For production deployment:
1. Update JWT secret in secure configuration
2. Switch from InMemory to a persistent database (SQL Server, PostgreSQL)
3. Configure proper CORS origins
4. Enable HTTPS
5. Set up proper logging and monitoring
6. Review security considerations in README.md

Happy coding! 🍋
