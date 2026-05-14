import type { EventType } from "@/services/eventsService"

export function getEventTypeLabel(type: EventType): string {
  switch (type) {
    case "Lan":
      return "LAN"
    case "Workshop":
      return "Workshop"
    case "Talk":
      return "Foredrag"
    case "Hackathon":
      return "Hackathon"
    default:
      return "Andet"
  }
}

export function getEventTypeBadgeClasses(type: EventType): string {
  switch (type) {
    case "Lan":
      return "bg-indigo-100 text-indigo-800"
    case "Workshop":
      return "bg-emerald-100 text-emerald-800"
    case "Talk":
      return "bg-pink-100 text-pink-800"
    case "Hackathon":
      return "bg-amber-100 text-amber-800"
    default:
      return "bg-slate-100 text-slate-700"
  }
}

const dateFormatter = new Intl.DateTimeFormat("da-DK", {
  weekday: "short",
  day: "2-digit",
  month: "short",
  year: "numeric",
})

const timeFormatter = new Intl.DateTimeFormat("da-DK", {
  hour: "2-digit",
  minute: "2-digit",
})

const dateTimeFormatter = new Intl.DateTimeFormat("da-DK", {
  weekday: "short",
  day: "2-digit",
  month: "short",
  hour: "2-digit",
  minute: "2-digit",
})

export function formatEventDate(value: string): string {
  return dateFormatter.format(new Date(value))
}

export function formatEventTime(value: string): string {
  return timeFormatter.format(new Date(value))
}

export function formatEventDateTime(value: string): string {
  return dateTimeFormatter.format(new Date(value))
}

/** Afsluttet event — samme logik som backend (`EndsAt` før nu). */
export function isEventPastByEndsAt(endsAt: string, nowMs: number = Date.now()): boolean {
  const t = new Date(endsAt).getTime()
  if (Number.isNaN(t)) return false
  return t < nowMs
}

export function formatEventRange(startsAt: string, endsAt: string): string {
  const start = new Date(startsAt)
  const end = new Date(endsAt)
  const sameDay =
    start.getFullYear() === end.getFullYear() &&
    start.getMonth() === end.getMonth() &&
    start.getDate() === end.getDate()
  if (sameDay) {
    return `${dateFormatter.format(start)} · ${timeFormatter.format(start)}–${timeFormatter.format(end)}`
  }
  return `${dateTimeFormatter.format(start)} – ${dateTimeFormatter.format(end)}`
}

export const EVENT_TYPE_OPTIONS: { value: EventType; label: string }[] = [
  { value: "Lan", label: "LAN" },
  { value: "Workshop", label: "Workshop" },
  { value: "Talk", label: "Foredrag" },
  { value: "Hackathon", label: "Hackathon" },
  { value: "Other", label: "Andet" },
]
