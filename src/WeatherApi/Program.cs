using SharedModels;
using WeatherApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IWeatherService, WeatherService>();

// Add CORS for local development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.WithOrigins("http://localhost:5000", "https://localhost:5001")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowWebApp");

// Health check endpoint
app.MapHealthChecks("/health");

// Weather API endpoints
app.MapGet("/api/weather/forecast", (IWeatherService weatherService, string? city) =>
{
    var forecasts = weatherService.GetForecast(city ?? "Default");
    return Results.Ok(forecasts);
})
.WithName("GetWeatherForecast")
.WithOpenApi()
.WithSummary("Get weather forecast for a city")
.WithDescription("Returns a 5-day weather forecast for the specified city");

app.MapGet("/api/weather/{city}", (IWeatherService weatherService, string city) =>
{
    var weather = weatherService.GetCurrentWeather(city);
    return Results.Ok(weather);
})
.WithName("GetCurrentWeather")
.WithOpenApi()
.WithSummary("Get current weather for a city")
.WithDescription("Returns current weather information for the specified city");

app.Run();
