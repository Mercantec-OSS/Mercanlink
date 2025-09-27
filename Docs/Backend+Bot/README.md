# Mercantec Space - Dokumentation

Velkommen til Mercantec Space dokumentationen. Dette indeks giver dig et overblik over alle tilgængelige dokumenter og deres formål.

## 📋 Dokument Oversigt

### 🏗️ Arkitektur og Design
- **[Database-User-Architecture.md](Database-User-Architecture.md)** - Omfattende teknisk dokumentation af user database arkitekturen
- **[Begrundelse for hvordan den nye model ser ud.md](Begrundelse%20for%20hvordan%20den%20nye%20model%20ser%20ud.md)** - Begrundelse og principper bag database designet
- **[ModelDiagram.canvas](ModelDiagram.canvas)** - Originalt arkitektur diagram
- **[Enhanced-ModelDiagram.canvas](Enhanced-ModelDiagram.canvas)** - Forbedret og detaljeret arkitektur diagram

### 🚀 Backend System
- **[Backend-Architecture.md](Backend-Architecture.md)** - Omfattende dokumentation af backend arkitekturen og lagdeling
- **[Backend-DBAccess-Layer.md](Backend-DBAccess-Layer.md)** - Detaljeret dokumentation af DBAccess laget
- **[Backend-Architecture-Diagram.canvas](Backend-Architecture-Diagram.canvas)** - Visuelt diagram af backend system struktur

### 🗄️ Database Management
- **[Database-Migrations.md](./Database-Migrations.md)** - Komplet oversigt over alle database migrationer
- **Migration Files** (Backend/Migrations/) - Faktiske migration scripts

## 🎯 Hurtig Reference

### For Udviklere
1. Start med [Backend-Architecture.md](Backend-Architecture.md) for at forstå backend strukturen
2. Læs [Database-User-Architecture.md](Database-User-Architecture.md) for at forstå database designet
3. Gennemgå [Backend-DBAccess-Layer.md](Backend-DBAccess-Layer.md) for data access patterns
4. Brug [Backend-Architecture-Diagram.canvas](Backend-Architecture-Diagram.canvas) som visuelt reference

### For System Administratører
1. Fokus på [Database-Migrations.md](./Database-Migrations.md) for migration management
2. Se [Database-User-Architecture.md](Database-User-Architecture.md) for performance og sikkerhed

### For Nye Team Medlemmer
1. Start med [Backend-Architecture.md](Backend-Architecture.md) for at forstå systemet
2. Læs [Begrundelse for hvordan den nye model ser ud.md](Begrundelse%20for%20hvordan%20den%20nye%20model%20ser%20ud.md) for at forstå design principperne
3. Gennemgå [Database-User-Architecture.md](Database-User-Architecture.md) for teknisk forståelse
4. Brug [Backend-Architecture-Diagram.canvas](Backend-Architecture-Diagram.canvas) til visuelt overblik

## 📊 System Struktur Oversigt

### Backend Lag
- **Controllers** - API endpoints og HTTP request handling
- **Services** - Business logic og application services
- **DBAccess** - Data access layer og database operationer
- **Data** - Entity Framework DbContext og model konfiguration

### Database Komponenter
- **Main User** - Central bruger hub med roller og relationer
- **Discord User** - Discord-specifikke data og XP system
- **Website User** - Web autentificering og brugerdata
- **School AD User** - Skole Active Directory integration

### Support Systemer
- **Refresh Tokens** - JWT token management
- **User Activities** - Aktivitet tracking og XP tildeling
- **Discord Verifications** - Verifikationskoder for Discord integration

---

*Sidst opdateret: $(date)*
