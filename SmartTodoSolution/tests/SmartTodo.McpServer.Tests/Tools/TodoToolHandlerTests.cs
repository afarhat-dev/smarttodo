using System.Text.Json;
using FluentAssertions;
using Moq;
using SmartTodo.Application.DTOs;
using SmartTodo.Application.Services;
using SmartTodo.Domain.Enums;
using SmartTodo.McpServer.Tools;
using Xunit;
namespace SmartTodo.McpServer.Tests.Tools;

public class TodoToolHandlerTests
{
    private readonly Mock<ITodoService> _mockTodoService;
    private readonly TodoToolHandler _handler;

    public TodoToolHandlerTests()
    {
        _mockTodoService = new Mock<ITodoService>();
        _handler = new TodoToolHandler(_mockTodoService.Object);
    }

    #region HandleToolCallAsync Tests

    [Fact]
    public async Task HandleToolCallAsync_WithUnknownTool_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var toolName = "unknown_tool";
        var parameters = JsonDocument.Parse("{}").RootElement;

        // Act
        Func<Task> act = async () => await _handler.HandleToolCallAsync(toolName, parameters);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Unknown tool: unknown_tool");
    }

    #endregion

    #region CreateTodo Tests

    [Fact]
    public async Task HandleToolCallAsync_CreateTodo_ShouldCreateTodoAndReturnSuccess()
    {
        // Arrange
        var parameters = JsonDocument.Parse(@"{
            ""title"": ""Test Todo"",
            ""description"": ""Test Description""
        }").RootElement;

        var expectedDto = new TodoItemDto(
            Guid.NewGuid(),
            "Test Todo",
            "Test Description",
            false,
            TodoStatus.NotStarted,
            DateTime.UtcNow,
            DateTime.UtcNow,
            null,
            null
        );

        _mockTodoService.Setup(s => s.CreateAsync(It.IsAny<CreateTodoItemDto>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _handler.HandleToolCallAsync("create_todo", parameters);

        // Assert
        result.Should().NotBeNull();
        var resultDict = result as IDictionary<string, object> ??
            JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(result))!;
        resultDict.Should().ContainKey("success");
        resultDict.Should().ContainKey("todo");
        resultDict.Should().ContainKey("message");

        _mockTodoService.Verify(s => s.CreateAsync(It.Is<CreateTodoItemDto>(
            dto => dto.Title == "Test Todo" && dto.Description == "Test Description"
        )), Times.Once);
    }

    [Fact]
    public async Task HandleToolCallAsync_CreateTodoWithoutDescription_ShouldCreateTodo()
    {
        // Arrange
        var parameters = JsonDocument.Parse(@"{
            ""title"": ""Test Todo""
        }").RootElement;

        var expectedDto = new TodoItemDto(
            Guid.NewGuid(),
            "Test Todo",
            null,
            false,
            TodoStatus.NotStarted,
            DateTime.UtcNow,
            DateTime.UtcNow,
            null,
            null
        );

        _mockTodoService.Setup(s => s.CreateAsync(It.IsAny<CreateTodoItemDto>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _handler.HandleToolCallAsync("create_todo", parameters);

        // Assert
        result.Should().NotBeNull();
        _mockTodoService.Verify(s => s.CreateAsync(It.Is<CreateTodoItemDto>(
            dto => dto.Title == "Test Todo" && dto.Description == null
        )), Times.Once);
    }

    [Fact]
    public async Task HandleToolCallAsync_CreateTodoWithoutParameters_ShouldThrowArgumentException()
    {
        // Act
        Func<Task> act = async () => await _handler.HandleToolCallAsync("create_todo", null);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Parameters are required for create_todo");
    }

    #endregion

    #region GetTodo Tests

    [Fact]
    public async Task HandleToolCallAsync_GetTodoWithExistingId_ShouldReturnTodo()
    {
        // Arrange
        var id = Guid.NewGuid();
        var parameters = JsonDocument.Parse($@"{{
            ""id"": ""{id}""
        }}").RootElement;

        var expectedDto = new TodoItemDto(
            id,
            "Test Todo",
            "Test Description",
            false,
            TodoStatus.NotStarted,
            DateTime.UtcNow,
            DateTime.UtcNow,
            null,
            null
        );

        _mockTodoService.Setup(s => s.GetByIdAsync(id))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _handler.HandleToolCallAsync("get_todo", parameters);

        // Assert
        result.Should().NotBeNull();
        _mockTodoService.Verify(s => s.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task HandleToolCallAsync_GetTodoWithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var parameters = JsonDocument.Parse($@"{{
            ""id"": ""{id}""
        }}").RootElement;

        _mockTodoService.Setup(s => s.GetByIdAsync(id))
            .ReturnsAsync((TodoItemDto?)null);

        // Act
        var result = await _handler.HandleToolCallAsync("get_todo", parameters);

        // Assert
        result.Should().NotBeNull();
        var resultDict = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(result))!;
        resultDict["success"].ToString().Should().Be("False");
    }

    [Fact]
    public async Task HandleToolCallAsync_GetTodoWithInvalidId_ShouldThrowArgumentException()
    {
        // Arrange
        var parameters = JsonDocument.Parse(@"{
            ""id"": ""invalid-guid""
        }").RootElement;

        // Act
        Func<Task> act = async () => await _handler.HandleToolCallAsync("get_todo", parameters);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid ID format");
    }

    #endregion

    #region ListTodos Tests

    [Fact]
    public async Task HandleToolCallAsync_ListTodosWithoutParameters_ShouldReturnAllTodos()
    {
        // Arrange
        var todos = new List<TodoItemDto>
        {
            new TodoItemDto(Guid.NewGuid(), "Todo 1", null, false, TodoStatus.NotStarted,
                DateTime.UtcNow, DateTime.UtcNow, null, null),
            new TodoItemDto(Guid.NewGuid(), "Todo 2", null, false, TodoStatus.NotStarted,
                DateTime.UtcNow, DateTime.UtcNow, null, null)
        };

        _mockTodoService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(todos);

        // Act
        var result = await _handler.HandleToolCallAsync("list_todos", null);

        // Assert
        result.Should().NotBeNull();
        _mockTodoService.Verify(s => s.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleToolCallAsync_ListTodosWithFilter_ShouldReturnFilteredTodos()
    {
        // Arrange
        var parameters = JsonDocument.Parse(@"{
            ""status"": ""InProgress"",
            ""isCompleted"": false
        }").RootElement;

        var todos = new List<TodoItemDto>
        {
            new TodoItemDto(Guid.NewGuid(), "Todo 1", null, false, TodoStatus.InProgress,
                DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow, null)
        };

        _mockTodoService.Setup(s => s.GetFilteredAsync(It.IsAny<TodoFilter>()))
            .ReturnsAsync(todos);

        // Act
        var result = await _handler.HandleToolCallAsync("list_todos", parameters);

        // Assert
        result.Should().NotBeNull();
        _mockTodoService.Verify(s => s.GetFilteredAsync(It.IsAny<TodoFilter>()), Times.Once);
    }

    #endregion

    #region UpdateTodo Tests

    [Fact]
    public async Task HandleToolCallAsync_UpdateTodoWithExistingId_ShouldUpdateAndReturnTodo()
    {
        // Arrange
        var id = Guid.NewGuid();
        var parameters = JsonDocument.Parse($@"{{
            ""id"": ""{id}"",
            ""title"": ""Updated Title""
        }}").RootElement;

        var updatedDto = new TodoItemDto(
            id,
            "Updated Title",
            null,
            false,
            TodoStatus.NotStarted,
            DateTime.UtcNow,
            DateTime.UtcNow,
            null,
            null
        );

        _mockTodoService.Setup(s => s.UpdateAsync(id, It.IsAny<UpdateTodoItemDto>()))
            .ReturnsAsync(updatedDto);

        // Act
        var result = await _handler.HandleToolCallAsync("update_todo", parameters);

        // Assert
        result.Should().NotBeNull();
        _mockTodoService.Verify(s => s.UpdateAsync(id, It.IsAny<UpdateTodoItemDto>()), Times.Once);
    }

    #endregion

    #region DeleteTodo Tests

    [Fact]
    public async Task HandleToolCallAsync_DeleteTodoWithExistingId_ShouldReturnSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var parameters = JsonDocument.Parse($@"{{
            ""id"": ""{id}""
        }}").RootElement;

        _mockTodoService.Setup(s => s.DeleteAsync(id))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.HandleToolCallAsync("delete_todo", parameters);

        // Assert
        result.Should().NotBeNull();
        var resultDict = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(result))!;
        resultDict["success"].ToString().Should().Be("True");
        _mockTodoService.Verify(s => s.DeleteAsync(id), Times.Once);
    }

    #endregion

    #region StatusUpdate Tests

    [Theory]
    [InlineData("start_todo")]
    [InlineData("complete_todo")]
    [InlineData("pause_todo")]
    [InlineData("resume_todo")]
    [InlineData("cancel_todo")]
    public async Task HandleToolCallAsync_StatusUpdateWithExistingId_ShouldUpdateStatus(string toolName)
    {
        // Arrange
        var id = Guid.NewGuid();
        var parameters = JsonDocument.Parse($@"{{
            ""id"": ""{id}""
        }}").RootElement;

        var updatedDto = new TodoItemDto(
            id,
            "Test Todo",
            null,
            false,
            TodoStatus.InProgress,
            DateTime.UtcNow,
            DateTime.UtcNow,
            DateTime.UtcNow,
            null
        );

        _mockTodoService.Setup(s => s.UpdateAsync(id, It.IsAny<UpdateTodoItemDto>()))
            .ReturnsAsync(updatedDto);

        // Act
        var result = await _handler.HandleToolCallAsync(toolName, parameters);

        // Assert
        result.Should().NotBeNull();
        _mockTodoService.Verify(s => s.UpdateAsync(id, It.IsAny<UpdateTodoItemDto>()), Times.Once);
    }

    #endregion
}
