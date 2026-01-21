using SmartTodo.Domain.Enums;

namespace SmartTodo.Domain.Entities;

public class TodoItem
{
    // Validation constants
    public const int MaxTitleLength = 200;
    public const int MaxDescriptionLength = 2000;

    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? StartDate { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public TodoStatus Status { get; private set; }
    public TodoPriority Priority { get; private set; }

    private TodoItem()
    {
        Title = string.Empty;
    }

    public TodoItem(string title, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        // Sanitize and validate title
        title = title.Trim();
        if (title.Length > MaxTitleLength)
            throw new ArgumentException($"Title cannot exceed {MaxTitleLength} characters", nameof(title));

        // Sanitize and validate description
        if (!string.IsNullOrWhiteSpace(description))
        {
            description = description.Trim();
            if (description.Length > MaxDescriptionLength)
                throw new ArgumentException($"Description cannot exceed {MaxDescriptionLength} characters", nameof(description));
        }

        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        IsCompleted = false;
        Status = TodoStatus.NotStarted;
        Priority = TodoPriority.Medium;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        // Sanitize and validate title
        title = title.Trim();
        if (title.Length > MaxTitleLength)
            throw new ArgumentException($"Title cannot exceed {MaxTitleLength} characters", nameof(title));

        Title = title;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string? description)
    {
        // Sanitize and validate description
        if (!string.IsNullOrWhiteSpace(description))
        {
            description = description.Trim();
            if (description.Length > MaxDescriptionLength)
                throw new ArgumentException($"Description cannot exceed {MaxDescriptionLength} characters", nameof(description));
        }

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
        // Allow starting/restarting tasks that are not already in progress
        if (Status != TodoStatus.InProgress)
        {
            // If restarting a completed or cancelled task, reset completion state
            if (Status == TodoStatus.Completed || Status == TodoStatus.Cancelled)
            {
                IsCompleted = false;
                CompletedAt = null;
            }

            // Set or update start date if not set
            if (!StartDate.HasValue)
            {
                StartDate = DateTime.UtcNow;
            }

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

    public void UpdatePriority(TodoPriority priority)
    {
        Priority = priority;
        UpdatedAt = DateTime.UtcNow;
    }
}
