using SmartTodo.Application.DTOs;
using SmartTodo.Application.Interfaces;
using SmartTodo.Domain.Entities;

namespace SmartTodo.Application.Services;

public class TodoService : ITodoService
{
    private readonly ITodoRepository _repository;

    public TodoService(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task<TodoItemDto?> GetByIdAsync(Guid id)
    {
        var todoItem = await _repository.GetByIdAsync(id);
        return todoItem != null ? MapToDto(todoItem) : null;
    }

    public async Task<IEnumerable<TodoItemDto>> GetAllAsync()
    {
        var todoItems = await _repository.GetAllAsync();
        return todoItems.Select(MapToDto);
    }

    public async Task<TodoItemDto> CreateAsync(CreateTodoItemDto createDto)
    {
        var todoItem = new TodoItem(createDto.Title, createDto.Description);
        var createdItem = await _repository.AddAsync(todoItem);
        return MapToDto(createdItem);
    }

    public async Task<TodoItemDto?> UpdateAsync(Guid id, UpdateTodoItemDto updateDto)
    {
        var todoItem = await _repository.GetByIdAsync(id);
        if (todoItem == null)
            return null;

        if (updateDto.Title != null)
            todoItem.UpdateTitle(updateDto.Title);

        if (updateDto.Description != null)
            todoItem.UpdateDescription(updateDto.Description);

        if (updateDto.IsCompleted.HasValue)
        {
            if (updateDto.IsCompleted.Value)
                todoItem.MarkAsCompleted();
            else
                todoItem.MarkAsIncomplete();
        }

        var updatedItem = await _repository.UpdateAsync(todoItem);
        return updatedItem != null ? MapToDto(updatedItem) : null;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _repository.DeleteAsync(id);
    }

    private static TodoItemDto MapToDto(TodoItem todoItem)
    {
        return new TodoItemDto(
            todoItem.Id,
            todoItem.Title,
            todoItem.Description,
            todoItem.IsCompleted,
            todoItem.CreatedAt,
            todoItem.CompletedAt
        );
    }
}
