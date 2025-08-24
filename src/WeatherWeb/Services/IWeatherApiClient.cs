using SharedModels;

namespace WeatherWeb.Services;

public interface IWeatherApiClient
{
    Task<IEnumerable<WeatherForecast>> GetForecastAsync(string city);
    Task<CurrentWeather> GetCurrentWeatherAsync(string city);
}