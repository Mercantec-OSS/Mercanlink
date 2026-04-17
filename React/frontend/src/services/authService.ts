import { authConfig } from "./authConfig"
import { storageService } from "./storageService"

export interface AuthTokens {
  accessToken: string
  refreshToken: string
  expiresAt: number
}

interface MercantecTokenResponse {
  access_token: string
  refresh_token: string
  token_type: string
  expires_in: number
}

const OAUTH_STATE_KEY = "oauth_state"
const OAUTH_VERIFIER_KEY = "oauth_code_verifier"
const ACCESS_TOKEN_KEY = "accessToken"
const REFRESH_TOKEN_KEY = "refreshToken"
const ACCESS_TOKEN_EXPIRY_KEY = "accessTokenExpiresAt"

function ensureClientId(): string {
  if (!authConfig.clientId) {
    throw new Error("VITE_AUTH_CLIENT_ID mangler. Sæt variablen før login.")
  }
  return authConfig.clientId
}

function toBase64Url(bytes: ArrayBuffer): string {
  const uint8 = new Uint8Array(bytes)
  let binary = ""
  for (const byte of uint8) {
    binary += String.fromCharCode(byte)
  }
  return btoa(binary).replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/g, "")
}

function generateRandomString(length = 64): string {
  const charset = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~"
  const randomValues = new Uint8Array(length)
  crypto.getRandomValues(randomValues)
  return Array.from(randomValues, (value) => charset[value % charset.length]).join("")
}

async function createCodeChallenge(codeVerifier: string): Promise<string> {
  const data = new TextEncoder().encode(codeVerifier)
  const digest = await crypto.subtle.digest("SHA-256", data)
  return toBase64Url(digest)
}

function buildTokenPayload(params: Record<string, string>): string {
  const body = new URLSearchParams()
  Object.entries(params).forEach(([key, value]) => body.set(key, value))
  return body.toString()
}

async function fetchTokens(body: string): Promise<AuthTokens> {
  const response = await fetch(authConfig.tokenEndpoint, {
    method: "POST",
    headers: {
      "Content-Type": "application/x-www-form-urlencoded",
    },
    body,
  })

  if (!response.ok) {
    const errorText = await response.text()
    throw new Error(`Token exchange fejlede (${response.status}): ${errorText}`)
  }

  const data = (await response.json()) as MercantecTokenResponse
  if (!data.access_token || !data.refresh_token || !data.expires_in) {
    throw new Error("Token-svar fra auth-service mangler felter.")
  }

  return {
    accessToken: data.access_token,
    refreshToken: data.refresh_token,
    expiresAt: Date.now() + data.expires_in * 1000,
  }
}

function clearOAuthState(): void {
  storageService.removeItem(OAUTH_STATE_KEY)
  storageService.removeItem(OAUTH_VERIFIER_KEY)
}

export function clearStoredTokens(): void {
  storageService.removeItem(ACCESS_TOKEN_KEY)
  storageService.removeItem(REFRESH_TOKEN_KEY)
  storageService.removeItem(ACCESS_TOKEN_EXPIRY_KEY)
}

export function storeTokens(tokens: AuthTokens): void {
  storageService.setItem(ACCESS_TOKEN_KEY, tokens.accessToken)
  storageService.setItem(REFRESH_TOKEN_KEY, tokens.refreshToken)
  storageService.setItem(ACCESS_TOKEN_EXPIRY_KEY, tokens.expiresAt)
}

export function getStoredTokens(): AuthTokens | null {
  const accessToken = storageService.getItem<string>(ACCESS_TOKEN_KEY)
  const refreshToken = storageService.getItem<string>(REFRESH_TOKEN_KEY)
  const expiresAt = storageService.getItem<number>(ACCESS_TOKEN_EXPIRY_KEY)

  if (!accessToken || !refreshToken || !expiresAt) {
    return null
  }

  return { accessToken, refreshToken, expiresAt }
}

export function getAccessToken(): string | null {
  return storageService.getItem<string>(ACCESS_TOKEN_KEY)
}

export function isAccessTokenExpired(expiryTimestampMs: number | null): boolean {
  if (!expiryTimestampMs) {
    return true
  }
  return Date.now() >= expiryTimestampMs - 10000
}

export async function startLoginRedirect(): Promise<void> {
  const clientId = ensureClientId()
  const state = generateRandomString(48)
  const codeVerifier = generateRandomString(96)
  const codeChallenge = await createCodeChallenge(codeVerifier)

  storageService.setItem(OAUTH_STATE_KEY, state)
  storageService.setItem(OAUTH_VERIFIER_KEY, codeVerifier)

  const query = new URLSearchParams({
    response_type: "code",
    client_id: clientId,
    redirect_uri: authConfig.redirectUri,
    state,
    code_challenge: codeChallenge,
    code_challenge_method: "S256",
  })

  window.location.assign(`${authConfig.authorizeEndpoint}?${query.toString()}`)
}

export async function exchangeCodeForTokens(code: string, returnedState: string): Promise<AuthTokens> {
  const expectedState = storageService.getItem<string>(OAUTH_STATE_KEY)
  const codeVerifier = storageService.getItem<string>(OAUTH_VERIFIER_KEY)
  clearOAuthState()

  if (!expectedState || expectedState !== returnedState) {
    throw new Error("State matcher ikke. Login-flow er afbrudt eller ugyldigt.")
  }

  if (!codeVerifier) {
    throw new Error("Mangler PKCE verifier. Start login igen.")
  }

  const body = buildTokenPayload({
    grant_type: "authorization_code",
    code,
    redirect_uri: authConfig.redirectUri,
    client_id: ensureClientId(),
    code_verifier: codeVerifier,
  })

  const tokens = await fetchTokens(body)
  storeTokens(tokens)
  return tokens
}

export async function refreshTokens(): Promise<AuthTokens> {
  const existingTokens = getStoredTokens()
  if (!existingTokens?.refreshToken) {
    throw new Error("Mangler refresh token.")
  }

  const body = buildTokenPayload({
    grant_type: "refresh_token",
    refresh_token: existingTokens.refreshToken,
    client_id: ensureClientId(),
  })

  const tokens = await fetchTokens(body)
  storeTokens(tokens)
  return tokens
}

export function getLogoutUrl(): string {
  const returnUrl = encodeURIComponent(authConfig.logoutReturnUrl)
  return `${authConfig.signoutEndpoint}?returnUrl=${returnUrl}`
}