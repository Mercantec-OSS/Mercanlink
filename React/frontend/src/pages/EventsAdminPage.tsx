import { useCallback, useEffect, useMemo, useRef, useState } from "react"
import Layout_alt from "@/components/templates/layout"
import { Card } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover"
import {
  Plus,
  Pencil,
  Send,
  Ban,
  Trash2,
  Users,
  X,
  HelpCircle,
} from "lucide-react"
import {
  cancelEvent,
  createEvent,
  deleteEvent,
  getAdminRegistrations,
  getEventDetailForAdmin,
  listAllEventsForAdmin,
  publishEvent,
  updateEvent,
  type CreateEventPayload,
  type EventDetail,
  type EventListItem,
  type EventRegistration,
  type EventType,
} from "@/services/eventsService"
import {
  EVENT_TYPE_OPTIONS,
  formatEventDateTime,
  getEventTypeBadgeClasses,
  getEventTypeLabel,
  isEventPastByEndsAt,
} from "@/lib/eventFormatting"
import { uploadEventBannerImage } from "@/services/mediaService"
import { BannerFocalEditor } from "@/components/events/BannerFocalEditor"
import { normalizeBannerFraming } from "@/lib/bannerFraming"

interface FormState {
  id?: string
  title: string
  slug: string
  description: string
  type: EventType
  startsAt: string
  endsAt: string
  location: string
  locationUrl: string
  bannerImageUrl: string
  bannerFocalX: number
  bannerFocalY: number
  bannerZoom: number
  capacity: string
  registrationDeadline: string
  bringOwnPc: boolean
  speakerName: string
  prerequisites: string
  teamSize: string
}

const emptyForm: FormState = {
  title: "",
  slug: "",
  description: "",
  type: "Other",
  startsAt: "",
  endsAt: "",
  location: "",
  locationUrl: "",
  bannerImageUrl: "",
  bannerFocalX: 50,
  bannerFocalY: 50,
  bannerZoom: 1,
  capacity: "",
  registrationDeadline: "",
  bringOwnPc: false,
  speakerName: "",
  prerequisites: "",
  teamSize: "",
}

