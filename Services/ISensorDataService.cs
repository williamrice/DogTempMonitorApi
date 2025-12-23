using TempMonitor.Models;

namespace TempMonitor.Services;

public interface ISensorDataService
{
    Task<TemperatureReading?> ProcessSensorDataAsync(SensorResponse sensorData, CancellationToken cancellationToken = default);
}
