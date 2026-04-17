# Mercantec Auth integration

Denne app bruger nu Mercantec Auth som eneste login-kilde via OAuth 2.0 Authorization Code + PKCE.

## 1) Registrer OAuth klient

Registrer en public client i `ClientApps` med:

- `client_id`: fx `mercantec-space-spa`
- `IsPublic=true`
- `RequirePkce=true`

Registrer præcise redirect-URI'er i `ClientAppRedirectUri`:

- `http://localhost:5173/auth/callback` (dev)
- `https://din-prod-origin/auth/callback` (prod)

## 2) Frontend miljøvariabler

Kopiér `React/frontend/.env.example` til lokal `.env` og sæt værdier:

- `VITE_AUTH_CLIENT_ID`
- `VITE_AUTH_REDIRECT_URI`
- `VITE_AUTH_LOGOUT_RETURN_URL`
- `VITE_API_BASE_URL`

`VITE_AUTH_ISSUER`, `VITE_AUTH_AUDIENCE` og `VITE_AUTH_SCOPES` kan normalt beholdes som standard.

## 3) Backend miljøvariabler

Backend validerer nu RS256-signatur via JWKS:

- `MERCANTEC_AUTH_ISSUER=https://auth.mercantec.tech`
- `MERCANTEC_AUTH_AUDIENCE=mercantec-apps`
- `MERCANTEC_AUTH_JWKS_URI=https://auth.mercantec.tech/.well-known/jwks.json`

## 4) Logout

Frontend rydder lokale tokens og viderestiller til:

`https://auth.mercantec.tech/signout?returnUrl=<url-encoded-return-url>`

Sørg for at return-url origin er whitelisted i auth-service CORS/origin konfiguration.
