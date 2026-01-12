using System.Collections.Concurrent;
using SmartTodo.Application.DTOs;
using SmartTodo.Application.Interfaces;
using SmartTodo.Domain.Entities;

namespace SmartTodo.Infrastructure.Repositories;

public class InMemoryTodoRepository : ITodoRepository
{
    private static readonly ConcurrentDictionary<Guid, TodoItem> _todos = new();

    public Task<TodoItem?> GetByIdAsync(Guid id)
    {
        _todos.TryGetValue(id, out var todoItem);
        return Task.FromResult(todoItem);
    }

    public Task<IEnumerable<TodoItem>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<TodoItem>>(_todos.Values.OrderBy(t => t.CreatedAt).ToList());
    }

    public Task<IEnumerable<TodoItem>> GetFilteredAsync(TodoFilter filter)
    {
        var query = _todos.Values.AsEnumerable();

        if (filter.Status.HasValue)
        {
            query = query.Where(t => t.Status == filter.Status.Value);
        }

        if (filter.IsCompleted.HasValue)
        {
            query = query.Where(t => t.IsCompleted == filter.IsCompleted.Value);
        }

        if (filter.CreatedFrom.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= filter.CreatedFrom.Value);
        }

        if (filter.CreatedTo.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= filter.CreatedTo.Value);
        }

        if (filter.UpdatedFrom.HasValue)
        {
            query = query.Where(t => t.UpdatedAt >= filter.UpdatedFrom.Value);
        }

        if (filter.UpdatedTo.HasValue)
        {
            query = query.Where(t => t.UpdatedAt <= filter.UpdatedTo.Value);
        }

        if (filter.StartDateFrom.HasValue)
        {
            query = query.Where(t => t.StartDate.HasValue && t.StartDate.Value >= filter.StartDateFrom.Value);
        }

        if (filter.StartDateTo.HasValue)
        {
            query = query.Where(t => t.StartDate.HasValue && t.StartDate.Value <= filter.StartDateTo.Value);
        }

        if (filter.CompletedFrom.HasValue)
        {
            query = query.Where(t => t.CompletedAt.HasValue && t.CompletedAt.Value >= filter.CompletedFrom.Value);
        }

        if (filter.CompletedTo.HasValue)
        {
            query = query.Where(t => t.CompletedAt.HasValue && t.CompletedAt.Value <= filter.CompletedTo.Value);
        }

        return Task.FromResult<IEnumerable<TodoItem>>(query.OrderBy(t => t.CreatedAt).ToList());
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
