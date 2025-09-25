import { useState } from "react"
import { useAuth } from "@/contexts/AuthContext"
import Navbar from "@/components/templates/navbar"
import Footer from "@/components/templates/footer"
import { useNavigate } from "react-router-dom"
import { LoginForm } from "@/components/templates/loginForm"
import { Button } from "@/components/ui/button"

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
      // Optionally navigate after login
    } catch {
      setError("Failed to login. Please check your credentials.")
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="flex flex-col min-h-svh bg-background">
      <Navbar />
      <div className="flex flex-1 items-center justify-center bg-black">

        <Button
          type="button"
          variant="outline"
          className="bg-white text-black mt-28 mr-8 absolute top-0 right-0 max-w"
          style={{ transform: "none" }}
          onClick={() => navigate("/")}
        >
          Back
        </Button>

        <LoginForm onSubmit={handleLogin} loading={loading} error={error} />

      </div>
      <Footer />
    </div>
  )
}