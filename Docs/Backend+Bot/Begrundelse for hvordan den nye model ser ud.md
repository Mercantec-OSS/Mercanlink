# Begrundelse for User Database Arkitektur

## Hovedprincipper

Grunden til at jeg har splittet vores [[ModelDiagram.canvas|Model]] ud som den er, er baseret på følgende principper:

### 1. Separation of Concerns
- **Discord-specifikke data** (XP, level, Discord ID) håndteres i `DiscordUsers` tabel
- **Web-autentificering** (email, password) håndteres i `WebsiteUsers` tabel  
- **Skole AD integration** (student ID) håndteres i `SchoolADUsers` tabel
- **Central brugeridentitet** samles i `Users` hovedtabel

### 2. Performance Optimering
- Når man besøger hjemmesiden, behøver man ikke at loade Discord-specifikke data
- Discord bot operationer kan fokusere på Discord-relaterede data
- Skole systemer kan kun tilgå relevante AD data

### 3. Skalerbarhed og Vedligeholdelse
- Hver brugerkilde kan udvikles og vedligeholdes uafhængigt
- Nye brugerkilder kan tilføjes uden at påvirke eksisterende
- Data migration og cleanup er lettere at håndtere

### 4. Data Integritet
- Unikke constraints på hver tabel sikrer data kvalitet
- Foreign key relationer sikrer referentiel integritet
- Cascade delete sikrer konsistent data cleanup

## Arkitektur Fordele

- **Modulær design**: Hver komponent har et specifikt ansvar
- **Fleksibel integration**: Understøtter forskellige autentificeringsmetoder
- **Optimeret queries**: Kun nødvendige data hentes per operation
- **Fremtidssikker**: Let at udvide med nye brugerkilder

![[ModelDiagram.canvas|ModelDiagram]]

## Se også
- [[Database-User-Architecture]] - Detaljeret teknisk dokumentation
- [[ModelDiagram.canvas]] - Interaktivt arkitektur diagram