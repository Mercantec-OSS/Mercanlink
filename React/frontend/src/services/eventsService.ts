import { apiClient } from "@/services/apiClient"

export type EventType = "Lan" | "Workshop" | "Talk" | "Hackathon" | "Other"
export type EventStatus = "Draft" | "Published" | "Cancelled"
export type EventRegistrationSource = "Web" | "Discord"

export interface EventListItem {
  id: string
  slug: string
  title: string
  type: EventType
  status: EventStatus
  startsAt: string
  endsAt: string
  location: string
  bannerImageUrl?: string | null
  capacity?: number | null
  registrationCount: number
  registrationDeadline?: string | null
}

export interface EventDetail extends EventListItem {
  description: string
  locationUrl?: string | null
  bringOwnPc?: boolean | null
  speakerName?: string | null
  prerequisites?: string | null
  teamSize?: number | null
  registrationOpen: boolean
  isFull: boolean
}

export interface EventRegistration {
  displayName: string
  email: string
  registeredAt: string
  source: EventRegistrationSource
}

export interface MyEventRegistration {
  eventId: string
  slug: string
  title: string
  startsAt: string
  registeredAt: string
}

export interface RegisterForEventPayload {
  confirmedDisplayName: string
  confirmedEmail: string
}

export interface CreateEventPayload {
  title: string
  slug?: string
  description: string
  type: EventType
  startsAt: string
  endsAt: string
  location: string
  locationUrl?: string | null
  bannerImageUrl?: string | null
  capacity?: number | null
  registrationDeadline?: string | null
  bringOwnPc?: boolean | null
  speakerName?: string | null
  prerequisites?: string | null
  teamSize?: number | null
}

export type UpdateEventPayload = CreateEventPayload

export type EventListStatusFilter = "upcoming" | "past" | "all"

export async function listEvents(params?: {
  status?: EventListStatusFilter
  type?: EventType
}): Promise<EventListItem[]> {
  const searchParams = new URLSearchParams()
  if (params?.status) searchParams.set("status", params.status)
  if (params?.type) searchParams.set("type", params.type)
  const qs = searchParams.toString()
  return apiClient<EventListItem[]>(`/events${qs ? `?${qs}` : ""}`)
}

export async function getEvent(slug: string): Promise<EventDetail> {
  return apiClient<EventDetail>(`/events/${encodeURIComponent(slug)}`)
}

export async function getEventRegistrations(slug: string): Promise<EventRegistration[]> {
  return apiClient<EventRegistration[]>(`/events/${encodeURIComponent(slug)}/registrations`)
}

export async function getMyRegistrations(): Promise<MyEventRegistration[]> {
  return apiClient<MyEventRegistration[]>("/events/my-registrations")
}

export async function registerForEvent(slug: string, payload: RegisterForEventPayload): Promise<void> {
  await apiClient(`/events/${encodeURIComponent(slug)}/register`, {
    method: "POST",
    body: JSON.stringify(payload),
  })
}

export async function unregisterFromEvent(slug: string): Promise<void> {
  await apiClient(`/events/${encodeURIComponent(slug)}/register`, {
    method: "DELETE",
  })
}

export function getEventIcsUrl(slug: string): string {
  const base = (import.meta.env.VITE_API_BASE_URL as string | undefined) || "/api"
  return `${base}/events/${encodeURIComponent(slug)}/ics`
}

// ---------- Admin ----------

export async function listAllEventsForAdmin(): Promise<EventListItem[]> {
  return apiClient<EventListItem[]>("/events/admin/all")
}

export async function createEvent(payload: CreateEventPayload): Promise<EventDetail> {
  return apiClient<EventDetail>("/events", {
    method: "POST",
    body: JSON.stringify(payload),
  })
}

export async function updateEvent(id: string, payload: UpdateEventPayload): Promise<EventDetail> {
  return apiClient<EventDetail>(`/events/${encodeURIComponent(id)}`, {
    method: "PUT",
    body: JSON.stringify(payload),
  })
}

export async function publishEvent(id: string): Promise<EventDetail> {
  return apiClient<EventDetail>(`/events/${encodeURIComponent(id)}/publish`, {
    method: "POST",
    body: JSON.stringify({}),
  })
}

export async function cancelEvent(id: string): Promise<EventDetail> {
  return apiClient<EventDetail>(`/events/${encodeURIComponent(id)}/cancel`, {
    method: "POST",
    body: JSON.stringify({}),
  })
}

export async function deleteEvent(id: string): Promise<void> {
  await apiClient(`/events/${encodeURIComponent(id)}`, {
    method: "DELETE",
  })
}

export async function getAdminRegistrations(eventId: string): Promise<EventRegistration[]> {
  return apiClient<EventRegistration[]>(`/events/${encodeURIComponent(eventId)}/admin-registrations`)
}
