import { getAccessToken, refreshTokens } from "./authService"

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "/api"

export async function uploadEventBannerImage(file: File): Promise<string> {
  const run = async (token: string | null) => {
    const form = new FormData()
    form.append("file", file)
    const headers = new Headers()
    if (token) headers.set("Authorization", `Bearer ${token}`)
    return fetch(`${API_BASE_URL}/media/events/banner`, { method: "POST", headers, body: form })
  }

  let response = await run(getAccessToken())
  if (response.status === 401) {
    try {
      const refreshed = await refreshTokens()
      response = await run(refreshed.accessToken)
    } catch {
      throw new Error("Sessionen er udløbet. Log venligst ind igen.")
    }
  }

  if (!response.ok) {
    const raw = await response.text().catch(() => "")
    let message: string | undefined
    try {
      const parsed = JSON.parse(raw) as { message?: string }
      message = parsed?.message
    } catch {
      /* body er ikke JSON */
    }
    throw new Error(message || raw || `Upload fejlede (${response.status})`)
  }

  const data = (await response.json()) as { url?: string }
  if (!data.url) throw new Error("Server returnerede ingen URL.")
  return data.url
}
