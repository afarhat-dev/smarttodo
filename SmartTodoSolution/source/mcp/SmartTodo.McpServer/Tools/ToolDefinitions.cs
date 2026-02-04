using SmartTodo.McpServer.Protocol;

namespace SmartTodo.McpServer.Tools;

public static class ToolDefinitions
{
    public static List<McpTool> GetAllTools()
    {
        return new List<McpTool>
        {
            CreateTodoTool(),
            GetTodoTool(),
            ListTodosTool(),
            UpdateTodoTool(),
            DeleteTodoTool(),
            StartTodoTool(),
            CompleteTodoTool(),
            PauseTodoTool(),
            ResumeTodoTool(),
            CancelTodoTool(),
            SetTodoPriorityTool(),
            AddTodoTagTool(),
            RemoveTodoTagTool(),
            ListTagsTool(),
            AddTodoDependencyTool(),
            RemoveTodoDependencyTool()
        };
    }

    private static McpTool CreateTodoTool()
    {
        return new McpTool
        {
            Name = "create_todo",
            Description = "Create a new todo item with optional priority and tags",
            InputSchema = new ToolInputSchema
            {
                Properties = new Dictionary<string, ToolProperty>
                {
                    ["title"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "The title of the todo item"
                    },
                    ["description"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "Optional description of the todo item"
                    },
                    ["priority"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "Priority level for the todo (defaults to Medium if not specified)",
                        Enum = new List<string> { "Low", "Medium", "High", "Critical" }
                    },
                    ["tags"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "Comma-separated list of tags to assign to the todo"
                    }
                },
                Required = new List<string> { "title" }
            }
        };
    }

    private static McpTool GetTodoTool()
    {
        return new McpTool
        {
            Name = "get_todo",
            Description = "Retrieve a specific todo item by ID",
            InputSchema = new ToolInputSchema
            {
                Properties = new Dictionary<string, ToolProperty>
                {
                    ["id"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "The ID of the todo item (GUID format)",
                        Format = "uuid"
                    }
                },
                Required = new List<string> { "id" }
            }
        };
    }

    private static McpTool ListTodosTool()
    {
        return new McpTool
        {
            Name = "list_todos",
            Description = "Get all todos with optional filtering by status, priority, completion state, tags, and date ranges",
            InputSchema = new ToolInputSchema
            {
                Properties = new Dictionary<string, ToolProperty>
                {
                    ["status"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "Filter by status",
                        Enum = new List<string> { "NotStarted", "InProgress", "OnHold", "Completed", "Cancelled" }
                    },
                    ["priority"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "Filter by priority level",
                        Enum = new List<string> { "Low", "Medium", "High", "Critical" }
                    },
                    ["isCompleted"] = new ToolProperty
                    {
                        Type = "boolean",
                        Description = "Filter by completion state"
                    },
                    ["tag"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "Filter by tag name"
                    },
                    ["createdFrom"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "Filter by created date from (ISO 8601 format)",
                        Format = "date-time"
                    },
                    ["createdTo"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "Filter by created date to (ISO 8601 format)",
                        Format = "date-time"
                    },
                    ["updatedFrom"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "Filter by updated date from (ISO 8601 format)",
                        Format = "date-time"
                    },
                    ["updatedTo"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "Filter by updated date to (ISO 8601 format)",
                        Format = "date-time"
                    }
                }
            }
        };
    }

    private static McpTool UpdateTodoTool()
    {
        return new McpTool
        {
            Name = "update_todo",
            Description = "Update an existing todo item (title, description, status, priority, tags, or completion state)",
            InputSchema = new ToolInputSchema
            {
                Properties = new Dictionary<string, ToolProperty>
                {
                    ["id"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "The ID of the todo item to update",
                        Format = "uuid"
                    },
                    ["title"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "New title for the todo item"
                    },
                    ["description"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "New description for the todo item"
                    },
                    ["status"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "New status for the todo item",
                        Enum = new List<string> { "NotStarted", "InProgress", "OnHold", "Completed", "Cancelled" }
                    },
                    ["priority"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "New priority level for the todo item",
                        Enum = new List<string> { "Low", "Medium", "High", "Critical" }
                    },
                    ["isCompleted"] = new ToolProperty
                    {
                        Type = "boolean",
                        Description = "Mark as completed or incomplete"
                    },
                    ["tags"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "Comma-separated list of tags (replaces existing tags)"
                    }
                },
                Required = new List<string> { "id" }
            }
        };
    }

    private static McpTool DeleteTodoTool()
    {
        return new McpTool
        {
            Name = "delete_todo",
            Description = "Delete a todo item by ID",
            InputSchema = new ToolInputSchema
            {
                Properties = new Dictionary<string, ToolProperty>
                {
                    ["id"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "The ID of the todo item to delete",
                        Format = "uuid"
                    }
                },
                Required = new List<string> { "id" }
            }
        };
    }

