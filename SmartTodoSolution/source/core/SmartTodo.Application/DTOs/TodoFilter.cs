using SmartTodo.Domain.Enums;

namespace SmartTodo.Application.DTOs;

public record TodoFilter(
    TodoStatus? Status = null,
    DateTime? CreatedFrom = null,
    DateTime? CreatedTo = null,
    DateTime? UpdatedFrom = null,
    DateTime? UpdatedTo = null,
    DateTime? StartDateFrom = null,
    DateTime? StartDateTo = null,
    DateTime? CompletedFrom = null,
    DateTime? CompletedTo = null,
    bool? IsCompleted = null
);
