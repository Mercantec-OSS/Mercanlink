import { useCallback, useEffect, useState } from "react"
import { Link, useNavigate, useParams } from "react-router-dom"
import Layout_alt from "@/components/templates/layout"
import { Button } from "@/components/ui/button"
import { Card } from "@/components/ui/card"
import {
  CalendarDays,
  MapPin,
  Users,
  Clock4,
  ChevronLeft,
  Download,
  Mic2,
  Laptop,
  CheckCircle2,
  AlertTriangle,
} from "lucide-react"
import { useAuth } from "@/contexts/AuthContext"
import {
  getEvent,
  getEventIcsUrl,
  getEventRegistrations,
  registerForEvent,
  unregisterFromEvent,
  type EventDetail,
  type EventRegistration,
} from "@/services/eventsService"
import {
  formatEventDateTime,
  formatEventRange,
  getEventTypeBadgeClasses,
  getEventTypeLabel,
} from "@/lib/eventFormatting"

export default function EventDetailPage() {
  const { slug } = useParams<{ slug: string }>()
  const navigate = useNavigate()
  const { isAuthenticated, login, user } = useAuth()

  const [event, setEvent] = useState<EventDetail | null>(null)
  const [registrations, setRegistrations] = useState<EventRegistration[] | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const [showDialog, setShowDialog] = useState(false)
  const [displayName, setDisplayName] = useState("")
  const [email, setEmail] = useState("")
  const [submitting, setSubmitting] = useState(false)
  const [actionMessage, setActionMessage] = useState<{ ok: boolean; text: string } | null>(null)
  const [imRegistered, setImRegistered] = useState(false)

  const loadEvent = useCallback(async () => {
    if (!slug) return
    setLoading(true)
    setError(null)
    try {
      const data = await getEvent(slug)
      setEvent(data)
    } catch (err) {
      setError(err instanceof Error ? err.message : "Kunne ikke hente event.")
    } finally {
      setLoading(false)
    }
  }, [slug])

  const loadRegistrations = useCallback(async () => {
    if (!slug || !isAuthenticated) {
      setRegistrations(null)
      return
    }
    try {
      const data = await getEventRegistrations(slug)
      setRegistrations(data)
      const myEmail = (user?.email || "").toLowerCase()
      const myName = (user?.username || "").toLowerCase()
      setImRegistered(
        data.some(
          (r) =>
            (myEmail && r.email.toLowerCase() === myEmail) ||
            (myName && r.displayName.toLowerCase() === myName),
        ),
      )
    } catch {
      setRegistrations([])
    }
  }, [slug, isAuthenticated, user?.email, user?.username])

  useEffect(() => {
    void loadEvent()
  }, [loadEvent])

  useEffect(() => {
    void loadRegistrations()
  }, [loadRegistrations])

  useEffect(() => {
    setDisplayName(user?.globalName || user?.username || "")
    setEmail(user?.email || "")
  }, [user?.globalName, user?.username, user?.email])

  const openDialog = () => {
    if (!isAuthenticated) {
      void login()
      return
    }
    setActionMessage(null)
    setShowDialog(true)
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!slug) return
    setSubmitting(true)
    setActionMessage(null)
    try {
      await registerForEvent(slug, {
        confirmedDisplayName: displayName.trim(),
        confirmedEmail: email.trim(),
      })
      setActionMessage({ ok: true, text: "Du er tilmeldt." })
      setShowDialog(false)
      await loadEvent()
      await loadRegistrations()
    } catch (err) {
      setActionMessage({
        ok: false,
        text: err instanceof Error ? err.message : "Tilmelding mislykkedes.",
      })
    } finally {
      setSubmitting(false)
    }
  }

  const handleUnregister = async () => {
    if (!slug) return
    setSubmitting(true)
    setActionMessage(null)
    try {
      await unregisterFromEvent(slug)
      setActionMessage({ ok: true, text: "Du er frameldt." })
      await loadEvent()
      await loadRegistrations()
    } catch (err) {
      setActionMessage({
        ok: false,
        text: err instanceof Error ? err.message : "Frameldning mislykkedes.",
      })
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <Layout_alt>
      <section className="container-shell py-10 sm:py-14">
        <Button
          type="button"
          variant="outline"
          size="sm"
          className="mb-6"
          onClick={() => navigate("/events")}
        >
          <ChevronLeft className="mr-1 h-4 w-4" />
          Tilbage til events
        </Button>

        {loading ? (
          <p className="text-sm text-slate-500">Henter event…</p>
        ) : error ? (
          <Card className="border-red-100 bg-red-50 p-5 text-sm text-red-700">{error}</Card>
        ) : !event ? (
          <Card className="border-slate-100 bg-slate-50 p-5 text-sm text-slate-600">
            Eventet blev ikke fundet.
          </Card>
        ) : (
          <div className="grid gap-8 lg:grid-cols-3">
            <div className="lg:col-span-2">
              <Card className="overflow-hidden border-slate-100">
                {event.bannerImageUrl ? (
                  <img
                    src={event.bannerImageUrl}
                    alt={event.title}
                    className="aspect-[16/7] w-full object-cover"
                  />
                ) : (
                  <div className="aspect-[16/7] w-full bg-gradient-to-br from-indigo-100 via-violet-100 to-pink-100" />
                )}
                <div className="p-6 sm:p-8">
                  <div className="mb-3 flex flex-wrap items-center gap-2">
                    <span
                      className={`rounded-full px-2.5 py-0.5 text-xs font-semibold ${getEventTypeBadgeClasses(
                        event.type,
                      )}`}
                    >
                      {getEventTypeLabel(event.type)}
                    </span>
                    {event.status === "Cancelled" && (
                      <span className="inline-flex items-center gap-1 rounded-full bg-red-100 px-2.5 py-0.5 text-xs font-semibold text-red-800">
                        <AlertTriangle className="h-3 w-3" /> Aflyst
                      </span>
                    )}
                  </div>
                  <h1 className="text-3xl font-extrabold tracking-[-0.02em] text-slate-900 sm:text-4xl">
                    {event.title}
                  </h1>
                  <div className="mt-6 grid gap-3 text-sm text-slate-700 sm:grid-cols-2">
                    <div className="flex items-center gap-2">
                      <CalendarDays className="h-4 w-4 text-indigo-500" />
                      {formatEventRange(event.startsAt, event.endsAt)}
                    </div>
                    <div className="flex items-center gap-2">
                      <MapPin className="h-4 w-4 text-indigo-500" />
                      {event.locationUrl ? (
                        <a
                          href={event.locationUrl}
                          target="_blank"
                          rel="noopener noreferrer"
                          className="text-indigo-700 hover:underline"
                        >
                          {event.location || "Mercantec"}
                        </a>
                      ) : (
                        <span>{event.location || "Mercantec"}</span>
                      )}
                    </div>
                    <div className="flex items-center gap-2">
                      <Users className="h-4 w-4 text-indigo-500" />
                      {event.capacity
                        ? `${event.registrationCount}/${event.capacity} tilmeldte`
                        : `${event.registrationCount} tilmeldte`}
                    </div>
                    {event.registrationDeadline && (
                      <div className="flex items-center gap-2">
                        <Clock4 className="h-4 w-4 text-indigo-500" />
                        Tilmelding senest {formatEventDateTime(event.registrationDeadline)}
                      </div>
                    )}
                    {event.speakerName && (
                      <div className="flex items-center gap-2">
                        <Mic2 className="h-4 w-4 text-indigo-500" />
                        Oplægsholder: {event.speakerName}
                      </div>
                    )}
                    {event.bringOwnPc && (
                      <div className="flex items-center gap-2">
                        <Laptop className="h-4 w-4 text-indigo-500" />
                        Medbring egen PC
                      </div>
                    )}
                    {event.teamSize && (
                      <div className="flex items-center gap-2">
                        <Users className="h-4 w-4 text-indigo-500" />
                        Holdstørrelse: {event.teamSize}
                      </div>
                    )}
                  </div>

                  <div className="prose prose-slate mt-8 max-w-none whitespace-pre-wrap text-slate-700">
                    {event.description}
                  </div>

                  {event.prerequisites && (
                    <div className="mt-6 rounded-lg border border-slate-100 bg-slate-50 p-4 text-sm text-slate-700">
                      <p className="font-semibold text-slate-900">Forudsætninger</p>
                      <p className="mt-1 whitespace-pre-wrap">{event.prerequisites}</p>
                    </div>
                  )}
                </div>
              </Card>
            </div>

            <aside className="space-y-5">
              <Card className="border-slate-100 p-5">
                <h2 className="text-lg font-bold text-slate-900">Tilmelding</h2>
                {event.status === "Cancelled" ? (
                  <p className="mt-2 text-sm text-slate-600">Eventet er aflyst.</p>
                ) : event.registrationOpen ? (
                  <p className="mt-2 text-sm text-slate-600">
                    {event.capacity
                      ? `${event.capacity - event.registrationCount} pladser tilbage.`
                      : "Der er stadig pladser."}
                  </p>
                ) : (
                  <p className="mt-2 text-sm text-slate-600">
                    {event.isFull
                      ? "Eventet er fyldt op."
                      : "Tilmelding er ikke åben."}
                  </p>
                )}

                <div className="mt-4 flex flex-col gap-2">
                  {imRegistered ? (
                    <Button variant="outline" disabled={submitting} onClick={() => void handleUnregister()}>
                      {submitting ? "Arbejder…" : "Frameld"}
                    </Button>
                  ) : (
                    <Button
                      disabled={!event.registrationOpen || submitting}
                      onClick={openDialog}
                    >
                      {!isAuthenticated ? "Log ind og tilmeld dig" : "Tilmeld dig"}
                    </Button>
                  )}
                  <a href={getEventIcsUrl(event.slug)} download={`${event.slug}.ics`}>
                    <Button variant="outline" type="button" className="w-full">
                      <Download className="mr-1 h-4 w-4" />
                      Tilføj til kalender (.ics)
                    </Button>
                  </a>
                </div>

                {actionMessage && (
                  <p
                    className={`mt-3 inline-flex items-center gap-1 text-sm ${
                      actionMessage.ok ? "text-emerald-700" : "text-red-600"
                    }`}
                    role="status"
                  >
                    {actionMessage.ok && <CheckCircle2 className="h-4 w-4" />}
                    {actionMessage.text}
                  </p>
                )}
              </Card>

              <Card className="border-slate-100 p-5">
                <h2 className="text-lg font-bold text-slate-900">Tilmeldte</h2>
                {!isAuthenticated ? (
                  <p className="mt-3 text-sm text-slate-600">
                    <Link to="/login" className="text-indigo-700 hover:underline">
                      Log ind
                    </Link>{" "}
                    for at se hvem der har tilmeldt sig.
                  </p>
                ) : registrations === null ? (
                  <p className="mt-3 text-sm text-slate-500">Henter…</p>
                ) : registrations.length === 0 ? (
                  <p className="mt-3 text-sm text-slate-500">Ingen tilmeldinger endnu.</p>
                ) : (
                  <ul className="mt-3 max-h-72 space-y-1.5 overflow-y-auto text-sm text-slate-700">
                    {registrations.map((r, i) => (
                      <li key={`${r.email}-${i}`} className="flex items-center justify-between gap-2">
                        <span>{r.displayName}</span>
                        <span className="rounded-full bg-slate-100 px-2 py-0.5 text-xs text-slate-600">
                          {r.source === "Discord" ? "Discord" : "Web"}
                        </span>
                      </li>
                    ))}
                  </ul>
                )}
              </Card>
            </aside>
          </div>
        )}

        {showDialog && event && (
          <div
            className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 p-4"
            onClick={() => setShowDialog(false)}
          >
            <Card
              className="w-full max-w-md border-slate-100 p-6"
              onClick={(e: React.MouseEvent) => e.stopPropagation()}
            >
              <h3 className="text-lg font-bold text-slate-900">Bekræft tilmelding</h3>
              <p className="mt-1 text-sm text-slate-600">
                Bekræft dit navn og din Mercantec edu-mail. Disse oplysninger sendes til arrangørerne.
              </p>
              <form onSubmit={handleSubmit} className="mt-4 space-y-3">
                <div>
                  <label className="text-xs font-semibold uppercase tracking-wide text-slate-500">Navn</label>
                  <input
                    type="text"
                    required
                    value={displayName}
                    onChange={(e) => setDisplayName(e.target.value)}
                    className="mt-1 w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                  />
                </div>
                <div>
                  <label className="text-xs font-semibold uppercase tracking-wide text-slate-500">Edu-mail</label>
                  <input
                    type="email"
                    required
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    placeholder="navn@edu.mercantec.dk"
                    className="mt-1 w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                  />
                </div>
                {actionMessage && !actionMessage.ok && (
                  <p className="text-sm text-red-600">{actionMessage.text}</p>
                )}
                <div className="flex justify-end gap-2 pt-2">
                  <Button type="button" variant="outline" onClick={() => setShowDialog(false)} disabled={submitting}>
                    Annuller
                  </Button>
                  <Button type="submit" disabled={submitting}>
                    {submitting ? "Tilmelder…" : "Bekræft tilmelding"}
                  </Button>
                </div>
              </form>
            </Card>
          </div>
        )}
      </section>
    </Layout_alt>
  )
}
