import { Navigate, Outlet } from "react-router-dom"
import { useAuth } from "../contexts/AuthContext"

interface ProtectedRouteProps {
  allowedRoles?: string[]
}

export function ProtectedRoute({ allowedRoles }: ProtectedRouteProps) {
  const { isAuthenticated, user } = useAuth()

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />
  }

  if (allowedRoles?.length) {
    const normalizedAllowedRoles = allowedRoles.map((role) => role.toLowerCase())
    const userRoles = user?.roles?.map((role) => role.toLowerCase()) ?? []
    const hasAllowedRole = userRoles.some((role) => normalizedAllowedRoles.includes(role))

    if (!hasAllowedRole) {
      return <Navigate to="/" replace />
    }
  }

  return <Outlet />
} 