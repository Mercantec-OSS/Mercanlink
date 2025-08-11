import { type User } from "@/types"
import { apiClient } from "./apiClient"

// Login functionality
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

// Signup functionality
export interface SignupCredentials {
  emailOrUsername: string
  password: string
}

interface SignupResponse {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: User
}

export const signupUser = (
  credentials: SignupCredentials,
): Promise<SignupResponse> =>
  apiClient("/Auth/signup", {
    method: "POST",
    body: JSON.stringify(credentials),
  })