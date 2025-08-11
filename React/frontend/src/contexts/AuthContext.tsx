import { createContext, useContext, useState } from "react"
import { useNavigate } from "react-router-dom"
import { type LoginCredentials, loginUser, type SignupCredentials } from "@/services/authService"
import { type User } from "@/types"
import { storageService } from "@/services/storageService"
import { decodeTokenAndMapToUser } from "@/services/jwtService"

interface AuthContextType {
  user: User | null
  login: (credentials: LoginCredentials) => Promise<void>
  signup: (credentials: SignupCredentials) => Promise<void>
  logout: () => void
  isAuthenticated: boolean
}

const AuthContext = createContext<AuthContextType | null>(null)

function getInitialUser(): User | null {
  const token = storageService.getItem<string>("accessToken")
  if (!token) return null
  return decodeTokenAndMapToUser(token)
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(getInitialUser)
  const navigate = useNavigate()

  const login = async (credentials: LoginCredentials) => {
    const { accessToken, refreshToken } = await loginUser(credentials)
    storageService.setItem("accessToken", accessToken)
    storageService.setItem("refreshToken", refreshToken)
    const newUser = decodeTokenAndMapToUser(accessToken)
    setUser(newUser)
    navigate("/users")
  }

  const signup = async (credentials: SignupCredentials) => {
    await login(credentials)
  }
  const logout = () => {
    storageService.removeItem("accessToken")
    storageService.removeItem("refreshToken")
    setUser(null)
    navigate("/login")
  }

  return (
    <AuthContext.Provider
      value={{ user, login, signup, logout, isAuthenticated: !!user }}
    >
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider")
  }
  return context
} 