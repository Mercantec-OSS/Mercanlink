import { Link } from "react-router-dom"
import { Button } from "@/components/ui/button"
import { useAuth } from "./contexts/AuthContext"

function App() {
  const { isAuthenticated } = useAuth()

  return (
    <div className="flex min-h-svh flex-col items-center justify-center gap-4">
      <h1 className="text-4xl font-bold">Home Page</h1>
      <Button asChild>
        {isAuthenticated ? (
          <Link to="/users">View Users</Link>
        ) : (
          <Link to="/login">Login</Link>
        )}
      </Button>
    </div>
  )
}

export default App