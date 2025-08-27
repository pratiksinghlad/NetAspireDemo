using SharedModels;

namespace WeatherApi.Services;

/// <summary>
/// Service for retrieving weather data
/// </summary>
public interface IWeatherService
{
    /// <summary>
    /// Gets the weather forecast for a specific city
    /// </summary>
    /// <param name="city">The city name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Weather forecast data</returns>
    Task<IEnumerable<WeatherForecast>> GetForecastAsync(string city, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current weather for a specific city
    /// </summary>
    /// <param name="city">The city name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current weather data</returns>
    Task<CurrentWeather> GetCurrentWeatherAsync(string city, CancellationToken cancellationToken = default);
}