namespace TempMonitor.Models.Events;

public class SensorDataReceivedEvent : IEvent
{
    public DateTime OccurredAt { get; init; }
    public int TemperatureReadingId { get; init; }
    public decimal? Temperature { get; init; }
    public decimal? Humidity { get; init; }
    public SensorStatus Status { get; init; }
    public DateTime Timestamp { get; init; }
}
