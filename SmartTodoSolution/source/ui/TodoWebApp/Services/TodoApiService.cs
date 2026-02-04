using System.Net.Http.Json;
using System.Text.Json;
using TodoWebApp.Models;

namespace TodoWebApp.Services;

public class TodoApiService : ITodoApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TodoApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public TodoApiService(HttpClient httpClient, ILogger<TodoApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<List<TodoItemDto>> GetAllTodosAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<TodoItemDto>>(
                "api/todo", _jsonOptions);
            return response ?? new List<TodoItemDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching todos from API");
            return new List<TodoItemDto>();
        }
    }

    public async Task<TodoItemDto?> GetTodoByIdAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<TodoItemDto>(
                $"api/todo/{id}", _jsonOptions);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching todo {TodoId} from API", id);
            return null;
        }
    }

    public async Task<TodoItemDto?> CreateTodoAsync(CreateTodoRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/todo", request);
            response.EnsureSuccessStatusCode();

            var createdTodo = await response.Content.ReadFromJsonAsync<TodoItemDto>(_jsonOptions);
            return createdTodo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating todo");
            return null;
        }
    }

    public async Task<bool> UpdateTodoAsync(Guid id, TodoItemDto todo)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/todo/{id}", todo);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating todo {TodoId}", id);
            return false;
        }
    }

    public async Task<bool> DeleteTodoAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/todo/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting todo {TodoId}", id);
            return false;
        }
    }

    public async Task<List<TagDto>> GetAllTagsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<TagDto>>(
                "api/todo/tags", _jsonOptions);
            return response ?? new List<TagDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching tags from API");
            return new List<TagDto>();
        }
    }

    public async Task<TodoItemDto?> AddDependencyAsync(Guid todoId, Guid dependencyId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/todo/{todoId}/dependencies/{dependencyId}", null);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TodoItemDto>(_jsonOptions);
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding dependency {DependencyId} to todo {TodoId}", dependencyId, todoId);
            return null;
        }
    }

    public async Task<TodoItemDto?> RemoveDependencyAsync(Guid todoId, Guid dependencyId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/todo/{todoId}/dependencies/{dependencyId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TodoItemDto>(_jsonOptions);
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing dependency {DependencyId} from todo {TodoId}", dependencyId, todoId);
            return null;
        }
    }
}
