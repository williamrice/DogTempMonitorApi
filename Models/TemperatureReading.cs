namespace TempMonitor.Models;

public class TemperatureReading
{
    public int Id { get; set; }
    public decimal? Temperature { get; set; }
    public decimal? Humidity { get; set; }
    public SensorStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Navigation property for 1-to-1 relationship
    public WeatherReading? WeatherReading { get; set; }
}