import { useState } from "react"
import { useAuth } from "@/contexts/AuthContext"
import Navbar from "@/components/templates/navbar"
import Footer from "@/components/templates/footer"
import { useNavigate } from "react-router-dom"
import LoginForm from "@/components/templates/loginForm"
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
      <div className="flex flex-col min-h-svh bg-gradient-to-br from-[#181c2c] via-[#23263a] to-[#10121a]">
      <Navbar />
      <div className="flex flex-1  justify-center ">
        {/* <img src="/src/components/images/bluey.jpg" className="absolute h-[600px] w-[2000px]" /> */}


        <LoginForm onSubmit={handleLogin} loading={loading} error={error} />
        <Button
          type="button"
          variant="outline"
          className="w-full mt-2 absolute bottom-4 left-1/2 -translate-x-1/2 max-w-sm"
          onClick={() => navigate(-1)}
        >
          Back
        </Button>
      </div>
      <Footer />
    </div>
  )
}