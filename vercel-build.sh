#!/usr/bin/env bash
# Build-script for Vercel. Vercels build-miljø har ikke .NET forhåndsinstallert,
# så vi installerer SDK-en og publiserer Blazor WebAssembly-frontend-en til
# statiske filer. Vercel serverer deretter mappen som er satt i vercel.json
# (publish-output/wwwroot).
set -euo pipefail

echo "==> Installerer .NET 10 SDK"
curl -sSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 10.0 --install-dir "$HOME/.dotnet"
export PATH="$HOME/.dotnet:$PATH"

echo "==> Publiserer WeatherApp (Blazor WebAssembly)"
dotnet publish WeatherApp/WeatherApp.csproj -c Release -o publish-output

echo "==> Ferdig. Statisk output ligger i publish-output/wwwroot"
