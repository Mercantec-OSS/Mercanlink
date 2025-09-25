import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { useState } from "react"

export interface SignupFormProps {
  onSubmit: (credentials: { emailOrUsername: string; password: string }) => Promise<void>
  loading?: boolean
  error?: string | null
}

export function SignupForm({ onSubmit, loading = false, error = null }: SignupFormProps) {
  const [username, setUsername] = useState("")
  const [password, setPassword] = useState("")
  const [focusedField, setFocusedField] = useState<"email" | "password" | null>(null)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    await onSubmit({ emailOrUsername: username, password })
  }

  return (
    <div className="relative flex items-center justify-center">
      {/* Side panel for requirements */}
      {(focusedField === "email" || focusedField === "password") && (
        <div className="absolute left-0 -translate-x-[120%] mr-8 p-6 bg-white text-black rounded shadow-lg max-w-xs">
          {focusedField === "email" && (
            <>
              <h2 className="text-lg font-semibold mb-2">Email Requirements</h2>
              <ul className="list-disc pl-5 text-sm">
                <li>Email must be a valid address (e.g. name@example.com)</li>
                <li>Username can also be used</li>
              </ul>
            </>
          )}
          {focusedField === "password" && (
            <>
              <h2 className="text-lg font-semibold mb-2">Password Requirements</h2>
              <ul className="list-disc pl-5 text-sm">
                <li>At least 8 characters</li>
                <li>Should contain letters and numbers</li>
              </ul>
            </>
          )}
        </div>
      )}
      <Card className="max-w-sm bg-white text-black shadow-lg">
        <CardHeader>
          <CardTitle className="text-2xl">Signup</CardTitle>
          <CardDescription>
            Enter your email or username below to signup to your account
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit}>
            <div className="grid gap-4">
              <div className="grid gap-2">
                <Label htmlFor="emailOrUsername">Email or Username</Label>
                <Input
                  id="emailOrUsername"
                  type="text"
                  placeholder="m.alac or name@example.com"
                  required
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                  disabled={loading}
                  onFocus={() => setFocusedField("email")}
                  onBlur={() => setFocusedField(null)}
                />
              </div>
              <div className="grid gap-2">
                <div className="flex items-center">
                  <Label htmlFor="password">Password</Label>
                </div>
                <Input
                  id="password"
                  type="password"
                  required
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  disabled={loading}
                  onFocus={() => setFocusedField("password")}
                  onBlur={() => setFocusedField(null)}
                />
              </div>
              {error && <p className="text-destructive text-sm">{error}</p>}
              <Button type="submit" className="w-full border" disabled={loading}>
                {loading ? "Logging in..." : "Signup"}
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}