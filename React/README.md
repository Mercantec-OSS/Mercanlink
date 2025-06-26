# Mercantec Space - React Frontend

Dette er frontend-applikationen til Mercantec Space-projektet. Applikationen er bygget med React og er designet til at interagere med vores .NET API.

## Teknologier

*   **Framework:** React (med Vite)
*   **Sprog:** TypeScript
*   **Styling:** Tailwind CSS
*   **UI Komponenter:** shadcn/ui
*   **Routing:** React Router DOM
*   **Sikkerhed:** jwt-decode, crypto-js

## Features

*   **Sikker Bruger-autentificering:** Login-system baseret på JWT (JSON Web Tokens).
*   **Krypteret `localStorage`:** `accessToken` og `refreshToken` gemmes krypteret for øget sikkerhed.
*   **Dynamisk Bruger-kontekst:** Brugeroplysninger afkodes direkte fra JWT'en, hvilket sikrer, at data er konsistent.
*   **Beskyttede Ruter:** Visse sider (f.eks. brugeroversigten) er kun tilgængelige for indloggede brugere.
*   **Service-orienteret Arkitektur:** API-kald er pænt opdelt i specialiserede services (`authService`, `userService`) for bedre vedligeholdelse og skalerbarhed.
*   **Moderne UI:** En ren og moderne brugerflade bygget med `shadcn/ui`.

## Kom godt i gang

Følg disse trin for at sætte projektet op og køre det lokalt.

### 1. Forudsætninger

Sørg for, at du har [Node.js](https://nodejs.org/) (version 18 eller nyere) installeret på din maskine.

### 2. Klon projektet

```bash
git clone <repository-url>
cd <projekt-mappe>/React/frontend
```

### 3. Installer afhængigheder

Kør følgende kommando i `frontend`-mappen for at installere alle de nødvendige pakker:

```bash
npm install
```

### 4. Opret miljøvariabler

Opret en fil ved navn `.env` i roden af `frontend`-mappen. Her skal du tilføje en hemmelig nøgle, som bruges til at kryptere data i `localStorage`.

```env
# .env

# Dette er en hemmelig nøgle til kryptering.
# Udskift den med din egen stærke, tilfældige streng for produktion.
VITE_STORAGE_SECRET_KEY=din-super-hemmelige-og-unikke-noegle
```

**Vigtigt:** Tilføj `.env` til din `.gitignore`-fil for at undgå at committe dine hemmelige nøgler.

### 5. Kør udviklingsserveren

Når alt er installeret, kan du starte udviklingsserveren:

```bash
npm run dev
```

Applikationen vil nu være tilgængelig på [http://localhost:5173](http://localhost:5173) (eller en anden port, hvis 5173 er optaget).

## Projektstruktur

Her er en detaljeret gennemgang af projektets mappestruktur. Den er designet til at være skalerbar og vedligeholdelsesvenlig ved at adskille ansvarsområder.

```
src/
│
├── assets/         # Statiske filer som billeder og ikoner.
│
├── components/     # Genbrugelige React-komponenter.
│   ├── ui/         # UI-komponenter fra shadcn/ui (Button, Card, etc.).
│   └── ProtectedRoute.tsx # En komponent, der beskytter ruter mod uautoriseret adgang.
│
├── contexts/       # Globale state-management-løsninger via React Context.
│   └── AuthContext.tsx   # Håndterer den globale autentificerings-tilstand (bruger, token, login/logout-funktioner).
│
├── lib/            # Hjælpefunktioner og biblioteks-specifikke konfigurationer.
│   └── utils.ts    # Standard hjælpefunktion fra shadcn/ui, f.eks. til at sammensætte classnames.
│
├── pages/          # Top-niveau komponenter for hver side/rute i applikationen.
│   ├── LoginPage.tsx # Login-siden med formularen til bruger-autentificering.
│   └── UsersPage.tsx # Siden, der viser listen af brugere (en beskyttet rute).
│
├── services/       # Applikationens kommunikations-lag, der håndterer al ekstern logik.
│   ├── apiClient.ts      # Den centrale `fetch`-klient. Alle API-kald går gennem denne. Den vedhæfter automatisk JWT Bearer-token til alle kald.
│   ├── authService.ts    # Håndterer alle API-kald relateret til autentificering (f.eks. `loginUser`).
│   ├── jwtService.ts     # En hjælpeservice til at afkode JWT'er og oversætte deres "claims" til vores interne `User`-model.
│   ├── storageService.ts # En sikker wrapper om `localStorage`, der automatisk krypterer og dekrypterer data (tokens).
│   └── userService.ts    # Håndterer alle API-kald relateret til brugere (f.eks. `getUsers`).
│
└── types/          # Globale TypeScript type- og interface-definitioner.
    └── index.ts    # Indeholder f.eks. `User`-interfacet, som bruges i hele applikationen.
``` 