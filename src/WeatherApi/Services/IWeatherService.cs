using SharedModels;

namespace WeatherApi.Services;

public interface IWeatherService
{
    IEnumerable<WeatherForecast> GetForecast(string city);
    CurrentWeather GetCurrentWeather(string city);
}