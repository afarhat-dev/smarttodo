# PostgreSQL Implementation for SmartTodo

## Overview
This implementation adds PostgreSQL database support to the SmartTodo MCP Server. The application can switch between in-memory and PostgreSQL storage using a configuration setting.

## Features
- ✅ Entity Framework Core 10.0 with Npgsql
- ✅ DbContext with proper entity configuration
- ✅ Repository pattern implementation
- ✅ Database connection pooling
- ✅ Transactional operations
- ✅ Configurable via appsettings.json

## Configuration

### appsettings.json
```json
{
    "Database": {
        "UsePostgreSQL": false  // Set to true to use PostgreSQL
    },
    "ConnectionStrings": {
        "PostgreSQL": "Host=localhost;Port=5432;Database=smarttodo;Username=postgres;Password=your_password"
    }
}
```

## Database Schema

### TodoItems Table
| Column | Type | Constraints |
|--------|------|-------------|
| Id | UUID | Primary Key |
| Title | VARCHAR(200) | NOT NULL |
| Description | VARCHAR(2000) | NULL |
| IsCompleted | BOOLEAN | NOT NULL |
| Status | VARCHAR | NOT NULL (enum as string) |
| CreatedAt | TIMESTAMP | NOT NULL |
| UpdatedAt | TIMESTAMP | NOT NULL |
| StartDate | TIMESTAMP | NULL |
| CompletedAt | TIMESTAMP | NULL |

### Indexes
- IX_TodoItems_Status
- IX_TodoItems_IsCompleted
- IX_TodoItems_CreatedAt
- IX_TodoItems_UpdatedAt

## Setup Instructions

### 1. Install PostgreSQL
Ensure PostgreSQL 12+ is installed and running.

### 2. Create Database
```sql
CREATE DATABASE smarttodo;
```

### 3. Update Connection String
Edit `appsettings.json` and update the PostgreSQL connection string with your credentials.

### 4. Run Migrations
From the infrastructure project directory:
```bash
cd SmartTodoSolution/source/infrastructure/SmartTodo.Infrastructure

# Add migration (first time only)
dotnet ef migrations add InitialCreate --startup-project ../../mcp/SmartTodo.McpServer

# Apply migration to database
dotnet ef database update --startup-project ../../mcp/SmartTodo.McpServer
```

### 5. Enable PostgreSQL
Set `Database:UsePostgreSQL` to `true` in appsettings.json

### 6. Run MCP Server
```bash
cd SmartTodoSolution/source/mcp/SmartTodo.McpServer
dotnet run
```

## Architecture

### Components

#### TodoDbContext
- Manages database connection
- Configures entity mappings
- Located in: `SmartTodo.Infrastructure/Persistence/TodoDbContext.cs`

#### TodoItemConfiguration
- Defines table schema
- Configures indexes
- Sets constraints
- Located in: `SmartTodo.Infrastructure/Persistence/Configurations/TodoItemConfiguration.cs`

#### PostgreSqlTodoRepository
- Implements ITodoRepository
- Handles CRUD operations
- Uses EF Core for data access
- Located in: `SmartTodo.Infrastructure/Repositories/PostgreSqlTodoRepository.cs`

### Dependency Injection

When PostgreSQL is enabled:
- `TodoDbContext` registered with connection pooling
- `ITodoRepository` → `PostgreSqlTodoRepository` (Transient)
- `ITodoService` → `TodoService` (Transient)
- Handlers registered as Transient

When In-Memory is used:
- `ITodoRepository` → `InMemoryTodoRepository` (Singleton)
- `ITodoService` → `TodoService` (Singleton)
- Handlers registered as Singleton

## Migration Commands

### Create New Migration
```bash
dotnet ef migrations add <MigrationName> \
  --project SmartTodoSolution/source/infrastructure/SmartTodo.Infrastructure \
  --startup-project SmartTodoSolution/source/mcp/SmartTodo.McpServer
```

### Update Database
```bash
dotnet ef database update \
  --project SmartTodoSolution/source/infrastructure/SmartTodo.Infrastructure \
  --startup-project SmartTodoSolution/source/mcp/SmartTodo.McpServer
```

### Rollback Migration
```bash
dotnet ef database update <PreviousMigrationName> \
  --project SmartTodoSolution/source/infrastructure/SmartTodo.Infrastructure \
  --startup-project SmartTodoSolution/source/mcp/SmartTodo.McpServer
```

### List Migrations
```bash
dotnet ef migrations list \
  --project SmartTodoSolution/source/infrastructure/SmartTodo.Infrastructure \
  --startup-project SmartTodoSolution/source/mcp/SmartTodo.McpServer
```

## Performance Considerations

1. **Connection Pooling**: Uses `AddDbContextPool` for better performance
2. **AsNoTracking**: Read operations use AsNoTracking for efficiency
3. **Indexes**: Proper indexes on commonly queried columns
4. **Transient Lifetime**: New context per request prevents context bloat

## Security

- ⚠️ **Important**: Never commit connection strings with real credentials
- Use environment variables for production:
  ```bash
  export ConnectionStrings__PostgreSQL="Host=...;Password=secret"
  ```
- Or use Azure Key Vault, AWS Secrets Manager, etc.

## Troubleshooting

### Connection Failed
- Verify PostgreSQL is running: `pg_isready`
- Check connection string format
- Ensure user has database permissions

### Migration Errors
- Delete migrations folder and recreate
- Check for pending migrations: `dotnet ef migrations list`
- Verify DbContext can be created

### Performance Issues
- Check index usage with EXPLAIN ANALYZE
- Monitor connection pool usage
- Consider adjusting pool size in connection string

## Switching Between Storage Types

The application can switch between In-Memory and PostgreSQL without code changes:

1. **To PostgreSQL**: Set `Database:UsePostgreSQL` to `true`
2. **To In-Memory**: Set `Database:UsePostgreSQL` to `false`

## Compatibility

- .NET 10.0
- Entity Framework Core 10.0.1
- Npgsql.EntityFrameworkCore.PostgreSQL 10.0.1
- PostgreSQL 12+

## Future Enhancements

- [ ] Database connection health checks
- [ ] Retry policies for transient failures
- [ ] Read replicas support
- [ ] Audit logging
- [ ] Soft delete support
- [ ] Multi-tenancy support

## Testing

Run tests with PostgreSQL:
```bash
# Set test connection string
export ConnectionStrings__PostgreSQL="Host=localhost;Database=smarttodo_test;..."

# Run tests
dotnet test --filter Category=Integration
```

## References

- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Npgsql Documentation](https://www.npgsql.org/efcore/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
