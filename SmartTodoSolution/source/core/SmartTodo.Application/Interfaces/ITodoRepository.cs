using SmartTodo.Domain.Entities;

namespace SmartTodo.Application.Interfaces;

public interface ITodoRepository
{
    Task<TodoItem?> GetByIdAsync(Guid id);
    Task<IEnumerable<TodoItem>> GetAllAsync();
    Task<TodoItem> AddAsync(TodoItem todoItem);
    Task<TodoItem?> UpdateAsync(TodoItem todoItem);
    Task<bool> DeleteAsync(Guid id);
}
