using TempMonitor.Models;

namespace TempMonitor.Services;

public interface IWeatherService
{
    Task<WeatherReading?> GetCurrentWeatherAsync(CancellationToken cancellationToken = default);
}
