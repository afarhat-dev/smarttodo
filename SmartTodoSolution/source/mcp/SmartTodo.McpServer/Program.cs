using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using SmartTodo.Application.Interfaces;
using SmartTodo.Application.Services;
using SmartTodo.Infrastructure.Persistence;
using SmartTodo.Infrastructure.Repositories;
using SmartTodo.McpServer.Configuration;
using SmartTodo.McpServer.Prompts;
using SmartTodo.McpServer.Resources;
using SmartTodo.McpServer.Server;
using SmartTodo.McpServer.Tools;

// Configure Serilog BEFORE building the host
// Logs only to files and Seq (NOT to console to avoid stdout/stderr issues)
// MCP protocol requires stdout for JSON-RPC messages only

// Determine the log directory - use base directory to ensure logs are always in the right place
var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
System.IO.Directory.CreateDirectory(logDirectory);
var logPath = Path.Combine(logDirectory, "mcp-server-.log");

var loggerConfig = new LoggerConfiguration()
    .WriteTo.File(
        path: logPath,
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
        serverUrl: "http://localhost:5341/"
    );
}
catch (Exception ex)
{
    // Seq not available, continue without it
    // Log startup warning to file instead of console
    try
    {
        var startupLogPath = Path.Combine(logDirectory, "startup.log");
        System.IO.File.AppendAllText(startupLogPath,
            $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - Warning: Could not configure Seq logging: {ex.Message}\n");
    }
    catch
    {
        // If we can't log the warning, just continue silently
    }
}

Log.Logger = loggerConfig.CreateLogger();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        // Clear default configuration sources to have more control
        config.Sources.Clear();

        // Strategy: Try multiple locations for appsettings.json
        // 1. AppContext.BaseDirectory (bin output directory - where DLL runs)
        // 2. Current working directory (useful when run with dotnet run)
        // 3. Project root (fallback for development)

        var baseDir = AppContext.BaseDirectory;
        var workingDir = Environment.CurrentDirectory;

        // Check where appsettings.json exists
        var appSettingsInBase = Path.Combine(baseDir, "appsettings.json");
        var appSettingsInWorking = Path.Combine(workingDir, "appsettings.json");

        string? configPath = null;
        if (File.Exists(appSettingsInBase))
        {
            configPath = baseDir;
        }
        else if (File.Exists(appSettingsInWorking))
        {
            configPath = workingDir;
        }

        // Set base path and add configuration
        if (configPath != null)
        {
            config.SetBasePath(configPath);
        }
        else
        {
            config.SetBasePath(baseDir); // Default to base directory
        }

        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        config.AddEnvironmentVariables();

        // Log the configuration path for debugging
        try
        {
            var logDir = Path.Combine(configPath ?? baseDir, "logs");
            System.IO.Directory.CreateDirectory(logDir);
            var logFile = Path.Combine(logDir, "startup.log");

            System.IO.File.AppendAllText(logFile,
                $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - Configuration base: {configPath ?? baseDir}\n");
            System.IO.File.AppendAllText(logFile,
                $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - AppContext.BaseDirectory: {baseDir}\n");
            System.IO.File.AppendAllText(logFile,
                $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - Working directory: {workingDir}\n");
            System.IO.File.AppendAllText(logFile,
                $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - Config found at: {(File.Exists(Path.Combine(configPath ?? baseDir, "appsettings.json")) ? "YES" : "NO")}\n");
        }
        catch
        {
            // Ignore logging errors
        }
    })
    .ConfigureLogging(logging =>
    {
        // CRITICAL: Clear all default logging providers (especially console)
        // This prevents ANY output to stdout/stderr from the host builder
        logging.ClearProviders();
    })
    .UseSerilog() // Use Serilog for all logging - file-based only
    .ConfigureServices((context, services) =>
    {
        // Configure MCP Server Settings
        var mcpSettings = new McpServerSettings();
        context.Configuration.GetSection("McpServer").Bind(mcpSettings);
        services.AddSingleton(mcpSettings);

        // Configure Database - check if UsePostgreSQL is enabled
        var usePostgreSql = context.Configuration.GetValue<bool>("Database:UsePostgreSQL");

        if (usePostgreSql)
        {
            // PostgreSQL configuration
            var connectionString = context.Configuration.GetConnectionString("PostgreSQL");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("PostgreSQL connection string is not configured. Please add 'ConnectionStrings:PostgreSQL' to appsettings.json");
            }

            // Register DbContext as singleton with pooling for better performance
            services.AddDbContextPool<TodoDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Register as transient to get new instances with each DbContext
            services.AddTransient<ITodoRepository, PostgreSqlTodoRepository>();
            services.AddTransient<ITodoService, TodoService>();

            // Register handlers as transient too
            services.AddTransient<TodoToolHandler>();
            services.AddTransient<TodoResourceHandler>();

            Log.Information("Using PostgreSQL database");
        }
        else
        {
            // In-Memory configuration
            services.AddSingleton<ITodoRepository, InMemoryTodoRepository>();
            services.AddSingleton<ITodoService, TodoService>();

            // Register handlers as singletons
            services.AddSingleton<TodoToolHandler>();
            services.AddSingleton<TodoResourceHandler>();

            Log.Information("Using In-Memory database");
        }

        // These are always singletons
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
