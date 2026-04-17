import React from "react"
import Layout_alt from "@/components/templates/layout"
import { Bot, Gamepad2, Rocket, CalendarDays, UserRound, Clock4, GraduationCap } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Card } from "@/components/ui/card"
import { Label } from "@/components/ui/label"
import { Input } from "@/components/ui/input"

type Elective = {
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
  const now = new Date()
  const today = new Date(now.getFullYear(), now.getMonth(), now.getDate())

  const availableElectives = electives.filter((elective) => {
    const endDate = new Date(`${elective.endDate}T23:59:59`)
    return endDate >= today
  })

  return (
    <Layout_alt>
      <section className="container-shell py-12 sm:py-16 lg:py-20">
        <div className="mb-10 max-w-2xl">
          <h1 className="text-4xl font-extrabold tracking-[-0.02em] text-slate-900 sm:text-5xl">
            Vælg dit næste <span className="brand-gradient-text">valgfag</span>
          </h1>
          <p className="mt-4 text-base leading-7 text-slate-600">
            Få overblik over kommende hold, undervisere og varighed. Når du har valgt dit forløb, kan du tilmelde dig
            direkte nedenfor.
          </p>
        </div>

        <div className="grid gap-5 md:grid-cols-3">
          {availableElectives.map((e) => {
            const Icon = e.icon
            return (
              <Card key={e.name} className="soft-card soft-card-hover p-5">
                <div className="mb-4 inline-flex h-12 w-12 items-center justify-center rounded-xl bg-indigo-50 text-indigo-600">
                  <Icon className="h-5 w-5" />
                </div>
                <h3 className="text-lg font-bold text-slate-900">{e.name}</h3>
                <ul className="mt-4 space-y-2 text-sm text-slate-600">
                  <li className="flex items-center gap-2">
                    <CalendarDays className="h-4 w-4 text-indigo-500" />
                    {e.week} · {e.date}
                  </li>
                  <li className="flex items-center gap-2">
                    <UserRound className="h-4 w-4 text-indigo-500" />
                    {e.teacher}
                  </li>
                  <li className="flex items-center gap-2">
                    <Clock4 className="h-4 w-4 text-indigo-500" />
                    {e.duration}
                  </li>
                  <li className="flex items-center gap-2">
                    <GraduationCap className="h-4 w-4 text-indigo-500" />
                    Anbefalet efter {e.recommended}
                  </li>
                </ul>
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

      <section className="container-shell pb-16 sm:pb-20 lg:pb-24">
        <Card className="soft-card border-slate-100 p-6 sm:p-8">
          <h2 className="text-2xl font-bold text-slate-900">Tilmelding</h2>
          <p className="mt-2 text-sm text-slate-600">Udfyld dine oplysninger og vælg det forløb, du ønsker at deltage i.</p>

          <form className="mt-8 grid gap-5 md:grid-cols-2">
            <div className="space-y-2">
              <Label htmlFor="name" className="text-sm font-semibold text-slate-700">Dit navn</Label>
              <Input id="name" name="name" type="text" required placeholder="Skriv dit navn" className="h-11" />
            </div>

            <div className="space-y-2">
              <Label htmlFor="id" className="text-sm font-semibold text-slate-700">Dit ID</Label>
              <Input id="id" name="id" type="text" required placeholder="Skriv dit ID" className="h-11" />
            </div>

            <div className="space-y-2 md:col-span-2">
              <Label htmlFor="elective" className="text-sm font-semibold text-slate-700">Vælg valgfag</Label>
              <select
                id="elective"
                name="elective"
                required
                disabled={availableElectives.length === 0}
                className="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm text-slate-700 outline-none transition focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/30"
              >
                <option value="">{availableElectives.length > 0 ? "Vælg et valgfag" : "Ingen åbne valgfag"}</option>
                {availableElectives.map((e) => (
                  <option key={e.name} value={e.name}>
                    {e.name}
                  </option>
                ))}
              </select>
            </div>

            <div className="md:col-span-2">
              <Button type="submit" className="h-11 w-full sm:w-auto">
                Tilmeld mig
              </Button>
            </div>
          </form>
        </Card>
      </section>
    </Layout_alt>
  )
}

export default Valgfag