using FluentAssertions;
using Moq;
using SmartTodo.Application.DTOs;
using SmartTodo.Application.Interfaces;
using SmartTodo.Application.Services;
using SmartTodo.Domain.Entities;
using SmartTodo.Domain.Enums;
using Xunit;
namespace SmartTodo.Application.Tests.Services;

public class TodoServiceTests
{
    private readonly Mock<ITodoRepository> _mockRepository;
    private readonly TodoService _service;

    public TodoServiceTests()
    {
        _mockRepository = new Mock<ITodoRepository>();
        _service = new TodoService(_mockRepository.Object);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnTodoItemDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var todoItem = new TodoItem("Test Todo", "Test Description");
        _mockRepository.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(todoItem);

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Todo");
        result.Description.Should().Be("Test Description");
        _mockRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((TodoItem?)null);

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllTodoItems()
    {
        // Arrange
        var todoItems = new List<TodoItem>
        {
            new TodoItem("Todo 1"),
            new TodoItem("Todo 2"),
            new TodoItem("Todo 3")
        };
        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(todoItems);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(dto => dto.Title == "Todo 1");
        result.Should().Contain(dto => dto.Title == "Todo 2");
        result.Should().Contain(dto => dto.Title == "Todo 3");
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithNoTodos_ShouldReturnEmptyList()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<TodoItem>());

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    #endregion

    #region GetFilteredAsync Tests

    [Fact]
    public async Task GetFilteredAsync_WithFilter_ShouldReturnFilteredTodos()
    {
        // Arrange
        var filter = new TodoFilter(Status: TodoStatus.InProgress);
        var todoItems = new List<TodoItem>
        {
            new TodoItem("Todo 1"),
            new TodoItem("Todo 2")
        };
        _mockRepository.Setup(r => r.GetFilteredAsync(filter))
            .ReturnsAsync(todoItems);

        // Act
        var result = await _service.GetFilteredAsync(filter);

        // Assert
        result.Should().HaveCount(2);
        _mockRepository.Verify(r => r.GetFilteredAsync(filter), Times.Once);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidDto_ShouldCreateAndReturnTodoItem()
    {
        // Arrange
        var createDto = new CreateTodoItemDto
        (
            "New Todo",
            "New Description"
        );
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<TodoItem>()))
            .ReturnsAsync((TodoItem todo) => todo);

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("New Todo");
        result.Description.Should().Be("New Description");
        result.Status.Should().Be(TodoStatus.NotStarted);
        result.IsCompleted.Should().BeFalse();
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<TodoItem>()), Times.Once);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithExistingId_ShouldUpdateAndReturnTodoItem()
    {
        // Arrange
        var id = Guid.NewGuid();
        var todoItem = new TodoItem("Original Title", "Original Description");
        var updateDto = new SmartTodo.Application.DTOs.UpdateTodoItemDto
        (
            "Updated Title",
            "Updated Description",           
            false,
            TodoStatus.NotStarted,
            TodoPriority.Low
        );

        _mockRepository.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(todoItem);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<TodoItem>()))
            .ReturnsAsync((TodoItem todo) => todo);

        // Act
        var result = await _service.UpdateAsync(id, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated Title");
        result.Description.Should().Be("Updated Description");
        _mockRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<TodoItem>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        var updateDto = new UpdateTodoItemDto("Updated Title",null,null,null,null);// ( Title = );
        _mockRepository.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((TodoItem?)null);

        // Act
        var result = await _service.UpdateAsync(id, updateDto);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<TodoItem>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithStatusChange_ShouldUpdateStatus()
    {
        // Arrange
        var id = Guid.NewGuid();
        var todoItem = new TodoItem("Test Todo");
        var updateDto = new UpdateTodoItemDto(null, null, null, TodoStatus.InProgress,null);

        _mockRepository.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(todoItem);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<TodoItem>()))
            .ReturnsAsync((TodoItem todo) => todo);

        // Act
        var result = await _service.UpdateAsync(id, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(TodoStatus.InProgress);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<TodoItem>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithIsCompletedTrue_ShouldMarkAsCompleted()
    {
        // Arrange
        var id = Guid.NewGuid();
        var todoItem = new TodoItem("Test Todo");
        var updateDto = new UpdateTodoItemDto(null, null, true, null,null);

        _mockRepository.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(todoItem);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<TodoItem>()))
            .ReturnsAsync((TodoItem todo) => todo);

        // Act
        var result = await _service.UpdateAsync(id, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.IsCompleted.Should().BeTrue();
        result.Status.Should().Be(TodoStatus.Completed);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<TodoItem>()), Times.Once);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithExistingId_ShouldReturnTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.DeleteAsync(id))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(id);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.DeleteAsync(id))
            .ReturnsAsync(false);

        // Act
        var result = await _service.DeleteAsync(id);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    #endregion
}
