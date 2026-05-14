import { useEffect, useMemo, useState } from "react"
import { useNavigate } from "react-router-dom"
import Layout_alt from "@/components/templates/layout"
import { Button } from "@/components/ui/button"
import { Card } from "@/components/ui/card"
import {
  CalendarDays,
  MapPin,
  Users,
  Clock4,
  ArrowRight,
  Filter,
} from "lucide-react"
import {
  listEvents,
  type EventListItem,
  type EventListStatusFilter,
  type EventType,
} from "@/services/eventsService"
import {
  EVENT_TYPE_OPTIONS,
  formatEventRange,
  getEventTypeBadgeClasses,
  getEventTypeLabel,
} from "@/lib/eventFormatting"
import { EventBannerFrame } from "@/components/events/EventBannerFrame"

const STATUS_FILTERS: { value: EventListStatusFilter; label: string }[] = [
  { value: "upcoming", label: "Kommende" },
  { value: "past", label: "Tidligere" },
  { value: "all", label: "Alle" },
]

export default function EventsPage() {
  const navigate = useNavigate()
  const [events, setEvents] = useState<EventListItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [statusFilter, setStatusFilter] = useState<EventListStatusFilter>("upcoming")
  const [typeFilter, setTypeFilter] = useState<EventType | "all">("all")

  useEffect(() => {
    let cancelled = false
    setLoading(true)
    setError(null)

    listEvents({
      status: statusFilter,
      type: typeFilter === "all" ? undefined : typeFilter,
    })
      .then((data) => {
        if (!cancelled) setEvents(data)
      })
      .catch((err: unknown) => {
        if (!cancelled) setError(err instanceof Error ? err.message : "Kunne ikke hente events.")
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })

    return () => {
      cancelled = true
    }
  }, [statusFilter, typeFilter])

  const visibleEvents = useMemo(() => events.filter((e) => e.status !== "Draft"), [events])

  return (
    <Layout_alt>
      <section className="container-shell py-12 sm:py-16">
        <div className="mb-10 max-w-2xl">
          <h1 className="text-4xl font-extrabold tracking-[-0.02em] text-slate-900 sm:text-5xl">
            Næste <span className="brand-gradient-text">events</span>
          </h1>
          <p className="mt-4 text-base leading-7 text-slate-600">
            Hold dig opdateret på LANs, workshops, foredrag og hackathons. Tryk på et kort for detaljer og tilmelding.
          </p>
        </div>

        <div className="mb-6 flex flex-wrap items-center gap-3">
          <div className="inline-flex items-center gap-2 text-xs font-semibold uppercase tracking-wide text-slate-500">
            <Filter className="h-3.5 w-3.5" /> Status
          </div>
          {STATUS_FILTERS.map((s) => (
            <button
              key={s.value}
              onClick={() => setStatusFilter(s.value)}
              className={`rounded-full px-3 py-1.5 text-sm font-semibold transition ${
                statusFilter === s.value
                  ? "bg-indigo-600 text-white"
                  : "bg-white text-slate-700 hover:bg-slate-100 border border-slate-200"
              }`}
              type="button"
            >
              {s.label}
            </button>
          ))}
          <div className="ml-2 inline-flex items-center gap-2 text-xs font-semibold uppercase tracking-wide text-slate-500">
            Type
          </div>
          <button
            onClick={() => setTypeFilter("all")}
            className={`rounded-full px-3 py-1.5 text-sm font-semibold transition ${
              typeFilter === "all"
                ? "bg-slate-900 text-white"
                : "bg-white text-slate-700 hover:bg-slate-100 border border-slate-200"
            }`}
            type="button"
          >
            Alle
          </button>
          {EVENT_TYPE_OPTIONS.map((opt) => (
            <button
              key={opt.value}
              onClick={() => setTypeFilter(opt.value)}
              className={`rounded-full px-3 py-1.5 text-sm font-semibold transition ${
                typeFilter === opt.value
                  ? "bg-slate-900 text-white"
                  : "bg-white text-slate-700 hover:bg-slate-100 border border-slate-200"
              }`}
              type="button"
            >
              {opt.label}
            </button>
          ))}
        </div>

        {error && (
          <p className="mb-4 text-sm text-red-600" role="alert">
            {error}
          </p>
        )}

        {loading ? (
          <p className="text-sm text-slate-500">Henter events…</p>
        ) : visibleEvents.length === 0 ? (
          <Card className="border-slate-100 bg-slate-50 p-6 text-sm text-slate-600">
            Der er ingen events at vise for det valgte filter.
          </Card>
        ) : (
          <div className="grid gap-5 md:grid-cols-2 lg:grid-cols-3">
            {visibleEvents.map((ev) => {
              const isCancelled = ev.status === "Cancelled"
              return (
                <Card
                  key={ev.id}
                  onClick={() => navigate(`/events/${ev.slug}`)}
                  className={`soft-card soft-card-hover flex cursor-pointer flex-col overflow-hidden border-slate-100 ${
                    isCancelled ? "opacity-70" : ""
                  }`}
                >
                  {ev.bannerImageUrl ? (
                    <EventBannerFrame
                      url={ev.bannerImageUrl}
                      alt={ev.title}
                      focalX={ev.bannerFocalX}
                      focalY={ev.bannerFocalY}
                      zoom={ev.bannerZoom}
                      className="aspect-[16/8] w-full"
                    />
                  ) : (
                    <div className="aspect-[16/8] w-full bg-gradient-to-br from-indigo-100 via-violet-100 to-pink-100" />
                  )}

                  <div className="flex flex-1 flex-col p-5">
                    <div className="mb-3 flex flex-wrap items-center gap-2">
                      <span
                        className={`rounded-full px-2.5 py-0.5 text-xs font-semibold ${getEventTypeBadgeClasses(
                          ev.type,
                        )}`}
                      >
                        {getEventTypeLabel(ev.type)}
                      </span>
                      {isCancelled && (
                        <span className="rounded-full bg-red-100 px-2.5 py-0.5 text-xs font-semibold text-red-800">
                          Aflyst
                        </span>
                      )}
                    </div>
                    <h3 className="text-lg font-bold text-slate-900">{ev.title}</h3>

                    <ul className="mt-4 space-y-2 text-sm text-slate-600">
                      <li className="flex items-center gap-2">
                        <CalendarDays className="h-4 w-4 shrink-0 text-indigo-500" />
                        {formatEventRange(ev.startsAt, ev.endsAt)}
                      </li>
                      <li className="flex items-center gap-2">
                        <MapPin className="h-4 w-4 shrink-0 text-indigo-500" />
                        {ev.location || "Mercantec"}
                      </li>
                      <li className="flex items-center gap-2">
                        <Users className="h-4 w-4 shrink-0 text-indigo-500" />
                        {ev.capacity
                          ? `${ev.registrationCount}/${ev.capacity} tilmeldte`
                          : `${ev.registrationCount} tilmeldte`}
                      </li>
                      {ev.registrationDeadline && (
                        <li className="flex items-center gap-2">
                          <Clock4 className="h-4 w-4 shrink-0 text-indigo-500" />
                          Tilmelding senest {formatEventRange(ev.registrationDeadline, ev.registrationDeadline).split(" · ")[0]}
                        </li>
                      )}
                    </ul>

                    <div className="mt-5">
                      <Button
                        type="button"
                        size="sm"
                        variant="outline"
                        className="group"
                        onClick={(e) => {
                          e.stopPropagation()
                          navigate(`/events/${ev.slug}`)
                        }}
                      >
                        Se detaljer
                        <ArrowRight className="ml-1 h-4 w-4 transition-transform group-hover:translate-x-1" />
                      </Button>
                    </div>
                  </div>
                </Card>
              )
            })}
          </div>
        )}
      </section>
    </Layout_alt>
  )
}
