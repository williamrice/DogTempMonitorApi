using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TempMonitor.Data;
using TempMonitor.Models;

namespace TempMonitor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TemperatureController(
    ApplicationDbContext context,
    ILogger<TemperatureController> logger) : ControllerBase
{
    /// <summary>
    /// Get latest temperature readings
    /// </summary>
    [HttpGet("latest")]
    public async Task<ActionResult<IEnumerable<TemperatureReading>>> GetLatest(
        [FromQuery] int count = 10)
    {
        var readings = await context.TemperatureReadings
            .OrderByDescending(r => r.Timestamp)
            .Take(count)
            .ToListAsync();

        return Ok(readings);
    }

    /// <summary>
    /// Get readings within date range
    /// </summary>
    [HttpGet("range")]
    public async Task<ActionResult<IEnumerable<TemperatureReading>>> GetRange(
        [FromQuery] DateTime start,
        [FromQuery] DateTime end)
    {
        var readings = await context.TemperatureReadings
            .Where(r => r.Timestamp >= start && r.Timestamp <= end)
            .OrderByDescending(r => r.Timestamp)
            .ToListAsync();

        return Ok(readings);
    }

    /// <summary>
    /// Get statistics for a date range
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetStats(
        [FromQuery] DateTime? start = null,
        [FromQuery] DateTime? end = null)
    {
        var query = context.TemperatureReadings.AsQueryable();

        if (start.HasValue)
            query = query.Where(r => r.Timestamp >= start.Value);
        if (end.HasValue)
            query = query.Where(r => r.Timestamp <= end.Value);

        var validReadings = query.Where(r => r.Status == SensorStatus.OK);
        
        if (!await validReadings.AnyAsync())
        {
            return Ok(new { Count = 0 });
        }

        var stats = new
        {
            Count = await validReadings.CountAsync(),
            AvgTemp = await validReadings.AverageAsync(r => r.Temperature),
            MinTemp = await validReadings.MinAsync(r => r.Temperature),
            MaxTemp = await validReadings.MaxAsync(r => r.Temperature),
            AvgHumidity = await validReadings.AverageAsync(r => r.Humidity),
            MinHumidity = await validReadings.MinAsync(r => r.Humidity),
            MaxHumidity = await validReadings.MaxAsync(r => r.Humidity)
        };

        return Ok(stats);
    }
}