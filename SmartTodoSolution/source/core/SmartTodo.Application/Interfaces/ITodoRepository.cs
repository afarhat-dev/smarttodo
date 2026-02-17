using SmartTodo.Application.DTOs;
using SmartTodo.Domain.Entities;

namespace SmartTodo.Application.Interfaces;

public interface ITodoRepository
{
    Task<TodoItem?> GetByIdAsync(Guid id);
    Task<IEnumerable<TodoItem>> GetAllAsync();
    Task<IEnumerable<TodoItem>> GetFilteredAsync(TodoFilter filter);
    Task<TodoItem> AddAsync(TodoItem todoItem);
    Task<TodoItem?> UpdateAsync(TodoItem todoItem);
    Task<bool> DeleteAsync(Guid id);

    // Tag operations
    Task<Tag?> GetTagByNameAsync(string name);
    Task<Tag> GetOrCreateTagAsync(string name);
    Task<IEnumerable<Tag>> GetAllTagsAsync();

    // Dependency operations
    Task AddDependencyAsync(Guid todoId, Guid dependencyId);
    Task RemoveDependencyAsync(Guid todoId, Guid dependencyId);
}
