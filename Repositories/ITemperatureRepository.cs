using TempMonitor.Models;

namespace TempMonitor.Repositories;

public interface ITemperatureRepository
{
    Task<TemperatureReading?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TemperatureReading> AddAsync(TemperatureReading temperatureReading, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
