using WeatherApi.Services;
using SharedModels;

namespace WeatherApi.Tests;

public class WeatherServiceTests
{
    private readonly IWeatherService _weatherService;

    public WeatherServiceTests()
    {
        _weatherService = new WeatherService();
    }

    [Fact]
    public void GetForecast_ReturnsValidForecast()
    {
        // Arrange
        const string city = "New York";

        // Act
        var forecasts = _weatherService.GetForecast(city);

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
    public void GetCurrentWeather_ReturnsValidWeather()
    {
        // Arrange
        const string city = "London";

        // Act
        var weather = _weatherService.GetCurrentWeather(city);

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
    public void GetForecast_DifferentCities_ReturnsConsistentData(string city)
    {
        // Act
        var forecasts = _weatherService.GetForecast(city);

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
    [InlineData("Unknown City")]
    [InlineData("Test123")]
    public void GetCurrentWeather_UnknownCities_ReturnsValidResponse(string city)
    {
        // Act
        var weather = _weatherService.GetCurrentWeather(city);

        // Assert
        Assert.NotNull(weather);
        Assert.Equal(city, weather.City);
        Assert.NotNull(weather.Summary);
    }

    [Fact]
    public void TemperatureConversion_IsCorrect()
    {
        // Arrange
        const string city = "Test";

        // Act
        var weather = _weatherService.GetCurrentWeather(city);

        // Assert
        var expectedF = 32 + (int)(weather.TemperatureC / 0.5556);
        Assert.Equal(expectedF, weather.TemperatureF);
    }
}