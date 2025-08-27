using SharedModels;

namespace WeatherApi.Services;

/// <summary>
/// Service for retrieving weather data with Redis caching
/// </summary>
public class WeatherService : IWeatherService
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private static readonly Dictionary<string, int> CityTemperatureBias = new()
    {
        { "New York", 5 },
        { "London", -2 },
        { "Tokyo", 8 },
        { "Sydney", 12 },
        { "Berlin", 0 },
        { "Default", 10 }
    };

    private readonly ICacheService _cacheService;
    private static readonly TimeSpan ForecastCacheDuration = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan CurrentWeatherCacheDuration = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Initializes a new instance of the WeatherService
    /// </summary>
    /// <param name="cacheService">The cache service</param>
    public WeatherService(ICacheService cacheService)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<WeatherForecast>> GetForecastAsync(string city, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be null or empty", nameof(city));

        var cacheKey = $"forecast:{city.ToLowerInvariant()}";
        
        // Try to get from cache first
        var cachedForecast = await _cacheService.GetAsync<WeatherForecast[]>(cacheKey, cancellationToken);
        if (cachedForecast != null)
        {
            return cachedForecast;
        }

        // Generate new forecast data
        var forecast = GenerateForecast(city);
        
        // Cache the result
        await _cacheService.SetAsync(cacheKey, forecast, ForecastCacheDuration, cancellationToken);
        
        return forecast;
    }

    /// <inheritdoc />
    public async Task<CurrentWeather> GetCurrentWeatherAsync(string city, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be null or empty", nameof(city));

        var cacheKey = $"current:{city.ToLowerInvariant()}";
        
        // Try to get from cache first
        var cachedWeather = await _cacheService.GetAsync<CurrentWeather>(cacheKey, cancellationToken);
        if (cachedWeather != null)
        {
            return cachedWeather;
        }

        // Generate new current weather data
        var currentWeather = GenerateCurrentWeather(city);
        
        // Cache the result
        await _cacheService.SetAsync(cacheKey, currentWeather, CurrentWeatherCacheDuration, cancellationToken);
        
        return currentWeather;
    }

    private WeatherForecast[] GenerateForecast(string city)
    {
        var temperatureBias = CityTemperatureBias.GetValueOrDefault(city, CityTemperatureBias["Default"]);
        
        return Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast(
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20 + temperatureBias, 35 + temperatureBias),
                Summaries[Random.Shared.Next(Summaries.Length)]
            )).ToArray();
    }

    private CurrentWeather GenerateCurrentWeather(string city)
    {
        var temperatureBias = CityTemperatureBias.GetValueOrDefault(city, CityTemperatureBias["Default"]);
        
        return new CurrentWeather(
            city,
            Random.Shared.Next(-10 + temperatureBias, 30 + temperatureBias),
            Summaries[Random.Shared.Next(Summaries.Length)],
            Random.Shared.Next(30, 95),
            Random.Shared.NextDouble() * 30,
            DateTime.UtcNow
        );
    }
}