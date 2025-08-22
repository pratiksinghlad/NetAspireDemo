namespace SharedModels;

public record WeatherForecast(
    DateOnly Date,
    int TemperatureC,
    string? Summary
)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
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
