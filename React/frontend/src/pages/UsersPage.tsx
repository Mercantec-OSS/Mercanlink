import { useEffect, useState } from "react"
import { type User } from "@/types"
import { getUsers } from "@/services/userService"
import { useAuth } from "@/contexts/AuthContext"
import { Button } from "@/components/ui/button"

import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"

export function UsersPage() {
  const [users, setUsers] = useState<User[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const { logout, user } = useAuth()

  useEffect(() => {
    async function loadUsers() {
      try {
        const data = await getUsers()
        setUsers(data)
      } catch (error) {
        if (error instanceof Error) {
          setError(error.message)
        } else {
          setError("An unknown error occurred")
        }
      } finally {
        setLoading(false)
      }
    }

    loadUsers()
  }, [])

  if (loading) {
    return <div>Loading...</div>
  }

  if (error) {
    return <div>Error: {error}</div>
  }

  return (
    <div className="container mx-auto py-10">
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <div>
            <CardTitle>Welcome, {user?.username ?? "User"}</CardTitle>
            <CardDescription>
              A list of all users in the system.
            </CardDescription>
          </div>
          <Button variant="outline" onClick={logout}>Logout</Button>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Avatar</TableHead>
                <TableHead>Username</TableHead>
                <TableHead>Level</TableHead>
                <TableHead>Experience</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {users.map((user) => (
                <TableRow key={user.id}>
                  <TableCell>
                    <Avatar>
                      <AvatarImage src={user.avatarUrl} alt={user.username} />
                      <AvatarFallback>{user.username.slice(0, 2).toUpperCase()}</AvatarFallback>
                    </Avatar>
                  </TableCell>
                  <TableCell>{user.username}</TableCell>
                  <TableCell>{user.level}</TableCell>
                  <TableCell>{user.experience}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  )
} 