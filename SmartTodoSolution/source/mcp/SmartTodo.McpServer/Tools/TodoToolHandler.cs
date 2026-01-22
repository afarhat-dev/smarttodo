using System.Text.Json;
using SmartTodo.Application.DTOs;
using SmartTodo.Application.Services;
using SmartTodo.Domain.Enums;

namespace SmartTodo.McpServer.Tools;

public class TodoToolHandler
{
    private readonly ITodoService _todoService;

    public TodoToolHandler(ITodoService todoService)
    {
        _todoService = todoService;
    }

    public async Task<object> HandleToolCallAsync(string toolName, JsonElement? parameters)
    {
        return toolName switch
        {
            "create_todo" => await CreateTodoAsync(parameters),
            "get_todo" => await GetTodoAsync(parameters),
            "list_todos" => await ListTodosAsync(parameters),
            "update_todo" => await UpdateTodoAsync(parameters),
            "delete_todo" => await DeleteTodoAsync(parameters),
            "start_todo" => await StartTodoAsync(parameters),
            "complete_todo" => await CompleteTodoAsync(parameters),
            "pause_todo" => await PauseTodoAsync(parameters),
            "resume_todo" => await ResumeTodoAsync(parameters),
            "cancel_todo" => await CancelTodoAsync(parameters),
            "set_todo_priority" => await SetTodoPriorityAsync(parameters),
            _ => throw new InvalidOperationException($"Unknown tool: {toolName}")
        };
    }

    private async Task<object> CreateTodoAsync(JsonElement? parameters)
    {
        if (!parameters.HasValue)
            throw new ArgumentException("Parameters are required for create_todo");

        var title = parameters.Value.GetProperty("title").GetString()
            ?? throw new ArgumentException("Title is required");

        var description = parameters.Value.TryGetProperty("description", out var descProp)
            ? descProp.GetString()
            : null;

        TodoPriority? priority = null;
        if (parameters.Value.TryGetProperty("priority", out var priorityProp) && priorityProp.ValueKind == JsonValueKind.String)
        {
            var priorityString = priorityProp.GetString();
            if (Enum.TryParse<TodoPriority>(priorityString, out var parsedPriority))
            {
                priority = parsedPriority;
            }
        }

        var createDto = new CreateTodoItemDto(title, description, priority);
        var result = await _todoService.CreateAsync(createDto);

        return new
        {
            success = true,
            todo = result,
            message = $"Todo '{result.Title}' created successfully with ID {result.Id}"
        };
    }

    private async Task<object> GetTodoAsync(JsonElement? parameters)
    {
        if (!parameters.HasValue)
            throw new ArgumentException("Parameters are required for get_todo");

        var idString = parameters.Value.GetProperty("id").GetString()
            ?? throw new ArgumentException("ID is required");

        if (!Guid.TryParse(idString, out var id))
            throw new ArgumentException("Invalid ID format");

        var todo = await _todoService.GetByIdAsync(id);

        if (todo == null)
        {
            return new
            {
                success = false,
                message = $"Todo with ID {id} not found"
            };
        }

        return new
        {
            success = true,
            todo
        };
    }

    private async Task<object> ListTodosAsync(JsonElement? parameters)
    {
        if (!parameters.HasValue || parameters.Value.ValueKind == JsonValueKind.Null)
        {
            var allTodos = await _todoService.GetAllAsync();
            return new
            {
                success = true,
                todos = allTodos,
                count = allTodos.Count()
            };
        }

        var filter = BuildFilter(parameters.Value);
        var filteredTodos = await _todoService.GetFilteredAsync(filter);

        return new
        {
            success = true,
            todos = filteredTodos,
            count = filteredTodos.Count(),
            appliedFilters = filter
        };
    }

    private async Task<object> UpdateTodoAsync(JsonElement? parameters)
    {
        if (!parameters.HasValue)
            throw new ArgumentException("Parameters are required for update_todo");

        var idString = parameters.Value.GetProperty("id").GetString()
            ?? throw new ArgumentException("ID is required");

        if (!Guid.TryParse(idString, out var id))
            throw new ArgumentException("Invalid ID format");

        var title = parameters.Value.TryGetProperty("title", out var titleProp)
            ? titleProp.GetString()
            : null;

        var description = parameters.Value.TryGetProperty("description", out var descProp)
            ? descProp.GetString()
            : null;

        var isCompleted = parameters.Value.TryGetProperty("isCompleted", out var completedProp)
            ? completedProp.GetBoolean()
            : (bool?)null;

        TodoStatus? status = null;
        if (parameters.Value.TryGetProperty("status", out var statusProp) && statusProp.ValueKind == JsonValueKind.String)
        {
            var statusString = statusProp.GetString();
            if (Enum.TryParse<TodoStatus>(statusString, out var parsedStatus))
            {
                status = parsedStatus;
            }
        }

        TodoPriority? priority = null;
        if (parameters.Value.TryGetProperty("priority", out var priorityProp) && priorityProp.ValueKind == JsonValueKind.String)
        {
            var priorityString = priorityProp.GetString();
            if (Enum.TryParse<TodoPriority>(priorityString, out var parsedPriority))
            {
                priority = parsedPriority;
            }
        }

        var updateDto = new UpdateTodoItemDto(title, description, isCompleted, status, priority);
        var result = await _todoService.UpdateAsync(id, updateDto);

        if (result == null)
        {
            return new
            {
                success = false,
                message = $"Todo with ID {id} not found"
            };
        }

        return new
        {
            success = true,
            todo = result,
            message = $"Todo '{result.Title}' updated successfully"
        };
    }

