namespace WeatherShared.Models;

public sealed record LocationOption(string Name, double Latitude, double Longitude)
{
    public string Key => FormattableString.Invariant($"{Latitude:F4},{Longitude:F4}");
}
