using SmartTodo.Domain.Enums;

namespace SmartTodo.Domain.Entities;

public class TodoItem
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? StartDate { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public TodoStatus Status { get; private set; }

    private TodoItem()
    {
        Title = string.Empty;
    }

    public TodoItem(string title, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        IsCompleted = false;
        Status = TodoStatus.NotStarted;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        Title = title;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsCompleted()
    {
        if (!IsCompleted)
        {
            IsCompleted = true;
            Status = TodoStatus.Completed;
            CompletedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void MarkAsIncomplete()
    {
        if (IsCompleted)
        {
            IsCompleted = false;
            Status = StartDate.HasValue ? TodoStatus.InProgress : TodoStatus.NotStarted;
            CompletedAt = null;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void StartTask()
    {
        if (Status == TodoStatus.NotStarted)
        {
            StartDate = DateTime.UtcNow;
            Status = TodoStatus.InProgress;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void PutOnHold()
    {
        if (Status != TodoStatus.Completed && Status != TodoStatus.Cancelled)
        {
            Status = TodoStatus.OnHold;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void ResumeTask()
    {
        if (Status == TodoStatus.OnHold)
        {
            Status = TodoStatus.InProgress;
            if (!StartDate.HasValue)
            {
                StartDate = DateTime.UtcNow;
            }
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void CancelTask()
    {
        if (Status != TodoStatus.Completed)
        {
            Status = TodoStatus.Cancelled;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
