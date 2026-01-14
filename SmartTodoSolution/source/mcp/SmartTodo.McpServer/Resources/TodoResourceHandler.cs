using System.Text.Json;
using SmartTodo.Application.DTOs;
using SmartTodo.Application.Services;
using SmartTodo.Domain.Enums;
using SmartTodo.McpServer.Protocol;

namespace SmartTodo.McpServer.Resources;

public class TodoResourceHandler
{
    private readonly ITodoService _todoService;

    public TodoResourceHandler(ITodoService todoService)
    {
        _todoService = todoService;
    }

    public static List<McpResource> GetAllResources()
    {
        return new List<McpResource>
        {
            new McpResource
            {
                Uri = "todo://items",
                Name = "All Todo Items",
                Description = "List of all todo items",
                MimeType = "application/json"
            },
            new McpResource
            {
                Uri = "todo://items/{id}",
                Name = "Todo Item by ID",
                Description = "Specific todo item details",
                MimeType = "application/json"
            },
            new McpResource
            {
                Uri = "todo://stats",
                Name = "Todo Statistics",
                Description = "Statistics about todo items (counts by status, completion rate, etc.)",
                MimeType = "application/json"
            },
            new McpResource
            {
                Uri = "todo://filters/templates",
                Name = "Filter Templates",
                Description = "Pre-defined filter templates for common queries",
                MimeType = "application/json"
            }
        };
    }

    public async Task<object> GetResourceAsync(string uri)
    {
        if (uri == "todo://items")
        {
            return await GetAllItemsAsync();
        }
        else if (uri.StartsWith("todo://items/"))
        {
            var id = uri.Substring("todo://items/".Length);
            return await GetItemByIdAsync(id);
        }
        else if (uri == "todo://stats")
        {
            return await GetStatsAsync();
        }
        else if (uri == "todo://filters/templates")
        {
            return GetFilterTemplates();
        }

        throw new InvalidOperationException($"Unknown resource URI: {uri}");
    }

    private async Task<object> GetAllItemsAsync()
    {
        var todos = await _todoService.GetAllAsync();
        return new
        {
            uri = "todo://items",
            mimeType = "application/json",
            content = todos
        };
    }

    private async Task<object> GetItemByIdAsync(string idString)
    {
        if (!Guid.TryParse(idString, out var id))
            throw new ArgumentException("Invalid ID format");

        var todo = await _todoService.GetByIdAsync(id);

        if (todo == null)
            throw new InvalidOperationException($"Todo with ID {id} not found");

        return new
        {
            uri = $"todo://items/{id}",
            mimeType = "application/json",
            content = todo
        };
    }

    private async Task<object> GetStatsAsync()
    {
        var allTodos = await _todoService.GetAllAsync();
        var todoList = allTodos.ToList();

        var stats = new
        {
            total = todoList.Count,
            byStatus = new
            {
                notStarted = todoList.Count(t => t.Status == TodoStatus.NotStarted),
                inProgress = todoList.Count(t => t.Status == TodoStatus.InProgress),
                onHold = todoList.Count(t => t.Status == TodoStatus.OnHold),
                completed = todoList.Count(t => t.Status == TodoStatus.Completed),
                cancelled = todoList.Count(t => t.Status == TodoStatus.Cancelled)
            },
            completionRate = todoList.Count > 0
                ? (double)todoList.Count(t => t.IsCompleted) / todoList.Count * 100
                : 0,
            todayCreated = todoList.Count(t => t.CreatedAt.Date == DateTime.UtcNow.Date),
            todayCompleted = todoList.Count(t => t.CompletedAt.HasValue && t.CompletedAt.Value.Date == DateTime.UtcNow.Date),
            overdue = todoList.Count(t => !t.IsCompleted && t.Status != TodoStatus.Cancelled)
        };

        return new
        {
            uri = "todo://stats",
            mimeType = "application/json",
            content = stats
        };
    }

    private static object GetFilterTemplates()
    {
        var templates = new
        {
            today = new
            {
                name = "Today's Todos",
                description = "Todos created or due today",
                filter = new TodoFilter(
                    Status: null,
                    CreatedFrom: DateTime.UtcNow.Date,
                    CreatedTo: DateTime.UtcNow.Date.AddDays(1),
                    UpdatedFrom: null,
                    UpdatedTo: null,
                    StartDateFrom: null,
                    StartDateTo: null,
                    CompletedFrom: null,
                    CompletedTo: null,
                    IsCompleted: null
                )
            },
            thisWeek = new
            {
                name = "This Week's Todos",
                description = "Todos created this week",
                filter = new TodoFilter(
                    Status: null,
                    CreatedFrom: DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek),
                    CreatedTo: null,
                    UpdatedFrom: null,
                    UpdatedTo: null,
                    StartDateFrom: null,
                    StartDateTo: null,
                    CompletedFrom: null,
                    CompletedTo: null,
                    IsCompleted: null
                )
            },
            inProgress = new
            {
                name = "In Progress",
                description = "Todos currently in progress",
                filter = new TodoFilter(
                    Status: TodoStatus.InProgress,
                    CreatedFrom: null,
                    CreatedTo: null,
                    UpdatedFrom: null,
                    UpdatedTo: null,
                    StartDateFrom: null,
                    StartDateTo: null,
                    CompletedFrom: null,
                    CompletedTo: null,
                    IsCompleted: null
                )
            },
            notStarted = new
            {
                name = "Not Started",
                description = "Todos that haven't been started yet",
                filter = new TodoFilter(
                    Status: TodoStatus.NotStarted,
                    CreatedFrom: null,
                    CreatedTo: null,
                    UpdatedFrom: null,
                    UpdatedTo: null,
                    StartDateFrom: null,
                    StartDateTo: null,
                    CompletedFrom: null,
                    CompletedTo: null,
                    IsCompleted: null
                )
            },
            completed = new
            {
                name = "Completed",
                description = "Completed todos",
                filter = new TodoFilter(
                    Status: TodoStatus.Completed,
                    CreatedFrom: null,
                    CreatedTo: null,
                    UpdatedFrom: null,
                    UpdatedTo: null,
                    StartDateFrom: null,
                    StartDateTo: null,
                    CompletedFrom: null,
                    CompletedTo: null,
                    IsCompleted: true
                )
            }
        };

        return new
        {
            uri = "todo://filters/templates",
            mimeType = "application/json",
            content = templates
        };
    }
}
