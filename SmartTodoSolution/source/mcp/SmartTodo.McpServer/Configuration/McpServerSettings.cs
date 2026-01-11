namespace SmartTodo.McpServer.Configuration;

public class McpServerSettings
{
    public string Name { get; set; } = "SmartTodo MCP Server";
    public string Version { get; set; } = "1.0.0";
    public ServerCapabilities Capabilities { get; set; } = new();
    public LoggingSettings Logging { get; set; } = new();
}

public class ServerCapabilities
{
    public bool Tools { get; set; } = true;
    public bool Resources { get; set; } = true;
    public bool Prompts { get; set; } = true;
}

public class LoggingSettings
{
    public string Level { get; set; } = "Information";
    public string? OutputPath { get; set; }
}
