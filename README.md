# Værvarsel

En liten værvarsel-app for norske steder. Du søker etter et sted, og appen viser
nåværende vær, en temperaturgraf for de neste timene og varsel både time for time
og for de neste 7 dagene. Værdataene kommer fra Meteorologisk institutt (Yr), og
stedsøket går mot OpenStreetMap (Nominatim).

## Funksjoner

- Søk etter norske steder med autofullføring av alternative treff.
- Nåværende vær: temperatur, vind, nedbør neste time og datakilde.
- Temperaturgraf (SVG) for de neste 12 timene.
- To visninger: «time for time» (24 timer) og «7 dager».
- Værsymboler og norske beskrivelser av værtype.
- Hele grensesnittet er på norsk.

## Teknologi

- **.NET 10**
- **WeatherApi** – ASP.NET Core Minimal API (backend / proxy mot eksterne tjenester)
- **WeatherApp** – Blazor WebAssembly (frontend)
- **WeatherShared** – delt klassebibliotek med felles datamodeller

## Forutsetninger

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Internettilgang (appen henter data fra `api.met.no` og `nominatim.openstreetmap.org`)

## Kom i gang

Backend og frontend kjøres som to separate prosesser. Start API-et først.

**1. Start API-et** (i én terminal):

```bash
cd WeatherApi
dotnet run
```

API-et lytter på `http://localhost:5078`.

**2. Start frontend-en** (i en annen terminal):

```bash
cd WeatherApp
dotnet run
```

Appen lytter på `http://localhost:5208` (og `https://localhost:7208`).

**3. Åpne appen** i nettleseren på <http://localhost:5208>.

> **Merk:** Frontend-en må kjøre på port 5208/7208. API-et tillater kun forespørsler
> fra disse adressene via CORS, så andre porter blir avvist.

## Konfigurasjon

Adressen til API-et settes i `WeatherApp/wwwroot/appsettings.json`:

```json
{
  "ApiBaseAddress": "http://localhost:5078"
}
```

Endrer du porten API-et kjører på, må du oppdatere både denne verdien og
CORS-reglene i `WeatherApi/Program.cs`.

## API-endepunkter

| Metode | Rute                                            | Beskrivelse                                  |
| ------ | ----------------------------------------------- | -------------------------------------------- |
| `GET`  | `/api/locations/search?query={tekst}`           | Søker etter norske steder (minst 2 tegn).    |
| `GET`  | `/api/weather?name={navn}&lat={lat}&lon={lon}&periods={n}` | Henter værvarsel for gitte koordinater. |

Eksempelforespørsler ligger i [`WeatherApi/WeatherApi.http`](WeatherApi/WeatherApi.http).

## Prosjektstruktur

```text
WeatherSolution.slnx          Løsningsfil (samler de tre prosjektene)
│
├── WeatherApi/               Backend – ASP.NET Core Minimal API
│   ├── Endpoints/            Definisjon av HTTP-endepunktene
│   ├── Services/             Yr (vær) og Nominatim (stedsøk)
│   └── Program.cs            Oppsett av tjenester, CORS og HttpClient
│
├── WeatherApp/               Frontend – Blazor WebAssembly
│   ├── Pages/                Hovedsiden (Home.razor) med graf og varselvisninger
│   ├── Services/             WeatherApiClient – kaller backend-en
│   ├── Layout/               Felles layout
│   └── wwwroot/              Statiske filer og konfigurasjon
│
└── WeatherShared/            Delt klassebibliotek
    └── Models/               LocationOption, WeatherForecastResponse
```

Se [ARCHITECTURE.md](ARCHITECTURE.md) for en nærmere beskrivelse av dataflyten
og designvalgene.

## Datakilder

- Værvarsel: [Meteorologisk institutt / Yr](https://api.met.no/) (Locationforecast 2.0)
- Stedsøk: [Nominatim / OpenStreetMap](https://nominatim.org/)

Begge tjenestene krever en identifiserbar `User-Agent`. Denne settes i
`WeatherApi/Program.cs` og bør tilpasses før eventuell produksjonsbruk, i tråd med
tjenestenes vilkår for bruk.
