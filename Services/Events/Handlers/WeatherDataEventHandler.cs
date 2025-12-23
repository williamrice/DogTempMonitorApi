using TempMonitor.Models;
using TempMonitor.Models.Events;
using TempMonitor.Repositories;

namespace TempMonitor.Services.Events.Handlers;

public class WeatherDataEventHandler : IEventHandler<SensorDataReceivedEvent>
{
    private readonly IWeatherService _weatherService;
    private readonly IWeatherRepository _weatherRepository;
    private readonly ILogger<WeatherDataEventHandler> _logger;

    public WeatherDataEventHandler(
        IWeatherService weatherService,
        IWeatherRepository weatherRepository,
        ILogger<WeatherDataEventHandler> logger)
    {
        _weatherService = weatherService;
        _weatherRepository = weatherRepository;
        _logger = logger;
    }

    public async Task HandleAsync(SensorDataReceivedEvent @event, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Fetching weather data for TemperatureReadingId: {TemperatureReadingId}",
                @event.TemperatureReadingId);

            var weatherReading = await _weatherService.GetCurrentWeatherAsync(cancellationToken);

            if (weatherReading == null)
            {
                _logger.LogWarning(
                    "Failed to fetch weather data for TemperatureReadingId: {TemperatureReadingId}",
                    @event.TemperatureReadingId);
                return;
            }

            // Link weather reading to temperature reading
            weatherReading.TemperatureReadingId = @event.TemperatureReadingId;
            
            await _weatherRepository.AddAsync(weatherReading, cancellationToken);
            await _weatherRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Weather data saved for TemperatureReadingId: {TemperatureReadingId}, Outdoor: {OutdoorTemp}°C",
                @event.TemperatureReadingId,
                weatherReading.Temperature);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error handling weather data for TemperatureReadingId: {TemperatureReadingId}",
                @event.TemperatureReadingId);
        }
    }
}
