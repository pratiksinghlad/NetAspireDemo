using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharedModels;
using WeatherWeb.Services;
using System.ComponentModel.DataAnnotations;

namespace WeatherWeb.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IWeatherApiClient _weatherApiClient;

    public IndexModel(ILogger<IndexModel> logger, IWeatherApiClient weatherApiClient)
    {
        _logger = logger;
        _weatherApiClient = weatherApiClient;
    }

    [BindProperty]
    [Required(ErrorMessage = "Please enter a city name")]
    [StringLength(100, ErrorMessage = "City name must be less than 100 characters")]
    public string CityName { get; set; } = string.Empty;

    public CurrentWeather? CurrentWeather { get; set; }
    public IEnumerable<WeatherForecast>? Forecasts { get; set; }
    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
        // Initialize with default data or show welcome message
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            _logger.LogInformation("Fetching weather data for city: {City}", CityName);

            // Fetch current weather and forecast in parallel
            var currentWeatherTask = _weatherApiClient.GetCurrentWeatherAsync(CityName);
            var forecastTask = _weatherApiClient.GetForecastAsync(CityName);

            await Task.WhenAll(currentWeatherTask, forecastTask);

            CurrentWeather = await currentWeatherTask;
            Forecasts = await forecastTask;

            if (CurrentWeather.Summary == "Service Unavailable" || CurrentWeather.Summary == "Error")
            {
                ErrorMessage = "Weather service is currently unavailable. Please try again later.";
            }
            else if (!Forecasts.Any())
            {
                ErrorMessage = "Unable to fetch weather forecast. The weather service may be temporarily unavailable.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weather data for city: {City}", CityName);
            ErrorMessage = "An error occurred while fetching weather data. Please try again.";
        }

        return Page();
    }
}
