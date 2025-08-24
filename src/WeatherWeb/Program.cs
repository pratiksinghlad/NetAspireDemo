using WeatherWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorPages();

// Configure HttpClient for Weather API
var weatherApiBaseUrl = builder.Configuration.GetValue<string>("WeatherApiBaseUrl") 
    ?? Environment.GetEnvironmentVariable("WEATHER_API_BASE_URL") 
    ?? "http://localhost:5001";

builder.Services.AddHttpClient<IWeatherApiClient, WeatherApiClient>(client =>
{
    client.BaseAddress = new Uri(weatherApiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Map default endpoints (includes health checks)
app.MapDefaultEndpoints();

app.MapRazorPages();

app.Run();
