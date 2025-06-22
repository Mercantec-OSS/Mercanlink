import { jwtDecode } from "jwt-decode"
import { type User } from "@/types"

interface DecodedJwtPayload {
  nameid: string
  email: string
  unique_name: string
  discord_id: string
  level: string
  experience: string
  role: string | string[]
  nbf: number
  exp: number
  iat: number
  iss: string
  aud: string
}

export function decodeTokenAndMapToUser(token: string): User | null {
  try {
    const decoded = jwtDecode<DecodedJwtPayload>(token)

    // Check if the token is expired
    if (decoded.exp * 1000 < Date.now()) {
      return null
    }

    return {
      id: decoded.nameid,
      email: decoded.email,
      username: decoded.unique_name,
      discordId: decoded.discord_id,
      level: parseInt(decoded.level, 10),
      experience: parseInt(decoded.experience, 10),
      roles: Array.isArray(decoded.role) ? decoded.role : [decoded.role],
      // The following fields are not in the JWT, so we set defaults
      globalName: decoded.unique_name,
      avatarUrl: "", // This information is not in the token.
      firstName: "",
      surnameInitial: "",
      passwordChanged: false, // Default value
      studentId: "",
      department: "",
      employeeType: "",
      adCreatedAt: "",
      lastAdSync: "",
      isActive: true, // Assume active if token is valid
      createdAt: new Date(decoded.iat * 1000).toISOString(),
    }
  } catch (error) {
    console.error("Failed to decode JWT:", error)
    return null
  }
} 