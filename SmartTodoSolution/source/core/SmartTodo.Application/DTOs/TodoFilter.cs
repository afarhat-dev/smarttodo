using SmartTodo.Domain.Enums;

namespace SmartTodo.Application.DTOs;

public record TodoFilter(
    TodoStatus? Status = null,
    TodoPriority? Priority = null,
    DateTime? CreatedFrom = null,
    DateTime? CreatedTo = null,
    DateTime? UpdatedFrom = null,
    DateTime? UpdatedTo = null,
    DateTime? StartDateFrom = null,
    DateTime? StartDateTo = null,
    DateTime? CompletedFrom = null,
    DateTime? CompletedTo = null,
    bool? IsCompleted = null,
    string? Tag = null,
    bool? HasDependencies = null
);
