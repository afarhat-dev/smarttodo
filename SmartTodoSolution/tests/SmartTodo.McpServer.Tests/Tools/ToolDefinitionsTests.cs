using FluentAssertions;
using SmartTodo.McpServer.Tools;
using Xunit;
namespace SmartTodo.McpServer.Tests.Tools;

public class ToolDefinitionsTests
{
    [Fact]
    public void GetAllTools_ShouldReturn16Tools()
    {
        // Act
        var tools = ToolDefinitions.GetAllTools();

        // Assert
        tools.Should().HaveCount(16);
    }

    [Fact]
    public void GetAllTools_ShouldContainCreateTodoTool()
    {
        // Act
        var tools = ToolDefinitions.GetAllTools();

        // Assert
        var createTool = tools.FirstOrDefault(t => t.Name == "create_todo");
        createTool.Should().NotBeNull();
        createTool!.Description.Should().Contain("Create a new todo item");
        createTool.InputSchema.Properties.Should().ContainKey("title");
        createTool.InputSchema.Properties.Should().ContainKey("description");
        createTool.InputSchema.Properties.Should().ContainKey("tags");
        createTool.InputSchema.Required.Should().Contain("title");
    }

    [Fact]
    public void GetAllTools_ShouldContainGetTodoTool()
    {
        // Act
        var tools = ToolDefinitions.GetAllTools();

        // Assert
        var getTool = tools.FirstOrDefault(t => t.Name == "get_todo");
        getTool.Should().NotBeNull();
        getTool!.Description.Should().Be("Retrieve a specific todo item by ID");
        getTool.InputSchema.Properties.Should().ContainKey("id");
        getTool.InputSchema.Required.Should().Contain("id");
    }

    [Fact]
    public void GetAllTools_ShouldContainListTodosTool()
    {
        // Act
        var tools = ToolDefinitions.GetAllTools();

        // Assert
        var listTool = tools.FirstOrDefault(t => t.Name == "list_todos");
        listTool.Should().NotBeNull();
        listTool!.Description.Should().Contain("Get all todos");
        listTool.InputSchema.Properties.Should().ContainKey("status");
        listTool.InputSchema.Properties.Should().ContainKey("isCompleted");
        listTool.InputSchema.Properties.Should().ContainKey("tag");
    }

    [Fact]
    public void GetAllTools_ShouldContainUpdateTodoTool()
    {
        // Act
        var tools = ToolDefinitions.GetAllTools();

        // Assert
        var updateTool = tools.FirstOrDefault(t => t.Name == "update_todo");
        updateTool.Should().NotBeNull();
        updateTool!.Description.Should().Contain("Update an existing todo item");
        updateTool.InputSchema.Properties.Should().ContainKey("tags");
        updateTool.InputSchema.Required.Should().Contain("id");
    }

    [Fact]
    public void GetAllTools_ShouldContainDeleteTodoTool()
    {
        // Act
        var tools = ToolDefinitions.GetAllTools();

        // Assert
        var deleteTool = tools.FirstOrDefault(t => t.Name == "delete_todo");
        deleteTool.Should().NotBeNull();
        deleteTool!.Description.Should().Be("Delete a todo item by ID");
    }

    [Fact]
    public void GetAllTools_ShouldContainStartTodoTool()
    {
        // Act
        var tools = ToolDefinitions.GetAllTools();

        // Assert
        var startTool = tools.FirstOrDefault(t => t.Name == "start_todo");
        startTool.Should().NotBeNull();
        startTool!.Description.Should().Contain("Mark a todo as started");
    }

    [Fact]
    public void GetAllTools_ShouldContainCompleteTodoTool()
    {
        // Act
        var tools = ToolDefinitions.GetAllTools();

        // Assert
        var completeTool = tools.FirstOrDefault(t => t.Name == "complete_todo");
        completeTool.Should().NotBeNull();
        completeTool!.Description.Should().Be("Mark a todo as completed");
    }

    [Fact]
    public void GetAllTools_ShouldContainPauseTodoTool()
    {
        // Act
        var tools = ToolDefinitions.GetAllTools();

        // Assert
        var pauseTool = tools.FirstOrDefault(t => t.Name == "pause_todo");
        pauseTool.Should().NotBeNull();
        pauseTool!.Description.Should().Be("Put a todo on hold");
    }

    [Fact]
    public void GetAllTools_ShouldContainResumeTodoTool()
    {
        // Act
        var tools = ToolDefinitions.GetAllTools();

        // Assert
        var resumeTool = tools.FirstOrDefault(t => t.Name == "resume_todo");
        resumeTool.Should().NotBeNull();
        resumeTool!.Description.Should().Be("Resume a paused todo");
    }

