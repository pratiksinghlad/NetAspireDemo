using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedModels;
using Testcontainers.Redis;
using WeatherApi.Services;

namespace WeatherApi.Tests;

/// <summary>
/// Integration tests for Redis caching functionality using Testcontainers
/// </summary>
public class RedisCacheIntegrationTests : IAsyncLifetime
{
    private readonly RedisContainer _redisContainer;
    private ServiceProvider? _serviceProvider;
    private ICacheService? _cacheService;

    public RedisCacheIntegrationTests()
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
        services.AddLogging(builder => builder.AddConsole());
        
        // Configure Redis distributed cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = _redisContainer.GetConnectionString();
        });
        
        services.AddScoped<ICacheService, RedisCacheService>();
        
        _serviceProvider = services.BuildServiceProvider();
        _cacheService = _serviceProvider.GetRequiredService<ICacheService>();
    }

    public async Task DisposeAsync()
    {
        _serviceProvider?.Dispose();
        await _redisContainer.DisposeAsync();
    }

    [Fact]
    public async Task SetAsync_ShouldStoreValueInRedis()
    {
        // Arrange
        var key = "test-key";
        var value = new WeatherForecast(DateOnly.FromDateTime(DateTime.Today), 25, "Sunny");

        // Act
        await _cacheService!.SetAsync(key, value);

        // Assert
        var retrieved = await _cacheService.GetAsync<WeatherForecast>(key);
        Assert.NotNull(retrieved);
        Assert.Equal(value.Date, retrieved.Date);
        Assert.Equal(value.TemperatureC, retrieved.TemperatureC);
        Assert.Equal(value.Summary, retrieved.Summary);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNullForNonExistentKey()
    {
        // Arrange
        var key = "non-existent-key";

        // Act
        var result = await _cacheService!.GetAsync<WeatherForecast>(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_WithExpiration_ShouldExpireAfterTimespan()
    {
        // Arrange
        var key = "expiring-key";
        var value = new CurrentWeather("TestCity", 20, "Clear", 60, 5.5, DateTime.UtcNow);
        var expiration = TimeSpan.FromSeconds(2);

        // Act
        await _cacheService!.SetAsync(key, value, expiration);
        
        // Verify it exists initially
        var immediate = await _cacheService.GetAsync<CurrentWeather>(key);
        Assert.NotNull(immediate);

        // Wait for expiration
        await Task.Delay(TimeSpan.FromSeconds(3));
        
        // Assert
        var expired = await _cacheService.GetAsync<CurrentWeather>(key);
        Assert.Null(expired);
    }

    [Fact]
    public async Task RemoveAsync_ShouldDeleteValueFromRedis()
    {
        // Arrange
        var key = "remove-test-key";
        var value = new WeatherForecast(DateOnly.FromDateTime(DateTime.Today), 15, "Cloudy");

        // Act
        await _cacheService!.SetAsync(key, value);
        var beforeRemove = await _cacheService.ExistsAsync(key);
        
        await _cacheService.RemoveAsync(key);
        var afterRemove = await _cacheService.ExistsAsync(key);

        // Assert
        Assert.True(beforeRemove);
        Assert.False(afterRemove);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnCorrectResult()
    {
        // Arrange
        var existingKey = "existing-key";
        var nonExistingKey = "non-existing-key";
        var value = new WeatherForecast(DateOnly.FromDateTime(DateTime.Today), 10, "Rainy");

        // Act
        await _cacheService!.SetAsync(existingKey, value);
        
        var existingResult = await _cacheService.ExistsAsync(existingKey);
        var nonExistingResult = await _cacheService.ExistsAsync(nonExistingKey);

        // Assert
        Assert.True(existingResult);
        Assert.False(nonExistingResult);
    }

    [Fact]
    public async Task SetAsync_WithInvalidKey_ShouldThrowArgumentException()
    {
        // Arrange
        var value = new WeatherForecast(DateOnly.FromDateTime(DateTime.Today), 0, "Snow");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _cacheService!.SetAsync("", value));
        await Assert.ThrowsAsync<ArgumentException>(() => _cacheService!.SetAsync(null!, value));
    }

    [Fact]
    public async Task SetAsync_WithNullValue_ShouldThrowArgumentNullException()
    {
        // Arrange
        var key = "null-value-test";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _cacheService!.SetAsync<WeatherForecast>(key, null!));
    }

    [Fact]
    public async Task GetAsync_WithComplexObject_ShouldSerializeAndDeserializeCorrectly()
    {
        // Arrange
        var key = "complex-object";
        var forecasts = new[]
        {
            new WeatherForecast(DateOnly.FromDateTime(DateTime.Today.AddDays(1)), 22, "Partly Cloudy"),
            new WeatherForecast(DateOnly.FromDateTime(DateTime.Today.AddDays(2)), 25, "Sunny"),
            new WeatherForecast(DateOnly.FromDateTime(DateTime.Today.AddDays(3)), 18, "Overcast")
        };

        // Act
        await _cacheService!.SetAsync(key, forecasts);
        var retrieved = await _cacheService.GetAsync<WeatherForecast[]>(key);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(3, retrieved.Length);
        Assert.Equal(forecasts[0].Date, retrieved[0].Date);
        Assert.Equal(forecasts[1].TemperatureC, retrieved[1].TemperatureC);
        Assert.Equal(forecasts[2].Summary, retrieved[2].Summary);
    }
}
