using TodoWebApp.Models;

namespace TodoWebApp.Services;

public interface ITodoApiService
{
    Task<List<TodoItemDto>> GetAllTodosAsync();
    Task<TodoItemDto?> GetTodoByIdAsync(Guid id);
    Task<TodoItemDto?> CreateTodoAsync(CreateTodoRequest request);
    Task<bool> UpdateTodoAsync(Guid id, TodoItemDto todo);
    Task<bool> DeleteTodoAsync(Guid id);
}
