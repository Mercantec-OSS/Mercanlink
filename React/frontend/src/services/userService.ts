import { type User } from "@/types"
import { apiClient } from "./apiClient"

export const getUsers = (): Promise<User[]> => apiClient("/User") 