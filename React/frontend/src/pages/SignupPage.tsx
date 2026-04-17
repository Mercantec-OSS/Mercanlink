import { useAuth } from "@/contexts/AuthContext"
import { useNavigate } from "react-router-dom"
import { Button } from "@/components/ui/button"
import Layout_alt from "@/components/templates/layout"
import { Rocket, ShieldCheck } from "lucide-react"
import { useState } from "react"

export function SignupPage() {
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)
  const { login } = useAuth()
  const navigate = useNavigate()

  const handleSignup = async () => {
    setError(null)
    setLoading(true)
    try {
      await login()
    } catch {
      setError("Viderestilling til login fejlede. Prøv igen.")
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
              Kontooprettelse styres via <span className="brand-gradient-text">Mercantec Auth</span>
            </h1>
            <p className="mt-4 text-base leading-7 text-slate-600">
              Lokal signup er udfaset. Brug Mercantec Auth for at logge ind eller oprette konto.
            </p>
            <div className="mt-6 inline-flex items-center gap-2 rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-medium text-emerald-700">
              <ShieldCheck className="h-4 w-4" />
              Sikker onboarding med samme loginflow som resten af platformen.
            </div>
          </div>

          <div className="flex w-full max-w-lg flex-col items-center gap-4 rounded-xl border border-slate-100 bg-white p-8 shadow-sm lg:items-stretch">
            {error && (
              <p className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-600">
                {error}
              </p>
            )}
            <Button type="button" className="h-11 w-full text-sm" disabled={loading} onClick={handleSignup}>
              {loading ? "Viderestiller..." : "Fortsæt til Mercantec Auth"}
            </Button>
            <Button type="button" variant="outline" className="w-full max-w-lg" onClick={() => navigate(-1)}>
              Tilbage
            </Button>
          </div>
        </div>
      </section>
    </Layout_alt>
  )
}