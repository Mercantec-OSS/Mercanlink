import React from "react"
import Layout_alt from "@/components/templates/layout"
import { Bot, Gamepad2, CalendarDays, UserRound, Clock4, GraduationCap } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Card } from "@/components/ui/card"
import { Label } from "@/components/ui/label"
import { Input } from "@/components/ui/input"

const electives = [
  {
    name: "Machine Learning",
    week: "Uge 8-9",
    date: "17/02 - 28/02",
    teacher: "MAGS",
    duration: "10 dage",
    recommended: "H4 / H5",
    icon: Bot,
  },
  {
    name: "Machine Learning (kun fysisk)",
    week: "Uge 37-38",
    date: "08/09 - 19/09",
    teacher: "Beate (BELJ)",
    duration: "10 dage",
    recommended: "H2",
    icon: Bot,
  },
  {
    name: "Game Design (3 uger)",
    week: "Uge 43-45",
    date: "20/10 - 07/11",
    teacher: "Kasper (KASC)",
    duration: "15 dage",
    recommended: "H1",
    icon: Gamepad2,
  },
]

const Valgfag: React.FC = () => {
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
          {electives.map((e) => {
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
                className="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm text-slate-700 outline-none transition focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/30"
              >
                <option value="">Vælg et valgfag</option>
                <option value="Machine-Learning">Machine Learning</option>
                <option value="Machine-Learning-Fysisk">Machine Learning (kun fysisk)</option>
                <option value="Game-Design">Game Design</option>
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