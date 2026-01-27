namespace TodoWebApp.Models;

public class TodoItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public int Status { get; set; } = 0;
    public int Priority { get; set; } = 1; // 0=Low, 1=Medium, 2=High, 3=Critical
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? CompletedAt { get; set; }
}
