import { useState } from "react"
import { useAuth } from "@/contexts/AuthContext"
import { useNavigate } from "react-router-dom"
import LoginForm from "@/components/templates/loginForm"
import { Button } from "@/components/ui/button"
import Layout_alt from "@/components/templates/layout"
import { ShieldCheck, Sparkles } from "lucide-react"

export function LoginPage() {
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)
  const { login } = useAuth()
  const navigate = useNavigate()

  const handleLogin = async ({ emailOrUsername, password }: { emailOrUsername: string; password: string }) => {
    setError(null)
    setLoading(true)
    try {
      await login({ emailOrUsername, password })
    } catch {
      setError("Log ind fejlede. Tjek dine oplysninger og prøv igen.")
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
              <Sparkles className="h-4 w-4" />
              Sikker adgang
            </p>
            <h1 className="text-4xl font-extrabold leading-[1.1] tracking-[-0.02em] text-slate-900 sm:text-5xl">
              Velkommen tilbage til <span className="brand-gradient-text">Mercanlink</span>
            </h1>
            <p className="mt-4 text-base leading-7 text-slate-600">
              Log ind for at håndtere valgfag, udfylde formularer og samarbejde med resten af holdet i et samlet workflow.
            </p>
            <div className="mt-6 inline-flex items-center gap-2 rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-medium text-emerald-700">
              <ShieldCheck className="h-4 w-4" />
              Din session er beskyttet og token-baseret.
            </div>
          </div>

          <div className="flex flex-col items-center gap-4 lg:items-end">
            <LoginForm onSubmit={handleLogin} loading={loading} error={error} />
            <Button type="button" variant="outline" className="w-full max-w-lg" onClick={() => navigate(-1)}>
              Tilbage
            </Button>
          </div>
        </div>
      </section>
    </Layout_alt>
  )
}