using Microsoft.EntityFrameworkCore;
using SmartTodo.Domain.Entities;
using SmartTodo.Infrastructure.Persistence.Configurations;

namespace SmartTodo.Infrastructure.Persistence;

public class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
    {
    }

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new TodoItemConfiguration());
    }
}
