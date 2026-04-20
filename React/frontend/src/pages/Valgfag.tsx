import React, { useCallback, useEffect, useMemo, useState } from "react"
import Layout_alt from "@/components/templates/layout"
import { Bot, Gamepad2, Rocket, CalendarDays, UserRound, Clock4, GraduationCap, Users } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Card } from "@/components/ui/card"
import { useAuth } from "@/contexts/AuthContext"
import { apiClient } from "@/services/apiClient"

type ElectiveEnrollmentParticipantDto = {
  displayName: string
  enrolledAt: string
}

type ElectiveEnrollmentsGroupDto = {
  electiveKey: string
  participants: ElectiveEnrollmentParticipantDto[]
}

type Elective = {
  electiveKey: string
  name: string
  week: string
  date: string
  teacher: string
  duration: string
  recommended: string
  startDate: string
  endDate: string
  icon: React.ComponentType<{ className?: string }>
}

const electives: Elective[] = [
  {
    electiveKey: "deploy-or-die-2026",
    name: "Deploy Or Die - Fra kode til internet",
    week: "Uge 24, 25, 26",
    date: "08/06 - 26/06",
    teacher: "Mathias (MAGS)",
    duration: "3 uger",
    recommended: "H2",
    startDate: "2026-06-08",
    endDate: "2026-06-26",
    icon: Rocket,
  },
  {
    electiveKey: "game-design-2026",
    name: "Game Design",
    week: "Uge 39, 40, 41",
    date: "21/09 - 09/10",
    teacher: "Kasper (KASC)",
    duration: "3 uger",
    recommended: "H3",
    startDate: "2026-09-21",
    endDate: "2026-10-09",
    icon: Gamepad2,
  },
  {
    electiveKey: "machine-learning-2027",
    name: "Machine Learning (først i 2027)",
    week: "Uge 7, 8, 9",
    date: "15/02 - 05/03",
    teacher: "Mathias (MAGS)",
    duration: "3 uger",
    recommended: "H3",
    startDate: "2027-02-15",
    endDate: "2027-03-05",
    icon: Bot,
  },
]

