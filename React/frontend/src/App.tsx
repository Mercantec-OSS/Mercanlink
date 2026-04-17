import Layout_alt from "@/components/templates/layout"
import { useNavigate } from "react-router-dom"
import { ArrowRight, CheckCircle2, Layers, Rocket, Users2 } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Card, CardDescription, CardHeader, CardTitle } from "./components/ui/card"
import { FaDiscord } from "react-icons/fa"
import teamsIcon from "@/components/images/icons8-microsoft-teams-2019-48.png"

const quickActions = [
  {
    title: "Valgfagsoverblik",
    description: "Find næste valgfag og tilmeld dig direkte med tydelig info om uge, underviser og niveau.",
    icon: Layers,
    href: "/valgfag",
  },
  {
    title: "Indsend materiale",
    description: "Del links, noter og læringsressourcer så hele holdet kan bygge videre på samme viden.",
    icon: Rocket,
    href: "/form",
  },
  {
    title: "Brugeroversigt",
    description: "Se aktive brugere, progression og teamets samlede momentum i én samlet visning.",
    icon: Users2,
    href: "/users",
  },
]

const highlights = [
  "Enterprise-lignende UI med tydelig informationsarkitektur",
  "Responsivt layout designet til mobil, tablet og desktop",
  "Fokus på klarhed, hastighed og pålidelig brugeroplevelse",
]

function App() {
  const navigate = useNavigate()

  return (
    <Layout_alt>
      <section className="container-shell py-12 sm:py-16 lg:py-20">
        <div className="grid items-center gap-10 lg:grid-cols-2 lg:gap-12">
          <div>
            <div className="mb-4 inline-flex items-center rounded-full border border-indigo-100 bg-white px-4 py-2 text-xs font-semibold uppercase tracking-wide text-indigo-700 shadow-[0_4px_20px_-2px_rgba(79,70,229,0.15)]">
              Designet til moderne læringsmiljøer
            </div>
            <h1 className="text-4xl font-extrabold leading-[1.1] tracking-[-0.02em] text-slate-900 sm:text-5xl lg:text-6xl">
              Ét samlet sted til{" "}
              <span className="brand-gradient-text">valgfag, viden og samarbejde</span>
            </h1>
            <p className="mt-5 max-w-xl text-base leading-7 text-slate-600 sm:text-lg">
              Mercanlink samler alt fra kursusvalg til deling af læringsmateriale i en platform, der føles professionel,
              hurtig og nem at bruge i hverdagen.
            </p>
            <div className="mt-8 flex flex-col gap-3 sm:flex-row">
              <Button size="lg" onClick={() => navigate("/valgfag")} className="group">
                Kom i gang
                <ArrowRight className="h-4 w-4 transition-transform group-hover:translate-x-1" />
              </Button>
              <Button size="lg" variant="outline" onClick={() => navigate("/form")}>
                Del materiale
              </Button>
            </div>
            <ul className="mt-8 space-y-3">
              {highlights.map((item) => (
                <li key={item} className="flex items-start gap-2 text-sm text-slate-600 sm:text-base">
                  <CheckCircle2 className="mt-0.5 h-5 w-5 shrink-0 text-indigo-600" />
                  {item}
                </li>
              ))}
            </ul>
          </div>

          <div className="relative perspective-[2000px]">
            <div className="rounded-2xl border border-indigo-100 bg-white p-3 shadow-[0_10px_30px_-8px_rgba(79,70,229,0.25)] transition-all duration-300 [transform:rotateX(5deg)_rotateY(-12deg)] hover:[transform:rotateX(2deg)_rotateY(-8deg)]">
              <img src="/Mercanlink-banner.png" alt="Mercanlink banner" className="h-full w-full rounded-xl object-cover" />
            </div>
            <div className="absolute -bottom-8 -left-3 flex flex-col gap-3 sm:flex-row">
              <a
                href="https://discord.gg/uHkYDgsKcm"
                target="_blank"
                rel="noopener noreferrer"
                className="group"
              >
                <Card className="soft-card soft-card-hover w-52 border-slate-100">
                  <CardHeader className="p-4">
                    <CardDescription className="text-xs uppercase tracking-wide text-slate-500">Fællesskab</CardDescription>
                    <CardTitle className="mt-1 flex items-center gap-2 text-base text-slate-900">
                      <FaDiscord className="h-4 w-4 text-[#5865F2]" />
                      Join vores Discord
                    </CardTitle>
                  </CardHeader>
                </Card>
              </a>

              <a
                href="https://teams.microsoft.com/l/team/19%3AILE0F1oc9fGJMPT6mFVjcQ7N10W1SE8XA9r1X4N6Pko1%40thread.tacv2/conversations?groupId=a014d18d-572a-4621-bf7d-682f06ec1fea&tenantId=17aab4ce-4b26-487e-9bea-1e2a70348bf0"
                target="_blank"
                rel="noopener noreferrer"
                className="group"
              >
                <Card className="soft-card soft-card-hover w-52 border-slate-100">
                  <CardHeader className="p-4">
                    <CardDescription className="text-xs uppercase tracking-wide text-slate-500"> Mercantec platform</CardDescription>
                    <CardTitle className="mt-1 flex items-center gap-2 text-base text-slate-900">
                      <img src={teamsIcon} alt="Microsoft Teams logo" className="h-4 w-4 object-contain" />
                      Join Teams kanalen
                    </CardTitle>
                  </CardHeader>
                </Card>
              </a>
            </div>
          </div>
        </div>
      </section>

      <section className="container-shell pb-16 sm:pb-20 lg:pb-24">
        <div className="grid gap-5 md:grid-cols-3">
          {quickActions.map((action) => {
            const Icon = action.icon
            return (
              <Card
                key={action.title}
                onClick={() => navigate(action.href)}
                className="soft-card soft-card-hover cursor-pointer border-slate-100 p-6"
              >
                <div className="mb-5 inline-flex h-12 w-12 items-center justify-center rounded-xl bg-indigo-50 text-indigo-600">
                  <Icon className="h-5 w-5" />
                </div>
                <CardTitle className="text-xl font-bold text-slate-900">{action.title}</CardTitle>
                <CardDescription className="mt-3 text-sm leading-6 text-slate-600">{action.description}</CardDescription>
              </Card>
            )
          })}
        </div>
      </section>
    </Layout_alt>
  )
}

export default App