using System.Globalization;
using System.Net.Http.Json;
using WeatherShared.Models;

namespace WeatherApp.Services;

public sealed record WeatherApiOptions(Uri BaseAddress);

public sealed class WeatherApiClient(HttpClient httpClient, WeatherApiOptions options)
{
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);

    public async Task<IReadOnlyList<LocationOption>> SearchLocationsAsync(string query, CancellationToken cancellationToken)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(RequestTimeout);

        try
        {
            return await httpClient.GetFromJsonAsync<IReadOnlyList<LocationOption>>(
                new Uri(options.BaseAddress, $"/api/locations/search?query={Uri.EscapeDataString(query)}"),
                timeoutCts.Token)
                ?? [];
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("API-et svarte ikke innen 10 sekunder.");
        }
    }

    public async Task<WeatherForecastResponse> GetWeatherAsync(LocationOption location, int periods, CancellationToken cancellationToken)
    {
        var query = string.Create(CultureInfo.InvariantCulture,
            $"/api/weather?name={Uri.EscapeDataString(location.Name)}&lat={location.Latitude:F4}&lon={location.Longitude:F4}&periods={periods}");

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(RequestTimeout);

        try
        {
            return await httpClient.GetFromJsonAsync<WeatherForecastResponse>(
                new Uri(options.BaseAddress, query),
                timeoutCts.Token)
                ?? throw new InvalidOperationException("API-et returnerte ingen værdata.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("API-et svarte ikke innen 10 sekunder.");
        }
    }
}
