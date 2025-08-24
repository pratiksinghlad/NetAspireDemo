using SharedModels;

namespace WeatherApi.Services;

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

    public IEnumerable<WeatherForecast> GetForecast(string city)
    {
        var temperatureBias = CityTemperatureBias.GetValueOrDefault(city, 0);
        
        return Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast(
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20 + temperatureBias, 35 + temperatureBias),
                Summaries[Random.Shared.Next(Summaries.Length)]
            )).ToArray();
    }

    public CurrentWeather GetCurrentWeather(string city)
    {
        var temperatureBias = CityTemperatureBias.GetValueOrDefault(city, 0);
        
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