using WeatherApi.Services;

namespace WeatherApi.Tests;

public class WeatherServiceTests
{
    private readonly IWeatherService _weatherService;
    private readonly ICacheService _cacheService;

    public WeatherServiceTests()
    {
        // Create a simple in-memory cache service for testing
        _cacheService = new InMemoryCacheService();
        _weatherService = new WeatherService(_cacheService);
    }

    [Fact]
    public async Task GetForecastAsync_ReturnsValidForecast()
    {
        // Arrange
        const string city = "New York";

        // Act
        var forecasts = await _weatherService.GetForecastAsync(city);

        // Assert
        Assert.NotNull(forecasts);
        Assert.Equal(5, forecasts.Count());
        
        foreach (var forecast in forecasts)
        {
            Assert.True(forecast.Date > DateOnly.FromDateTime(DateTime.Now));
            Assert.NotNull(forecast.Summary);
            Assert.NotEmpty(forecast.Summary);
            Assert.InRange(forecast.TemperatureC, -25, 40); // Reasonable temperature range
            Assert.InRange(forecast.TemperatureF, -13, 104); // Converted F range
        }
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_ReturnsValidWeather()
    {
        // Arrange
        const string city = "London";

        // Act
        var weather = await _weatherService.GetCurrentWeatherAsync(city);

        // Assert
        Assert.NotNull(weather);
        Assert.Equal(city, weather.City);
        Assert.NotNull(weather.Summary);
        Assert.NotEmpty(weather.Summary);
        Assert.InRange(weather.TemperatureC, -15, 35); // Reasonable temperature range
        Assert.InRange(weather.TemperatureF, 5, 95); // Converted F range
        Assert.InRange(weather.Humidity, 30, 95);
        Assert.InRange(weather.WindSpeed, 0, 30);
        Assert.True(weather.LastUpdated <= DateTime.UtcNow);
    }

    [Theory]
    [InlineData("Tokyo")]
    [InlineData("Sydney")]
    [InlineData("Berlin")]
    [InlineData("Default")]
    public async Task GetForecastAsync_DifferentCities_ReturnsConsistentData(string city)
    {
        // Act
        var forecasts = await _weatherService.GetForecastAsync(city);

        // Assert
        Assert.NotNull(forecasts);
        Assert.Equal(5, forecasts.Count());
        
        var forecastList = forecasts.ToList();
        for (int i = 0; i < forecastList.Count - 1; i++)
        {
            Assert.True(forecastList[i].Date < forecastList[i + 1].Date, 
                "Forecasts should be ordered by date");
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetCurrentWeatherAsync_InvalidCities_ShouldThrowException(string city)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _weatherService.GetCurrentWeatherAsync(city));
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_NullCity_ShouldThrowException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _weatherService.GetCurrentWeatherAsync(null!));
    }

    [Theory]
    [InlineData("Unknown City")]
    [InlineData("Test123")]
    [InlineData("ValidCityName")]
    public async Task GetCurrentWeatherAsync_ValidCities_ShouldReturnValidResponse(string city)
    {
        // Act
        var weather = await _weatherService.GetCurrentWeatherAsync(city);

        // Assert
        Assert.NotNull(weather);
        Assert.Equal(city, weather.City);
        Assert.NotNull(weather.Summary);
    }

    [Fact]
    public async Task TemperatureConversion_IsCorrect()
    {
        // Arrange
        const string city = "Test";

        // Act
        var weather = await _weatherService.GetCurrentWeatherAsync(city);

        // Assert
        var expectedF = 32 + (int)(weather.TemperatureC / 0.5556);
        Assert.Equal(expectedF, weather.TemperatureF);
    }

    [Fact]
    public async Task GetForecastAsync_CalledTwice_ReturnsCachedData()
    {
        // Arrange
        const string city = "CacheTest";

        // Act
        var firstCall = await _weatherService.GetForecastAsync(city);
        var secondCall = await _weatherService.GetForecastAsync(city);

        // Assert
        Assert.Equal(firstCall.Count(), secondCall.Count());
        var firstList = firstCall.ToList();
        var secondList = secondCall.ToList();
        
        for (int i = 0; i < firstList.Count; i++)
        {
            Assert.Equal(firstList[i].Date, secondList[i].Date);
            Assert.Equal(firstList[i].TemperatureC, secondList[i].TemperatureC);
            Assert.Equal(firstList[i].Summary, secondList[i].Summary);
        }
    }
}

/// <summary>
/// Simple in-memory cache service for unit testing
/// </summary>
public class InMemoryCacheService : ICacheService
{
    private readonly Dictionary<string, (object Value, DateTime? Expiry)> _cache = new();

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.Expiry == null || entry.Expiry > DateTime.UtcNow)
            {
                return Task.FromResult(entry.Value as T);
            }
            _cache.Remove(key);
        }
        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var expiry = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : (DateTime?)null;
        _cache[key] = (value, expiry);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.Expiry == null || entry.Expiry > DateTime.UtcNow)
            {
                return Task.FromResult(true);
            }
            _cache.Remove(key);
        }
        return Task.FromResult(false);
    }
}