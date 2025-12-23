using TempMonitor.Models;

namespace TempMonitor.Repositories;

public interface IWeatherRepository
{
    Task<WeatherReading?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<WeatherReading?> GetByTemperatureReadingIdAsync(int temperatureReadingId, CancellationToken cancellationToken = default);
    Task<WeatherReading> AddAsync(WeatherReading weatherReading, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
