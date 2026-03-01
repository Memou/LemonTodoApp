# Database Migrations

This folder contains Entity Framework Core database migrations for the LemonTodo application.

## Creating a New Migration

When you're ready to switch from InMemoryDatabase to a real database (e.g., SQL Server, PostgreSQL), use the following commands:

```bash
# Add a new migration
dotnet ef migrations add InitialCreate --output-dir Data/Migrations

# Update the database
dotnet ef database update
```

## Note

The project currently uses InMemoryDatabase for development. Migrations will be needed when you switch to a persistent database provider.
