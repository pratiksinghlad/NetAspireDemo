using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace WeatherApi.Services;

/// <summary>
/// Redis implementation of the cache service using IDistributedCache
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the RedisCacheService
    /// </summary>
    /// <param name="distributedCache">The distributed cache implementation</param>
    public RedisCacheService(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));

        var cached = await _distributedCache.GetStringAsync(key, cancellationToken);
        
        if (string.IsNullOrEmpty(cached))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(cached, _jsonOptions);
        }
        catch (JsonException)
        {
            // If deserialization fails, remove the corrupted entry
            await RemoveAsync(key, cancellationToken);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));

        if (value == null)
            throw new ArgumentNullException(nameof(value));

        var serialized = JsonSerializer.Serialize(value, _jsonOptions);
        
        var options = new DistributedCacheEntryOptions();
        if (expiration.HasValue)
        {
            options.SetAbsoluteExpiration(expiration.Value);
        }

        await _distributedCache.SetStringAsync(key, serialized, options, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));

        await _distributedCache.RemoveAsync(key, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));

        var cached = await _distributedCache.GetStringAsync(key, cancellationToken);
        return !string.IsNullOrEmpty(cached);
    }
}
