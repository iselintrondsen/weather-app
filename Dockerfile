# Dockerfile for WeatherApi (backend).
# Bygger og kjører ASP.NET Core Minimal API-et. Fungerer på Render, Fly.io,
# Azure Container Apps og andre Docker-baserte .NET-verter.
#
# Bygges fra rotmappen i repoet (Docker-konteksten må inkludere både
# WeatherApi og WeatherShared):
#   docker build -t weather-api .
#   docker run -p 8080:8080 weather-api

# --- Byggesteg ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Kopier prosjektfiler først for å utnytte Docker sin lag-cache ved restore.
COPY WeatherShared/WeatherShared.csproj WeatherShared/
COPY WeatherApi/WeatherApi.csproj WeatherApi/
RUN dotnet restore WeatherApi/WeatherApi.csproj

# Kopier resten av kildekoden og publiser.
COPY WeatherShared/ WeatherShared/
COPY WeatherApi/ WeatherApi/
RUN dotnet publish WeatherApi/WeatherApi.csproj -c Release -o /app

# --- Kjøresteg ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Lytt på porten plattformen oppgir (Render/Azure setter $PORT), ellers 8080.
EXPOSE 8080
CMD ["/bin/sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} dotnet WeatherApi.dll"]
