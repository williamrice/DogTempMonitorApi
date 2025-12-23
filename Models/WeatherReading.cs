using System.Text.Json.Serialization;

namespace TempMonitor.Models;

public class WeatherReading
{
    public int Id { get; set; }

    // Foreign key to TemperatureReading (1-to-1)
    public int TemperatureReadingId { get; set; }

    // Outdoor weather data from OpenWeatherMap (all temperatures in Celsius)
    public decimal Temperature { get; set; }
    public decimal FeelsLike { get; set; }
    public decimal Humidity { get; set; }
    public int Pressure { get; set; }

    // Weather conditions
    public string WeatherMain { get; set; } = string.Empty;
    public string WeatherDescription { get; set; } = string.Empty;

    // Wind data
    public decimal WindSpeed { get; set; }
    public int? WindDegree { get; set; }

    // Additional weather data
    public int Clouds { get; set; }
    public int? Visibility { get; set; }

    // Sun times (UTC)
    public DateTime Sunrise { get; set; }
    public DateTime Sunset { get; set; }

    // When this weather data was queried
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Navigation property - ignore to prevent circular reference in JSON
    [JsonIgnore]
    public TemperatureReading TemperatureReading { get; set; } = null!;
}
