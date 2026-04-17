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

function normalizeIssuer(value: string | undefined): string {
  return (value || "").replace(/\/+$/, "")
}

export function decodeTokenAndMapToUser(token: string): User | null {
  try {
    const decoded = jwtDecode<DecodedJwtPayload>(token)

    // Check if the token is expired
    if (decoded.exp * 1000 < Date.now()) {
      return null
    }

    const audiences = Array.isArray(decoded.aud) ? decoded.aud : [decoded.aud]
    const normalizedTokenIssuer = normalizeIssuer(decoded.iss)
    const normalizedExpectedIssuer = normalizeIssuer(authConfig.issuer)
    const issuerMatches = normalizedTokenIssuer === normalizedExpectedIssuer
    const audienceMatches = audiences.includes(authConfig.audience)

    if (!issuerMatches || !audienceMatches) {
      // We keep the user logged in client-side if token is otherwise valid.
      // Backend remains source-of-truth and performs strict JWT validation.
      console.warn("JWT issuer/audience afviger fra forventet konfiguration", {
        tokenIssuer: decoded.iss,
        expectedIssuer: authConfig.issuer,
        tokenAudience: audiences,
        expectedAudience: authConfig.audience,
      })
    }

    const username = decoded.preferred_username || decoded.name || decoded.email || "Ukendt bruger"

    return {
      id: decoded.sub,
      email: decoded.email || "",
      username,
      loginMethod: decoded.login_method || "ukendt",
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