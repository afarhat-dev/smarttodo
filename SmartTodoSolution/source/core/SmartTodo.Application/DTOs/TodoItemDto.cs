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
    DateTime? CompletedAt,
    List<TagDto> Tags,
    List<DependencyDto> Dependencies
);

public record CreateTodoItemDto(
    string Title,
    string? Description,
    TodoPriority? Priority = null,
    List<string>? Tags = null
);

public record UpdateTodoItemDto(
    string? Title,
    string? Description,
    bool? IsCompleted,
    TodoStatus? Status,
    TodoPriority? Priority,
    List<string>? Tags = null
);

public record TagDto(
    Guid Id,
    string Name
);

public record DependencyDto(
    Guid Id,
    string Title,
    bool IsCompleted,
    TodoStatus Status
);
