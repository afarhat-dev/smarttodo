using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SmartTodo.Application.Interfaces;
using SmartTodo.Application.Services;
using SmartTodo.Infrastructure.Repositories;
using SmartTodo.McpServer.Configuration;
using SmartTodo.McpServer.Server;
using SmartTodo.McpServer.Tools;
using SmartTodo.McpServer.Resources;
using SmartTodo.McpServer.Prompts;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        // Configure MCP Server Settings
        var mcpSettings = new McpServerSettings();
        context.Configuration.GetSection("McpServer").Bind(mcpSettings);
        services.AddSingleton(mcpSettings);

        // Register Application Layer Services
        services.AddSingleton<ITodoRepository, InMemoryTodoRepository>();
        services.AddScoped<ITodoService, TodoService>();

        // Register MCP Server Components
        services.AddSingleton<TodoToolHandler>();
        services.AddSingleton<TodoResourceHandler>();
        services.AddSingleton<TodoPromptHandler>();
        services.AddSingleton<McpServerHost>();

        // Add Background Service
        services.AddHostedService<McpServerBackgroundService>();
    })
    .ConfigureLogging((context, logging) =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Information);
    })
    .Build();

await host.RunAsync();

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
