namespace SmartTodo.Domain.Entities;

public class Tag
{
    public const int MaxNameLength = 50;

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<TodoItem> _todoItems = new();
    public IReadOnlyCollection<TodoItem> TodoItems => _todoItems.AsReadOnly();

    private Tag()
    {
        Name = string.Empty;
    }

    public Tag(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tag name cannot be empty", nameof(name));

        name = name.Trim().ToLowerInvariant();
        if (name.Length > MaxNameLength)
            throw new ArgumentException($"Tag name cannot exceed {MaxNameLength} characters", nameof(name));

        Id = Guid.NewGuid();
        Name = name;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tag name cannot be empty", nameof(name));

        name = name.Trim().ToLowerInvariant();
        if (name.Length > MaxNameLength)
            throw new ArgumentException($"Tag name cannot exceed {MaxNameLength} characters", nameof(name));

        Name = name;
    }
}
