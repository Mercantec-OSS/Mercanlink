import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { useState } from "react"
import { Eye, EyeOff } from "lucide-react"

export interface SignupFormProps {
  onSubmit: (credentials: { emailOrUsername: string; password: string }) => Promise<void>
  loading?: boolean
  error?: string | null
}

export function SignupForm({ onSubmit, loading = false, error = null }: SignupFormProps) {
  const [username, setUsername] = useState("")
  const [password, setPassword] = useState("")
  const [showPassword, setShowPassword] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    await onSubmit({ emailOrUsername: username, password })
  }

  return (
    <Card className="soft-card w-full max-w-lg border-slate-100 p-2">
      <CardHeader className="px-6 pb-3 pt-6 text-left">
        <CardTitle className="text-3xl font-extrabold tracking-[-0.02em] text-slate-900">Opret konto</CardTitle>
        <CardDescription className="mt-2 text-base text-slate-600">
          Opret en bruger og få adgang til valgfag, formularer og materialer.
        </CardDescription>
      </CardHeader>

      <CardContent className="px-6 pb-6">
        <form onSubmit={handleSubmit} className="space-y-5">
          <div className="space-y-2">
            <Label htmlFor="emailOrUsername" className="text-sm font-semibold text-slate-700">
              Email eller brugernavn
            </Label>
            <Input
              id="emailOrUsername"
              type="text"
              required
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              disabled={loading}
              placeholder="fx elev@mercantec.dk"
              className="h-11"
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="password" className="text-sm font-semibold text-slate-700">
              Password
            </Label>
            <div className="relative">
              <Input
                id="password"
                type={showPassword ? "text" : "password"}
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                disabled={loading}
                placeholder="Vælg et sikkert password"
                className="h-11 pr-11"
              />
              <button
                type="button"
                className="absolute right-2 top-1/2 -translate-y-1/2 rounded-md p-2 text-slate-500 transition hover:bg-slate-100 hover:text-indigo-600"
                tabIndex={-1}
                onClick={() => setShowPassword((v) => !v)}
                aria-label={showPassword ? "Skjul password" : "Vis password"}
              >
                {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
              </button>
            </div>
          </div>

          {error && (
            <p className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-600">
              {error}
            </p>
          )}

          <Button type="submit" className="h-11 w-full text-sm" disabled={loading}>
            {loading ? "Opretter konto..." : "Opret konto"}
          </Button>

          <CardDescription className="text-xs text-slate-500">
            Din konto bruges til at holde styr på valgfag og fælles materiale.
          </CardDescription>
        </form>
      </CardContent>
    </Card>
  )
}