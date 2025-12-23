using System.Text.Json;
using TempMonitor.Models;

namespace TempMonitor.Services;

public class WeatherService : IWeatherService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<WeatherService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<WeatherReading?> GetCurrentWeatherAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var apiKey = _configuration["OpenWeatherMap:ApiKey"];
            var latitude = _configuration["OpenWeatherMap:Latitude"];
            var longitude = _configuration["OpenWeatherMap:Longitude"];
            var units = _configuration["OpenWeatherMap:Units"] ?? "metric";

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("OpenWeatherMap API key is not configured");
                return null;
            }

            if (string.IsNullOrEmpty(latitude) || string.IsNullOrEmpty(longitude))
            {
                _logger.LogError("OpenWeatherMap latitude/longitude is not configured");
                return null;
            }

            var url = $"https://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&appid={apiKey}&units={units}";

            _logger.LogInformation("Fetching weather data from OpenWeatherMap for lat={Latitude}, lon={Longitude}", latitude, longitude);

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OpenWeatherMap API returned status code {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var weatherResponse = JsonSerializer.Deserialize<OpenWeatherResponse>(content);

            if (weatherResponse?.Main == null || weatherResponse.Weather == null || !weatherResponse.Weather.Any())
            {
                _logger.LogError("Invalid weather response from OpenWeatherMap");
                return null;
            }

            // Convert Unix timestamps to DateTime
            var sunrise = DateTimeOffset.FromUnixTimeSeconds(weatherResponse.Sys?.Sunrise ?? 0).UtcDateTime;
            var sunset = DateTimeOffset.FromUnixTimeSeconds(weatherResponse.Sys?.Sunset ?? 0).UtcDateTime;

            var weatherReading = new WeatherReading
            {
                Temperature = weatherResponse.Main.Temp,
                FeelsLike = weatherResponse.Main.FeelsLike,
                Humidity = weatherResponse.Main.Humidity,
                Pressure = weatherResponse.Main.Pressure,
                WeatherMain = weatherResponse.Weather.First().Main,
                WeatherDescription = weatherResponse.Weather.First().Description,
                WindSpeed = weatherResponse.Wind?.Speed ?? 0,
                WindDegree = weatherResponse.Wind?.Deg,
                Clouds = weatherResponse.Clouds?.All ?? 0,
                Visibility = weatherResponse.Visibility,
                Sunrise = sunrise,
                Sunset = sunset,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation(
                "Weather data retrieved: {Temp}°C, {Humidity}%, {Conditions}",
                weatherReading.Temperature,
                weatherReading.Humidity,
                weatherReading.WeatherMain);

            return weatherReading;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching weather data");
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing weather response");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching weather data");
            return null;
        }
    }
}