    [Fact]
    public void GetAllTools_ShouldContainCancelTodoTool()
    {
        // Act
        var tools = ToolDefinitions.GetAllTools();

        // Assert
        var cancelTool = tools.FirstOrDefault(t => t.Name == "cancel_todo");
        cancelTool.Should().NotBeNull();
        cancelTool!.Description.Should().Be("Cancel a todo item");
    }

    [Fact]
    public void GetAllTools_ShouldContainAddTodoTagTool()
    {
        // Act
        var tools = ToolDefinitions.GetAllTools();

        // Assert
        var tool = tools.FirstOrDefault(t => t.Name == "add_todo_tag");
        tool.Should().NotBeNull();
        tool!.Description.Should().Contain("Add a tag");
        tool.InputSchema.Properties.Should().ContainKey("id");
        tool.InputSchema.Properties.Should().ContainKey("tag");
        tool.InputSchema.Required.Should().Contain("id");
        tool.InputSchema.Required.Should().Contain("tag");
    }

    [Fact]
    public void GetAllTools_ShouldContainRemoveTodoTagTool()
    {
        // Act
        var tools = ToolDefinitions.GetAllTools();

        // Assert
        var tool = tools.FirstOrDefault(t => t.Name == "remove_todo_tag");
        tool.Should().NotBeNull();
        tool!.Description.Should().Contain("Remove a tag");
        tool.InputSchema.Required.Should().Contain("id");
        tool.InputSchema.Required.Should().Contain("tag");
    }

    [Fact]
    public void GetAllTools_ShouldContainListTagsTool()
    {
        // Act
        var tools = ToolDefinitions.GetAllTools();

        // Assert
        var tool = tools.FirstOrDefault(t => t.Name == "list_tags");
        tool.Should().NotBeNull();
        tool!.Description.Should().Contain("List all available tags");
    }

    [Fact]
    public void GetAllTools_ShouldContainAddTodoDependencyTool()
    {
        // Act
        var tools = ToolDefinitions.GetAllTools();

        // Assert
        var tool = tools.FirstOrDefault(t => t.Name == "add_todo_dependency");
        tool.Should().NotBeNull();
        tool!.Description.Should().Contain("dependency");
        tool.InputSchema.Properties.Should().ContainKey("id");
        tool.InputSchema.Properties.Should().ContainKey("dependencyId");
        tool.InputSchema.Required.Should().Contain("id");
        tool.InputSchema.Required.Should().Contain("dependencyId");
    }

    [Fact]
    public void GetAllTools_ShouldContainRemoveTodoDependencyTool()
    {
        // Act
        var tools = ToolDefinitions.GetAllTools();

        // Assert
        var tool = tools.FirstOrDefault(t => t.Name == "remove_todo_dependency");
        tool.Should().NotBeNull();
        tool!.Description.Should().Contain("Remove a dependency");
        tool.InputSchema.Required.Should().Contain("id");
        tool.InputSchema.Required.Should().Contain("dependencyId");
    }

    [Theory]
    [InlineData("create_todo")]
    [InlineData("get_todo")]
    [InlineData("list_todos")]
    [InlineData("update_todo")]
    [InlineData("delete_todo")]
    [InlineData("start_todo")]
    [InlineData("complete_todo")]
    [InlineData("pause_todo")]
    [InlineData("resume_todo")]
    [InlineData("cancel_todo")]
    [InlineData("set_todo_priority")]
    [InlineData("add_todo_tag")]
    [InlineData("remove_todo_tag")]
    [InlineData("list_tags")]
    [InlineData("add_todo_dependency")]
    [InlineData("remove_todo_dependency")]
    public void GetAllTools_AllToolsShouldHaveInputSchema(string toolName)
    {
        // Act
        var tools = ToolDefinitions.GetAllTools();

        // Assert
        var tool = tools.FirstOrDefault(t => t.Name == toolName);
        tool.Should().NotBeNull();
        tool!.InputSchema.Should().NotBeNull();
        tool.InputSchema.Type.Should().Be("object");
    }

    [Fact]
    public void GetAllTools_StatusEnumShouldHaveAllValues()
    {
        // Act
        var tools = ToolDefinitions.GetAllTools();

        // Assert
        var listTool = tools.First(t => t.Name == "list_todos");
        var statusProperty = listTool.InputSchema.Properties["status"];
        statusProperty.Enum.Should().NotBeNull();
        statusProperty.Enum.Should().HaveCount(5);
        statusProperty.Enum.Should().Contain(new[] {
            "NotStarted", "InProgress", "OnHold", "Completed", "Cancelled"
        });
    }
}
