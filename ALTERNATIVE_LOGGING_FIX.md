# Alternative Logging Configuration

If you get compilation errors with the `standardOutput` parameter, use this alternative:

## Option 1: Remove Console Sink (Simplest)
Since MCP protocol requires that structured messages go to stdout and logs should NOT go to stdout, the safest approach is to only log to files and Seq:

```csharp
var loggerConfig = new LoggerConfiguration()
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
    loggerConfig.WriteTo.Seq(serverUrl: "http://localhost:5341/");
}
catch (Exception ex)
{
    // Seq not available, continue without it
    System.IO.File.AppendAllText("logs/startup.log",
        $"Warning: Could not configure Seq logging: {ex.Message}\n");
}

Log.Logger = loggerConfig.CreateLogger();
```

## Option 2: Use TextWriter for stderr
If you need console output for debugging, use TextWriter:

```csharp
var loggerConfig = new LoggerConfiguration()
    .WriteTo.Console(
        System.Console.Error  // This overload takes TextWriter directly
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
```

## Which Option to Use?

- **Use Option 1** for production MCP server (no console output)
- **Use Option 2** if you need console logs for debugging

Both options ensure that logs don't corrupt the JSON-RPC communication on stdout.
