# 🍋 LemonTodo - Full Stack Task Management Application

A production-ready task management application built with .NET 10 and React, demonstrating modern software engineering practices, security, and architectural design.

## 📋 Table of Contents
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Architecture](#architecture)
- [Setup Instructions](#setup-instructions)
- [API Documentation](#api-documentation)
- [Security](#security)
- [Testing](#testing)
- [Design Decisions](#design-decisions)
- [Future Enhancements](#future-enhancements)

## ✨ Features

### Core Requirements
- **Authentication**: JWT-based authentication with secure password hashing (PBKDF2)
- **Task Management**: Full CRUD operations for tasks
- **Authorization**: Users can only access their own tasks
- **Data Persistence**: EF Core InMemory database

### Production-Minded Features
1. **Task Statistics Dashboard**: Real-time metrics showing total, completed, pending, and overdue tasks
2. **Advanced Filtering & Sorting**: Filter by completion status, priority; Sort by date, priority, due date, or title
3. **Priority Management**: 4-level priority system (Low, Medium, High, Urgent)
4. **Due Date Tracking**: Track and highlight overdue tasks
5. **Bulk Operations**: Delete multiple tasks at once
6. **Input Validation**: Comprehensive validation on both client and server
7. **Error Handling**: Graceful error handling with user-friendly messages
8. **Responsive UI**: Mobile-friendly modern design with centered layout

## 🛠 Technology Stack

### Backend
- **.NET 10**: Latest .NET framework
- **ASP.NET Core Web API**: RESTful API
- **Entity Framework Core**: InMemory database provider
- **JWT Authentication**: Secure token-based auth
- **xUnit**: Unit testing framework
- **Moq**: Mocking framework for tests

### Frontend
- **React 19**: Modern React with hooks
- **Vite**: Fast build tool and dev server
- **CSS3**: Modern styling with CSS Grid and Flexbox

## 🏗 Architecture

### Backend Architecture
```
LemonTodo.Server/
├── Controllers/          # API endpoints
│   ├── AuthController    # Authentication endpoints
│   └── TasksController   # Task management endpoints
├── Models/               # Domain models
│   ├── User              # User entity
│   └── TodoTask          # Task entity
├── DTOs/                 # Data Transfer Objects
│   ├── AuthDTOs          # Auth request/response models
│   └── TaskDTOs          # Task request/response models
├── Data/                 # Database context
│   └── ApplicationDbContext
├── Services/             # Business logic
│   ├── JwtTokenService   # JWT generation/validation
│   └── PasswordHasher    # Password hashing/verification
└── Program.cs            # Application configuration
```

### Key Design Patterns
1. **Repository Pattern**: EF Core DbContext acts as repository
2. **DTO Pattern**: Clean separation between API contracts and domain models
3. **Dependency Injection**: All services registered in DI container
4. **Service Layer**: Business logic separated from controllers

## 🚀 Setup Instructions

### Prerequisites
- .NET 10 SDK
- Node.js (v18 or later)
- Visual Studio 2025 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd LemonTodo
   ```

2. **Backend Setup**
   ```bash
   # Restore dependencies
   dotnet restore
   ```

3. **Frontend Setup**
   ```bash
   cd lemontodo.client
   npm install
   ```

4. **Run the Application**
   ```bash
   # From the root directory
   dotnet run --project LemonTodo.Server
   ```
   
   The application will start on `https://localhost:5001` (or the port specified in launchSettings.json)

5. **Run Tests**
   ```bash
   dotnet test
   ```

### Configuration

The application uses `appsettings.json` for configuration:

```json
{
  "Jwt": {
    "Secret": "",  // Generated at runtime if not provided
    "Issuer": "LemonTodoAPI",
    "Audience": "LemonTodoClient"
  },
  "Frontend": {
    "Url": "https://localhost:58900"
  }
}
```

**Important**: For production, set a strong JWT secret in environment variables or secure configuration.

## 📚 API Documentation

### Authentication Endpoints

#### Register User
```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "string",
  "password": "string"
}

Response: 200 OK
{
  "token": "string",
  "username": "string"
}
```

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "string",
  "password": "string"
}

Response: 200 OK
{
  "token": "string",
  "username": "string"
}
```

### Task Endpoints (Requires Authentication)

#### Get All Tasks
```http
GET /api/tasks?isCompleted={bool}&priority={int}&sortBy={string}&descending={bool}
Authorization: Bearer {token}

Response: 200 OK
{
  "tasks": [...],
  "totalCount": 0,
  "statistics": {
    "totalTasks": 0,
    "completedTasks": 0,
    "pendingTasks": 0,
    "overdueTasks": 0
  }
}
```

#### Get Single Task
```http
GET /api/tasks/{id}
Authorization: Bearer {token}
```

#### Create Task
```http
POST /api/tasks
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "string",
  "description": "string",
  "priority": 1,
  "dueDate": "2024-12-31"
}
```

#### Update Task
```http
PUT /api/tasks/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "string",
  "description": "string",
  "isCompleted": true,
  "priority": 2,
  "dueDate": "2024-12-31"
}
```

#### Delete Task
```http
DELETE /api/tasks/{id}
Authorization: Bearer {token}
```

#### Bulk Delete Tasks
```http
POST /api/tasks/bulk-delete
Authorization: Bearer {token}
Content-Type: application/json

[1, 2, 3]
```

## 🔒 Security

### Authentication & Authorization
- **JWT Tokens**: HS256 algorithm with 7-day expiration
- **Password Security**: PBKDF2 with 100,000 iterations, SHA256, random salt per user
- **Authorization**: Bearer token required for all task operations
- **User Isolation**: Database-level user ID filtering ensures data isolation

### Input Validation
- **Server-side validation**: DataAnnotations on DTOs
- **Client-side validation**: HTML5 validation + React form handling
- **SQL Injection Protection**: EF Core parameterized queries
- **XSS Protection**: React automatically escapes content

### Configuration Security
- **No hardcoded secrets**: JWT secret generated at runtime for development
- **Environment-based config**: Production secrets should use environment variables or Azure Key Vault
- **CORS**: Configured for specific origins only

### Error Handling
- **Safe error messages**: Generic errors to client, detailed logs server-side
- **Try-catch blocks**: All controller actions wrapped in exception handling
- **Logging**: Structured logging with different severity levels

## 🧪 Testing

### Test Coverage
The project includes comprehensive unit tests covering:

1. **PasswordHasherTests**: Password hashing and verification logic
2. **AuthControllerTests**: Registration, login, duplicate username handling
3. **TasksControllerTests**: CRUD operations, authorization, filtering, statistics

### Running Tests
```bash
dotnet test
```

### Test Approach
- **Isolated tests**: Each test uses a unique in-memory database
- **Mocking**: External dependencies mocked with Moq
- **Arrange-Act-Assert**: Clear test structure
- **Edge cases**: Invalid inputs, unauthorized access, not found scenarios

## 💡 Design Decisions

### Why EF Core InMemory?
- Fast for development and testing
- No external dependencies
- Easy to reset between tests
- Note: For production, switch to SQL Server, PostgreSQL, or Cosmos DB

### Why JWT?
- Stateless authentication (no server-side session storage)
- Scalable across multiple servers
- Industry-standard for SPAs
- Easy to validate without database lookup

### Why React?
- Component-based architecture
- Large ecosystem and community
- Excellent performance with Virtual DOM
- Easy state management with hooks

### Why Not Use Identity?
- Demonstrates understanding of authentication fundamentals
- Lighter weight for this use case
- More control over the implementation
- Could easily migrate to ASP.NET Core Identity in future

### DTOs vs Direct Model Exposure
- **Security**: Prevents over-posting attacks
- **Flexibility**: API contract independent of database schema
- **Validation**: Clear validation rules at API boundary
- **Versioning**: Easier to version API without changing domain models

## 🚀 Future Enhancements

### High Priority
1. **Database Migration**: Switch to persistent database (SQL Server/PostgreSQL)
2. **Refresh Tokens**: Implement refresh token flow for better security
3. **Email Verification**: Add email confirmation on registration
4. **Password Reset**: Forgot password functionality
5. **Task Sharing**: Share tasks with other users
6. **Task Categories**: Organize tasks by categories/projects

### Features
1. **Recurring Tasks**: Support for repeating tasks
2. **File Attachments**: Attach files to tasks
3. **Comments**: Add comments/notes to tasks
4. **Notifications**: Email or push notifications for due dates
5. **Search**: Full-text search across tasks
6. **Tags**: Tag system for better organization
7. **Dark Mode**: UI theme toggle

### Technical Improvements
1. **Rate Limiting**: Protect against abuse
2. **Caching**: Redis caching for frequently accessed data
3. **API Versioning**: Support multiple API versions
4. **GraphQL**: Alternative to REST API
5. **Real-time Updates**: SignalR for live task updates
6. **Containerization**: Docker support
7. **CI/CD**: GitHub Actions or Azure DevOps pipelines
8. **Monitoring**: Application Insights or similar
9. **Health Checks**: API health monitoring endpoints
10. **Swagger/OpenAPI**: Interactive API documentation

### Scalability Considerations
1. **Horizontal Scaling**: Stateless design supports load balancing
2. **Database Optimization**: Indexes on frequently queried columns
3. **Pagination**: Implement pagination for large task lists
4. **Lazy Loading**: Defer loading of task details until needed
5. **CDN**: Serve static frontend assets from CDN
6. **Background Jobs**: Use Hangfire for scheduled tasks

## 📝 Assumptions

1. **Single Device**: No multi-device synchronization in current version
2. **English Only**: No internationalization (yet)
3. **Individual Use**: Designed for personal task management
4. **Modern Browsers**: Assumes ES6+ JavaScript support
5. **HTTPS**: Production deployment should use HTTPS
6. **Time Zones**: All dates stored in UTC, displayed in local time

## 👨‍💻 Development Notes

### Code Quality
- **Consistent naming**: PascalCase for C#, camelCase for JavaScript
- **Comments**: Minimal, code should be self-documenting
- **Error handling**: Consistent patterns across codebase
- **Logging**: Structured logging with context

### Git Workflow
- Feature branches for new development
- Pull requests with code review
- Semantic versioning for releases

## 📄 License

This project is for evaluation purposes.

## 🙏 Acknowledgments

Built as a take-home test demonstrating:
- Clean architecture principles
- RESTful API design
- Security best practices
- Production-ready code quality
- Comprehensive testing approach
