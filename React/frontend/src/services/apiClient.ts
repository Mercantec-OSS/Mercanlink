import { storageService } from "./storageService"

const API_BASE_URL = "http://localhost:5053/api"

export async function apiClient<T>(
  url: string,
  options?: RequestInit,
): Promise<T> {
  const token = storageService.getItem<string>("accessToken")
  const headers = new Headers(options?.headers)

  if (token) {
    headers.append("Authorization", `Bearer ${token}`)
  }
  if (!headers.has("Content-Type") && options?.body) {
    headers.append("Content-Type", "application/json")
  }

  const response = await fetch(`${API_BASE_URL}${url}`, {
    ...options,
    headers,
  })

  if (!response.ok) {
    const errorBody = await response.text()
    console.error("API Request Failed:", response.status, errorBody)
    throw new Error(`API request failed: ${response.statusText}`)
  }

  if (response.status === 204 || response.headers.get("Content-Length") === "0") {
    return Promise.resolve({} as T)
  }

  return response.json()
}
