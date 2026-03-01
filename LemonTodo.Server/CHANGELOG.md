# LemonTodo Server - Change Log

## Project Creation

This project was created using Visual Studio with the following steps:
- Create new ASP.NET Core Web API project
- Update project file to add a reference to the frontend project and set SPA properties
- Update `launchSettings.json` to register the SPA proxy as a startup assembly
- Add project to the startup projects list

## Security Enhancements

### ✅ Implemented (Production-Ready)

1. **Proper Configuration Handling**
   - User Secrets configured for development
   - Environment variable support
   - Production validation (fails fast if JWT secret missing)
   - See `SECRETS.md` for details

2. **No Secret Leakage**
   - Fixed: Replaced Console.WriteLine with proper ILogger in JWT events
   - Fixed: User enumeration attack prevention (combined login errors)
   - Generic error messages to clients
   - Detailed logging server-side only

3. **Safe Error Handling**
   - Generic error messages to clients (no stack traces)
   - Full exception details logged server-side
   - Appropriate HTTP status codes

4. **Correct Authorization Boundaries**
   - All task endpoints require authentication
   - User isolation enforced in database queries
   - JWT claims properly validated

5. **Defensive Input Validation**
   - Multi-layer validation (DTOs + Custom Validators + DB Constraints)
   - Added: Description length validation
   - Added: Enum range validation for Priority
   - Date validation for business rules

### ⚠️ Recommended for Production (TODO)

- Add security headers middleware (X-Frame-Options, CSP, etc.)
- Add rate limiting for authentication endpoints
- Set up monitoring/alerting (Application Insights/CloudWatch)
- Add health check endpoints
- Switch to persistent database (from InMemoryDatabase)

### 📚 Documentation

- `SECURITY.md` - Complete security implementation guide
- `SECRETS.md` - Secrets management for all environments
- `USER_SECRETS_SETUP.md` - Quick User Secrets setup guide

## Architecture

The project uses:
- **Minimal APIs** for lightweight endpoint definitions
- **Handler pattern** for business logic (single responsibility)
- **Static validators** for input validation
- **Service interfaces** for cross-cutting concerns (JWT, Password Hashing)
- **EF Core** for data access (with DbContext as repository/unit of work)

**Architecture Grade: Appropriate abstractions without over-engineering**

## Future Improvements (Optional)

- Consider Result Pattern if error handling becomes inconsistent (not needed yet)
- Consider Repository Pattern if switching databases frequently (using EF Core is fine)
- Consider MediatR if handler count exceeds 50+ (currently ~10)
