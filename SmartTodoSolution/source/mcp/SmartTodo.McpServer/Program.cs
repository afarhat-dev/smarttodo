using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using SmartTodo.Application.Interfaces;
using SmartTodo.Application.Services;
using SmartTodo.Infrastructure.Repositories;
using SmartTodo.McpServer.Configuration;
using SmartTodo.McpServer.Prompts;
using SmartTodo.McpServer.Resources;
using SmartTodo.McpServer.Server;
using SmartTodo.McpServer.Tools;

// Configure Serilog BEFORE building the host
// Logs go to stderr (for MCP compliance), files, and optionally Seq

var loggerConfig = new LoggerConfiguration()
    .WriteTo.Console(
        standardErrorStream: true, // Critical: Write to stderr, NOT stdout
        restrictedToMinimumLevel: LogEventLevel.Information
    )
    .WriteTo.File(
        path: "logs/mcp-server-.log",
        rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: LogEventLevel.Information
    )
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Extensions.Hosting", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning);

// Try to add Seq sink if available (optional)
try
{
    loggerConfig.WriteTo.Seq(
        serverUrl: "http://localhost:5341/",
        restrictedToMinimumLevel: LogEventLevel.Information,
        apiKey: null,
        compact: true
    );
}
catch (Exception ex)
{
    // Seq not available, continue without it
    Console.Error.WriteLine($"Warning: Could not configure Seq logging: {ex.Message}");
}

Log.Logger = loggerConfig.CreateLogger();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    })
    .UseSerilog() // Use Serilog for all logging - THIS IS CRITICAL
    .ConfigureServices((context, services) =>
    {
        // Configure MCP Server Settings
        var mcpSettings = new McpServerSettings();
        context.Configuration.GetSection("McpServer").Bind(mcpSettings);
        services.AddSingleton(mcpSettings);
        // Register Application Layer Services
        // Both must be singleton since handlers are singleton
        services.AddSingleton<ITodoRepository, InMemoryTodoRepository>();
        services.AddSingleton<ITodoService, TodoService>();
        // Register MCP Server Components
        services.AddSingleton<TodoToolHandler>();
        services.AddSingleton<TodoResourceHandler>();
        services.AddSingleton<TodoPromptHandler>();
        services.AddSingleton<McpServerHost>();
        // Add Background Service
        services.AddHostedService<McpServerBackgroundService>();
    })
    .Build();
try{ 
    await host.RunAsync();
}
finally
{    Log.CloseAndFlush(); // Ensure all logs are flushed before exit
}

// Background service to run the MCP server
public class McpServerBackgroundService : BackgroundService
{
    private readonly McpServerHost _mcpServerHost;
    private readonly ILogger<McpServerBackgroundService> _logger;

    public McpServerBackgroundService(
        McpServerHost mcpServerHost,
        ILogger<McpServerBackgroundService> logger)
    {
        _mcpServerHost = mcpServerHost;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MCP Server Background Service starting");

        try
        {
            await _mcpServerHost.StartAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "MCP Server failed");
            throw;
        }
    }
}
