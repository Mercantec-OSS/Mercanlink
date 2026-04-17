# Bot monitorering og alarmer (Uptime Kuma)

Denne runbook beskriver, hvordan drift alarmeres, hvis botten/API'en går ned eller mister Discord-gateway forbindelsen.

## 1) Start monitor-stack

`docker-compose.yml` indeholder nu:
- `backend` med `/health` endpoint
- `backend` `healthcheck` + `restart: unless-stopped`
- `uptime-kuma` på port `9303`

Start/redeploy:

```bash
docker compose up -d --build backend db frontend uptime-kuma
```

## 2) Åbn Uptime Kuma

- URL: `http://<server-ip-eller-domæne>:9303`
- Opret admin-bruger første gang.

## 3) Opret monitor til backend/bot

I Uptime Kuma:
1. Add New Monitor
2. Type: `HTTP(s)`
3. Name: `Mercantec Backend+Bot Health`
4. URL: `http://backend:8080/health` (internt i compose-netværket)
5. Heartbeat Interval: `30` sekunder
6. Retries: `2` eller `3`
7. Request Timeout: `10` sekunder
8. Accepted status codes: `200-299`

Forventning:
- `200` = healthy
- `503` = unhealthy (fx DB nede eller Discord bot stale/disconnected)

## 4) Opsæt flere alarmkanaler

### Discord notifikation
1. Opret en Discord webhook i jeres driftskanal.
2. Uptime Kuma -> Settings -> Notifications -> Setup Notification -> `Discord`.
3. Indsæt webhook URL og test notifikation.
4. Tilknyt notifikation til monitoren.

### Email notifikation
1. Uptime Kuma -> Settings -> Notifications -> Setup Notification -> `SMTP / Email`.
2. Konfigurer SMTP host, port, brugernavn og afsender.
3. Angiv modtagerliste (fx drift + lead).
4. Send test notifikation og tilknyt monitoren.

### SMS (valgfrit)
- Brug fx Twilio/MessageBird integration via Uptime Kuma notification provider.
- Tilknyt samme monitor for kritiske alarmer.

## 5) Hardening / anti-flap anbefaling

- Hold interval på `30s`.
- Sæt retries til `2-3` for at reducere false positives.
- Behold `DISCORD_HEALTH_STALE_SECONDS=180` som udgangspunkt.
- Hvis I får falske alarmer pga. netværk, prøv `240-300`.

## 6) Verificer at alarmer virker

### Test A: App-process ned
```bash
docker compose stop backend
```
Forventning: alarm indenfor ~1-2 minutter.

### Test B: Genopretning
```bash
docker compose start backend
```
Forventning: recovery-notifikation udsendes.

## 7) Driftsrutine

- Kør alarm-test månedligt.
- Opdater webhook/smtp credentials ved rotation.
- Hold Uptime Kuma image opdateret ved planlagt vedligehold.
