using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TempMonitor.Data;

namespace TempMonitor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        ApplicationDbContext dbContext,
        ILogger<HealthController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetHealth()
    {
        try
        {
            // Check database connectivity
            var canConnect = await _dbContext.Database.CanConnectAsync();
            
            var health = new
            {
                status = canConnect ? "healthy" : "unhealthy",
                timestamp = DateTime.UtcNow,
                service = "TempMonitor API",
                version = "1.0.0",
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                database = canConnect ? "connected" : "disconnected"
            };

            if (!canConnect)
            {
                _logger.LogWarning("Health check failed: Database connection unavailable");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, health);
            }

            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed with exception");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = "unhealthy",
                timestamp = DateTime.UtcNow,
                service = "TempMonitor API",
                error = "Health check failed"
            });
        }
    }
}
