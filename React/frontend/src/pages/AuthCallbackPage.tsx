import { useEffect, useState } from "react"
import { useNavigate, useSearchParams } from "react-router-dom"
import { useAuth } from "@/contexts/AuthContext"
import Layout_alt from "@/components/templates/layout"

export function AuthCallbackPage() {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const { completeLogin } = useAuth()
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const code = searchParams.get("code")
    const state = searchParams.get("state")
    const oauthError = searchParams.get("error")

    if (oauthError) {
      setError(`Login blev afvist: ${oauthError}`)
      return
    }

    if (!code || !state) {
      setError("Mangler authorization code eller state i callback.")
      return
    }

    completeLogin(code, state).catch((callbackError) => {
      const message =
        callbackError instanceof Error
          ? callbackError.message
          : "Der opstod en ukendt fejl under login."
      setError(message)
    })
  }, [completeLogin, searchParams, navigate])

  return (
    <Layout_alt>
      <section className="container-shell flex min-h-[50vh] items-center justify-center py-16">
        <div className="w-full max-w-xl rounded-xl border border-slate-100 bg-white p-8 shadow-sm">
          <h1 className="text-2xl font-bold text-slate-900">Afslutter login</h1>
          {!error ? (
            <p className="mt-3 text-sm text-slate-600">
              Vi validerer din login-session hos Mercantec Auth...
            </p>
          ) : (
            <>
              <p className="mt-3 rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-600">
                {error}
              </p>
              <button
                type="button"
                className="mt-4 rounded-lg border border-slate-200 px-4 py-2 text-sm font-medium text-slate-700"
                onClick={() => navigate("/login")}
              >
                Tilbage til login
              </button>
            </>
          )}
        </div>
      </section>
    </Layout_alt>
  )
}
