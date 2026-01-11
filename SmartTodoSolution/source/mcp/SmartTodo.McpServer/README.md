# SmartTodo MCP Server

A Model Context Protocol (MCP) server that enables AI assistants like Claude to manage todo items through standardized tools, resources, and prompts.

## Overview

The SmartTodo MCP Server exposes todo management functionality through the MCP protocol, allowing AI assistants to:
- Create, read, update, and delete todo items
- Filter todos by status, dates, and completion state
- Manage todo lifecycle (start, pause, resume, complete, cancel)
- Access todo statistics and pre-defined filter templates
- Use guided prompts for common todo management tasks

## Architecture

The MCP server follows clean architecture principles:

```
┌─────────────────────────────────────────┐
│         MCP Server Layer                │
│  (MCP Protocol Implementation)          │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│      Application Layer                  │
│  (Shared Business Logic & Services)     │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│      Infrastructure Layer               │
│  (In-Memory Repository)                 │
└─────────────────────────────────────────┘
```

## Installation

### Prerequisites
- .NET 10.0 SDK or later
- Claude Desktop (for testing and usage)

### Building the Server

```bash
cd SmartTodoSolution/source/mcp/SmartTodo.McpServer
dotnet build
```

### Running the Server

```bash
dotnet run
```

## Claude Desktop Integration

To use this MCP server with Claude Desktop, add the following configuration to your Claude Desktop settings:

**Location:** `~/Library/Application Support/Claude/claude_desktop_config.json` (macOS)
or `%APPDATA%\Claude\claude_desktop_config.json` (Windows)

```json
{
  "mcpServers": {
    "smarttodo": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/path/to/SmartTodoSolution/source/mcp/SmartTodo.McpServer"
      ]
    }
  }
}
```

Replace `/path/to/` with the actual path to your project.

## Available Tools

### 1. create_todo
Create a new todo item.

**Parameters:**
- `title` (required, string): The title of the todo item
- `description` (optional, string): Description of the todo item

**Example:**
```json
{
  "name": "create_todo",
  "arguments": {
    "title": "Review pull request",
    "description": "Review the new authentication feature PR"
  }
}
```

### 2. get_todo
Retrieve a specific todo item by ID.

**Parameters:**
- `id` (required, string): The GUID of the todo item

**Example:**
```json
{
  "name": "get_todo",
  "arguments": {
    "id": "123e4567-e89b-12d3-a456-426614174000"
  }
}
```

### 3. list_todos
Get all todos with optional filtering.

**Parameters (all optional):**
- `status` (string): Filter by status (NotStarted, InProgress, OnHold, Completed, Cancelled)
- `isCompleted` (boolean): Filter by completion state
- `createdFrom` (string): Filter by created date from (ISO 8601)
- `createdTo` (string): Filter by created date to (ISO 8601)
- `updatedFrom` (string): Filter by updated date from (ISO 8601)
- `updatedTo` (string): Filter by updated date to (ISO 8601)

**Example:**
```json
{
  "name": "list_todos",
  "arguments": {
    "status": "InProgress",
    "createdFrom": "2024-01-01T00:00:00Z"
  }
}
```

### 4. update_todo
Update an existing todo item.

**Parameters:**
- `id` (required, string): The GUID of the todo item
- `title` (optional, string): New title
- `description` (optional, string): New description
- `status` (optional, string): New status
- `isCompleted` (optional, boolean): Completion state

**Example:**
```json
{
  "name": "update_todo",
  "arguments": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "title": "Updated title",
    "status": "InProgress"
  }
}
```

### 5. delete_todo
Delete a todo item.

**Parameters:**
- `id` (required, string): The GUID of the todo item to delete

### 6. start_todo
Mark a todo as started (sets StartDate, changes status to InProgress).

**Parameters:**
- `id` (required, string): The GUID of the todo item

### 7. complete_todo
Mark a todo as completed.

**Parameters:**
- `id` (required, string): The GUID of the todo item

### 8. pause_todo
Put a todo on hold.

**Parameters:**
- `id` (required, string): The GUID of the todo item

### 9. resume_todo
Resume a paused todo.

**Parameters:**
- `id` (required, string): The GUID of the todo item

### 10. cancel_todo
Cancel a todo item.

