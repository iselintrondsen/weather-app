# Publisering

Denne guiden viser hvordan du publiserer værvarsel-appen med **frontend på Vercel**
og **API-et på en .NET-vennlig host**.

## Hvorfor to steder?

Vercel kan bare hoste statiske filer og serverless-funksjoner (Node, Python, Go).
Frontend-en (`WeatherApp`, Blazor WebAssembly) blir til statiske filer ved publisering,
så den passer perfekt på Vercel. API-et (`WeatherApi`, ASP.NET Core) er derimot en
kjørende server, og må hostes et sted som faktisk kjører .NET. Vi pakker det som et
Docker-bilde, så det kan kjøre på Render, Fly.io, Azure Container Apps med flere.

Rekkefølgen under er viktig: vi deployer **API-et først** for å få adressen til det,
og kobler så frontend-en til.

---

## Steg 1 – Deploy API-et (Render som eksempel)

Repoet har en `Dockerfile` i rotmappen som bygger og kjører API-et.

1. Logg inn på [Render](https://render.com) og velg **New → Web Service**.
2. Koble til GitHub-repoet ditt.
3. Velg **Docker** som «Language/Runtime» (Render finner `Dockerfile` automatisk).
4. Under **Environment variables**, legg til:

   | Variabel         | Verdi (eksempel)                                  |
   | ---------------- | ------------------------------------------------- |
   | `AllowedOrigins` | (settes i steg 4, etter at Vercel-domenet finnes) |
   | `UserAgent`      | `Vaervarsel/1.0 (din-epost@example.com)`          |

   `UserAgent` bør være en ekte kontaktadresse – både met.no og Nominatim krever det
   i vilkårene sine.
5. Trykk **Create Web Service**. Render bygger og starter API-et, og gir deg en URL,
   for eksempel `https://vaer-api.onrender.com`.
6. Test at det lever ved å åpne i nettleseren:

   ```
   https://vaer-api.onrender.com/api/locations/search?query=Oslo
   ```

   Du skal få JSON med Oslo-treff.

> **Porten:** Du trenger ikke sette den. `Dockerfile` lytter på `$PORT` som Render
> oppgir automatisk (faller tilbake til 8080 lokalt).

**Alternativer til Render:** Samme `Dockerfile` fungerer på Fly.io (`fly launch`) og
Azure Container Apps. På Fly.io setter du `internal_port = 8080` i `fly.toml`.

---

## Steg 2 – Pek frontend-en på API-et

Åpne `WeatherApp/wwwroot/appsettings.Production.json` og bytt ut plassholderen med
den faktiske API-adressen fra steg 1:

```json
{
  "ApiBaseAddress": "https://vaer-api.onrender.com"
}
```

Denne fila brukes automatisk i produksjon. `appsettings.json` (med `localhost:5078`)
brukes fortsatt når du kjører lokalt. Husk å committe endringen.

---

## Steg 3 – Deploy frontend-en til Vercel

Repoet har `vercel.json` og `vercel-build.sh` i rotmappen. Build-scriptet installerer
.NET SDK i Vercels build-miljø og publiserer Blazor WASM til `publish-output/wwwroot`,
som `vercel.json` peker på.

1. Logg inn på [Vercel](https://vercel.com) og velg **Add New → Project**.
2. Importer GitHub-repoet ditt.
3. Vercel leser `vercel.json` automatisk – du trenger ikke endre Build- eller
   Output-innstillinger. La «Framework Preset» stå på **Other**.
4. Trykk **Deploy**. Etter bygget får du en adresse, for eksempel
   `https://vaervarsel.vercel.app`.

---

## Steg 4 – Slipp Vercel-domenet inn i API-ets CORS

Nå som du har Vercel-adressen, må API-et godta forespørsler fra den. Gå tilbake til
Render → tjenesten din → **Environment**, og sett:

```
AllowedOrigins = https://vaervarsel.vercel.app
```

Du kan oppgi flere domener separert med komma eller semikolon (for eksempel hvis du
også vil tillate et eget domene). Render starter tjenesten på nytt automatisk – ingen
ny build trengs.

Åpne så Vercel-adressen i nettleseren. Appen skal nå hente vær fra det live API-et.

---

## Feilsøking

**CORS-feil i nettleserkonsollen** («blocked by CORS policy»): `AllowedOrigins` på
API-et matcher ikke Vercel-domenet eksakt. Sjekk at protokoll (`https://`) og domene
stemmer, uten skråstrek på slutten.

**Frontend laster, men ingen værdata:** Sjekk at `ApiBaseAddress` i
`appsettings.Production.json` peker på riktig API-URL, og at API-et faktisk svarer
(åpne `/api/locations/search?query=Oslo` direkte).

**Render-tjenesten «sover»:** Gratisnivået på Render legger tjenesten i dvale ved
inaktivitet, så første forespørsel etter en stund kan ta noen sekunder.

**Vercel-bygget feiler på globalisering:** Hvis `dotnet publish` klager på ICU under
bygget, legg til miljøvariabelen `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1` i Vercels
prosjektinnstillinger. Dette påvirker kun selve byggesteget, ikke hvordan appen
formaterer tall og datoer i nettleseren.

---

## Oppsummering av filene

| Fil                                          | Rolle                                              |
| -------------------------------------------- | -------------------------------------------------- |
| `Dockerfile`                                 | Bygger og kjører API-et på en container-host       |
| `.dockerignore`                              | Holder Docker-konteksten liten                     |
| `vercel.json`                                | Vercel-konfig (build-kommando, output, SPA-routing)|
| `vercel-build.sh`                            | Installerer .NET SDK og publiserer frontend-en     |
| `WeatherApp/wwwroot/appsettings.Production.json` | API-adresse i produksjon                       |
