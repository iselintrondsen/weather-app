using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WeatherApp;
using WeatherApp.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<WeatherApiClient>();
builder.Services.AddScoped(sp =>
{
    var apiBaseAddress = builder.Configuration["ApiBaseAddress"] ?? "http://localhost:5078";
    return new WeatherApiOptions(new Uri(apiBaseAddress));
});

await builder.Build().RunAsync();
