import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { useState, useEffect } from "react"
import { FaEye, FaEyeSlash } from "react-icons/fa"

export interface SignupFormProps {
  onSubmit: (credentials: { emailOrUsername: string; password: string }) => Promise<void>
  loading?: boolean
  error?: string | null
}

export function SignupForm({ onSubmit, loading = false, error = null }: SignupFormProps) {
  const [username, setUsername] = useState("")
  const [password, setPassword] = useState("")
  const [focusedField, setFocusedField] = useState<"email" | "password" | null>(null)
  const [showPassword, setShowPassword] = useState(false)
  const [errorAnim, setErrorAnim] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    await onSubmit({ emailOrUsername: username, password })
  }

  useEffect(() => {
    if (error) {
      setErrorAnim(true);
      const timer = setTimeout(() => setErrorAnim(false), 800);
      return () => clearTimeout(timer);
    }
  }, [error]);

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
      <Card className="text-white/80 border-0 h-[600px] w-full max-w-2xl backdrop-blur-md shadow-2xl bg-gradient-to-br from-[#232a36] to-[#181c22] rounded-3xl hover:shadow-blue-500/30 transition-shadow duration-300">
        <CardHeader className="text-center pb-2">
          <CardTitle className="text-5xl text-white font-extrabold mt-10 drop-shadow-lg tracking-wide">Signup</CardTitle>
          <CardDescription className="mt-5 text-lg text-white/80">
            Enter your email or username below to create your account
          </CardDescription>
        </CardHeader>
        <CardContent className="mt-10 px-2 md:px-8">
          <form onSubmit={handleSubmit}>
            <div className="flex flex-col items-center justify-center gap-8">
              <div className="relative w-full md:w-[500px]">
                <Input
                  className="peer bg-[#232a36] border border-blue-400 focus:border-blue-600 text-white/90 w-full h-16 px-4 rounded-xl placeholder:text-2xl placeholder:text-white/50 transition-all shadow-md"
                  id="emailOrUsername"
                  type="text"
                  placeholder=""
                  required
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                  disabled={loading}
                  onFocus={() => setFocusedField('email')}
                  onBlur={() => setFocusedField(null)}
                />
                <Label
                  htmlFor="emailOrUsername"
                  className={`absolute left-4 transition-all
                    ${focusedField === "email" || username
                      ? "-top-5 text-blue-400 text-base bg-[#232a36] px-2 rounded"
                      : "top-5 text-white/70 text-xl"}
                  `}
                >Email or Username</Label>
              </div>
              <div className="relative w-full md:w-[500px] mt-8">
                <Input
                  className="peer bg-[#232a36] border border-blue-400 focus:border-blue-600 text-white/90 w-full h-16 px-4 rounded-xl placeholder:text-2xl placeholder:text-white/50 transition-all pr-10 shadow-md"
                  id="password"
                  type={showPassword ? "text" : "password"}
                  placeholder=""
                  required
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  disabled={loading}
                  onFocus={() => setFocusedField('password')}
                  onBlur={() => setFocusedField(null)}
                />
                <Label
                  htmlFor="password"
                  className={`absolute left-4 transition-all
                    ${focusedField === "password" || password
                      ? "-top-5 text-blue-400 text-base bg-[#232a36] px-2 rounded"
                      : "top-5 text-white/70 text-xl"}
                  `}
                >Password</Label>
                <button
                  type="button"
                  className="absolute right-4 top-5 text-white/70 hover:text-blue-400 focus:outline-none text-[21px]"
                  tabIndex={-1}
                  onClick={() => setShowPassword((v) => !v)}
                >
                  {showPassword ? <FaEyeSlash /> : <FaEye />}
                </button>
              </div>
              {error && (
                <p className={`text-destructive text-md mt-2 text-red-500 transition-all duration-300 ${errorAnim ? 'animate-shake' : ''}`}>
                  {error}
                </p>
              )}
              <Button
                type="submit"
                className="absolute bottom-12 h-16 text-xl left-1/2 -translate-x-1/2 mt-10 justify-center border-none bg-gradient-to-r from-blue-500 to-blue-700 text-white rounded-xl shadow-lg hover:scale-[1.04] hover:from-blue-600 hover:to-blue-800 active:scale-95 transition-all cursor-pointer w-[300px] mx-auto font-bold tracking-wide"
                disabled={loading}
              >
                {loading ? (
                  <span className="flex items-center justify-center gap-2">
                    <svg className="animate-spin h-5 w-5 text-white" viewBox="0 0 24 24"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" /><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8z" /></svg>
                    Signing up...
                  </span>
                ) : "Signup"}
              </Button>
              <style>{`
                @keyframes shake {
                  0% { transform: translateX(0); }
                  20% { transform: translateX(-8px); }
                  40% { transform: translateX(8px); }
                  60% { transform: translateX(-8px); }
                  80% { transform: translateX(8px); }
                  100% { transform: translateX(0); }
                }
                .animate-shake {
                  animation: shake 0.3s;
                }
              `}</style>
            </div>
          </form>
        </CardContent>

      </Card >
    </div>
  )
}