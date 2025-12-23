using Microsoft.AspNetCore.Mvc;
using TempMonitor.Models;
using TempMonitor.Services;

namespace TempMonitor.Controllers;

[ApiController]
[Route("api")]
public class SensorDataController : ControllerBase
{
    private readonly ISensorDataService _sensorDataService;
    private readonly ILogger<SensorDataController> _logger;

    public SensorDataController(
        ISensorDataService sensorDataService,
        ILogger<SensorDataController> logger)
    {
        _sensorDataService = sensorDataService;
        _logger = logger;
    }

    /// <summary>
    /// Receive sensor data from ESP8266 devices
    /// </summary>
    [HttpPost("sensor-data")]
    public async Task<IActionResult> ReceiveSensorData([FromBody] SensorResponse sensorData, CancellationToken cancellationToken)
    {
        if (sensorData == null)
        {
            _logger.LogWarning("Received null sensor data");
            return BadRequest(new { error = "Invalid sensor data" });
        }

        _logger.LogInformation(
            "Received sensor data from {RemoteIp}: Temp={Temp}°C, Humidity={Humidity}%, Status={Status}",
            HttpContext.Connection.RemoteIpAddress,
            sensorData.Temp,
            sensorData.Humidity,
            sensorData.Status);

        var result = await _sensorDataService.ProcessSensorDataAsync(sensorData, cancellationToken);

        if (result == null)
        {
            _logger.LogError("Failed to process sensor data");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to process sensor data" });
        }

        return Ok(new
        {
            message = "Sensor data received and processed",
            temperatureReadingId = result.Id,
            timestamp = result.Timestamp
        });
    }

    /// <summary>
    /// Receive error reports from ESP8266 devices
    /// </summary>
    [HttpPost("errors")]
    public IActionResult ReceiveError([FromBody] ErrorReport errorReport)
    {
        if (errorReport == null || string.IsNullOrEmpty(errorReport.Message))
        {
            _logger.LogWarning("Received invalid error report");
            return BadRequest(new { error = "Invalid error report" });
        }

        _logger.LogWarning(
            "Error reported from {RemoteIp}: {Message}",
            HttpContext.Connection.RemoteIpAddress,
            errorReport.Message);

        return Ok(new { message = "Error report received" });
    }
}

/// <summary>
/// Model for error reports from ESP8266
/// </summary>
public class ErrorReport
{
    public string Message { get; set; } = string.Empty;
}
