import { jwtDecode } from "jwt-decode"
import { type User } from "@/types"
import { authConfig } from "./authConfig"

interface DecodedJwtPayload {
  sub: string
  email?: string
  name?: string
  preferred_username?: string
  login_method?: string
  role?: string | string[]
  exp: number
  iat: number
  iss: string
  aud: string | string[]
}

export function decodeTokenAndMapToUser(token: string): User | null {
  try {
    const decoded = jwtDecode<DecodedJwtPayload>(token)

    // Check if the token is expired
    if (decoded.exp * 1000 < Date.now()) {
      return null
    }

    const audiences = Array.isArray(decoded.aud) ? decoded.aud : [decoded.aud]
    if (decoded.iss !== authConfig.issuer || !audiences.includes(authConfig.audience)) {
      return null
    }

    const username = decoded.preferred_username || decoded.name || decoded.email || "Ukendt bruger"

    return {
      id: decoded.sub,
      email: decoded.email || "",
      username,
      discordId: "",
      level: 1,
      experience: 0,
      roles: decoded.role
        ? (Array.isArray(decoded.role) ? decoded.role : [decoded.role])
        : [],
      globalName: decoded.name || username,
      avatarUrl: "",
      firstName: "",
      surnameInitial: "",
      passwordChanged: false,
      studentId: "",
      department: "",
      employeeType: "",
      adCreatedAt: "",
      lastAdSync: "",
      isActive: true,
      createdAt: new Date(decoded.iat * 1000).toISOString(),
    }
  } catch (error) {
    console.error("Failed to decode JWT:", error)
    return null
  }
} 