    private static McpTool StartTodoTool()
    {
        return new McpTool
        {
            Name = "start_todo",
            Description = "Mark a todo as started (sets StartDate and status to InProgress)",
            InputSchema = new ToolInputSchema
            {
                Properties = new Dictionary<string, ToolProperty>
                {
                    ["id"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "The ID of the todo item to start",
                        Format = "uuid"
                    }
                },
                Required = new List<string> { "id" }
            }
        };
    }

    private static McpTool CompleteTodoTool()
    {
        return new McpTool
        {
            Name = "complete_todo",
            Description = "Mark a todo as completed",
            InputSchema = new ToolInputSchema
            {
                Properties = new Dictionary<string, ToolProperty>
                {
                    ["id"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "The ID of the todo item to complete",
                        Format = "uuid"
                    }
                },
                Required = new List<string> { "id" }
            }
        };
    }

    private static McpTool PauseTodoTool()
    {
        return new McpTool
        {
            Name = "pause_todo",
            Description = "Put a todo on hold",
            InputSchema = new ToolInputSchema
            {
                Properties = new Dictionary<string, ToolProperty>
                {
                    ["id"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "The ID of the todo item to pause",
                        Format = "uuid"
                    }
                },
                Required = new List<string> { "id" }
            }
        };
    }

    private static McpTool ResumeTodoTool()
    {
        return new McpTool
        {
            Name = "resume_todo",
            Description = "Resume a paused todo",
            InputSchema = new ToolInputSchema
            {
                Properties = new Dictionary<string, ToolProperty>
                {
                    ["id"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "The ID of the todo item to resume",
                        Format = "uuid"
                    }
                },
                Required = new List<string> { "id" }
            }
        };
    }

    private static McpTool CancelTodoTool()
    {
        return new McpTool
        {
            Name = "cancel_todo",
            Description = "Cancel a todo item",
            InputSchema = new ToolInputSchema
            {
                Properties = new Dictionary<string, ToolProperty>
                {
                    ["id"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "The ID of the todo item to cancel",
                        Format = "uuid"
                    }
                },
                Required = new List<string> { "id" }
            }
        };
    }

    private static McpTool SetTodoPriorityTool()
    {
        return new McpTool
        {
            Name = "set_todo_priority",
            Description = "Set the priority level of a todo item (Low, Medium, High, or Critical)",
            InputSchema = new ToolInputSchema
            {
                Properties = new Dictionary<string, ToolProperty>
                {
                    ["id"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "The ID of the todo item",
                        Format = "uuid"
                    },
                    ["priority"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "The priority level to set",
                        Enum = new List<string> { "Low", "Medium", "High", "Critical" }
                    }
                },
                Required = new List<string> { "id", "priority" }
            }
        };
    }

    private static McpTool AddTodoTagTool()
    {
        return new McpTool
        {
            Name = "add_todo_tag",
            Description = "Add a tag to a todo item",
            InputSchema = new ToolInputSchema
            {
                Properties = new Dictionary<string, ToolProperty>
                {
                    ["id"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "The ID of the todo item",
                        Format = "uuid"
                    },
                    ["tag"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "The tag name to add"
                    }
                },
                Required = new List<string> { "id", "tag" }
            }
        };
    }

    private static McpTool RemoveTodoTagTool()
    {
        return new McpTool
        {
            Name = "remove_todo_tag",
            Description = "Remove a tag from a todo item",
            InputSchema = new ToolInputSchema
            {
                Properties = new Dictionary<string, ToolProperty>
                {
                    ["id"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "The ID of the todo item",
                        Format = "uuid"
                    },
                    ["tag"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "The tag name to remove"
                    }
                },
                Required = new List<string> { "id", "tag" }
            }
        };
    }

    private static McpTool ListTagsTool()
    {
        return new McpTool
        {
            Name = "list_tags",
            Description = "List all available tags",
            InputSchema = new ToolInputSchema
            {
                Properties = new Dictionary<string, ToolProperty>()
            }
        };
    }

    private static McpTool AddTodoDependencyTool()
    {
        return new McpTool
        {
            Name = "add_todo_dependency",
            Description = "Add a dependency to a todo item (the dependency must be completed before the todo can start)",
            InputSchema = new ToolInputSchema
            {
                Properties = new Dictionary<string, ToolProperty>
                {
                    ["id"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "The ID of the todo item",
                        Format = "uuid"
                    },
                    ["dependencyId"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "The ID of the todo item that this task depends on",
                        Format = "uuid"
                    }
                },
                Required = new List<string> { "id", "dependencyId" }
            }
        };
    }

    private static McpTool RemoveTodoDependencyTool()
    {
        return new McpTool
        {
            Name = "remove_todo_dependency",
            Description = "Remove a dependency from a todo item",
            InputSchema = new ToolInputSchema
            {
                Properties = new Dictionary<string, ToolProperty>
                {
                    ["id"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "The ID of the todo item",
                        Format = "uuid"
                    },
                    ["dependencyId"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "The ID of the dependency to remove",
                        Format = "uuid"
                    }
                },
                Required = new List<string> { "id", "dependencyId" }
            }
        };
    }
}