**Parameters:**
- `id` (required, string): The GUID of the todo item

## Available Resources

### 1. todo://items
Lists all todo items.

**Format:** JSON
**Dynamic:** Yes

### 2. todo://items/{id}
Retrieves a specific todo item by ID.

**Format:** JSON
**Dynamic:** Yes

### 3. todo://stats
Provides statistics about todo items.

**Format:** JSON
**Dynamic:** Yes

**Statistics Include:**
- Total count
- Count by status
- Completion rate
- Today's created/completed counts

### 4. todo://filters/templates
Pre-defined filter templates for common queries.

**Format:** JSON
**Dynamic:** No

**Available Templates:**
- `today`: Todos created or due today
- `thisWeek`: Todos created this week
- `inProgress`: Todos currently in progress
- `notStarted`: Todos that haven't been started
- `completed`: Completed todos

## Available Prompts

### 1. manage-todos
Provides guidance on managing your todo list efficiently.

**Arguments:** None

**Usage:**
```
Use the manage-todos prompt to get started with todo management
```

### 2. plan-day
Helps plan your day based on your todos.

**Arguments:**
- `date` (optional): The date to plan for (defaults to today)

**Usage:**
```
Use the plan-day prompt to organize my tasks for tomorrow
```

### 3. review-progress
Reviews todo completion progress.

**Arguments:**
- `timeframe` (optional): The timeframe to review (day, week, month)

**Usage:**
```
Use the review-progress prompt with timeframe "week"
```

## Example Conversations with Claude

### Creating and Managing Todos

**User:** "Create a todo to review the MCP server implementation"

**Claude:** *Uses create_todo tool*
> Created todo "Review MCP server implementation" with ID abc123...

**User:** "Start working on it"

**Claude:** *Uses start_todo tool with the ID*
> Todo started! Status changed to InProgress with start date set.

### Filtering and Viewing

**User:** "Show me all todos that are in progress"

**Claude:** *Uses list_todos tool with status filter*
> Here are your in-progress todos:
> 1. Review MCP server implementation
> 2. Write documentation
> ...

### Using Resources

**User:** "What are my todo statistics?"

**Claude:** *Reads todo://stats resource*
> Here's your todo overview:
> - Total: 15 todos
> - Completed: 8 (53.3% completion rate)
> - In Progress: 4
> - Not Started: 2
> - On Hold: 1

## Configuration

Edit `appsettings.json` to customize server behavior:

```json
{
  "McpServer": {
    "Name": "SmartTodo MCP Server",
    "Version": "1.0.0",
    "Capabilities": {
      "Tools": true,
      "Resources": true,
      "Prompts": true
    },
    "Logging": {
      "Level": "Information",
      "OutputPath": "./logs/mcp-server.log"
    }
  }
}
```

## Troubleshooting

### Server not connecting to Claude Desktop

1. Check that the path in `claude_desktop_config.json` is correct
2. Ensure .NET 10.0 SDK is installed: `dotnet --version`
3. Verify the server builds successfully: `dotnet build`
4. Check Claude Desktop logs for connection errors

### Tools not appearing in Claude

1. Restart Claude Desktop after updating configuration
2. Verify MCP server capabilities are enabled in `appsettings.json`
3. Check server logs for initialization errors

### Data not persisting

The MCP server currently uses in-memory storage. Data will be lost when the server restarts. For persistent storage, the infrastructure layer can be updated to use PostgreSQL.

## Development

### Project Structure

```
SmartTodo.McpServer/
├── Configuration/       # Server configuration classes
├── Protocol/           # MCP protocol message definitions
├── Server/             # MCP server host implementation
├── Tools/              # Tool definitions and handlers
├── Resources/          # Resource definitions and handlers
├── Prompts/            # Prompt definitions and handlers
├── Program.cs          # Entry point and DI setup
└── appsettings.json    # Configuration file
```

### Adding New Tools

1. Add tool definition in `Tools/ToolDefinitions.cs`
2. Implement tool handler in `Tools/TodoToolHandler.cs`
3. Update documentation

### Adding New Resources

1. Add resource definition in `Resources/TodoResourceHandler.cs`
2. Implement resource handler method
3. Update documentation

## License

MIT License - See LICENSE file for details

## Support

For issues and questions, please create an issue in the GitHub repository.
