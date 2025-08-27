var builder = DistributedApplication.CreateBuilder(args);

// Add Redis container
var redis = builder.AddRedis("cache");

// Add the WeatherApi service with Redis reference
var weatherApi = builder.AddProject("weatherapi", "../WeatherApi/WeatherApi.csproj")
    .WithReference(redis)
    .WithExternalHttpEndpoints();

// Add the WeatherWeb service with dependency on WeatherApi  
builder.AddProject("weatherweb", "../WeatherWeb/WeatherWeb.csproj")
    .WithReference(weatherApi)
    .WithExternalHttpEndpoints();

var app = builder.Build();

app.Run();
