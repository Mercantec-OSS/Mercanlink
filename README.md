# Mercantec-Space

Mercantec-Space er en platform for socialt og fagligt fællesskab for nuværende og tidligere elever på Mercantec. Projektet består af en .NET backend, Discord bot, PostgreSQL database og en Svelte 5 admin frontend.

## Projektstruktur

- **Backend/** – .NET 9 Web API, Discord bot, XP/level system, JWT auth
- **admin/** – Svelte 5 admin-app (frontend)

## Konfigurationsfiler og miljøvariabler

### Backend/appsettings.json
Indeholder basis konfiguration for backend:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Discord": {
    "Token": "<DISCORD_BOT_TOKEN>"
  },
  "ConnectionStrings": {
    "DefaultConnection": "<POSTGRES_CONNECTION_STRING>"
  }
}
```

**Vigtigste nøgler:**
- `Discord:Token` – Token til Discord botten (skal holdes hemmelig!)
- `ConnectionStrings:DefaultConnection` – Forbindelsesstreng til PostgreSQL (Neon.tech eller lokal)
- `Logging` – Logniveauer for backend

### JWT konfiguration
JWT settings kan sættes i `appsettings.json` eller som miljøvariabler:
- `JWT_SECRET` – Hemmelig nøgle til signering af tokens (minimum 32 tegn, bør kun sættes som miljøvariabel i produktion)
- `JwtConfig:Issuer` – Hvem udsteder tokens (default: MercantecSpace)
- `JwtConfig:Audience` – Hvem tokens er til (default: MercantecSpaceUsers)
- `JwtConfig:ExpiryMinutes` – Hvor længe access tokens er gyldige (default: 60)
- `JwtConfig:RefreshTokenExpiryDays` – Hvor længe refresh tokens er gyldige (default: 7)

### XP/Level system
XP/level konfiguration kan sættes i `appsettings.json` under `XPConfig` eller som miljøvariabler:
- `XPConfig:ActivityRewards` – Hvor meget XP gives for forskellige aktiviteter
- `XPConfig:ActivityCooldowns` – Cooldown (i sekunder) for XP-aktiviteter
- `XPConfig:DailyLimits` – Maks antal XP-aktiviteter pr. dag
- `XPConfig:BaseXP` – XP for første level
- `XPConfig:LevelMultiplier` – Hvor meget XP stiger pr. level

### Særlige miljøvariabler
- `ASPNETCORE_ENVIRONMENT` – Sæt til `Development` for udvikling (giver Swagger, detaljeret fejl, mv.)

## Swagger & API-dokumentation
- Swagger UI: `/swagger` (kun i development)
- Alle endpoints er dokumenteret med XML-kommentarer

## Sikkerhed
- Brug JWT authentication til alle API-kald fra frontend
- Discord linking kræver nu verificering via engangskode sendt til Discord DM
- Sensitive nøgler (Discord, JWT_SECRET) bør **aldrig** committes til repoet

## Kørsel
1. **Backend:**
   - `cd Backend`
   - `dotnet run` (eller `dotnet watch run` for hot reload)
2. **Admin frontend:**
   - `cd admin`
   - `npm install`
   - `npm run dev`

## Database
- PostgreSQL (Neon.tech cloud eller lokal)
- Migrations håndteres med `dotnet ef migrations add <Navn>` og `dotnet ef database update`

## Kontakt & bidrag
- Kontakt: [Mercantec-Dev Discord](https://discord.gg/mercantec)
- Issues og PRs er velkomne!

## Admin frontend (SvelteKit)

Admin-dashboardet ligger i mappen `AdminHub/` og er bygget med SvelteKit 5.

### Vigtige konfigurationer og værktøjer

- **SvelteKit** – Moderne fullstack framework til Svelte apps (routing, SSR, API, mm.)
- **TypeScript** – Giver typesikkerhed og bedre udviklingsoplevelse
- **TailwindCSS** – Utility-first CSS framework til hurtig styling
- **Prettier** – Automatisk kodeformatering
- **Vitest** – Lynhurtig test-runner til unit tests
- **Playwright** – End-to-end browser tests (integrationstests)
- **Storybook** – UI-komponent-dokumentation og visuel test af komponenter
- **mdsvex** – Gør det muligt at bruge Markdown i Svelte-komponenter

### Scripts
- `npm run dev` – Starter udviklingsserveren
- `npm run build` – Bygger appen til produktion
- `npm run preview` – Forhåndsvisning af produktion
- `npm run test` – Kører Vitest tests
- `npm run storybook` – Starter Storybook UI
- `npm run playwright test` – Kører Playwright end-to-end tests

### Hvad bruges de enkelte værktøjer til?
- **Storybook**: Giver et interaktivt miljø til at udvikle, dokumentere og teste UI-komponenter isoleret. Kør `npm run storybook` og se alle komponenter i browseren.
- **Vitest**: Unit- og komponenttests. Hurtig feedback på logik og rendering.
- **Playwright**: Automatiserede browser-tests, fx klik, navigation og integration mellem komponenter.
- **TailwindCSS**: Utility-klasser til styling direkte i markup. Gør det hurtigt at bygge responsivt og moderne UI.
- **Prettier**: Ensretter kodeformat automatisk ved gem/commit.
- **mdsvex**: Gør det muligt at skrive Markdown direkte i Svelte-komponenter, fx til dokumentation eller indhold.

### Konfiguration
- **Konfigurationsfiler** ligger i `AdminHub/`:
  - `svelte.config.js` – SvelteKit konfiguration
  - `tailwind.config.js` – TailwindCSS opsætning
  - `vite.config.ts` – Vite bundler config
  - `.storybook/` – Storybook opsætning
  - `tsconfig.json` – TypeScript opsætning

### Kom godt i gang
1. `cd AdminHub`
2. `npm install`
3. `npm run dev` (eller `npm run storybook` for Storybook UI)

> **Bemærk:** Admin-appen er kun tilgængelig for brugere med de rette rettigheder (kræver login via backendens JWT API).

#######
