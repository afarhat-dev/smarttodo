using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using SmartTodo.Application.Interfaces;
using SmartTodo.Application.Services;
using SmartTodo.Infrastructure.Persistence;
using SmartTodo.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var usePostgreSql = builder.Configuration.GetValue<bool>("Database:UsePostgreSQL");

if (usePostgreSql)
{
    // PostgreSQL configuration
    var connectionString = builder.Configuration.GetConnectionString("PostgreSQL");

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException(
            "PostgreSQL connection string is not configured. " +
            "Please set ConnectionStrings:PostgreSQL in appsettings.json");
    }

    builder.Services.AddDbContextPool<TodoDbContext>(options =>
        options.UseNpgsql(connectionString));

    builder.Services.AddScoped<ITodoRepository, PostgreSqlTodoRepository>();
    builder.Services.AddScoped<ITodoService, TodoService>();

    builder.Logging.AddConsole();
    builder.Logging.AddDebug();
    Console.WriteLine("Using PostgreSQL database");
}
else
{
    // In-Memory configuration
    builder.Services.AddScoped<ITodoRepository, InMemoryTodoRepository>();
    builder.Services.AddScoped<ITodoService, TodoService>();

    Console.WriteLine("Using In-Memory database");
}



builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartTodo API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("v1/swagger.json", "My API V1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseDeveloperExceptionPage();
app.Run();
