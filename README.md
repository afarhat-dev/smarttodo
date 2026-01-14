# SmartTodo

A comprehensive todo management system built with clean architecture principles, featuring both REST API and MCP (Model Context Protocol) server interfaces.

## Overview

SmartTodo is a modern todo list application that demonstrates clean architecture, separation of concerns, and dual interface support. It allows you to manage your todos through:
- **REST API**: Traditional HTTP endpoints for web and mobile applications
- **MCP Server**: AI assistant integration for natural language todo management

## Architecture

The project follows clean architecture with clear separation of layers:

```
SmartTodo
├── Domain Layer          # Core business entities and enums
├── Application Layer     # Business logic, interfaces, DTOs
├── Infrastructure Layer  # Data access (in-memory, future: PostgreSQL)
├── API Layer            # REST API controllers
└── MCP Server Layer     # Model Context Protocol implementation
```

### Project Structure

```
SmartTodoSolution/
├── source/
│   ├── core/
│   │   ├── SmartTodo.Domain/           # Domain entities and business rules
│   │   └── SmartTodo.Application/      # Application services and DTOs
│   ├── infrastructure/
│   │   └── SmartTodo.Infrastructure/   # Repository implementations
│   ├── mcp/
│   │   └── SmartTodo.McpServer/        # MCP server for AI integration
│   └── ui/
│       └── StdApi/                     # REST API
```

## Features

### Todo Management
- ✅ Create, read, update, delete todos
- ✅ Rich metadata: CreatedAt, UpdatedAt, StartDate, CompletedAt
- ✅ Status tracking: NotStarted, InProgress, OnHold, Completed, Cancelled
- ✅ Comprehensive filtering by status, dates, and completion state
- ✅ Lifecycle methods: Start, Pause, Resume, Complete, Cancel

### REST API
- Full CRUD operations via HTTP endpoints
- Swagger/OpenAPI documentation
- Query parameter filtering
- Structured error responses

### MCP Server (AI Integration)
- 10 specialized tools for todo management
- 4 resource endpoints for data access
- 3 guided prompts for common workflows
- Compatible with Claude Desktop and other MCP clients

## Getting Started

### Prerequisites
- .NET 10.0 SDK or later
- (Optional) Claude Desktop for MCP integration

### Installation

1. Clone the repository:
```bash
git clone https://github.com/afarhat-dev/smarttodo.git
cd smarttodo
```

2. Build the solution:
```bash
cd SmartTodoSolution
dotnet build
```

### Running the REST API

```bash
cd source/ui/StdApi
dotnet run
```

The API will be available at `https://localhost:5001` (HTTPS) or `http://localhost:5000` (HTTP).

Access Swagger documentation at: `https://localhost:5001/swagger`

### Running the MCP Server

```bash
cd source/mcp/SmartTodo.McpServer
dotnet run
```

For Claude Desktop integration, see the [MCP Server README](SmartTodoSolution/source/mcp/SmartTodo.McpServer/README.md).

## API Documentation

### REST API Endpoints

#### Create Todo
```http
POST /api/todo
Content-Type: application/json

{
  "title": "Review pull request",
  "description": "Review the new authentication feature"
}
```

#### Get All Todos (with filtering)
```http
GET /api/todo?status=InProgress&createdFrom=2024-01-01
```

#### Get Todo by ID
```http
GET /api/todo/{id}
```

#### Update Todo
```http
PUT /api/todo/{id}
Content-Type: application/json

{
  "title": "Updated title",
  "status": "InProgress"
}
```

#### Delete Todo
```http
DELETE /api/todo/{id}
```

### MCP Tools

The MCP server provides 10 tools:
- `create_todo` - Create a new todo
- `get_todo` - Get a specific todo
- `list_todos` - List todos with filtering
- `update_todo` - Update a todo
- `delete_todo` - Delete a todo
- `start_todo` - Start working on a todo
- `complete_todo` - Mark todo as complete
- `pause_todo` - Put todo on hold
- `resume_todo` - Resume a paused todo
- `cancel_todo` - Cancel a todo

For detailed MCP documentation, see [MCP Server README](SmartTodoSolution/source/mcp/SmartTodo.McpServer/README.md).

## Data Model

### TodoItem Entity

```csharp
{
  "id": "guid",
  "title": "string",
  "description": "string?",
  "isCompleted": "boolean",
  "status": "NotStarted | InProgress | OnHold | Completed | Cancelled",
  "createdAt": "DateTime",
  "updatedAt": "DateTime",
  "startDate": "DateTime?",
  "completedAt": "DateTime?"
}
```

### TodoStatus Enum

- **NotStarted** (0): Todo has been created but not started
- **InProgress** (1): Work is currently in progress
- **OnHold** (2): Todo is paused temporarily
- **Completed** (2): Todo is finished
- **Cancelled** (4): Todo has been cancelled

## Filtering Options

Both REST API and MCP server support filtering by:
- **Status**: Filter by current status
- **IsCompleted**: Filter by completion state
- **CreatedAt**: Date range for creation date
- **UpdatedAt**: Date range for last update
- **StartDate**: Date range for start date
- **CompletedAt**: Date range for completion date

## Example Usage

### REST API Example

```bash
# Create a todo
curl -X POST https://localhost:5001/api/todo \
  -H "Content-Type: application/json" \
  -d '{"title": "Write documentation", "description": "Complete README"}'

# List todos in progress
curl https://localhost:5001/api/todo?status=InProgress

# Update todo status
curl -X PUT https://localhost:5001/api/todo/{id} \
  -H "Content-Type: application/json" \
  -d '{"status": "Completed"}'
```

### MCP Example (with Claude)

```
User: "Create a todo to review the MCP implementation"
Claude: [Uses create_todo tool]
        ✅ Created todo "Review MCP implementation" with ID abc123...

User: "Show me all my in-progress todos"
Claude: [Uses list_todos with status filter]
        Here are your in-progress todos:
        1. Review MCP implementation
        2. Write unit tests
        ...

User: "Start working on the first one"
Claude: [Uses start_todo]
        ✅ Started "Review MCP implementation" - Status: InProgress
```

## Development

### Adding New Features

1. **Domain Layer**: Add new entities or modify business rules
2. **Application Layer**: Update services and DTOs
3. **Infrastructure Layer**: Implement data access
4. **API/MCP Layer**: Expose through REST or MCP

### Running Tests

```bash
dotnet test
```

### Code Structure

The project follows these principles:
- **Single Responsibility**: Each class has one reason to change
- **Dependency Inversion**: Depend on abstractions, not concretions
- **Clean Architecture**: Dependencies point inward
- **SOLID Principles**: Throughout the codebase

## Future Enhancements

- [ ] PostgreSQL database implementation
- [ ] User authentication and authorization
- [ ] Todo categories and tags
- [ ] Recurring todos
- [ ] Due dates and reminders
- [ ] Attachments and notes
- [ ] Real-time updates via SignalR
- [ ] Mobile applications
- [ ] Web frontend

## Technology Stack

- **.NET 10.0**: Application framework
- **C# 13**: Programming language
- **ASP.NET Core**: REST API framework
- **MCP (Model Context Protocol)**: AI integration
- **JSON-RPC 2.0**: MCP protocol transport
- **Swagger/OpenAPI**: API documentation

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For issues, questions, or contributions, please create an issue in the GitHub repository.

## Acknowledgments

- Built with clean architecture principles
- MCP protocol by Anthropic
- Inspired by modern todo applications

---

**Note**: This project uses in-memory storage by default. For production use, implement a PostgreSQL repository by creating a new class that implements `ITodoRepository`.
