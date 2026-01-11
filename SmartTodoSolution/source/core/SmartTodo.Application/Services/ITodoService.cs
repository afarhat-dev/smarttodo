using SmartTodo.Application.DTOs;

namespace SmartTodo.Application.Services;

public interface ITodoService
{
    Task<TodoItemDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<TodoItemDto>> GetAllAsync();
    Task<IEnumerable<TodoItemDto>> GetFilteredAsync(TodoFilter filter);
    Task<TodoItemDto> CreateAsync(CreateTodoItemDto createDto);
    Task<TodoItemDto?> UpdateAsync(Guid id, UpdateTodoItemDto updateDto);
    Task<bool> DeleteAsync(Guid id);
}
