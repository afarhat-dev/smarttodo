using System.Text.Json;
using SmartTodo.McpServer.Protocol;

namespace SmartTodo.McpServer.Prompts;

public class TodoPromptHandler
{
    public static List<McpPrompt> GetAllPrompts()
    {
        return new List<McpPrompt>
        {
            new McpPrompt
            {
                Name = "manage-todos",
                Description = "Help me manage my todo list efficiently",
                Arguments = null
            },
            new McpPrompt
            {
                Name = "plan-day",
                Description = "Help me plan my day based on my todos",
                Arguments = new List<PromptArgument>
                {
                    new PromptArgument
                    {
                        Name = "date",
                        Description = "The date to plan for (defaults to today)",
                        Required = false
                    }
                }
            },
            new McpPrompt
            {
                Name = "review-progress",
                Description = "Review my todo completion progress",
                Arguments = new List<PromptArgument>
                {
                    new PromptArgument
                    {
                        Name = "timeframe",
                        Description = "The timeframe to review (day, week, month)",
                        Required = false
                    }
                }
            }
        };
    }

    public object GetPrompt(string promptName, JsonElement? arguments)
    {
        return promptName switch
        {
            "manage-todos" => GetManageTodosPrompt(),
            "plan-day" => GetPlanDayPrompt(arguments),
            "review-progress" => GetReviewProgressPrompt(arguments),
            _ => throw new InvalidOperationException($"Unknown prompt: {promptName}")
        };
    }

    private static object GetManageTodosPrompt()
    {
        var promptText = @"I'll help you manage your todo list efficiently. Here are some things I can do:

1. **Create todos**: Add new tasks with titles and descriptions
2. **View todos**: See all your tasks or filter by status
3. **Update todos**: Modify task details or change their status
4. **Track progress**: Start, pause, resume, or complete tasks
5. **Organize**: Filter todos by date ranges, status, or completion state

Available tools:
- create_todo: Create a new todo item
- list_todos: View all todos with optional filtering
- start_todo: Begin working on a task
- complete_todo: Mark a task as done
- pause_todo: Put a task on hold
- update_todo: Modify task details
- delete_todo: Remove a task

What would you like to do with your todos?";

        return new
        {
            description = "Todo management assistance",
            messages = new[]
            {
                new
                {
                    role = "assistant",
                    content = new
                    {
                        type = "text",
                        text = promptText
                    }
                }
            }
        };
    }

    private static object GetPlanDayPrompt(JsonElement? arguments)
    {
        var dateString = "today";
        if (arguments.HasValue && arguments.Value.TryGetProperty("date", out var dateProp))
        {
            dateString = dateProp.GetString() ?? "today";
        }

        var promptText = $@"Let me help you plan your day for {dateString}. I'll:

1. **Review your current todos** - Check what tasks you have
2. **Identify priorities** - Help you determine what's most important
3. **Suggest a schedule** - Organize tasks in a logical order
4. **Set realistic goals** - Ensure you don't overcommit

To get started, I'll need to see your current todos. I'll use the list_todos tool to check what tasks you have, focusing on:
- Tasks not yet started
- Tasks currently in progress
- Tasks that might be urgent or important

Would you like me to proceed with reviewing your todos and creating a plan?";

        return new
        {
            description = $"Day planning assistance for {dateString}",
            messages = new[]
            {
                new
                {
                    role = "assistant",
                    content = new
                    {
                        type = "text",
                        text = promptText
                    }
                }
            }
        };
    }

    private static object GetReviewProgressPrompt(JsonElement? arguments)
    {
        var timeframe = "today";
        if (arguments.HasValue && arguments.Value.TryGetProperty("timeframe", out var timeframeProp))
        {
            timeframe = timeframeProp.GetString() ?? "today";
        }

        var promptText = $@"Let me review your todo completion progress for {timeframe}. I'll analyze:

1. **Completion rate** - How many tasks you've completed
2. **Status distribution** - Breakdown of task statuses
3. **Productivity trends** - Insights into your productivity
4. **Recommendations** - Suggestions for improvement

I'll use the todo://stats resource and filtering to gather this information.

Key metrics I'll look at:
- Total tasks created vs completed
- Tasks started but not finished
- Tasks on hold or cancelled
- Average time to completion

Would you like me to generate your progress report?";

        return new
        {
            description = $"Progress review for {timeframe}",
            messages = new[]
            {
                new
                {
                    role = "assistant",
                    content = new
                    {
                        type = "text",
                        text = promptText
                    }
                }
            }
        };
    }
}
