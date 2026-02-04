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

    public async Task<IEnumerable<TodoItemDto>> GetFilteredAsync(TodoFilter filter)
    {
        var todoItems = await _repository.GetFilteredAsync(filter);
        return todoItems.Select(MapToDto);
    }

    public async Task<TodoItemDto> CreateAsync(CreateTodoItemDto createDto)
    {
        var todoItem = new TodoItem(createDto.Title, createDto.Description);

        if (createDto.Priority.HasValue)
            todoItem.UpdatePriority(createDto.Priority.Value);

        if (createDto.Tags != null)
        {
            foreach (var tagName in createDto.Tags)
            {
                var tag = await _repository.GetOrCreateTagAsync(tagName);
                todoItem.AddTag(tag);
            }
        }

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

        if (updateDto.Status.HasValue)
        {
            switch (updateDto.Status.Value)
            {
                case Domain.Enums.TodoStatus.InProgress:
                    todoItem.StartTask();
                    break;
                case Domain.Enums.TodoStatus.OnHold:
                    todoItem.PutOnHold();
                    break;
                case Domain.Enums.TodoStatus.Completed:
                    todoItem.MarkAsCompleted();
                    break;
                case Domain.Enums.TodoStatus.Cancelled:
                    todoItem.CancelTask();
                    break;
                case Domain.Enums.TodoStatus.NotStarted:
                    todoItem.MarkAsIncomplete();
                    break;
            }
        }

        if (updateDto.IsCompleted.HasValue)
        {
            if (updateDto.IsCompleted.Value)
                todoItem.MarkAsCompleted();
            else
                todoItem.MarkAsIncomplete();
        }

        if (updateDto.Priority.HasValue)
        {
            todoItem.UpdatePriority(updateDto.Priority.Value);
        }

        if (updateDto.Tags != null)
        {
            // Remove tags not in the new list
            var currentTags = todoItem.Tags.ToList();
            foreach (var tag in currentTags)
            {
                if (!updateDto.Tags.Contains(tag.Name, StringComparer.OrdinalIgnoreCase))
                    todoItem.RemoveTag(tag);
            }

            // Add new tags
            foreach (var tagName in updateDto.Tags)
            {
                if (!todoItem.Tags.Any(t => t.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase)))
                {
                    var tag = await _repository.GetOrCreateTagAsync(tagName);
                    todoItem.AddTag(tag);
                }
            }
        }

        var updatedItem = await _repository.UpdateAsync(todoItem);
        return updatedItem != null ? MapToDto(updatedItem) : null;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<TagDto>> GetAllTagsAsync()
    {
        var tags = await _repository.GetAllTagsAsync();
        return tags.Select(t => new TagDto(t.Id, t.Name));
    }

    public async Task<TodoItemDto?> AddDependencyAsync(Guid todoId, Guid dependencyId)
    {
        await _repository.AddDependencyAsync(todoId, dependencyId);
        var todoItem = await _repository.GetByIdAsync(todoId);
        return todoItem != null ? MapToDto(todoItem) : null;
    }

    public async Task<TodoItemDto?> RemoveDependencyAsync(Guid todoId, Guid dependencyId)
    {
        await _repository.RemoveDependencyAsync(todoId, dependencyId);
        var todoItem = await _repository.GetByIdAsync(todoId);
        return todoItem != null ? MapToDto(todoItem) : null;
    }

    private static TodoItemDto MapToDto(TodoItem todoItem)
    {
        return new TodoItemDto(
            todoItem.Id,
            todoItem.Title,
            todoItem.Description,
            todoItem.IsCompleted,
            todoItem.Status,
            todoItem.Priority,
            todoItem.CreatedAt,
            todoItem.UpdatedAt,
            todoItem.StartDate,
            todoItem.CompletedAt,
            todoItem.Tags.Select(t => new TagDto(t.Id, t.Name)).ToList(),
            todoItem.Dependencies.Select(d => new DependencyDto(d.Id, d.Title, d.IsCompleted, d.Status)).ToList()
        );
    }
}
