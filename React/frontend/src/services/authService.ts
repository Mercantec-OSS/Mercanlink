import { type User } from "@/types"
import { apiClient } from "./apiClient"

export interface LoginCredentials {
  emailOrUsername: string
  password?: string
}

interface LoginResponse {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: User
}

export const loginUser = (
  credentials: LoginCredentials,
): Promise<LoginResponse> =>
  apiClient("/Auth/login", {
    method: "POST",
    body: JSON.stringify(credentials),
  }) 