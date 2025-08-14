import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { useState } from "react"

export interface LoginFormProps {
  onSubmit: (credentials: { emailOrUsername: string; password: string }) => Promise<void>
  loading?: boolean
  error?: string | null
}

export function LoginForm({ onSubmit, loading = false, error = null }: LoginFormProps) {
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
        <div className="absolute left-0 -translate-x-[120%] mr-8 p-6 bg-white text-black rounded shadow-lg max-w-xs ">
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
      <Card className=" text-white/50  border-0 h-[600px] w-[550px] backdrop-blur-md shadow-lg bg-[#101828]">
        <CardHeader className="text-center">

          <CardTitle className="text-2xl text-white/100 mt-10 bg-amber-">Login</CardTitle>
          <CardDescription className="mt-5 text-md">
            Enter your email or username below to login to your account
          </CardDescription>

        </CardHeader>
        <CardContent className=" mt-10" >
          <form onSubmit={handleSubmit}>

            <div className="flex flex-col items-center justify-center gap-4 ">
              <div className="grid gap-2 w-full  items-center">
                <Label className="text-white/100 pl-26 text-lg" htmlFor="emailOrUsername">Email or Username</Label>
                <Input
                  className="bg-[#1c2433] border-white/50 text-white/70 placeholder:text-white/70 w-[300px] h-12 mx-auto placeholder:text-lg text-lg"
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
              <div className="grid gap-2 w-full  items-center">
                <Label className="text-white/100 pl-26 text-lg" htmlFor="password">Password</Label>
                <Input
                  className="bg-[#1c2433] border-white/50 w-[300px] mx-auto h-12 placeholder:text-lg text-lg"
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
              {error && <p className="text-destructive text-md mt-2 text-red-500 ">{error}</p>}
              <Button type="submit" className=" absolute bottom-12 h-12 text-lg left-1/2 -translate-x-1/2 mt-10 justify-center border bg-[#1c2430] border-white/50 text-white/100 hover:text-black hover:bg-gray-600 active:scale-95 transition-transform cursor-pointer w-[200px] mx-auto" disabled={loading}>
                {loading ? "Logging in..." : "Login"}
              </Button>
            </div>

          </form>
        </CardContent>

      </Card >
    </div >
  )
}