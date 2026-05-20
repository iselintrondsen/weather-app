using WeatherShared.Models;

namespace WeatherApi.Services;

public interface IWeatherProvider
{
    string Name { get; }

    Task<WeatherForecastResponse> GetForecastAsync(
        string locationName,
        double latitude,
        double longitude,
        int maxPeriods,
        CancellationToken cancellationToken);
}
