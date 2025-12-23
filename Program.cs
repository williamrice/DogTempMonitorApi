using Microsoft.EntityFrameworkCore;
using Serilog;
using TempMonitor.Data;
using TempMonitor.Middleware;
using TempMonitor.Models.Events;
using TempMonitor.Repositories;
using TempMonitor.Services;
using TempMonitor.Services.Events;
using TempMonitor.Services.Events.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration
Log.Logger = new LoggerConfiguration()
		.ReadFrom.Configuration(builder.Configuration)
		.WriteTo.Console()
		.WriteTo.File("logs/tempmonitor-.txt", rollingInterval: RollingInterval.Day)
		.CreateLogger();

builder.Host.UseSerilog();

// Database configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
		options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddHttpClient();

// Register Event Bus (Singleton for in-memory pub/sub)
builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();

// Register Event Handlers (Scoped for DB access)
builder.Services.AddScoped<IEventHandler<SensorDataReceivedEvent>, WeatherDataEventHandler>();

// Register Background Service for event processing
builder.Services.AddHostedService<EventProcessorHostedService>();

// Register repositories
builder.Services.AddScoped<ITemperatureRepository, TemperatureRepository>();
builder.Services.AddScoped<IWeatherRepository, WeatherRepository>();

// Register services
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<ISensorDataService, SensorDataService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi(); // Modern .NET 9 OpenAPI

var app = builder.Build();

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

// Subscribe event handlers
var eventBus = app.Services.GetRequiredService<IEventBus>();
eventBus.Subscribe<SensorDataReceivedEvent>(async (@event, cancellationToken) =>
{
	using var scope = app.Services.CreateScope();
	var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<SensorDataReceivedEvent>>();
	await handler.HandleAsync(@event, cancellationToken);
});

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi(); // Generates /openapi/v1.json
}

// Add API key authentication middleware
app.UseApiKeyAuthentication();

app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
