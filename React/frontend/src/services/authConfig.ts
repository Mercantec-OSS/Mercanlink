const DEFAULT_ISSUER = "https://auth.mercantec.tech"

const issuer = (import.meta.env.VITE_AUTH_ISSUER || DEFAULT_ISSUER).replace(/\/+$/, "")
const defaultRedirectUri = typeof window !== "undefined" ? `${window.location.origin}/auth/callback` : ""
const defaultLogoutReturnUrl = typeof window !== "undefined" ? `${window.location.origin}/login` : ""

export const authConfig = {
  issuer,
  clientId: import.meta.env.VITE_AUTH_CLIENT_ID || "",
  audience: import.meta.env.VITE_AUTH_AUDIENCE || "mercantec-apps",
  scopes: import.meta.env.VITE_AUTH_SCOPES || "openid profile email",
  redirectUri: import.meta.env.VITE_AUTH_REDIRECT_URI || defaultRedirectUri,
  logoutReturnUrl: import.meta.env.VITE_AUTH_LOGOUT_RETURN_URL || defaultLogoutReturnUrl,
  authorizeEndpoint: `${issuer}/oauth/authorize`,
  tokenEndpoint: `${issuer}/oauth/token`,
  signoutEndpoint: `${issuer}/signout`,
}
