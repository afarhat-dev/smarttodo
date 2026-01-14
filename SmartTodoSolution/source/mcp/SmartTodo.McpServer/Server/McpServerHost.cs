using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartTodo.McpServer.Configuration;
using SmartTodo.McpServer.Protocol;
using SmartTodo.McpServer.Tools;
using SmartTodo.McpServer.Resources;
using SmartTodo.McpServer.Prompts;

namespace SmartTodo.McpServer.Server;

public class McpServerHost
{
    private readonly McpServerSettings _settings;
    private readonly IServiceProvider _serviceProvider;
    private readonly TodoPromptHandler _promptHandler;
    private readonly ILogger<McpServerHost> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public McpServerHost(
        McpServerSettings settings,
        IServiceProvider serviceProvider,
        TodoPromptHandler promptHandler,
        ILogger<McpServerHost> logger)
    {
        _settings = settings;
        _serviceProvider = serviceProvider;
        _promptHandler = promptHandler;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false, // CRITICAL: Must be false for stdio JSON-RPC (single line per message)
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull // Omit null fields (JSON-RPC requires either result OR error, not both)
        };
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting {ServerName} v{Version}", _settings.Name, _settings.Version);

        try
        {
            using var stdin = Console.OpenStandardInput();
            using var stdout = Console.OpenStandardOutput();
            using var reader = new StreamReader(stdin);
            using var writer = new StreamWriter(stdout) { AutoFlush = true };

            while (!cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                if (string.IsNullOrEmpty(line))
                    continue;

                _logger.LogDebug("Received message: {Message}", line);

                try
                {
                    // Parse as a generic JSON document first to check if it's a notification
                    using var doc = JsonDocument.Parse(line);
                    var root = doc.RootElement;

                    // Check if this is a notification (no 'id' field)
                    if (!root.TryGetProperty("id", out _))
                    {
                        // This is a notification - handle it without sending a response
                        var notification = JsonSerializer.Deserialize<McpNotification>(line, _jsonOptions);
                        if (notification != null)
                        {
                            HandleNotification(notification);
                        }
                        continue;
                    }

                    // This is a request - deserialize and handle it
                    var request = JsonSerializer.Deserialize<McpRequest>(line, _jsonOptions);
                    if (request == null)
                    {
                        _logger.LogWarning("Failed to deserialize request");
                        continue;
                    }

                    var response = await HandleRequestAsync(request, cancellationToken);
                    var responseJson = JsonSerializer.Serialize(response, _jsonOptions);

                    await writer.WriteLineAsync(responseJson);
                    _logger.LogDebug("Sent response: {Response}", responseJson);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");

                    // Only send error response for requests (not notifications)
                    var errorResponse = new McpResponse
                    {
                        Id = null,
                        Error = new McpError
                        {
                            Code = -32603,
                            Message = "Internal error",
                            Data = ex.Message
                        }
                    };
                    await writer.WriteLineAsync(JsonSerializer.Serialize(errorResponse, _jsonOptions));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fatal error in MCP server");
            throw;
        }
    }

    private async Task<McpResponse> HandleRequestAsync(McpRequest request, CancellationToken cancellationToken)
    {
        try
        {
            object? result = request.Method switch
            {
                "initialize" => HandleInitialize(request.Params),
                "tools/list" => HandleToolsList(),
                "tools/call" => await HandleToolsCallAsync(request.Params, cancellationToken),
                "resources/list" => HandleResourcesList(),
                "resources/read" => await HandleResourcesReadAsync(request.Params, cancellationToken),
                "prompts/list" => HandlePromptsList(),
                "prompts/get" => HandlePromptsGet(request.Params),
                _ => throw new InvalidOperationException($"Unknown method: {request.Method}")
            };

            return new McpResponse
            {
                Id = request.Id,
                Result = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling method {Method}", request.Method);
            return new McpResponse
            {
                Id = request.Id,
                Error = new McpError
                {
                    Code = -32603,
                    Message = ex.Message,
                    Data = ex.StackTrace
                }
            };
        }
    }

    private object HandleInitialize(object? parameters)
    {
        _logger.LogInformation("Client initializing connection");
        return new
        {
            protocolVersion = "2024-11-05",
            capabilities = new
            {
                tools = _settings.Capabilities.Tools ? new { } : null,
                resources = _settings.Capabilities.Resources ? new { } : null,
                prompts = _settings.Capabilities.Prompts ? new { } : null
            },
            serverInfo = new
            {
                name = _settings.Name,
                version = _settings.Version
            }
        };
    }

    private object HandleToolsList()
    {
        _logger.LogDebug("Listing available tools");
        return new
        {
            tools = ToolDefinitions.GetAllTools()
        };
    }

    private async Task<object> HandleToolsCallAsync(object? parameters, CancellationToken cancellationToken)
    {
        if (parameters == null)
            throw new ArgumentException("Parameters are required for tools/call");

        var paramsJson = JsonSerializer.SerializeToElement(parameters, _jsonOptions);
        var toolName = paramsJson.GetProperty("name").GetString()
            ?? throw new ArgumentException("Tool name is required");

        _logger.LogInformation("Calling tool: {ToolName}", toolName);

        JsonElement? toolParams = null;
        if (paramsJson.TryGetProperty("arguments", out var args))
        {
            toolParams = args;
        }

        // Resolve handler from service provider to support transient lifetime
        var toolHandler = _serviceProvider.GetRequiredService<TodoToolHandler>();
        var result = await toolHandler.HandleToolCallAsync(toolName, toolParams);

        return new
        {
            content = new[]
            {
                new
                {
                    type = "text",
                    text = JsonSerializer.Serialize(result, _jsonOptions)
                }
            }
        };
    }

    private object HandleResourcesList()
    {
        _logger.LogDebug("Listing available resources");
        return new
        {
            resources = TodoResourceHandler.GetAllResources()
        };
    }

    private async Task<object> HandleResourcesReadAsync(object? parameters, CancellationToken cancellationToken)
    {
        if (parameters == null)
            throw new ArgumentException("Parameters are required for resources/read");

        var paramsJson = JsonSerializer.SerializeToElement(parameters, _jsonOptions);
        var uri = paramsJson.GetProperty("uri").GetString()
            ?? throw new ArgumentException("URI is required");

        _logger.LogInformation("Reading resource: {Uri}", uri);

        // Resolve handler from service provider to support transient lifetime
        var resourceHandler = _serviceProvider.GetRequiredService<TodoResourceHandler>();
        var result = await resourceHandler.GetResourceAsync(uri);

        return new
        {
            contents = new[]
            {
                result
            }
        };
    }

    private object HandlePromptsList()
    {
        _logger.LogDebug("Listing available prompts");
        return new
        {
            prompts = TodoPromptHandler.GetAllPrompts()
        };
    }

    private object HandlePromptsGet(object? parameters)
    {
        if (parameters == null)
            throw new ArgumentException("Parameters are required for prompts/get");

        var paramsJson = JsonSerializer.SerializeToElement(parameters, _jsonOptions);
        var promptName = paramsJson.GetProperty("name").GetString()
            ?? throw new ArgumentException("Prompt name is required");

        _logger.LogInformation("Getting prompt: {PromptName}", promptName);

        JsonElement? promptArgs = null;
        if (paramsJson.TryGetProperty("arguments", out var args))
        {
            promptArgs = args;
        }

        return _promptHandler.GetPrompt(promptName, promptArgs);
    }

    private void HandleNotification(McpNotification notification)
    {
        // Handle notifications (these don't require responses)
        switch (notification.Method)
        {
            case "notifications/initialized":
                _logger.LogInformation("Client initialization complete");
                break;
            case "notifications/cancelled":
                _logger.LogInformation("Client cancelled operation");
                break;
            default:
                _logger.LogDebug("Received notification: {Method}", notification.Method);
                break;
        }
    }
}
