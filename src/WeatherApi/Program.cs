using WeatherApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults
builder.AddServiceDefaults();

// Add Redis caching using connection string from Aspire
builder.Services.AddStackExchangeRedisCache(options =>
{
    // The connection name "cache" corresponds to the Redis resource name in AppHost
    options.Configuration = builder.Configuration.GetConnectionString("cache");
});

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ICacheService, RedisCacheService>();
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowWebApp");

// Map default endpoints (includes health checks)
app.MapDefaultEndpoints();

// Weather API endpoints
app.MapGet("/api/weather/forecast", async (IWeatherService weatherService, string? city, CancellationToken cancellationToken) =>
{
    var forecasts = await weatherService.GetForecastAsync(city ?? "Default", cancellationToken);
    return Results.Ok(forecasts);
})
.WithName("GetWeatherForecast")
.WithOpenApi()
.WithSummary("Get weather forecast for a city")
.WithDescription("Returns a 5-day weather forecast for the specified city");

app.MapGet("/api/weather/{city}", async (IWeatherService weatherService, string city, CancellationToken cancellationToken) =>
{
    var weather = await weatherService.GetCurrentWeatherAsync(city, cancellationToken);
    return Results.Ok(weather);
})
.WithName("GetCurrentWeather")
.WithOpenApi()
.WithSummary("Get current weather for a city")
.WithDescription("Returns current weather information for the specified city");

app.Run();
