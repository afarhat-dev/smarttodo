using FluentAssertions;
using SmartTodo.Domain.Entities;
using SmartTodo.Domain.Enums;

namespace SmartTodo.Domain.Tests.Entities;

public class TodoItemTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidTitle_ShouldCreateTodoItem()
    {
        // Arrange
        var title = "Test Todo";
        var description = "Test Description";

        // Act
        var todo = new TodoItem(title, description);

        // Assert
        todo.Id.Should().NotBeEmpty();
        todo.Title.Should().Be(title);
        todo.Description.Should().Be(description);
        todo.IsCompleted.Should().BeFalse();
        todo.Status.Should().Be(TodoStatus.NotStarted);
        todo.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        todo.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        todo.StartDate.Should().BeNull();
        todo.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNullDescription_ShouldCreateTodoItem()
    {
        // Arrange
        var title = "Test Todo";

        // Act
        var todo = new TodoItem(title);

        // Assert
        todo.Title.Should().Be(title);
        todo.Description.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidTitle_ShouldThrowArgumentException(string? invalidTitle)
    {
        // Act
        var act = () => new TodoItem(invalidTitle!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Title cannot be empty*");
    }

    [Fact]
    public void Constructor_WithTitleExceedingMaxLength_ShouldThrowArgumentException()
    {
        // Arrange
        var longTitle = new string('a', TodoItem.MaxTitleLength + 1);

        // Act
        var act = () => new TodoItem(longTitle);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"Title cannot exceed {TodoItem.MaxTitleLength} characters*");
    }

    [Fact]
    public void Constructor_WithDescriptionExceedingMaxLength_ShouldThrowArgumentException()
    {
        // Arrange
        var title = "Test Todo";
        var longDescription = new string('a', TodoItem.MaxDescriptionLength + 1);

        // Act
        var act = () => new TodoItem(title, longDescription);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"Description cannot exceed {TodoItem.MaxDescriptionLength} characters*");
    }

    [Fact]
    public void Constructor_WithWhitespaceInTitleAndDescription_ShouldTrimThem()
    {
        // Arrange
        var title = "  Test Todo  ";
        var description = "  Test Description  ";

        // Act
        var todo = new TodoItem(title, description);

        // Assert
        todo.Title.Should().Be("Test Todo");
        todo.Description.Should().Be("Test Description");
    }

    #endregion

    #region UpdateTitle Tests

    [Fact]
    public void UpdateTitle_WithValidTitle_ShouldUpdateTitle()
    {
        // Arrange
        var todo = new TodoItem("Original Title");
        var newTitle = "Updated Title";

        // Act
        todo.UpdateTitle(newTitle);

        // Assert
        todo.Title.Should().Be(newTitle);
        todo.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateTitle_WithInvalidTitle_ShouldThrowArgumentException(string? invalidTitle)
    {
        // Arrange
        var todo = new TodoItem("Original Title");

        // Act
        var act = () => todo.UpdateTitle(invalidTitle!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Title cannot be empty*");
    }

    [Fact]
    public void UpdateTitle_WithTitleExceedingMaxLength_ShouldThrowArgumentException()
    {
        // Arrange
        var todo = new TodoItem("Original Title");
        var longTitle = new string('a', TodoItem.MaxTitleLength + 1);

        // Act
        var act = () => todo.UpdateTitle(longTitle);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"Title cannot exceed {TodoItem.MaxTitleLength} characters*");
    }

    #endregion

    #region UpdateDescription Tests

    [Fact]
    public void UpdateDescription_WithValidDescription_ShouldUpdateDescription()
    {
        // Arrange
        var todo = new TodoItem("Test Todo", "Original Description");
        var newDescription = "Updated Description";

        // Act
        todo.UpdateDescription(newDescription);

        // Assert
        todo.Description.Should().Be(newDescription);
        todo.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateDescription_WithNull_ShouldSetDescriptionToNull()
    {
        // Arrange
        var todo = new TodoItem("Test Todo", "Original Description");

        // Act
        todo.UpdateDescription(null);

        // Assert
        todo.Description.Should().BeNull();
    }

    [Fact]
    public void UpdateDescription_WithDescriptionExceedingMaxLength_ShouldThrowArgumentException()
    {
        // Arrange
        var todo = new TodoItem("Test Todo");
        var longDescription = new string('a', TodoItem.MaxDescriptionLength + 1);

        // Act
        var act = () => todo.UpdateDescription(longDescription);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"Description cannot exceed {TodoItem.MaxDescriptionLength} characters*");
    }

    #endregion

    #region StartTask Tests

    [Fact]
    public void StartTask_WhenNotStarted_ShouldSetStatusToInProgress()
    {
        // Arrange
        var todo = new TodoItem("Test Todo");

        // Act
        todo.StartTask();

        // Assert
        todo.Status.Should().Be(TodoStatus.InProgress);
        todo.StartDate.Should().NotBeNull();
        todo.StartDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void StartTask_WhenCompleted_ShouldResetAndStartTask()
    {
        // Arrange
        var todo = new TodoItem("Test Todo");
        todo.StartTask();
        todo.MarkAsCompleted();

        // Act
        todo.StartTask();

        // Assert
        todo.Status.Should().Be(TodoStatus.InProgress);
        todo.IsCompleted.Should().BeFalse();
        todo.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void StartTask_WhenCancelled_ShouldResetAndStartTask()
    {
        // Arrange
        var todo = new TodoItem("Test Todo");
        todo.CancelTask();

        // Act
        todo.StartTask();

        // Assert
        todo.Status.Should().Be(TodoStatus.InProgress);
        todo.IsCompleted.Should().BeFalse();
        todo.StartDate.Should().NotBeNull();
    }

    [Fact]
    public void StartTask_WhenAlreadyInProgress_ShouldNotChangStatus()
    {
        // Arrange
        var todo = new TodoItem("Test Todo");
        todo.StartTask();
        var originalStartDate = todo.StartDate;

        // Wait a bit
        Thread.Sleep(10);

        // Act
        todo.StartTask();

        // Assert
        todo.Status.Should().Be(TodoStatus.InProgress);
        todo.StartDate.Should().Be(originalStartDate);
    }

    #endregion

    #region MarkAsCompleted Tests

    [Fact]
    public void MarkAsCompleted_WhenNotCompleted_ShouldMarkAsCompleted()
    {
        // Arrange
        var todo = new TodoItem("Test Todo");
        todo.StartTask();

        // Act
        todo.MarkAsCompleted();

        // Assert
        todo.IsCompleted.Should().BeTrue();
        todo.Status.Should().Be(TodoStatus.Completed);
        todo.CompletedAt.Should().NotBeNull();
        todo.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MarkAsCompleted_WhenAlreadyCompleted_ShouldNotChange()
    {
        // Arrange
        var todo = new TodoItem("Test Todo");
        todo.StartTask();
        todo.MarkAsCompleted();
        var originalCompletedAt = todo.CompletedAt;

        // Wait a bit
        Thread.Sleep(10);

        // Act
        todo.MarkAsCompleted();

        // Assert
        todo.CompletedAt.Should().Be(originalCompletedAt);
    }

    #endregion

    #region MarkAsIncomplete Tests

    [Fact]
    public void MarkAsIncomplete_WhenCompleted_ShouldMarkAsIncomplete()
    {
        // Arrange
        var todo = new TodoItem("Test Todo");
        todo.StartTask();
        todo.MarkAsCompleted();

        // Act
        todo.MarkAsIncomplete();

        // Assert
        todo.IsCompleted.Should().BeFalse();
        todo.Status.Should().Be(TodoStatus.InProgress);
        todo.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void MarkAsIncomplete_WhenCompletedButNeverStarted_ShouldMarkAsNotStarted()
    {
        // Arrange
        var todo = new TodoItem("Test Todo");
        todo.MarkAsCompleted();

        // Act
        todo.MarkAsIncomplete();

        // Assert
        todo.Status.Should().Be(TodoStatus.NotStarted);
        todo.StartDate.Should().BeNull();
    }

    #endregion

    #region PutOnHold Tests

    [Fact]
    public void PutOnHold_WhenInProgress_ShouldSetStatusToOnHold()
    {
        // Arrange
        var todo = new TodoItem("Test Todo");
        todo.StartTask();

        // Act
        todo.PutOnHold();

        // Assert
        todo.Status.Should().Be(TodoStatus.OnHold);
    }

    [Fact]
    public void PutOnHold_WhenCompleted_ShouldNotChange()
    {
        // Arrange
        var todo = new TodoItem("Test Todo");
        todo.StartTask();
        todo.MarkAsCompleted();

        // Act
        todo.PutOnHold();

        // Assert
        todo.Status.Should().Be(TodoStatus.Completed);
    }

    [Fact]
    public void PutOnHold_WhenCancelled_ShouldNotChange()
    {
        // Arrange
        var todo = new TodoItem("Test Todo");
        todo.CancelTask();

        // Act
        todo.PutOnHold();

        // Assert
        todo.Status.Should().Be(TodoStatus.Cancelled);
    }

    #endregion

    #region ResumeTask Tests

    [Fact]
    public void ResumeTask_WhenOnHold_ShouldSetStatusToInProgress()
    {
        // Arrange
        var todo = new TodoItem("Test Todo");
        todo.StartTask();
        todo.PutOnHold();

        // Act
        todo.ResumeTask();

        // Assert
        todo.Status.Should().Be(TodoStatus.InProgress);
        todo.StartDate.Should().NotBeNull();
    }

    [Fact]
    public void ResumeTask_WhenOnHoldButNeverStarted_ShouldSetStartDate()
    {
        // Arrange
        var todo = new TodoItem("Test Todo");
        todo.PutOnHold();

        // Act
        todo.ResumeTask();

        // Assert
        todo.Status.Should().Be(TodoStatus.InProgress);
        todo.StartDate.Should().NotBeNull();
    }

    [Fact]
    public void ResumeTask_WhenNotOnHold_ShouldNotChange()
    {
        // Arrange
        var todo = new TodoItem("Test Todo");
        todo.StartTask();

        // Act
        todo.ResumeTask();

        // Assert
        todo.Status.Should().Be(TodoStatus.InProgress);
    }

    #endregion

    #region CancelTask Tests

    [Fact]
    public void CancelTask_WhenNotCompleted_ShouldSetStatusToCancelled()
    {
        // Arrange
        var todo = new TodoItem("Test Todo");
        todo.StartTask();

        // Act
        todo.CancelTask();

        // Assert
        todo.Status.Should().Be(TodoStatus.Cancelled);
    }

    [Fact]
    public void CancelTask_WhenCompleted_ShouldNotChange()
    {
        // Arrange
        var todo = new TodoItem("Test Todo");
        todo.StartTask();
        todo.MarkAsCompleted();

        // Act
        todo.CancelTask();

        // Assert
        todo.Status.Should().Be(TodoStatus.Completed);
    }

    #endregion
}
