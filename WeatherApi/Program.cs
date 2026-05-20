using WeatherApi.Endpoints;
using WeatherApi.Services;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicy = "LocalBlazorClient";

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        // Tjenesten aksepterer bare forespørsler fra de lokale Blazor-utviklingsadressene.
        policy.WithOrigins("http://localhost:5208", "https://localhost:7208")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddHttpClient<IWeatherProvider, YrWeatherProvider>(client =>
{
    client.BaseAddress = new Uri("https://api.met.no/");
    // Meteorologisk institutt krever en beskrivende User-Agent slik at de kan kontakte klienter ved behov.
    client.DefaultRequestHeaders.UserAgent.ParseAdd("LocalWeatherDemo/1.0 github.com/local-demo");
});

builder.Services.AddHttpClient<ILocationSearchProvider, NominatimLocationSearchProvider>(client =>
{
    client.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");
    // Nominatim krever også en identifiserbar klient for ansvarlig bruk av tjenesten.
    client.DefaultRequestHeaders.UserAgent.ParseAdd("LocalWeatherDemo/1.0 github.com/local-demo");
    client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("nb-NO,nb;q=0.9,no;q=0.8");
});

var app = builder.Build();

app.UseCors(CorsPolicy);

app.MapWeatherEndpoints();

app.Run();
