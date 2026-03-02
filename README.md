# Task Flow

Full-stack task management app built with .NET 10 and React 19, demonstrating modern architecture and security practices.

## ✨ How To Run

Download or clone the code.

-Open the solution in visual studio,preferably 2026.

-Set LemonTodo.Server as the startup project,from top of the IDE at the startup project dropdown.

-Select https profile from next to that dropdown and run the application (F5 or Ctrl+F5).

It should download frontend dependencies for a minute or two and start the server and client should open up in the browser automatically. 

## Tech Stack

**Backend:** .NET 10, ASP.NET Core (Minimal APIs), EF Core (InMemory), JWT, xUnit, Moq  
**Frontend:** React 19, Vite, Material-UI, Vitest, React Testing Library

## Features

For dev environment, it has an optional two step secret management, (at this point just for JWT secret):

It will look for a .net User Secret locally.This will allow the user to stay logged in, while restarting the app.

If that local secret lookup doesnt work out it will create a temporary JWT secret in memory.

You can setup userSecret via this command in windows for local jwt secret storage:

dotnet user-secrets set "Jwt:Secret" "your-secret-value-here_at_least_15_characthers" --project LemonTodo.Server

Its totally optional.

(The UserSecretsId field in the backend .csproj file, is not a secret but a random location pointer for the local secret to be created at)


- **JWT Authentication** with PBKDF2 password hashing (100k iterations)
- **Full CRUD** task management with priority levels (Low/Medium/High/Urgent)
- **Real-time Dashboard** with task statistics and overdue tracking
- **Advanced Filtering** by status and priority, sorting by multiple criteria
- **Import/Export** tasks (JSON/CSV formats)
- **Material-UI** responsive design with smart UX (auto-focus, date picker)
- **Comprehensive Validation** on client and server with custom validators
- **Global Error Handling** with user-friendly messages

## ✨ Potential Enhancements

-Use result pattern at handlers and return more specific error messages.

-For prod or upper environments use cloud secret vaults or environment variables instead of using user secrets. 

-Implement health endpoints,application insight for better logging and monitoring.

-Add rate limiter.


**Authentication:** Refresh tokens(InMemory database doesn't persist refresh tokens), email verification, password reset, OAuth  
**Scalability:** Pagination, indexing, CDN


## Architecture

**Backend:** Minimal APIs with Handler pattern, DTOs, DI, Middleware pipeline  
**Frontend:** React hooks, component composition, Material-UI theming



##  Quick Start

**Prerequisites:** .NET 10 SDK, Node.js 18+

```bash
# Clone and navigate
git clone <repository-url>
cd LemonTodo

# Restore backend
dotnet restore

# Install frontend dependencies
cd lemontodo.client
npm install
cd ..

# Run application
dotnet run --project LemonTodo.Server

# Run tests
dotnet test
```>

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get JWT token

### Tasks (requires Bearer token)
- `GET /api/tasks` - Get all user tasks
- `GET /api/tasks/{id}` - Get specific task
- `POST /api/tasks` - Create task
- `PUT /api/tasks/{id}` - Update task
- `DELETE /api/tasks/{id}` - Delete task
- `PUT /api/tasks/bulk-delete` - Delete multiple tasks
- `GET /api/tasks/export?format=json|csv` - Export tasks
- `POST /api/tasks/import` - Import tasks from JSON file

**Sample Request:**
```json
POST /api/tasks
Authorization: Bearer {token}

{
  "title": "Complete project",
  "description": "Finish the LemonTodo app",
  "priority": 2,
  "dueDate": "2024-12-31"
}
```
</details>
```

## 🔒 Security

- **JWT**: HS256, 7-day expiration
- **Passwords**: PBKDF2 (100k iterations, SHA256, per-user salt)
- **Authorization**: Bearer token required, user isolation at DB level
- **Validation**: Client + server-side with custom validators
- **Protection**: EF Core parameterized queries (SQL injection), React auto-escaping (XSS)
- **Secrets**: Runtime JWT generation (dev), environment vars (prod)

## 🧪 Testing

**Backend Tests (xUnit + Moq):** PasswordHasher, Auth handlers, Task handlers, Validators, Middleware,Integration tests for endpoints  
**Frontend Tests (Vitest + RTL):** Components, hooks, error boundaries

Run: `dotnet test` (backend) | `npm test` (frontend)

**Approach:** Isolated in-memory DB per test, mocking, AAA pattern, edge cases
