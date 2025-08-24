var builder = DistributedApplication.CreateBuilder(args);

// Add the WeatherApi service
var weatherApi = builder.AddProject("weatherapi", "../WeatherApi/WeatherApi.csproj")
    .WithExternalHttpEndpoints();

// Add the WeatherWeb service with dependency on WeatherApi  
builder.AddProject("weatherweb", "../WeatherWeb/WeatherWeb.csproj")
    .WithReference(weatherApi)
    .WithExternalHttpEndpoints();

var app = builder.Build();

app.Run();
