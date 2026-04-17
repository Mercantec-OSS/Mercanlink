import { useState } from "react"
import { useAuth } from "@/contexts/AuthContext"
import { useNavigate } from "react-router-dom"
import { SignupForm } from "@/components/templates/signupForm"
import { Button } from "@/components/ui/button"
import Layout_alt from "@/components/templates/layout"
import { Rocket, ShieldCheck } from "lucide-react"

export function SignupPage() {
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)
  const { signup } = useAuth()
  const navigate = useNavigate()

  const handleSignup = async ({ emailOrUsername, password }: { emailOrUsername: string; password: string }) => {
    setError(null)
    setLoading(true)
    try {
      await signup({ emailOrUsername, password })
    } catch {
      setError("Oprettelse fejlede. Tjek dine oplysninger og prøv igen.")
    } finally {
      setLoading(false)
    }
  }

  return (
    <Layout_alt>
      <section className="container-shell flex flex-1 items-center py-12 sm:py-16">
        <div className="grid w-full items-center gap-10 lg:grid-cols-2">
          <div className="max-w-xl">
            <p className="mb-4 inline-flex items-center gap-2 rounded-full border border-indigo-100 bg-white px-4 py-2 text-xs font-semibold uppercase tracking-wide text-indigo-700">
              <Rocket className="h-4 w-4" />
              Hurtig oprettelse
            </p>
            <h1 className="text-4xl font-extrabold leading-[1.1] tracking-[-0.02em] text-slate-900 sm:text-5xl">
              Start med <span className="brand-gradient-text">Mercanlink</span> i dag
            </h1>
            <p className="mt-4 text-base leading-7 text-slate-600">
              Opret en konto for at få adgang til jeres valgfagsoverblik og dele materialer med resten af teamet.
            </p>
            <div className="mt-6 inline-flex items-center gap-2 rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-medium text-emerald-700">
              <ShieldCheck className="h-4 w-4" />
              Sikker onboarding med samme loginflow som resten af platformen.
            </div>
          </div>

          <div className="flex flex-col items-center gap-4 lg:items-end">
            <SignupForm onSubmit={handleSignup} loading={loading} error={error} />
            <Button type="button" variant="outline" className="w-full max-w-lg" onClick={() => navigate(-1)}>
              Tilbage
            </Button>
          </div>
        </div>
      </section>
    </Layout_alt>
  )
}