using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using WeatherShared.Models;

namespace WeatherApi.Services;

public sealed class NominatimLocationSearchProvider(HttpClient httpClient) : ILocationSearchProvider
{
    public async Task<IReadOnlyList<LocationOption>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        var url = $"search?format=jsonv2&limit=6&countrycodes=no&q={Uri.EscapeDataString(query)}";
        var results = await httpClient.GetFromJsonAsync<IReadOnlyList<NominatimResult>>(url, cancellationToken)
            ?? [];

        return results
            .Select(ToLocationOption)
            .Where(location => !string.IsNullOrWhiteSpace(location.Name))
            .DistinctBy(location => location.Key)
            .ToArray();
    }

    private static LocationOption ToLocationOption(NominatimResult result)
    {
        var latitude = double.Parse(result.Latitude, CultureInfo.InvariantCulture);
        var longitude = double.Parse(result.Longitude, CultureInfo.InvariantCulture);
        var name = result.Name ?? result.DisplayName.Split(',').FirstOrDefault() ?? result.DisplayName;

        return new LocationOption(name.Trim(), latitude, longitude);
    }

    private sealed record NominatimResult(
        [property: JsonPropertyName("display_name")] string DisplayName,
        [property: JsonPropertyName("lat")] string Latitude,
        [property: JsonPropertyName("lon")] string Longitude,
        [property: JsonPropertyName("name")] string? Name);
}
