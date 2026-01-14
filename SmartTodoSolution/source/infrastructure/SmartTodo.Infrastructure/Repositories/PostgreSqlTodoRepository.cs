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
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<TodoItem>> GetAllAsync()
    {
        return await _context.TodoItems
            .AsNoTracking()
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TodoItem>> GetFilteredAsync(TodoFilter filter)
    {
        var query = _context.TodoItems.AsNoTracking().AsQueryable();

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
        var existingItem = await _context.TodoItems.FindAsync(todoItem.Id);
        if (existingItem == null)
            return null;

        // Detach the existing tracked entity
        _context.Entry(existingItem).State = EntityState.Detached;

        // Attach and update the new entity
        _context.TodoItems.Update(todoItem);
        await _context.SaveChangesAsync();

        return todoItem;
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
}
