namespace SharedModels;


public static class TemperatureConverter
{
    public static int ToFahrenheit(int temperatureC)
    {
        return 32 + (int)(temperatureC * 9.0 / 5.0);
    }
}

public record WeatherForecast(
    DateOnly Date,
    int TemperatureC,
    string? Summary
)
{
    public int TemperatureF => TemperatureConverter.ToFahrenheit(TemperatureC);
}

public record CurrentWeather(
    string City,
    int TemperatureC,
    string? Summary,
    int Humidity,
    double WindSpeed,
    DateTime LastUpdated
)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
