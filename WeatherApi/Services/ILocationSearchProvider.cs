using WeatherShared.Models;

namespace WeatherApi.Services;

public interface ILocationSearchProvider
{
    Task<IReadOnlyList<LocationOption>> SearchAsync(string query, CancellationToken cancellationToken);
}
