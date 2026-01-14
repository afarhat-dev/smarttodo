using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTodo.Domain.Entities;
using SmartTodo.Domain.Enums;

namespace SmartTodo.Infrastructure.Persistence.Configurations;

public class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        builder.ToTable("TodoItems");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .IsRequired()
            .ValueGeneratedNever(); // GUID is generated in domain

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(TodoItem.MaxTitleLength);

        builder.Property(t => t.Description)
            .HasMaxLength(TodoItem.MaxDescriptionLength);

        builder.Property(t => t.IsCompleted)
            .IsRequired();

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>(); // Store enum as string

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .IsRequired();

        builder.Property(t => t.StartDate);

        builder.Property(t => t.CompletedAt);

        // Indexes for performance
        builder.HasIndex(t => t.Status)
            .HasDatabaseName("IX_TodoItems_Status");

        builder.HasIndex(t => t.IsCompleted)
            .HasDatabaseName("IX_TodoItems_IsCompleted");

        builder.HasIndex(t => t.CreatedAt)
            .HasDatabaseName("IX_TodoItems_CreatedAt");

        builder.HasIndex(t => t.UpdatedAt)
            .HasDatabaseName("IX_TodoItems_UpdatedAt");
    }
}
