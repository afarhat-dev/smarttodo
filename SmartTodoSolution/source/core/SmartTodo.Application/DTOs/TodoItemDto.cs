namespace SmartTodo.Application.DTOs;

public record TodoItemDto(
    Guid Id,
    string Title,
    string? Description,
    bool IsCompleted,
    DateTime CreatedAt,
    DateTime? CompletedAt
);

public record CreateTodoItemDto(
    string Title,
    string? Description
);

public record UpdateTodoItemDto(
    string? Title,
    string? Description,
    bool? IsCompleted
);
