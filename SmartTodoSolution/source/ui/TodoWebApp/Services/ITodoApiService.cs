using TodoWebApp.Models;

namespace TodoWebApp.Services;

public interface ITodoApiService
{
    Task<List<TodoItemDto>> GetAllTodosAsync();
    Task<TodoItemDto?> GetTodoByIdAsync(Guid id);
    Task<TodoItemDto?> CreateTodoAsync(CreateTodoRequest request);
    Task<bool> UpdateTodoAsync(Guid id, TodoItemDto todo);
    Task<bool> DeleteTodoAsync(Guid id);
    Task<List<TagDto>> GetAllTagsAsync();
    Task<TodoItemDto?> AddDependencyAsync(Guid todoId, Guid dependencyId);
    Task<TodoItemDto?> RemoveDependencyAsync(Guid todoId, Guid dependencyId);
}
