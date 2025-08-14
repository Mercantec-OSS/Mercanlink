import { useState } from "react"
import { useAuth } from "@/contexts/AuthContext"
import Navbar from "@/components/templates/navbar"
import Footer from "@/components/templates/footer"
import { useNavigate } from "react-router-dom"
import { SignupForm } from "@/components/templates/signupForm"
import { Button } from "@/components/ui/button"

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
      // Optionally navigate after signup
    } catch {
      setError("Failed to signup. Please check your credentials.")
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="flex flex-col min-h-svh bg-background">
      <Navbar />
      <div className="flex flex-1 items-center justify-center bg-[#101828]">
        <SignupForm onSubmit={handleSignup} loading={loading} error={error} />
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