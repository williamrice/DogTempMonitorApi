using TempMonitor.Models;
using TempMonitor.Models.Events;
using TempMonitor.Repositories;
using TempMonitor.Services.Events;

namespace TempMonitor.Services;

public class SensorDataService : ISensorDataService
{
    private readonly ITemperatureRepository _temperatureRepository;
    private readonly IEventBus _eventBus;
    private readonly ILogger<SensorDataService> _logger;

    public SensorDataService(
        ITemperatureRepository temperatureRepository,
        IEventBus eventBus,
        ILogger<SensorDataService> logger)
    {
        _temperatureRepository = temperatureRepository;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<TemperatureReading?> ProcessSensorDataAsync(SensorResponse sensorData, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Processing sensor data: Temp={Temp}°C, Humidity={Humidity}%, Message={Message}",
                sensorData.Temp,
                sensorData.Humidity,
                sensorData.Message);

            // Create temperature reading from sensor data
            var temperatureReading = new TemperatureReading
            {
                Temperature = sensorData.Temp,
                Humidity = sensorData.Humidity,
                Status = sensorData.Status,
                Message = sensorData.Message,
                Timestamp = DateTime.UtcNow
            };

            // Save temperature reading immediately
            await _temperatureRepository.AddAsync(temperatureReading, cancellationToken);
            await _temperatureRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Sensor data saved. TemperatureReadingId={Id}, Temp={Temp}°C",
                temperatureReading.Id,
                temperatureReading.Temperature);

            // Publish event for async processing
            var sensorEvent = new SensorDataReceivedEvent
            {
                OccurredAt = DateTime.UtcNow,
                TemperatureReadingId = temperatureReading.Id,
                Temperature = temperatureReading.Temperature,
                Humidity = temperatureReading.Humidity,
                Status = temperatureReading.Status,
                Timestamp = temperatureReading.Timestamp
            };

            await _eventBus.PublishAsync(sensorEvent, cancellationToken);

            _logger.LogDebug(
                "Event published for TemperatureReadingId: {Id}",
                temperatureReading.Id);

            return temperatureReading;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing sensor data");
            return null;
        }
    }
}