function isoToInput(value?: string | null): string {
  if (!value) return ""
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return ""
  const pad = (n: number) => String(n).padStart(2, "0")
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(
    d.getMinutes(),
  )}`
}

function inputToIso(value: string): string {
  if (!value) return ""
  return new Date(value).toISOString()
}

function buildPayload(form: FormState): CreateEventPayload {
  return {
    title: form.title.trim(),
    slug: form.slug.trim() || undefined,
    description: form.description,
    type: form.type,
    startsAt: inputToIso(form.startsAt),
    endsAt: inputToIso(form.endsAt),
    location: form.location.trim(),
    locationUrl: form.locationUrl.trim() || null,
    bannerImageUrl: form.bannerImageUrl.trim() || null,
    bannerFocalX: form.bannerImageUrl.trim() ? form.bannerFocalX : null,
    bannerFocalY: form.bannerImageUrl.trim() ? form.bannerFocalY : null,
    bannerZoom: form.bannerImageUrl.trim() ? form.bannerZoom : null,
    capacity: form.capacity ? Number(form.capacity) : null,
    registrationDeadline: form.registrationDeadline ? inputToIso(form.registrationDeadline) : null,
    bringOwnPc: form.type === "Lan" ? form.bringOwnPc : null,
    speakerName: form.type === "Talk" ? form.speakerName.trim() || null : null,
    prerequisites: form.type === "Workshop" ? form.prerequisites.trim() || null : null,
    teamSize: form.type === "Hackathon" && form.teamSize ? Number(form.teamSize) : null,
  }
}

function mapEventDetailToFormState(detail: EventDetail): FormState {
  const b = normalizeBannerFraming(detail.bannerFocalX, detail.bannerFocalY, detail.bannerZoom)
  return {
    id: detail.id,
    title: detail.title,
    slug: detail.slug,
    description: detail.description,
    type: normalizeEventType(detail.type),
    startsAt: isoToInput(detail.startsAt),
    endsAt: isoToInput(detail.endsAt),
    location: detail.location,
    locationUrl: detail.locationUrl ?? "",
    bannerImageUrl: detail.bannerImageUrl ?? "",
    bannerFocalX: b.focalX,
    bannerFocalY: b.focalY,
    bannerZoom: b.zoom,
    capacity: detail.capacity != null ? String(detail.capacity) : "",
    registrationDeadline: isoToInput(detail.registrationDeadline),
    bringOwnPc: detail.bringOwnPc ?? false,
    speakerName: detail.speakerName ?? "",
    prerequisites: detail.prerequisites ?? "",
    teamSize: detail.teamSize != null ? String(detail.teamSize) : "",
  }
}

const EVENT_TYPES_ALL: EventType[] = ["Lan", "Workshop", "Talk", "Hackathon", "Other"]

/** API / ældre data kan afvige i casing — sikrer at LAN m.m. matcher og type-specifikke felter vises. */
function normalizeEventType(value: unknown): EventType {
  const s = String(value ?? "").trim()
  const exact = EVENT_TYPES_ALL.find((t) => t === s)
  if (exact) return exact
  const ci = EVENT_TYPES_ALL.find((t) => t.toLowerCase() === s.toLowerCase())
  return ci ?? "Other"
}

export default function EventsAdminPage() {
  const [events, setEvents] = useState<EventListItem[]>([])
  const [loading, setLoading] = useState(true)
  const [actionMessage, setActionMessage] = useState<{ ok: boolean; text: string } | null>(null)
  const [showForm, setShowForm] = useState(false)
  const [form, setForm] = useState<FormState>(emptyForm)
  const [submitting, setSubmitting] = useState(false)
  const [bannerUploading, setBannerUploading] = useState(false)
  const [editLoading, setEditLoading] = useState(false)
  const [busyId, setBusyId] = useState<string | null>(null)
  const [registrationsForId, setRegistrationsForId] = useState<string | null>(null)
  const [registrations, setRegistrations] = useState<EventRegistration[]>([])
  const [actionsHelpOpen, setActionsHelpOpen] = useState(false)
  const [showPastEvents, setShowPastEvents] = useState(false)
  const actionsHelpCloseTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  const cancelActionsHelpClose = useCallback(() => {
    if (actionsHelpCloseTimerRef.current != null) {
      clearTimeout(actionsHelpCloseTimerRef.current)
      actionsHelpCloseTimerRef.current = null
    }
  }, [])

  const scheduleActionsHelpClose = useCallback(() => {
    cancelActionsHelpClose()
    actionsHelpCloseTimerRef.current = setTimeout(() => setActionsHelpOpen(false), 200)
  }, [cancelActionsHelpClose])

  useEffect(() => {
    return () => cancelActionsHelpClose()
  }, [cancelActionsHelpClose])

  const loadEvents = useCallback(async () => {
    setLoading(true)
    try {
      const data = await listAllEventsForAdmin()
      setEvents(data)
    } catch (err) {
      setActionMessage({
        ok: false,
        text: err instanceof Error ? err.message : "Kunne ikke hente events.",
      })
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void loadEvents()
  }, [loadEvents])

  const sortedEvents = useMemo(
    () => [...events].sort((a, b) => new Date(b.startsAt).getTime() - new Date(a.startsAt).getTime()),
    [events],
  )

  const { visibleEvents, pastCount } = useMemo(() => {
    const past = sortedEvents.filter((e) => isEventPastByEndsAt(e.endsAt))
    const visible = showPastEvents ? sortedEvents : sortedEvents.filter((e) => !isEventPastByEndsAt(e.endsAt))
    return { visibleEvents: visible, pastCount: past.length }
  }, [sortedEvents, showPastEvents])

  const openCreate = () => {
    setForm(emptyForm)
    setShowForm(true)
  }

  const openEdit = async (ev: EventListItem) => {
    setEditLoading(true)
    setActionMessage(null)
    try {
      const detail = await getEventDetailForAdmin(ev.id)
      setForm(mapEventDetailToFormState(detail))
      setShowForm(true)
    } catch (err) {
      setActionMessage({
        ok: false,
        text: err instanceof Error ? err.message : "Kunne ikke hente event til redigering.",
      })
    } finally {
      setEditLoading(false)
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setSubmitting(true)
    setActionMessage(null)
    try {
      const payload = buildPayload(form)
      if (!payload.title) throw new Error("Titel er påkrævet.")
      if (!payload.startsAt || !payload.endsAt) throw new Error("Start- og slut-tidspunkt er påkrævet.")
      if (new Date(payload.endsAt) <= new Date(payload.startsAt)) {
        throw new Error("Sluttidspunkt skal være efter starttidspunkt.")
      }

      if (form.id) {
        await updateEvent(form.id, payload)
        setActionMessage({ ok: true, text: "Event opdateret." })
      } else {
        await createEvent(payload)
        setActionMessage({ ok: true, text: "Event oprettet (status: Draft)." })
      }
      setShowForm(false)
      await loadEvents()
    } catch (err) {
      setActionMessage({
        ok: false,
        text: err instanceof Error ? err.message : "Handlingen mislykkedes.",
      })
    } finally {
      setSubmitting(false)
    }
  }

  const handlePublish = async (id: string) => {
    setBusyId(id)
    setActionMessage(null)
    try {
      await publishEvent(id)
      setActionMessage({ ok: true, text: "Event publiceret og annonceret på Discord." })
      await loadEvents()
    } catch (err) {
      setActionMessage({ ok: false, text: err instanceof Error ? err.message : "Publicering fejlede." })
    } finally {
      setBusyId(null)
    }
  }

  const handleCancel = async (id: string) => {
    if (!confirm("Er du sikker på at du vil aflyse dette event?")) return
    setBusyId(id)
    setActionMessage(null)
    try {
      await cancelEvent(id)
      setActionMessage({ ok: true, text: "Event aflyst." })
      await loadEvents()
    } catch (err) {
      setActionMessage({ ok: false, text: err instanceof Error ? err.message : "Aflysning fejlede." })
    } finally {
      setBusyId(null)
    }
  }

  const handleDelete = async (id: string) => {
    if (!confirm("Slet dette event permanent? Tilmeldinger slettes også.")) return
    setBusyId(id)
    setActionMessage(null)
    try {
      await deleteEvent(id)
      setActionMessage({ ok: true, text: "Event slettet." })
      await loadEvents()
    } catch (err) {
      setActionMessage({ ok: false, text: err instanceof Error ? err.message : "Sletning fejlede." })
    } finally {
      setBusyId(null)
    }
  }

  const openRegistrations = async (id: string) => {
    setRegistrationsForId(id)
    setRegistrations([])
    try {
      const data = await getAdminRegistrations(id)
      setRegistrations(data)
    } catch (err) {
      setActionMessage({
        ok: false,
        text: err instanceof Error ? err.message : "Kunne ikke hente tilmeldinger.",
      })
      setRegistrationsForId(null)
    }
  }

  return (
    <Layout_alt>
      <section className="container-shell py-12 sm:py-16">
        <div className="mb-8 flex flex-wrap items-start justify-between gap-4">
          <div>
            <h1 className="text-3xl font-bold tracking-[-0.02em] text-slate-900 sm:text-4xl">
              Events – administration
            </h1>
            <p className="mt-3 text-slate-600">
              Opret events, publicer dem til Discord og hold styr på tilmeldinger.
            </p>
          </div>
          <div className="flex gap-2">
            <Button variant="outline" onClick={() => void loadEvents()}>
              Opdater
            </Button>
            <Button onClick={openCreate}>
              <Plus className="mr-1 h-4 w-4" />
              Nyt event
            </Button>
          </div>
        </div>

        {actionMessage && (
          <div
            className={`mb-6 rounded-lg border px-4 py-3 text-sm ${
              actionMessage.ok
                ? "border-emerald-200 bg-emerald-50 text-emerald-800"
                : "border-red-200 bg-red-50 text-red-700"
            }`}
            role="status"
          >
            {actionMessage.text}
          </div>
        )}

        <Card className="overflow-hidden border-slate-100">
          {loading ? (
            <div className="p-6 text-sm text-slate-500">Henter events…</div>
          ) : sortedEvents.length === 0 ? (
            <div className="p-6 text-sm text-slate-500">Ingen events oprettet endnu.</div>
          ) : visibleEvents.length === 0 && !showPastEvents ? (
            <div className="space-y-4 p-6">
              <p className="text-sm text-slate-600">
                Alle {sortedEvents.length} event{sortedEvents.length === 1 ? "" : "s"} er afsluttet (slutdato passeret).
                Slå «Vis tidligere events» til for at se dem i listen.
              </p>
              <label className="inline-flex cursor-pointer items-center gap-2 text-sm text-slate-800">
                <input
                  type="checkbox"
                  className="h-4 w-4 rounded border-slate-300 text-indigo-600 focus:ring-indigo-500"
                  checked={showPastEvents}
                  onChange={(e) => setShowPastEvents(e.target.checked)}
                />
                Vis tidligere events
              </label>
            </div>
          ) : (
            <div>
              <div className="flex flex-wrap items-center justify-between gap-3 border-b border-slate-100 bg-slate-50/80 px-4 py-3">
                <label className="inline-flex cursor-pointer items-center gap-2 text-sm text-slate-800">
                  <input
                    type="checkbox"
                    className="h-4 w-4 rounded border-slate-300 text-indigo-600 focus:ring-indigo-500"
                    checked={showPastEvents}
                    onChange={(e) => setShowPastEvents(e.target.checked)}
                  />
                  Vis tidligere events
                </label>
                {!showPastEvents && pastCount > 0 ? (
                  <span className="text-xs text-slate-500">
                    {pastCount} afsluttet{pastCount === 1 ? "" : "e"} skjult — tænd for at vise alle.
                  </span>
                ) : showPastEvents && pastCount > 0 ? (
                  <span className="text-xs text-slate-500">
                    Viser alle {sortedEvents.length} — {pastCount} afsluttet{pastCount === 1 ? "" : "e"}.
                  </span>
                ) : (
                  <span className="text-xs text-slate-500">Kun kommende / igangværende events.</span>
                )}
              </div>
              <div className="overflow-x-auto">
              <table className="w-full text-left text-sm">
                <thead className="border-b border-slate-100 bg-slate-50 text-xs uppercase text-slate-500">
                  <tr>
                    <th className="px-4 py-3">Titel</th>
                    <th className="px-4 py-3">Type</th>
                    <th className="px-4 py-3">Status</th>
                    <th className="px-4 py-3">Start</th>
                    <th className="px-4 py-3">Tilmeldte</th>
                    <th className="px-4 py-3 text-right align-bottom">
                      <div className="flex justify-end">
                        <Popover open={actionsHelpOpen} onOpenChange={setActionsHelpOpen} modal={false}>
                          <PopoverTrigger asChild>
                            <button
                              type="button"
                              className="inline-flex cursor-help items-center gap-1.5 rounded-md px-1 py-0.5 text-xs font-semibold uppercase tracking-wide text-slate-500 outline-none transition-colors hover:text-indigo-700 focus-visible:ring-2 focus-visible:ring-indigo-500 focus-visible:ring-offset-2"
                              aria-expanded={actionsHelpOpen}
                              aria-label="Forklaring af handlinger i tabellen"
                              onMouseEnter={() => {
                                cancelActionsHelpClose()
                                setActionsHelpOpen(true)
                              }}
                              onMouseLeave={() => scheduleActionsHelpClose()}
                            >
                              Handlinger
                              <HelpCircle className="h-3.5 w-3.5 shrink-0 text-slate-400" aria-hidden />
                            </button>
                          </PopoverTrigger>
                          <PopoverContent
                            side="top"
                            align="end"
                            sideOffset={8}
                            className="w-[min(22rem,calc(100vw-2rem))] border-slate-200 p-3 text-left text-xs leading-snug text-slate-700 shadow-lg"
                            onMouseEnter={cancelActionsHelpClose}
                            onMouseLeave={scheduleActionsHelpClose}
                            onOpenAutoFocus={(e) => e.preventDefault()}
                          >
                            <p className="mb-2 font-semibold text-slate-900">Knapper i hver række</p>
                            <ul className="list-inside list-disc space-y-1.5 text-slate-600">
                              <li>
                                <span className="font-medium text-slate-800">Personer</span> — se tilmeldinger
                              </li>
                              <li>
                                <span className="font-medium text-slate-800">Blyant</span> — rediger event
                              </li>
                              <li>
                                <span className="font-medium text-slate-800">Send</span> — publicer (kun kladder) og
                                annoncer på Discord
                              </li>
                              <li>
                                <span className="font-medium text-slate-800">Forbud</span> — aflys event
                              </li>
                              <li>
                                <span className="font-medium text-slate-800">Skrald</span> — slet permanent
                              </li>
                            </ul>
                          </PopoverContent>
                        </Popover>
                      </div>
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {visibleEvents.map((ev) => {
                    const past = isEventPastByEndsAt(ev.endsAt)
                    return (
                    <tr
                      key={ev.id}
                      className={`border-b border-slate-100 last:border-b-0 ${past && showPastEvents ? "bg-slate-50/60" : ""}`}
                    >
                      <td className="px-4 py-3 font-medium text-slate-900">{ev.title}</td>
                      <td className="px-4 py-3">
                        <span
                          className={`rounded-full px-2 py-0.5 text-xs font-semibold ${getEventTypeBadgeClasses(
                            ev.type,
                          )}`}
                        >
                          {getEventTypeLabel(ev.type)}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-slate-600">{ev.status}</td>
                      <td className="px-4 py-3 text-slate-600">{formatEventDateTime(ev.startsAt)}</td>
                      <td className="px-4 py-3 text-slate-600">
                        {ev.registrationCount}
                        {ev.capacity ? `/${ev.capacity}` : ""}
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex flex-wrap items-center justify-end gap-1.5">
                          <Button
                            size="sm"
                            variant="outline"
                            title="Se tilmeldinger"
                            aria-label="Se tilmeldinger"
                            onClick={() => openRegistrations(ev.id)}
                          >
                            <Users className="h-3.5 w-3.5" />
                          </Button>
                          <Button
                            size="sm"
                            variant="outline"
                            title="Rediger event"
                            aria-label="Rediger event"
                            disabled={editLoading || busyId === ev.id}
                            onClick={() => void openEdit(ev)}
                          >
                            <Pencil className="h-3.5 w-3.5" />
                          </Button>
                          {ev.status !== "Published" && ev.status !== "Cancelled" && (
                            <Button
                              size="sm"
                              variant="outline"
                              title="Publicer event og annoncer på Discord"
                              aria-label="Publicer event og annoncer på Discord"
                              disabled={busyId === ev.id}
                              onClick={() => void handlePublish(ev.id)}
                            >
                              <Send className="h-3.5 w-3.5" />
                            </Button>
                          )}
                          {ev.status !== "Cancelled" && (
                            <Button
                              size="sm"
                              variant="outline"
                              title="Aflys event"
                              aria-label="Aflys event"
                              disabled={busyId === ev.id}
                              onClick={() => void handleCancel(ev.id)}
                            >
                              <Ban className="h-3.5 w-3.5" />
                            </Button>
                          )}
                          <Button
                            size="sm"
                            variant="outline"
                            title="Slet event permanent"
                            aria-label="Slet event permanent"
                            disabled={busyId === ev.id}
                            onClick={() => void handleDelete(ev.id)}
                          >
                            <Trash2 className="h-3.5 w-3.5" />
                          </Button>
                        </div>
                      </td>
                    </tr>
                    )
                  })}
                </tbody>
              </table>
              </div>
            </div>
          )}
        </Card>

        {showForm && (
          <div
            className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 p-4"
            onClick={() => setShowForm(false)}
          >
            <Card
              className="w-full max-w-3xl border-slate-100 p-6"
              onClick={(e: React.MouseEvent) => e.stopPropagation()}
            >
              <div className="mb-4 flex items-start justify-between">
                <h3 className="text-lg font-bold text-slate-900">
                  {form.id ? "Rediger event" : "Nyt event"}
                </h3>
                <button
                  className="rounded-md p-1 text-slate-500 hover:bg-slate-100"
                  onClick={() => setShowForm(false)}
                  type="button"
                  aria-label="Luk"
                >
                  <X className="h-4 w-4" />
                </button>
              </div>

              <form onSubmit={handleSubmit} className="grid gap-3 sm:grid-cols-2">
                <Field label="Titel" required className="sm:col-span-2">
                  <input
                    type="text"
                    required
                    value={form.title}
                    onChange={(e) => setForm({ ...form, title: e.target.value })}
                    className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                  />
                </Field>
                <Field label="Slug (valgfri)">
                  <input
                    type="text"
                    value={form.slug}
                    onChange={(e) => setForm({ ...form, slug: e.target.value })}
                    placeholder="auto-genereres fra titel"
                    className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                  />
                </Field>
                <Field label="Type" required>
                  <select
                    value={form.type}
                    onChange={(e) => setForm({ ...form, type: normalizeEventType(e.target.value) })}
                    className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                  >
                    {EVENT_TYPE_OPTIONS.map((opt) => (
                      <option key={opt.value} value={opt.value}>
                        {opt.label}
                      </option>
                    ))}
                  </select>
                </Field>

                {form.type === "Lan" && (
                  <Field label="Medbring egen PC?" className="sm:col-span-2">
                    <label className="inline-flex cursor-pointer items-center gap-2 text-sm text-slate-800">
                      <input
                        type="checkbox"
                        className="h-4 w-4 rounded border-slate-300 text-indigo-600 focus:ring-indigo-500"
                        checked={form.bringOwnPc}
                        onChange={(e) => setForm({ ...form, bringOwnPc: e.target.checked })}
                      />
                      Ja, deltagerne skal medbringe egen PC
                    </label>
                  </Field>
                )}
                {form.type === "Talk" && (
                  <Field label="Oplægsholder" className="sm:col-span-2">
                    <input
                      type="text"
                      value={form.speakerName}
                      onChange={(e) => setForm({ ...form, speakerName: e.target.value })}
                      className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                    />
                  </Field>
                )}
                {form.type === "Workshop" && (
                  <Field label="Forudsætninger (valgfri)" className="sm:col-span-2">
                    <textarea
                      rows={3}
                      value={form.prerequisites}
                      onChange={(e) => setForm({ ...form, prerequisites: e.target.value })}
                      className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                    />
                  </Field>
                )}
                {form.type === "Hackathon" && (
                  <Field label="Holdstørrelse (valgfri)">
                    <input
                      type="number"
                      min={1}
                      value={form.teamSize}
                      onChange={(e) => setForm({ ...form, teamSize: e.target.value })}
                      className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                    />
                  </Field>
                )}

                <Field label="Start" required>
                  <input
                    type="datetime-local"
                    required
                    value={form.startsAt}
                    onChange={(e) => setForm({ ...form, startsAt: e.target.value })}
                    className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                  />
                </Field>
                <Field label="Slut" required>
                  <input
                    type="datetime-local"
                    required
                    value={form.endsAt}
                    onChange={(e) => setForm({ ...form, endsAt: e.target.value })}
                    className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                  />
                </Field>
                <Field label="Tilmeldingsfrist (valgfri)">
                  <input
                    type="datetime-local"
                    value={form.registrationDeadline}
                    onChange={(e) => setForm({ ...form, registrationDeadline: e.target.value })}
                    className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                  />
                </Field>
                <Field label="Kapacitet (valgfri)">
                  <input
                    type="number"
                    min={1}
                    value={form.capacity}
                    onChange={(e) => setForm({ ...form, capacity: e.target.value })}
                    placeholder="ubegrænset"
                    className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                  />
                </Field>
                <Field label="Lokation" required className="sm:col-span-2">
                  <input
                    type="text"
                    required
                    value={form.location}
                    onChange={(e) => setForm({ ...form, location: e.target.value })}
                    className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                  />
                </Field>
                <Field label="Lokations-URL (valgfri)">
                  <input
                    type="url"
                    value={form.locationUrl}
                    onChange={(e) => setForm({ ...form, locationUrl: e.target.value })}
                    className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                  />
                </Field>
                <Field label="Banner (valgfri)" className="sm:col-span-2">
                  <div className="flex flex-col gap-2">
                    <input
                      type="file"
                      accept="image/jpeg,image/png,image/gif,image/webp"
                      disabled={bannerUploading}
                      onChange={async (e) => {
                        const f = e.target.files?.[0]
                        e.target.value = ""
                        if (!f) return
                        setBannerUploading(true)
                        setActionMessage(null)
                        try {
                          const url = await uploadEventBannerImage(f)
                          setForm((prev) => ({
                            ...prev,
                            bannerImageUrl: url,
                            bannerFocalX: 50,
                            bannerFocalY: 50,
                            bannerZoom: 1,
                          }))
                          setActionMessage({ ok: true, text: "Banner uploadet — husk at gemme eventet." })
                        } catch (err) {
                          setActionMessage({
                            ok: false,
                            text: err instanceof Error ? err.message : "Upload fejlede.",
                          })
                        } finally {
                          setBannerUploading(false)
                        }
                      }}
                      className="block w-full text-sm text-slate-600 file:mr-3 file:rounded-md file:border-0 file:bg-indigo-50 file:px-3 file:py-1.5 file:text-sm file:font-medium file:text-indigo-700 hover:file:bg-indigo-100"
                    />
                    <p className="text-xs text-slate-500">
                      Upload til MinIO (S3), max ca. 5 MB — JPEG, PNG, GIF eller WebP. Du kan også indsætte en
                      direkte URL nedenfor.
                    </p>
                    <input
                      type="url"
                      value={form.bannerImageUrl}
                      onChange={(e) => {
                        const v = e.target.value
                        setForm((prev) => ({
                          ...prev,
                          bannerImageUrl: v,
                          ...(v.trim()
                            ? {}
                            : { bannerFocalX: 50, bannerFocalY: 50, bannerZoom: 1 }),
                        }))
                      }}
                      placeholder="https://… (eller brug fil-upload)"
                      className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                    />
                    {form.bannerImageUrl ? (
                      <BannerFocalEditor
                        imageUrl={form.bannerImageUrl}
                        value={{
                          focalX: form.bannerFocalX,
                          focalY: form.bannerFocalY,
                          zoom: form.bannerZoom,
                        }}
                        onChange={({ focalX, focalY, zoom }) =>
                          setForm((prev) => ({ ...prev, bannerFocalX: focalX, bannerFocalY: focalY, bannerZoom: zoom }))
                        }
                      />
                    ) : null}
                  </div>
                </Field>
                <Field label="Beskrivelse" required className="sm:col-span-2">
                  <textarea
                    rows={6}
                    required
                    value={form.description}
                    onChange={(e) => setForm({ ...form, description: e.target.value })}
                    className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                  />
                </Field>

                <div className="flex justify-end gap-2 pt-2 sm:col-span-2">
                  <Button type="button" variant="outline" onClick={() => setShowForm(false)} disabled={submitting}>
                    Annuller
                  </Button>
                  <Button type="submit" disabled={submitting}>
                    {submitting ? "Gemmer…" : form.id ? "Gem ændringer" : "Opret event"}
                  </Button>
                </div>
              </form>
            </Card>
          </div>
        )}

        {registrationsForId && (
          <div
            className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 p-4"
            onClick={() => setRegistrationsForId(null)}
          >
            <Card
              className="w-full max-w-xl border-slate-100 p-6"
              onClick={(e: React.MouseEvent) => e.stopPropagation()}
            >
              <div className="mb-4 flex items-start justify-between">
                <h3 className="text-lg font-bold text-slate-900">Tilmeldinger</h3>
                <button
                  className="rounded-md p-1 text-slate-500 hover:bg-slate-100"
                  onClick={() => setRegistrationsForId(null)}
                  type="button"
                  aria-label="Luk"
                >
                  <X className="h-4 w-4" />
                </button>
              </div>
              {registrations.length === 0 ? (
                <p className="text-sm text-slate-500">Ingen tilmeldinger endnu.</p>
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full text-left text-sm">
                    <thead className="border-b border-slate-100 text-xs uppercase text-slate-500">
                      <tr>
                        <th className="px-2 py-2">Navn</th>
                        <th className="px-2 py-2">Email</th>
                        <th className="px-2 py-2">Tilmeldt</th>
                        <th className="px-2 py-2">Kilde</th>
                      </tr>
                    </thead>
                    <tbody>
                      {registrations.map((r, i) => (
                        <tr key={i} className="border-b border-slate-100 last:border-b-0">
                          <td className="px-2 py-2">{r.displayName}</td>
                          <td className="px-2 py-2">{r.email}</td>
                          <td className="px-2 py-2 text-slate-600">{formatEventDateTime(r.registeredAt)}</td>
                          <td className="px-2 py-2 text-slate-600">{r.source}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </Card>
          </div>
        )}
      </section>
    </Layout_alt>
  )
}

function Field({
  label,
  required,
  className,
  children,
}: {
  label: string
  required?: boolean
  className?: string
  children: React.ReactNode
}) {
  return (
    <div className={className}>
      <label className="mb-1 block text-xs font-semibold uppercase tracking-wide text-slate-500">
        {label} {required && <span className="text-red-500">*</span>}
      </label>
      {children}
    </div>
  )
}
