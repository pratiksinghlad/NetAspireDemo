using SharedModels;
using System.Text.Json;

namespace WeatherWeb.Services;

public class WeatherApiClient : IWeatherApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WeatherApiClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public WeatherApiClient(HttpClient httpClient, ILogger<WeatherApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<IEnumerable<WeatherForecast>> GetForecastAsync(string city)
    {
        try
        {
            _logger.LogInformation("Fetching weather forecast for city: {City}", city);
            
            var response = await _httpClient.GetAsync($"/api/weather/forecast?city={Uri.EscapeDataString(city)}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var forecasts = JsonSerializer.Deserialize<WeatherForecast[]>(json, _jsonOptions);
                return forecasts ?? Array.Empty<WeatherForecast>();
            }
            
            _logger.LogWarning("Failed to fetch weather forecast. Status: {StatusCode}", response.StatusCode);
            return Array.Empty<WeatherForecast>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weather forecast for city: {City}", city);
            return Array.Empty<WeatherForecast>();
        }
    }

    public async Task<CurrentWeather> GetCurrentWeatherAsync(string city)
    {
        try
        {
            _logger.LogInformation("Fetching current weather for city: {City}", city);
            
            var response = await _httpClient.GetAsync($"/api/weather/{Uri.EscapeDataString(city)}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var weather = JsonSerializer.Deserialize<CurrentWeather>(json, _jsonOptions);
                return weather ?? new CurrentWeather(city, 0, "Unknown", 0, 0, DateTime.UtcNow);
            }
            
            _logger.LogWarning("Failed to fetch current weather. Status: {StatusCode}", response.StatusCode);
            return new CurrentWeather(city, 0, "Service Unavailable", 0, 0, DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching current weather for city: {City}", city);
            return new CurrentWeather(city, 0, "Error", 0, 0, DateTime.UtcNow);
        }
    }
}