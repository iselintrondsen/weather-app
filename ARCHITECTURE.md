# Arkitektur

Dette dokumentet beskriver hvordan værvarsel-appen er bygd opp, hvordan data flyter
gjennom systemet, og hvilke designvalg som ligger bak.

## Oversikt

Løsningen består av tre prosjekter:

| Prosjekt          | Type                        | Ansvar                                                        |
| ----------------- | --------------------------- | ------------------------------------------------------------- |
| `WeatherApp`      | Blazor WebAssembly          | Brukergrensesnitt som kjører i nettleseren.                   |
| `WeatherApi`      | ASP.NET Core Minimal API    | Mellomledd (proxy) mot eksterne tjenester.                   |
| `WeatherShared`   | Klassebibliotek (.NET)      | Felles datamodeller som deles av frontend og backend.        |

Frontend snakker aldri direkte med de eksterne tjenestene. All kommunikasjon går via
`WeatherApi`, som kapsler inn hvilke leverandører som faktisk brukes.

## Komponent- og dataflyt

```mermaid
flowchart TD
    subgraph Nettleser["Nettleser"]
        UI["WeatherApp<br/>(Blazor WebAssembly)"]
        Client["WeatherApiClient"]
        UI --> Client
    end

    subgraph Backend["WeatherApi (localhost:5078)"]
        Endpoints["Endepunkter<br/>/api/locations/search<br/>/api/weather"]
        Nominatim["NominatimLocationSearchProvider"]
        Yr["YrWeatherProvider"]
        Endpoints --> Nominatim
        Endpoints --> Yr
    end

    subgraph Eksternt["Eksterne tjenester"]
        OSM["Nominatim / OpenStreetMap<br/>nominatim.openstreetmap.org"]
        MET["Meteorologisk institutt / Yr<br/>api.met.no"]
    end

    Shared["WeatherShared<br/>(felles modeller)"]

    Client -- "HTTP/JSON" --> Endpoints
    Nominatim -- "stedsøk" --> OSM
    Yr -- "værvarsel" --> MET

    Shared -.deles av.-> UI
    Shared -.deles av.-> Endpoints
```

## Typisk forløp

```mermaid
sequenceDiagram
    actor Bruker
    participant App as WeatherApp
    participant Api as WeatherApi
    participant OSM as Nominatim
    participant MET as Yr (api.met.no)

    Bruker->>App: Skriver «Oslo» og søker
    App->>Api: GET /api/locations/search?query=Oslo
    Api->>OSM: Søk etter norske steder
    OSM-->>Api: Liste med treff (navn + koordinater)
    Api-->>App: LocationOption[]
    App->>Api: GET /api/weather?name=Oslo&lat=..&lon=..
    Api->>MET: Locationforecast 2.0 (lat/lon)
    MET-->>Api: Værdata (timeserie)
    Api-->>App: WeatherForecastResponse
    App-->>Bruker: Viser graf, time-for-time og 7-dagersvarsel
```

## Komponentene i detalj

### WeatherApp (frontend)

Blazor WebAssembly-app som kjører i nettleseren. Hovedsiden (`Pages/Home.razor`)
håndterer stedsøk, valg av sted og visning av varselet. Den bygger blant annet en
SVG-temperaturgraf for de neste 12 timene og grupperer timesdata til et 7-dagersvarsel
på klienten.

All kommunikasjon med backend går gjennom `Services/WeatherApiClient`, som legger på
en tidsavbrudd-grense på 10 sekunder per kall slik at grensesnittet ikke henger ved
treg respons. Adressen til API-et leses fra `wwwroot/appsettings.json`
(`ApiBaseAddress`).

### WeatherApi (backend)

Et ASP.NET Core Minimal API som fungerer som mellomledd. Endepunktene defineres i
`Endpoints/WeatherEndpoints.cs` og validerer inndata (f.eks. at koordinater er gyldige
desimaltall innenfor gyldig område) før de kaller en leverandør.

Selve integrasjonene ligger bak grensesnitt, slik at endepunktene ikke er bundet til en
konkret leverandør:

- `IWeatherProvider` → `YrWeatherProvider` henter værvarsel fra `api.met.no`
  (Locationforecast 2.0, compact) og oversetter Yr sine symbolkoder til norske
  beskrivelser (f.eks. `clearsky` → «Klarvær»).
- `ILocationSearchProvider` → `NominatimLocationSearchProvider` søker etter steder via
  Nominatim, begrenset til norske treff (`countrycodes=no`).

Begge leverandørene konfigureres med en egen `HttpClient` i `Program.cs`, der det
settes en identifiserbar `User-Agent` slik tjenestene krever.

### WeatherShared (felles modeller)

Et lite klassebibliotek med datamodellene som både frontend og backend bruker:
`LocationOption` (sted med navn og koordinater) og `WeatherForecastResponse` /
`WeatherForecastPeriod` (selve værvarselet). Ved å samle dem ett sted unngår vi at de
samme typene defineres dobbelt i de to prosjektene.

## Designvalg

**Backend som proxy.** Frontend kaller aldri `api.met.no` eller Nominatim direkte.
Det gjør at appen ikke er bundet til en bestemt værleverandør, holder eksterne detaljer
(URL-er, `User-Agent`, oversettelse av symbolkoder) på ett sted, og lar `WeatherApi`
styre hvilke klienter som slipper til via CORS.

**Grensesnitt for leverandørene.** `IWeatherProvider` og `ILocationSearchProvider`
gjør at en datakilde kan byttes ut uten å endre endepunktene. Vil man for eksempel
bruke en annen værtjeneste, lager man en ny implementasjon og registrerer den i
`Program.cs`.

**Delt modellbibliotek.** `WeatherShared` fjerner duplisering av datamodellene. JSON-en
som sendes over nettet er navnebasert, så det samme settet med typer brukes både ved
serialisering (API) og deserialisering (app).

**CORS-låsing.** API-et godtar kun forespørsler fra de lokale Blazor-adressene
(`http://localhost:5208` og `https://localhost:7208`). Endrer du portene, må CORS-reglene
i `Program.cs` oppdateres tilsvarende.

## Porter og konfigurasjon

| Komponent  | Adresse                                          |
| ---------- | ------------------------------------------------ |
| WeatherApi | `http://localhost:5078`                          |
| WeatherApp | `http://localhost:5208`, `https://localhost:7208` |

Se [README.md](README.md) for hvordan du starter prosjektene.
