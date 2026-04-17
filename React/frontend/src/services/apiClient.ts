import { clearStoredTokens, getAccessToken, refreshTokens } from "./authService"

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "/api"

export async function apiClient<T>(
  url: string,
  options?: RequestInit,
): Promise<T> {
  const executeRequest = async (token: string | null): Promise<Response> => {
    const headers = new Headers(options?.headers)

    if (token) {
      headers.set("Authorization", `Bearer ${token}`)
    }
    if (!headers.has("Content-Type") && options?.body) {
      headers.set("Content-Type", "application/json")
    }

    return fetch(`${API_BASE_URL}${url}`, {
      ...options,
      headers,
    })
  }

  let response = await executeRequest(getAccessToken())

  if (response.status === 401) {
    try {
      const refreshed = await refreshTokens()
      response = await executeRequest(refreshed.accessToken)
    } catch {
      clearStoredTokens()
      throw new Error("Sessionen er udløbet. Log venligst ind igen.")
    }
  }

  if (!response.ok) {
    const errorBody = await response.text()
    console.error("API Request Failed:", response.status, errorBody)
    const trimmedBody = errorBody.trim()
    throw new Error(trimmedBody || `API request failed: ${response.statusText}`)
  }

  if (response.status === 204 || response.headers.get("Content-Length") === "0") {
    return Promise.resolve({} as T)
  }

  return response.json()
}
