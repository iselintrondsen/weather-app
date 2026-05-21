using WeatherApi.Endpoints;
using WeatherApi.Services;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicy = "BlazorClient";

// Tillatte CORS-origins leses fra konfigurasjon (miljøvariabelen AllowedOrigins,
// komma- eller semikolon-separert). I produksjon settes Vercel-domenet her.
// Faller tilbake til de lokale utviklingsadressene når variabelen ikke er satt.
var allowedOrigins = (builder.Configuration["AllowedOrigins"]
        ?? "http://localhost:5208;https://localhost:7208")
    .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Met.no og Nominatim krever en identifiserbar User-Agent. I produksjon bør denne
// settes til en ekte kontaktadresse via miljøvariabelen UserAgent, slik tjenestenes
// vilkår krever.
var userAgent = builder.Configuration["UserAgent"]
    ?? "WeatherAppDemo/1.0 (https://github.com/local-demo)";

builder.Services.AddHttpClient<IWeatherProvider, YrWeatherProvider>(client =>
{
    client.BaseAddress = new Uri("https://api.met.no/");
    client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
});

builder.Services.AddHttpClient<ILocationSearchProvider, NominatimLocationSearchProvider>(client =>
{
    client.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");
    client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
    client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("nb-NO,nb;q=0.9,no;q=0.8");
});

var app = builder.Build();

app.UseCors(CorsPolicy);

app.MapWeatherEndpoints();

app.Run();
