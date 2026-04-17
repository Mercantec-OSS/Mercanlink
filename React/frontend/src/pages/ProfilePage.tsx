import Layout_alt from "@/components/templates/layout"
import { useAuth } from "@/contexts/AuthContext"
import { BadgeCheck, KeyRound, Mail, Shield, User as UserIcon } from "lucide-react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"

function prettyLoginMethod(method: string | undefined): string {
  if (!method) {
    return "Ukendt"
  }

  const map: Record<string, string> = {
    password: "Adgangskode",
    google: "Google",
    github: "GitHub",
    discord: "Discord",
    "microsoft-work": "Microsoft Work",
  }

  return map[method] || method
}

export function ProfilePage() {
  const { user } = useAuth()

  return (
    <Layout_alt>
      <section className="container-shell py-12 sm:py-16 lg:py-20">
        <div className="mx-auto max-w-3xl">
          <Card className="soft-card border-slate-100">
            <CardHeader>
              <CardTitle className="text-2xl font-bold tracking-[-0.02em] text-slate-900">
                Din profil
              </CardTitle>
              <CardDescription className="text-slate-600">
                Her kan du se hvem der er logget ind og hvordan login blev gennemført.
              </CardDescription>
            </CardHeader>

            <CardContent className="grid gap-4 sm:grid-cols-2">
              <div className="rounded-lg border border-slate-100 bg-white p-4">
                <p className="mb-2 inline-flex items-center gap-2 text-sm font-semibold text-slate-700">
                  <UserIcon className="h-4 w-4 text-indigo-600" />
                  Bruger
                </p>
                <p className="text-sm text-slate-600">{user?.username || "Ukendt bruger"}</p>
              </div>

              <div className="rounded-lg border border-slate-100 bg-white p-4">
                <p className="mb-2 inline-flex items-center gap-2 text-sm font-semibold text-slate-700">
                  <Mail className="h-4 w-4 text-indigo-600" />
                  E-mail
                </p>
                <p className="text-sm text-slate-600">{user?.email || "Ingen e-mail i token"}</p>
              </div>

              <div className="rounded-lg border border-slate-100 bg-white p-4">
                <p className="mb-2 inline-flex items-center gap-2 text-sm font-semibold text-slate-700">
                  <KeyRound className="h-4 w-4 text-indigo-600" />
                  Login-metode
                </p>
                <p className="text-sm text-slate-600">{prettyLoginMethod(user?.loginMethod)}</p>
              </div>

              <div className="rounded-lg border border-slate-100 bg-white p-4">
                <p className="mb-2 inline-flex items-center gap-2 text-sm font-semibold text-slate-700">
                  <BadgeCheck className="h-4 w-4 text-indigo-600" />
                  Bruger-ID
                </p>
                <p className="break-all text-sm text-slate-600">{user?.id || "Ikke tilgængelig"}</p>
              </div>

              <div className="rounded-lg border border-slate-100 bg-white p-4 sm:col-span-2">
                <p className="mb-2 inline-flex items-center gap-2 text-sm font-semibold text-slate-700">
                  <Shield className="h-4 w-4 text-indigo-600" />
                  Roller
                </p>
                {user?.roles?.length ? (
                  <div className="flex flex-wrap gap-2">
                    {user.roles.map((role) => (
                      <span
                        key={role}
                        className="rounded-full border border-indigo-200 bg-indigo-50 px-3 py-1 text-xs font-medium text-indigo-700"
                      >
                        {role}
                      </span>
                    ))}
                  </div>
                ) : (
                  <p className="text-sm text-slate-600">Ingen roller i token.</p>
                )}
              </div>
            </CardContent>
          </Card>
        </div>
      </section>
    </Layout_alt>
  )
}
