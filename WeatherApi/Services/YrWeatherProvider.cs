using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using WeatherShared.Models;

namespace WeatherApi.Services;

public sealed class YrWeatherProvider(HttpClient httpClient) : IWeatherProvider
{
    public string Name => "Meteorologisk institutt";

    public async Task<WeatherForecastResponse> GetForecastAsync(
        string locationName,
        double latitude,
        double longitude,
        int maxPeriods,
        CancellationToken cancellationToken)
    {
        var url = string.Create(CultureInfo.InvariantCulture,
            $"weatherapi/locationforecast/2.0/compact?lat={latitude:F4}&lon={longitude:F4}");
        var data = await httpClient.GetFromJsonAsync<YrLocationForecast>(url, cancellationToken)
            ?? throw new InvalidOperationException("Meteorologisk institutt returnerte ingen værdata.");

        var forecasts = data.Properties.TimeSeries
            .Take(maxPeriods)
            .Select(item =>
            {
                // Meteorologisk institutt oppgir øyeblikksverdier separat fra nedbør og værsymbol for neste time.
                var details = item.Data.Instant.Details;
                return new WeatherForecastPeriod(
                    item.Time,
                    details.AirTemperature,
                    details.WindSpeed,
                    item.Data.Next1Hours?.Details.PrecipitationAmount,
                    item.Data.Next1Hours?.Summary.SymbolCode ?? item.Data.Next6Hours?.Summary.SymbolCode,
                    ToNorwegianSummary(item.Data.Next1Hours?.Summary.SymbolCode ?? item.Data.Next6Hours?.Summary.SymbolCode));
            })
            .ToArray();

        return new WeatherForecastResponse(
            locationName,
            latitude,
            longitude,
            Name,
            data.Properties.Meta.UpdatedAt,
            forecasts);
    }

    private sealed record YrLocationForecast(YrProperties Properties);

    private sealed record YrProperties(
        YrMeta Meta,
        [property: JsonPropertyName("timeseries")] IReadOnlyList<YrTimeSeries> TimeSeries);

    private sealed record YrMeta(
        [property: JsonPropertyName("updated_at")] DateTimeOffset UpdatedAt);

    private sealed record YrTimeSeries(DateTimeOffset Time, YrData Data);

    private sealed record YrData(
        YrInstant Instant,
        [property: JsonPropertyName("next_1_hours")] YrForecastBlock? Next1Hours,
        [property: JsonPropertyName("next_6_hours")] YrForecastBlock? Next6Hours);

    private sealed record YrInstant(YrInstantDetails Details);

    private sealed record YrInstantDetails(
        [property: JsonPropertyName("air_temperature")] double? AirTemperature,
        [property: JsonPropertyName("wind_speed")] double? WindSpeed);

    private sealed record YrForecastBlock(YrSummary Summary, YrPeriodDetails Details);

    private sealed record YrSummary(
        [property: JsonPropertyName("symbol_code")] string? SymbolCode);

    private sealed record YrPeriodDetails(
        [property: JsonPropertyName("precipitation_amount")] double? PrecipitationAmount);

    private static string ToNorwegianSummary(string? symbolCode)
    {
        var normalizedCode = symbolCode?.Split('_', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

        return normalizedCode switch
        {
            "clearsky" => "Klarvær",
            "fair" => "Lettskyet",
            "partlycloudy" => "Delvis skyet",
            "cloudy" => "Skyet",
            "rainshowers" => "Regnbyger",
            "rainshowersandthunder" => "Regnbyger og torden",
            "sleetshowers" => "Sluddbyger",
            "snowshowers" => "Snøbyger",
            "rain" => "Regn",
            "heavyrain" => "Kraftig regn",
            "heavyrainandthunder" => "Kraftig regn og torden",
            "sleet" => "Sludd",
            "snow" => "Snø",
            "snowandthunder" => "Snø og torden",
            "fog" => "Tåke",
            "sleetshowersandthunder" => "Sluddbyger og torden",
            "snowshowersandthunder" => "Snøbyger og torden",
            "rainandthunder" => "Regn og torden",
            "sleetandthunder" => "Sludd og torden",
            "lightrainshowersandthunder" => "Lette regnbyger og torden",
            "heavyrainshowersandthunder" => "Kraftige regnbyger og torden",
            "lightsleetshowersandthunder" => "Lette sluddbyger og torden",
            "heavysleetshowersandthunder" => "Kraftige sluddbyger og torden",
            "lightsnowshowersandthunder" => "Lette snøbyger og torden",
            "heavysnowshowersandthunder" => "Kraftige snøbyger og torden",
            "lightrainandthunder" => "Lett regn og torden",
            "lightsleetandthunder" => "Lett sludd og torden",
            "heavysleetandthunder" => "Kraftig sludd og torden",
            "lightsnowandthunder" => "Lett snø og torden",
            "heavysnowandthunder" => "Kraftig snø og torden",
            "lightrainshowers" => "Lette regnbyger",
            "heavyrainshowers" => "Kraftige regnbyger",
            "lightsleetshowers" => "Lette sluddbyger",
            "heavysleetshowers" => "Kraftige sluddbyger",
            "lightsnowshowers" => "Lette snøbyger",
            "heavysnowshowers" => "Kraftige snøbyger",
            "lightrain" => "Lett regn",
            "lightsleet" => "Lett sludd",
            "heavysleet" => "Kraftig sludd",
            "lightsnow" => "Lett snø",
            "heavysnow" => "Kraftig snø",
            _ => "Ukjent værtype"
        };
    }
}
