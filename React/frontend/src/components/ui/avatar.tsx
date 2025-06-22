// --- Forklaring af "use client" ---
// Denne linje er en instruktion til React. Den siger: "Denne komponent er interaktiv
// og skal køre i brugerens browser (klienten)". Det er nødvendigt for komponenter,
// der bruger hooks som `useState` eller `useEffect`, eller som reagerer på klik,
// hvilket denne komponent gør "under motorhjelmen".
"use client"

import * as React from "react"

// --- Forklaring af Radix UI (`AvatarPrimitive`) ---
// Her importerer vi "motoren" til vores komponent.
// Radix UI er et fantastisk bibliotek, der giver os "headless" (ustylede) komponenter.
// Tænk på det som et bilskelet: Det har en motor, hjul og styring (al funktionaliteten),
// men intet karrosseri (intet design). Det betyder, at den håndterer alt det svære,
// som f.eks. tilgængelighed (for skærmlæsere) og logikken for, hvornår billedet er loadet.
import * as AvatarPrimitive from "@radix-ui/react-avatar"

// --- Forklaring af `cn` (classnames) ---
// `cn` er en lille, men meget nyttig hjælpefunktion.
// Den bruges til at flette Tailwind CSS-klasser sammen. Det er smart, fordi
// vi kan give vores komponent nogle standardklasser, og samtidig tillade en
// udvikler at tilføje sine egne klasser, når de bruger komponenten.
// `cn` sørger for, at de bliver flettet korrekt.
import { cn } from "@/lib/utils"

// --- Komponenten er opdelt i tre dele for fleksibilitet ---

// 1. `<Avatar>`: Dette er den ydre "container".
// Den gør ikke andet end at holde sammen på de andre dele.
function Avatar({ className, ...props }: React.ComponentProps<typeof AvatarPrimitive.Root>) {
  return (
    <AvatarPrimitive.Root
      data-slot="avatar"
      // Her bruger vi `cn` til at sige:
      // "Brug disse standard-klasser, OG tilføj de klasser,
      // som udvikleren evt. har sendt med i `className`-prop'en."
      className={cn(
        "relative flex size-8 shrink-0 overflow-hidden rounded-full",
        className
      )}
      {...props}
    />
  )
}

// 2. `<AvatarImage>`: Denne del er selve billedet.
function AvatarImage({ className, ...props }: React.ComponentProps<typeof AvatarPrimitive.Image>) {
  return (
    // Radix-motoren håndterer selv at loade billedet og fortælle
    // containeren (<Avatar>), hvornår det er færdigt, eller hvis det fejler.
    <AvatarPrimitive.Image
      data-slot="avatar-image"
      className={cn("aspect-square size-full", className)}
      {...props}
    />
  )
}

// 3. `<AvatarFallback>`: Dette er nødplanen.
// Den vises automatisk, hvis `<AvatarImage>` ikke kan loade billedet.
function AvatarFallback({ className, ...props }: React.ComponentProps<typeof AvatarPrimitive.Fallback>) {
  return (
    // Indeni denne komponent skriver man typisk brugerens initialer.
    <AvatarPrimitive.Fallback
      data-slot="avatar-fallback"
      className={cn(
        "bg-muted flex size-full items-center justify-center rounded-full",
        className
      )}
      {...props}
    />
  )
}

// Vi eksporterer alle tre dele, så de kan bruges sammen.
export { Avatar, AvatarImage, AvatarFallback }
