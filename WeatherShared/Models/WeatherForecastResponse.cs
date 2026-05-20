namespace WeatherShared.Models;

public sealed record WeatherForecastResponse(
    string LocationName,
    double Latitude,
    double Longitude,
    string Provider,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<WeatherForecastPeriod> Forecasts);

public sealed record WeatherForecastPeriod(
    DateTimeOffset Time,
    double? TemperatureC,
    double? WindSpeedMetersPerSecond,
    double? PrecipitationMillimeters,
    string? SymbolCode,
    string Summary);
