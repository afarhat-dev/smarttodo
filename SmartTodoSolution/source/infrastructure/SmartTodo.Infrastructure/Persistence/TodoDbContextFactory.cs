using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SmartTodo.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for creating TodoDbContext instances during migrations.
/// This allows EF Core tools to create the DbContext without running the full application.
/// </summary>
public class TodoDbContextFactory : IDesignTimeDbContextFactory<TodoDbContext>
{
    public TodoDbContext CreateDbContext(string[] args)
    {
        // Strategy: Try to find appsettings.json in multiple locations
        // 1. Current directory (when running from Infrastructure project)
        // 2. MCP Server project directory
        // 3. StdApi project directory

        var currentDir = Directory.GetCurrentDirectory();
        var configPath = FindConfigurationPath(currentDir);

        if (string.IsNullOrEmpty(configPath))
        {
            throw new InvalidOperationException(
                "Could not find appsettings.json. Please ensure you're running from the project directory or that appsettings.json exists in the startup project.");
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(configPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("PostgreSQL")
            ?? configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "No PostgreSQL or DefaultConnection string found in appsettings.json");
        }

        var optionsBuilder = new DbContextOptionsBuilder<TodoDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new TodoDbContext(optionsBuilder.Options);
    }

    private string? FindConfigurationPath(string startPath)
    {
        // Try current directory first
        if (File.Exists(Path.Combine(startPath, "appsettings.json")))
        {
            return startPath;
        }

        // Try to navigate to MCP project
        var mcpPath = Path.GetFullPath(Path.Combine(startPath, "../../mcp/SmartTodo.McpServer"));
        if (Directory.Exists(mcpPath) && File.Exists(Path.Combine(mcpPath, "appsettings.json")))
        {
            return mcpPath;
        }

        // Try to navigate to StdApi project
        var apiPath = Path.GetFullPath(Path.Combine(startPath, "../../ui/StdApi"));
        if (Directory.Exists(apiPath) && File.Exists(Path.Combine(apiPath, "appsettings.json")))
        {
            return apiPath;
        }

        // Look for solution directory and search from there
        var solutionDir = FindSolutionDirectory(startPath);
        if (solutionDir != null)
        {
            var mcpFromSolution = Path.Combine(solutionDir, "source/mcp/SmartTodo.McpServer");
            if (Directory.Exists(mcpFromSolution) && File.Exists(Path.Combine(mcpFromSolution, "appsettings.json")))
            {
                return mcpFromSolution;
            }

            var apiFromSolution = Path.Combine(solutionDir, "source/ui/StdApi");
            if (Directory.Exists(apiFromSolution) && File.Exists(Path.Combine(apiFromSolution, "appsettings.json")))
            {
                return apiFromSolution;
            }
        }

        return null;
    }

    private string? FindSolutionDirectory(string startPath)
    {
        var directory = new DirectoryInfo(startPath);

        while (directory != null)
        {
            if (directory.GetFiles("*.sln").Any())
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
