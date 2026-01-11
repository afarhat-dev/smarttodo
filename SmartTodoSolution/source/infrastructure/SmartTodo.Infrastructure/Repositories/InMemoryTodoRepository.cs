using System.Collections.Concurrent;
using SmartTodo.Application.Interfaces;
using SmartTodo.Domain.Entities;

namespace SmartTodo.Infrastructure.Repositories;

public class InMemoryTodoRepository : ITodoRepository
{
    private readonly ConcurrentDictionary<Guid, TodoItem> _todos = new();

    public Task<TodoItem?> GetByIdAsync(Guid id)
    {
        _todos.TryGetValue(id, out var todoItem);
        return Task.FromResult(todoItem);
    }

    public Task<IEnumerable<TodoItem>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<TodoItem>>(_todos.Values.OrderBy(t => t.CreatedAt).ToList());
    }

    public Task<TodoItem> AddAsync(TodoItem todoItem)
    {
        if (!_todos.TryAdd(todoItem.Id, todoItem))
            throw new InvalidOperationException($"Todo item with ID {todoItem.Id} already exists");

        return Task.FromResult(todoItem);
    }

    public Task<TodoItem?> UpdateAsync(TodoItem todoItem)
    {
        if (_todos.ContainsKey(todoItem.Id))
        {
            _todos[todoItem.Id] = todoItem;
            return Task.FromResult<TodoItem?>(todoItem);
        }

        return Task.FromResult<TodoItem?>(null);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        return Task.FromResult(_todos.TryRemove(id, out _));
    }
}