const Valgfag: React.FC = () => {
  const { isAuthenticated, login } = useAuth()

  const [groups, setGroups] = useState<ElectiveEnrollmentsGroupDto[]>([])
  const [myKeys, setMyKeys] = useState<string[]>([])
  const [listLoading, setListLoading] = useState(true)
  const [listError, setListError] = useState<string | null>(null)
  const [pendingKey, setPendingKey] = useState<string | null>(null)
  const [actionHint, setActionHint] = useState<{ electiveKey: string; ok: boolean; text: string } | null>(null)

  const availableElectives = electives.filter((elective) => {
    const endDate = new Date(`${elective.endDate}T23:59:59`)
    const today = new Date()
    const todayStart = new Date(today.getFullYear(), today.getMonth(), today.getDate())
    return endDate >= todayStart
  })

  const participantsByKey = useMemo(() => {
    const m = new Map<string, ElectiveEnrollmentParticipantDto[]>()
    for (const g of groups) {
      m.set(g.electiveKey, g.participants)
    }
    return m
  }, [groups])

  const loadPublicEnrollments = useCallback(async () => {
    setListError(null)
    setListLoading(true)
    try {
      const data = await apiClient<ElectiveEnrollmentsGroupDto[]>("/valgfag/enrollments")
      setGroups(data)
    } catch (e) {
      setListError(e instanceof Error ? e.message : "Kunne ikke hente tilmeldinger.")
    } finally {
      setListLoading(false)
    }
  }, [])

  const loadMyEnrollments = useCallback(async () => {
    try {
      const keys = await apiClient<string[]>("/valgfag/my-enrollments")
      setMyKeys(keys)
    } catch {
      setMyKeys([])
    }
  }, [])

  useEffect(() => {
    void loadPublicEnrollments()
  }, [loadPublicEnrollments])

  useEffect(() => {
    if (isAuthenticated) void loadMyEnrollments()
    else setMyKeys([])
  }, [isAuthenticated, loadMyEnrollments])

  const join = async (electiveKey: string) => {
    setActionHint(null)
    if (!isAuthenticated) {
      void login()
      return
    }
    setPendingKey(electiveKey)
    try {
      await apiClient("/valgfag/enroll", {
        method: "POST",
        body: JSON.stringify({ electiveKey }),
      })
      setActionHint({ electiveKey, ok: true, text: "Du er tilmeldt." })
      await loadPublicEnrollments()
      await loadMyEnrollments()
    } catch (err) {
      const text = err instanceof Error ? err.message : "Tilmelding mislykkedes."
      setActionHint({ electiveKey, ok: false, text })
    } finally {
      setPendingKey(null)
    }
  }

  const unjoin = async (electiveKey: string) => {
    setActionHint(null)
    if (!isAuthenticated) return
    setPendingKey(electiveKey)
    try {
      const encoded = encodeURIComponent(electiveKey)
      await apiClient(`/valgfag/enroll/${encoded}`, { method: "DELETE" })
      setActionHint({ electiveKey, ok: true, text: "Du er frameldt." })
      await loadPublicEnrollments()
      await loadMyEnrollments()
    } catch (err) {
      const text = err instanceof Error ? err.message : "Frameldning mislykkedes."
      setActionHint({ electiveKey, ok: false, text })
    } finally {
      setPendingKey(null)
    }
  }

  return (
    <Layout_alt>
      <section className="container-shell py-12 sm:py-16 lg:pb-24">
        <div className="mb-10 max-w-2xl">
          <h1 className="text-4xl font-extrabold tracking-[-0.02em] text-slate-900 sm:text-5xl">
            Vælg dit næste <span className="brand-gradient-text">valgfag</span>
          </h1>
          <p className="mt-4 text-base leading-7 text-slate-600">
            Få overblik over kommende hold, undervisere og varighed. Brug knapperne på kortene for at tilmelde eller
            framelde dig med din Mercantec-konto — alle kan se hvem der er tilmeldt.
          </p>
        </div>

        {listError && (
          <p className="mb-4 text-sm text-red-600" role="alert">
            {listError}
          </p>
        )}

        <div className="grid gap-5 md:grid-cols-3">
          {availableElectives.map((e) => {
            const Icon = e.icon
            const participants = participantsByKey.get(e.electiveKey) ?? []
            const imEnrolled = myKeys.includes(e.electiveKey)
            const busy = pendingKey === e.electiveKey
            const hint = actionHint?.electiveKey === e.electiveKey ? actionHint : null
            return (
              <Card key={e.electiveKey} className="soft-card soft-card-hover flex flex-col p-5">
                <div className="mb-4 inline-flex h-12 w-12 items-center justify-center rounded-xl bg-indigo-50 text-indigo-600">
                  <Icon className="h-5 w-5" />
                </div>
                <div className="flex flex-wrap items-center gap-2">
                  <h3 className="text-lg font-bold text-slate-900">{e.name}</h3>
                  {imEnrolled && (
                    <span className="rounded-full bg-emerald-100 px-2 py-0.5 text-xs font-semibold text-emerald-800">
                      Du er tilmeldt
                    </span>
                  )}
                </div>
                <ul className="mt-4 space-y-2 text-sm text-slate-600">
                  <li className="flex items-center gap-2">
                    <CalendarDays className="h-4 w-4 shrink-0 text-indigo-500" />
                    {e.week} · {e.date}
                  </li>
                  <li className="flex items-center gap-2">
                    <UserRound className="h-4 w-4 shrink-0 text-indigo-500" />
                    {e.teacher}
                  </li>
                  <li className="flex items-center gap-2">
                    <Clock4 className="h-4 w-4 shrink-0 text-indigo-500" />
                    {e.duration}
                  </li>
                  <li className="flex items-center gap-2">
                    <GraduationCap className="h-4 w-4 shrink-0 text-indigo-500" />
                    Anbefalet efter {e.recommended}
                  </li>
                </ul>

                <div className="mt-5 flex flex-wrap gap-2">
                  {imEnrolled ? (
                    <Button
                      type="button"
                      variant="outline"
                      className="h-10"
                      disabled={busy}
                      onClick={() => void unjoin(e.electiveKey)}
                    >
                      {busy ? "Arbejder…" : "Frameld"}
                    </Button>
                  ) : (
                    <Button type="button" className="h-10" disabled={busy} onClick={() => void join(e.electiveKey)}>
                      {busy ? "Arbejder…" : isAuthenticated ? "Tilmeld dig" : "Log ind og tilmeld dig"}
                    </Button>
                  )}
                </div>
                {hint && (
                  <p className={`mt-2 text-sm ${hint.ok ? "text-emerald-700" : "text-red-600"}`} role="status">
                    {hint.text}
                  </p>
                )}

                <div className="mt-5 border-t border-slate-100 pt-4">
                  <div className="mb-2 flex items-center gap-2 text-xs font-semibold uppercase tracking-wide text-slate-500">
                    <Users className="h-3.5 w-3.5" />
                    Tilmeldte ({listLoading ? "…" : participants.length})
                  </div>
                  {listLoading ? (
                    <p className="text-sm text-slate-500">Henter liste…</p>
                  ) : participants.length === 0 ? (
                    <p className="text-sm text-slate-500">Ingen tilmeldinger endnu.</p>
                  ) : (
                    <ul className="max-h-40 space-y-1 overflow-y-auto text-sm text-slate-700">
                      {participants.map((p, i) => (
                        <li key={`${p.displayName}-${p.enrolledAt}-${i}`}>{p.displayName}</li>
                      ))}
                    </ul>
                  )}
                </div>
              </Card>
            )
          })}
        </div>
        {availableElectives.length === 0 && (
          <Card className="mt-5 border-slate-100 bg-slate-50 p-5 text-sm text-slate-600">
            Der er ingen åbne valgfag lige nu. Kig tilbage senere for næste forløb.
          </Card>
        )}
      </section>
    </Layout_alt>
  )
}

export default Valgfag
