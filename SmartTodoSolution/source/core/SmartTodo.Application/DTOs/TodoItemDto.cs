using SmartTodo.Domain.Enums;

namespace SmartTodo.Application.DTOs;

public record TodoItemDto(
    Guid Id,
    string Title,
    string? Description,
    bool IsCompleted,
    TodoStatus Status,
    TodoPriority Priority,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? StartDate,
    DateTime? CompletedAt
);

public record CreateTodoItemDto(
    string Title,
    string? Description,
    TodoPriority? Priority = null
);

public record UpdateTodoItemDto(
    string? Title,
    string? Description,
    bool? IsCompleted,
    TodoStatus? Status,
    TodoPriority? Priority
);
