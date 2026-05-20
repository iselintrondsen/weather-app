using System.Globalization;
using WeatherShared.Models;
using WeatherApi.Services;

namespace WeatherApi.Endpoints;

public static class WeatherEndpoints
{
    public static void MapWeatherEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/locations/search", SearchLocationsAsync);

        endpoints.MapGet("/api/weather", GetWeatherAsync)
            .WithName("GetWeather");
    }

    private static async Task<IResult> SearchLocationsAsync(
        string query,
        ILocationSearchProvider locationSearchProvider,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
        {
            return Results.Ok(Array.Empty<LocationOption>());
        }

        // Stedsøk går via lokal tjeneste, slik at frontend ikke bindes til valgt søkeleverandør.
        var locations = await locationSearchProvider.SearchAsync(query.Trim(), cancellationToken);
        return Results.Ok(locations);
    }

    private static async Task<IResult> GetWeatherAsync(
        string? name,
        string lat,
        string lon,
        int? periods,
        IWeatherProvider weatherProvider,
        CancellationToken cancellationToken)
    {
        if (!TryParseCoordinates(lat, lon, out var latitude, out var longitude))
        {
            return Results.BadRequest("Latitude og longitude må være gyldige desimaltall.");
        }

        if (!CoordinatesAreValid(latitude, longitude))
        {
            return Results.BadRequest("Latitude eller longitude er utenfor gyldig område.");
        }

        var locationName = string.IsNullOrWhiteSpace(name)
            ? FormattableString.Invariant($"{latitude:F4}, {longitude:F4}")
            : name.Trim();
        var maxPeriods = Math.Clamp(periods ?? 84, 1, 120);

        var forecast = await weatherProvider.GetForecastAsync(locationName, latitude, longitude, maxPeriods, cancellationToken);
        return Results.Ok(forecast);
    }

    private static bool TryParseCoordinates(
        string lat,
        string lon,
        out double latitude,
        out double longitude)
    {
        // Begge verdier parses før retur, slik at begge out-parametere alltid er satt.
        var latitudeIsValid = double.TryParse(lat, NumberStyles.Float, CultureInfo.InvariantCulture, out latitude);
        var longitudeIsValid = double.TryParse(lon, NumberStyles.Float, CultureInfo.InvariantCulture, out longitude);

        return latitudeIsValid && longitudeIsValid;
    }

    private static bool CoordinatesAreValid(double latitude, double longitude) =>
        latitude is >= -90 and <= 90 &&
        longitude is >= -180 and <= 180;
}
