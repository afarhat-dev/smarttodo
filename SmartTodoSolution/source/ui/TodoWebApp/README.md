# TodoWebApp

A Blazor Server web application for displaying and managing tasks from the SmartTodo system.

## Features

- View all todos in a responsive table layout
- Create new todos with title and description
- Mark todos as complete/incomplete
- Delete todos
- Filter and sort todos by status
- Real-time status badges (Not Started, In Progress, Completed, etc.)
- Statistics dashboard showing task counts by status

## Technology Stack

- **Blazor Server** - Interactive web UI framework
- **ASP.NET Core 10.0** - Web hosting
- **Bootstrap 5.3** - UI styling
- **HttpClient** - API communication with StdApi

## Configuration

The application connects to the StdApi REST API. Configure the API base URL in `appsettings.json`:

```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:5208"
  }
}
```

### Environment-Specific Configuration

- **Development**: `appsettings.Development.json` - API at `http://localhost:5208`
- **Docker**: `appsettings.Docker.json` - API at `http://api:8080`

## Running Locally

### Prerequisites

- .NET 10.0 SDK
- StdApi running (default: http://localhost:5208)

### Steps

1. Navigate to the project directory:
   ```bash
   cd SmartTodoSolution/source/ui/TodoWebApp
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. Open your browser to `https://localhost:5001` or `http://localhost:5000`

## Running with Docker

The application is configured to run in Docker Compose alongside the API and PostgreSQL:

```bash
cd SmartTodoSolution/deployment
docker-compose up -d
```

Access points:
- Web App: http://localhost:5209
- API: http://localhost:5208
- PostgreSQL: localhost:5432

## Project Structure

```
TodoWebApp/
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor       # Main layout component
│   │   ├── NavMenu.razor          # Navigation menu
│   │   └── NavMenu.razor.css      # Navigation styles
│   ├── Pages/
│   │   ├── Home.razor             # Home page
│   │   └── Todos.razor            # Todo list page
│   ├── App.razor                  # Root component
│   ├── Routes.razor               # Routing configuration
│   └── _Imports.razor             # Global imports
├── Models/
│   ├── TodoItemDto.cs             # Todo item data transfer object
│   └── CreateTodoRequest.cs       # Create todo request model
├── Services/
│   ├── ITodoApiService.cs         # API service interface
│   └── TodoApiService.cs          # API service implementation
├── wwwroot/
│   └── app.css                    # Global styles
├── Program.cs                     # Application entry point
├── TodoWebApp.csproj              # Project file
└── appsettings.json               # Configuration
```

## API Integration

The application communicates with the StdApi through the `TodoApiService`:

- **GET /api/todo** - List all todos
- **GET /api/todo/{id}** - Get specific todo
- **POST /api/todo** - Create new todo
- **PUT /api/todo/{id}** - Update todo
- **DELETE /api/todo/{id}** - Delete todo

## UI Components

### Home Page
- Welcome message
- Quick links to todo management

### Todos Page
- **Create Dialog**: Form to create new todos
- **Statistics**: Badge counts for task status
- **Todo Table**: Responsive table with:
  - Status badges
  - Title and description
  - Created/Updated timestamps
  - Complete/Reopen actions
  - Delete actions

## Status Types

- **NotStarted** (Secondary badge) - Task not yet started
- **InProgress** (Warning badge) - Task currently being worked on
- **Completed** (Success badge) - Task finished
- **OnHold** (Info badge) - Task paused
- **Cancelled** (Dark badge) - Task cancelled

## Development Notes

- Uses Interactive Server render mode for real-time updates
- Bootstrap CDN for styling (no local dependencies)
- Minimal, focused UI - displays tasks cleanly
- Error handling with user-friendly messages
- Auto-refresh after CRUD operations
