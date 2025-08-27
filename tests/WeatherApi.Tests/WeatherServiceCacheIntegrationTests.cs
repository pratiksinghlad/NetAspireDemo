using Microsoft.Extensions.DependencyInjection;
using SharedModels;
using Testcontainers.Redis;
using WeatherApi.Services;

namespace WeatherApi.Tests;

/// <summary>
/// Integration tests for WeatherService with Redis caching
/// </summary>
public class WeatherServiceCacheIntegrationTests : IAsyncLifetime
{
    private readonly RedisContainer _redisContainer;
    private ServiceProvider? _serviceProvider;
    private IWeatherService? _weatherService;
    private ICacheService? _cacheService;

    public WeatherServiceCacheIntegrationTests()
    {
        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithPortBinding(6379, true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _redisContainer.StartAsync();

        var services = new ServiceCollection();
        
        // Configure Redis distributed cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = _redisContainer.GetConnectionString();
        });
        
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<IWeatherService, WeatherService>();
        
        _serviceProvider = services.BuildServiceProvider();
        _weatherService = _serviceProvider.GetRequiredService<IWeatherService>();
        _cacheService = _serviceProvider.GetRequiredService<ICacheService>();
    }

    public async Task DisposeAsync()
    {
        _serviceProvider?.Dispose();
        await _redisContainer.DisposeAsync();
    }

    [Fact]
    public async Task GetForecastAsync_ShouldCacheResults()
    {
        // Arrange
        var city = "TestCity";
        var cacheKey = $"forecast:{city.ToLowerInvariant()}";

        // Act - First call should generate and cache data
        var firstCall = await _weatherService!.GetForecastAsync(city);
        
        // Verify data was cached
        var cachedData = await _cacheService!.GetAsync<WeatherForecast[]>(cacheKey);
        
        // Second call should return cached data
        var secondCall = await _weatherService.GetForecastAsync(city);

        // Assert
        Assert.NotNull(firstCall);
        Assert.Equal(5, firstCall.Count());
        
        Assert.NotNull(cachedData);
        Assert.Equal(5, cachedData.Length);
        
        // Both calls should return the same data (from cache)
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

    [Fact]
    public async Task GetCurrentWeatherAsync_ShouldCacheResults()
    {
        // Arrange
        var city = "TestCity";
        var cacheKey = $"current:{city.ToLowerInvariant()}";

        // Act - First call should generate and cache data
        var firstCall = await _weatherService!.GetCurrentWeatherAsync(city);
        
        // Verify data was cached
        var cachedData = await _cacheService!.GetAsync<CurrentWeather>(cacheKey);
        
        // Second call should return cached data
        var secondCall = await _weatherService.GetCurrentWeatherAsync(city);

        // Assert
        Assert.NotNull(firstCall);
        Assert.Equal(city, firstCall.City);
        
        Assert.NotNull(cachedData);
        Assert.Equal(city, cachedData!.City);
        
        // Both calls should return the same data (from cache)
        Assert.NotNull(secondCall);
        Assert.Equal(firstCall.City, secondCall.City);
        Assert.Equal(firstCall.TemperatureC, secondCall.TemperatureC);
        Assert.Equal(firstCall.TemperatureF, secondCall.TemperatureF);
        Assert.Equal(firstCall.Summary, secondCall.Summary);
        Assert.Equal(firstCall.Humidity, secondCall.Humidity);
        Assert.Equal(firstCall.WindSpeed, secondCall.WindSpeed);
    }

    [Fact]
    public async Task GetForecastAsync_WithDifferentCities_ShouldCacheSeparately()
    {
        // Arrange
        var city1 = "NewYork";
        var city2 = "London";

        // Act
        var forecast1 = await _weatherService!.GetForecastAsync(city1);
        var forecast2 = await _weatherService.GetForecastAsync(city2);

        // Assert
        Assert.NotNull(forecast1);
        Assert.NotNull(forecast2);
        
        // Different cities should have different forecasts
        Assert.NotEqual(forecast1, forecast2);
        
        // Verify both are cached separately
        var cached1 = await _cacheService!.GetAsync<WeatherForecast[]>($"forecast:{city1.ToLowerInvariant()}");
        var cached2 = await _cacheService.GetAsync<WeatherForecast[]>($"forecast:{city2.ToLowerInvariant()}");
        
        Assert.NotNull(cached1);
        Assert.NotNull(cached2);
        
        // Verify cached data matches the original forecasts
        Assert.Equal(forecast1.Count(), cached1!.Length);
        Assert.Equal(forecast2.Count(), cached2!.Length);
        
        for (int i = 0; i < forecast1.Count(); i++)
        {
            var f1 = forecast1.ElementAt(i);
            var c1 = cached1[i];
            Assert.Equal(f1.Date, c1.Date);
            Assert.Equal(f1.TemperatureC, c1.TemperatureC);
            Assert.Equal(f1.Summary, c1.Summary);
        }
        
        for (int i = 0; i < forecast2.Count(); i++)
        {
            var f2 = forecast2.ElementAt(i);
            var c2 = cached2[i];
            Assert.Equal(f2.Date, c2.Date);
            Assert.Equal(f2.TemperatureC, c2.TemperatureC);
            Assert.Equal(f2.Summary, c2.Summary);
        }
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_WithInvalidCity_ShouldThrowException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _weatherService!.GetCurrentWeatherAsync(""));
        await Assert.ThrowsAsync<ArgumentException>(() => _weatherService!.GetCurrentWeatherAsync("   "));
        await Assert.ThrowsAsync<ArgumentException>(() => _weatherService!.GetCurrentWeatherAsync(null!));
    }

    [Fact]
    public async Task GetForecastAsync_WithInvalidCity_ShouldThrowException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _weatherService!.GetForecastAsync(""));
        await Assert.ThrowsAsync<ArgumentException>(() => _weatherService!.GetForecastAsync("   "));
        await Assert.ThrowsAsync<ArgumentException>(() => _weatherService!.GetForecastAsync(null!));
    }

    [Theory]
    [InlineData("New York")]
    [InlineData("London")]
    [InlineData("Tokyo")]
    [InlineData("Sydney")]
    [InlineData("Berlin")]
    public async Task GetForecastAsync_WithKnownCities_ShouldReturnConsistentResults(string city)
    {
        // Act
        var forecast1 = await _weatherService!.GetForecastAsync(city);
        var forecast2 = await _weatherService.GetForecastAsync(city); // Should come from cache

        // Assert
        Assert.NotNull(forecast1);
        Assert.Equal(5, forecast1.Count());
        
        Assert.NotNull(forecast2);
        Assert.Equal(forecast1.Count(), forecast2.Count());
        
        // Compare each element since they should be identical (from cache)
        for (int i = 0; i < forecast1.Count(); i++)
        {
            var f1 = forecast1.ElementAt(i);
            var f2 = forecast2.ElementAt(i);
            Assert.Equal(f1.Date, f2.Date);
            Assert.Equal(f1.TemperatureC, f2.TemperatureC);
            Assert.Equal(f1.Summary, f2.Summary);
        }
        
        // All forecasts should be for future dates
        foreach (var forecast in forecast1)
        {
            Assert.True(forecast.Date > DateOnly.FromDateTime(DateTime.Today));
        }
    }

    [Fact]
    public async Task WeatherService_ShouldGenerateReasonableData()
    {
        // Arrange
        var city = "TestDataCity";

        // Act
        var forecast = await _weatherService!.GetForecastAsync(city);
        var currentWeather = await _weatherService.GetCurrentWeatherAsync(city);

        // Assert
        foreach (var day in forecast)
        {
            Assert.InRange(day.TemperatureC, -50, 60); // Reasonable temperature range
            Assert.NotNull(day.Summary);
            Assert.NotEmpty(day.Summary.Trim());
            Assert.True(day.Date > DateOnly.FromDateTime(DateTime.Today));
        }

        Assert.Equal(city, currentWeather.City);
        Assert.InRange(currentWeather.TemperatureC, -50, 60);
        Assert.NotNull(currentWeather.Summary);
        Assert.NotEmpty(currentWeather.Summary.Trim());
        Assert.InRange(currentWeather.Humidity, 0, 100);
        Assert.InRange(currentWeather.WindSpeed, 0, 200);
        
        // Check that LastUpdated is within a reasonable time window
        var timeDifference = Math.Abs((DateTime.UtcNow - currentWeather.LastUpdated).TotalMinutes);
        Assert.True(timeDifference <= 1, $"LastUpdated should be within 1 minute of UtcNow, but was {timeDifference} minutes away");
    }
}
