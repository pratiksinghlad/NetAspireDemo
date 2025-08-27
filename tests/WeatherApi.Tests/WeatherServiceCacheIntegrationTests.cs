using FluentAssertions;
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
        firstCall.Should().NotBeNull();
        firstCall.Should().HaveCount(5);
        
        cachedData.Should().NotBeNull();
        cachedData.Should().HaveCount(5);
        
        // Both calls should return the same data (from cache)
        secondCall.Should().BeEquivalentTo(firstCall);
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
        firstCall.Should().NotBeNull();
        firstCall.City.Should().Be(city);
        
        cachedData.Should().NotBeNull();
        cachedData!.City.Should().Be(city);
        
        // Both calls should return the same data (from cache)
        secondCall.Should().BeEquivalentTo(firstCall);
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
        forecast1.Should().NotBeNull();
        forecast2.Should().NotBeNull();
        
        // Different cities should have different forecasts
        forecast1.Should().NotBeEquivalentTo(forecast2);
        
        // Verify both are cached separately
        var cached1 = await _cacheService!.GetAsync<WeatherForecast[]>($"forecast:{city1.ToLowerInvariant()}");
        var cached2 = await _cacheService.GetAsync<WeatherForecast[]>($"forecast:{city2.ToLowerInvariant()}");
        
        cached1.Should().NotBeNull();
        cached2.Should().NotBeNull();
        cached1.Should().BeEquivalentTo(forecast1);
        cached2.Should().BeEquivalentTo(forecast2);
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
        forecast1.Should().NotBeNull();
        forecast1.Should().HaveCount(5);
        forecast2.Should().BeEquivalentTo(forecast1);
        
        // All forecasts should be for future dates
        foreach (var forecast in forecast1)
        {
            forecast.Date.Should().BeAfter(DateOnly.FromDateTime(DateTime.Today));
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
            day.TemperatureC.Should().BeInRange(-50, 60); // Reasonable temperature range
            day.Summary.Should().NotBeNullOrWhiteSpace();
            day.Date.Should().BeAfter(DateOnly.FromDateTime(DateTime.Today));
        }

        currentWeather.City.Should().Be(city);
        currentWeather.TemperatureC.Should().BeInRange(-50, 60);
        currentWeather.Summary.Should().NotBeNullOrWhiteSpace();
        currentWeather.Humidity.Should().BeInRange(0, 100);
        currentWeather.WindSpeed.Should().BeInRange(0, 200);
        currentWeather.LastUpdated.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }
}
