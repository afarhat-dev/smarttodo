using Microsoft.EntityFrameworkCore;
using SmartTodo.Application.DTOs;
using SmartTodo.Application.Interfaces;
using SmartTodo.Domain.Entities;
using SmartTodo.Infrastructure.Persistence;

namespace SmartTodo.Infrastructure.Repositories;

public class PostgreSqlTodoRepository : ITodoRepository
{
    private readonly TodoDbContext _context;

    public PostgreSqlTodoRepository(TodoDbContext context)
    {
        _context = context;
    }

    public async Task<TodoItem?> GetByIdAsync(Guid id)
    {
        return await _context.TodoItems
            .Include(t => t.Tags)
            .Include(t => t.Dependencies)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<TodoItem>> GetAllAsync()
    {
        return await _context.TodoItems
            .Include(t => t.Tags)
            .Include(t => t.Dependencies)
            .AsNoTracking()
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TodoItem>> GetFilteredAsync(TodoFilter filter)
    {
        var query = _context.TodoItems
            .Include(t => t.Tags)
            .Include(t => t.Dependencies)
            .AsNoTracking()
            .AsQueryable();

        if (filter.Status.HasValue)
        {
            query = query.Where(t => t.Status == filter.Status.Value);
        }

        if (filter.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == filter.Priority.Value);
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

        if (!string.IsNullOrWhiteSpace(filter.Tag))
        {
            var tagName = filter.Tag.Trim().ToLowerInvariant();
            query = query.Where(t => t.Tags.Any(tag => tag.Name == tagName));
        }

        if (filter.HasDependencies.HasValue)
        {
            if (filter.HasDependencies.Value)
                query = query.Where(t => t.Dependencies.Any());
            else
                query = query.Where(t => !t.Dependencies.Any());
        }

        return await query.OrderBy(t => t.CreatedAt).ToListAsync();
    }

    public async Task<TodoItem> AddAsync(TodoItem todoItem)
    {
        await _context.TodoItems.AddAsync(todoItem);
        await _context.SaveChangesAsync();
        return todoItem;
    }

    public async Task<TodoItem?> UpdateAsync(TodoItem todoItem)
    {
        var existingItem = await _context.TodoItems
            .Include(t => t.Tags)
            .Include(t => t.Dependencies)
            .FirstOrDefaultAsync(t => t.Id == todoItem.Id);

        if (existingItem == null)
            return null;

        _context.Entry(existingItem).CurrentValues.SetValues(todoItem);

        // Update tags
        existingItem.Tags.ToList().ForEach(t =>
        {
            if (!todoItem.Tags.Any(nt => nt.Id == t.Id))
                existingItem.RemoveTag(t);
        });
        foreach (var tag in todoItem.Tags)
        {
            if (!existingItem.Tags.Any(t => t.Id == tag.Id))
            {
                var trackedTag = await _context.Tags.FindAsync(tag.Id) ?? tag;
                existingItem.AddTag(trackedTag);
            }
        }

        // Update dependencies
        var existingDeps = existingItem.Dependencies.ToList();
        foreach (var dep in existingDeps)
        {
            if (!todoItem.Dependencies.Any(d => d.Id == dep.Id))
                existingItem.RemoveDependency(dep);
        }
        foreach (var dep in todoItem.Dependencies)
        {
            if (!existingItem.Dependencies.Any(d => d.Id == dep.Id))
            {
                var trackedDep = await _context.TodoItems.FindAsync(dep.Id) ?? dep;
                existingItem.AddDependency(trackedDep);
            }
        }

        await _context.SaveChangesAsync();

        return existingItem;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var todoItem = await _context.TodoItems.FindAsync(id);
        if (todoItem == null)
            return false;

        _context.TodoItems.Remove(todoItem);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Tag?> GetTagByNameAsync(string name)
    {
        var normalizedName = name.Trim().ToLowerInvariant();
        return await _context.Tags.FirstOrDefaultAsync(t => t.Name == normalizedName);
    }

    public async Task<Tag> GetOrCreateTagAsync(string name)
    {
        var existing = await GetTagByNameAsync(name);
        if (existing != null)
            return existing;

        var tag = new Tag(name);
        await _context.Tags.AddAsync(tag);
        await _context.SaveChangesAsync();
        return tag;
    }

    public async Task<IEnumerable<Tag>> GetAllTagsAsync()
    {
        return await _context.Tags.AsNoTracking().OrderBy(t => t.Name).ToListAsync();
    }

    public async Task AddDependencyAsync(Guid todoId, Guid dependencyId)
    {
        var todo = await _context.TodoItems
            .Include(t => t.Dependencies)
            .FirstOrDefaultAsync(t => t.Id == todoId)
            ?? throw new InvalidOperationException($"Todo with ID {todoId} not found");

        var dependency = await _context.TodoItems
            .Include(t => t.Dependencies)
            .FirstOrDefaultAsync(t => t.Id == dependencyId)
            ?? throw new InvalidOperationException($"Dependency todo with ID {dependencyId} not found");

        todo.AddDependency(dependency);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveDependencyAsync(Guid todoId, Guid dependencyId)
    {
        var todo = await _context.TodoItems
            .Include(t => t.Dependencies)
            .FirstOrDefaultAsync(t => t.Id == todoId)
            ?? throw new InvalidOperationException($"Todo with ID {todoId} not found");

        var dependency = await _context.TodoItems.FindAsync(dependencyId)
            ?? throw new InvalidOperationException($"Dependency todo with ID {dependencyId} not found");

        todo.RemoveDependency(dependency);
        await _context.SaveChangesAsync();
    }
}
