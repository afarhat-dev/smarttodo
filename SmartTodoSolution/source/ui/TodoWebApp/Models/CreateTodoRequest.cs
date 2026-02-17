namespace TodoWebApp.Models;

public class CreateTodoRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? Priority { get; set; }
    public List<string>? Tags { get; set; }
}
