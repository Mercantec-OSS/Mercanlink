import { createContext, useContext, useState } from "react"
import { useNavigate } from "react-router-dom"
import { type User } from "@/types"
import { decodeTokenAndMapToUser } from "@/services/jwtService"
import {
  clearStoredTokens,
  exchangeCodeForTokens,
  getAccessToken,
  getLogoutUrl,
  startLoginRedirect,
} from "@/services/authService"

interface AuthContextType {
  user: User | null
  login: () => Promise<void>
  completeLogin: (code: string, state: string) => Promise<void>
  logout: () => void
  isAuthenticated: boolean
}

const AuthContext = createContext<AuthContextType | null>(null)

function getInitialUser(): User | null {
  const token = getAccessToken()
  if (!token) return null
  return decodeTokenAndMapToUser(token)
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(getInitialUser)
  const navigate = useNavigate()

  const login = async () => {
    await startLoginRedirect()
  }

  const completeLogin = async (code: string, state: string) => {
    const { accessToken } = await exchangeCodeForTokens(code, state)
    const newUser = decodeTokenAndMapToUser(accessToken)
    if (!newUser) {
      throw new Error("Access token kunne ikke valideres.")
    }
    setUser(newUser)
    navigate("/users")
  }

  const logout = () => {
    clearStoredTokens()
    setUser(null)
    window.location.assign(getLogoutUrl())
  }

  return (
    <AuthContext.Provider
      value={{ user, login, completeLogin, logout, isAuthenticated: !!user }}
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