    private async Task<object> DeleteTodoAsync(JsonElement? parameters)
    {
        if (!parameters.HasValue)
            throw new ArgumentException("Parameters are required for delete_todo");

        var idString = parameters.Value.GetProperty("id").GetString()
            ?? throw new ArgumentException("ID is required");

        if (!Guid.TryParse(idString, out var id))
            throw new ArgumentException("Invalid ID format");

        var result = await _todoService.DeleteAsync(id);

        if (!result)
        {
            return new
            {
                success = false,
                message = $"Todo with ID {id} not found"
            };
        }

        return new
        {
            success = true,
            message = $"Todo with ID {id} deleted successfully"
        };
    }

    private async Task<object> StartTodoAsync(JsonElement? parameters)
    {
        return await UpdateTodoStatusAsync(parameters, TodoStatus.InProgress, "started");
    }

    private async Task<object> CompleteTodoAsync(JsonElement? parameters)
    {
        return await UpdateTodoStatusAsync(parameters, TodoStatus.Completed, "completed");
    }

    private async Task<object> PauseTodoAsync(JsonElement? parameters)
    {
        return await UpdateTodoStatusAsync(parameters, TodoStatus.OnHold, "paused");
    }

    private async Task<object> ResumeTodoAsync(JsonElement? parameters)
    {
        return await UpdateTodoStatusAsync(parameters, TodoStatus.InProgress, "resumed");
    }

    private async Task<object> CancelTodoAsync(JsonElement? parameters)
    {
        return await UpdateTodoStatusAsync(parameters, TodoStatus.Cancelled, "cancelled");
    }

    private async Task<object> SetTodoPriorityAsync(JsonElement? parameters)
    {
        if (!parameters.HasValue)
            throw new ArgumentException("Parameters are required for set_todo_priority");

        var idString = parameters.Value.GetProperty("id").GetString()
            ?? throw new ArgumentException("ID is required");

        if (!Guid.TryParse(idString, out var id))
            throw new ArgumentException("Invalid ID format");

        var priorityString = parameters.Value.GetProperty("priority").GetString()
            ?? throw new ArgumentException("Priority is required");

        if (!Enum.TryParse<TodoPriority>(priorityString, true, out var priority))
            throw new ArgumentException($"Invalid priority value. Valid values are: Low, Medium, High, Critical");

        var updateDto = new UpdateTodoItemDto(null, null, null, null, priority);
        var result = await _todoService.UpdateAsync(id, updateDto);

        if (result == null)
        {
            return new
            {
                success = false,
                message = $"Todo with ID {id} not found"
            };
        }

        return new
        {
            success = true,
            todo = result,
            message = $"Todo '{result.Title}' priority set to {priority} successfully"
        };
    }

    private async Task<object> UpdateTodoStatusAsync(JsonElement? parameters, TodoStatus status, string action)
    {
        if (!parameters.HasValue)
            throw new ArgumentException($"Parameters are required for {action} action");

        var idString = parameters.Value.GetProperty("id").GetString()
            ?? throw new ArgumentException("ID is required");

        if (!Guid.TryParse(idString, out var id))
            throw new ArgumentException("Invalid ID format");

        var updateDto = new UpdateTodoItemDto(null, null, null, status, null);
        var result = await _todoService.UpdateAsync(id, updateDto);

        if (result == null)
        {
            return new
            {
                success = false,
                message = $"Todo with ID {id} not found"
            };
        }

        return new
        {
            success = true,
            todo = result,
            message = $"Todo '{result.Title}' {action} successfully"
        };
    }

    private static TodoFilter BuildFilter(JsonElement parameters)
    {
        TodoStatus? status = null;
        if (parameters.TryGetProperty("status", out var statusProp) && statusProp.ValueKind == JsonValueKind.String)
        {
            var statusString = statusProp.GetString();
            if (Enum.TryParse<TodoStatus>(statusString, out var parsedStatus))
            {
                status = parsedStatus;
            }
        }

        TodoPriority? priority = null;
        if (parameters.TryGetProperty("priority", out var priorityProp) && priorityProp.ValueKind == JsonValueKind.String)
        {
            var priorityString = priorityProp.GetString();
            if (Enum.TryParse<TodoPriority>(priorityString, out var parsedPriority))
            {
                priority = parsedPriority;
            }
        }

        var isCompleted = parameters.TryGetProperty("isCompleted", out var completedProp)
            ? completedProp.GetBoolean()
            : (bool?)null;

        var createdFrom = TryGetDateTime(parameters, "createdFrom");
        var createdTo = TryGetDateTime(parameters, "createdTo");
        var updatedFrom = TryGetDateTime(parameters, "updatedFrom");
        var updatedTo = TryGetDateTime(parameters, "updatedTo");
        var startDateFrom = TryGetDateTime(parameters, "startDateFrom");
        var startDateTo = TryGetDateTime(parameters, "startDateTo");
        var completedFrom = TryGetDateTime(parameters, "completedFrom");
        var completedTo = TryGetDateTime(parameters, "completedTo");

        return new TodoFilter(
            status,
            priority,
            createdFrom,
            createdTo,
            updatedFrom,
            updatedTo,
            startDateFrom,
            startDateTo,
            completedFrom,
            completedTo,
            isCompleted
        );
    }

    private static DateTime? TryGetDateTime(JsonElement parameters, string propertyName)
    {
        if (parameters.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String)
        {
            var dateString = prop.GetString();
            if (DateTime.TryParse(dateString, out var date))
            {
                return date;
            }
        }
        return null;
    }
}
