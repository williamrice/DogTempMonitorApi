using System.Text.Json.Serialization;

namespace TempMonitor.Models;

public class SensorResponse
{
    [JsonPropertyName("temp")]
    public decimal? Temp { get; set; }
    
    [JsonPropertyName("humidity")]
    public decimal? Humidity { get; set; }
    
    [JsonPropertyName("status")]
    public SensorStatus Status { get; set; }
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}