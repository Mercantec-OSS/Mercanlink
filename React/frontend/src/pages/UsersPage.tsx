import { useEffect, useState } from "react"
import { type User } from "@/types"
import { getUsers } from "@/services/userService"
import { useAuth } from "@/contexts/AuthContext"
import { Button } from "@/components/ui/button"
import Layout_alt from "@/components/templates/layout"
import { LogOut } from "lucide-react"

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
          setError("Ukendt fejl opstod")
        }
      } finally {
        setLoading(false)
      }
    }

    loadUsers()
  }, [])

  if (loading) {
    return (
      <Layout_alt>
        <div className="container-shell py-16 text-slate-600">Indlæser brugere...</div>
      </Layout_alt>
    )
  }

  if (error) {
    return (
      <Layout_alt>
        <div className="container-shell py-16 text-red-600">Fejl: {error}</div>
      </Layout_alt>
    )
  }

  return (
    <Layout_alt>
      <section className="container-shell py-12 sm:py-16 lg:py-20">
        <Card className="soft-card border-slate-100">
          <CardHeader className="flex flex-col gap-4 border-b border-slate-100 py-6 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <CardTitle className="text-2xl font-bold tracking-[-0.02em] text-slate-900">
                Velkommen, {user?.username ?? "bruger"}
              </CardTitle>
              <CardDescription className="mt-2 text-slate-600">
                Her er en oversigt over brugere i systemet.
              </CardDescription>
            </div>
            <Button variant="outline" onClick={logout} className="inline-flex items-center gap-2">
              <LogOut className="h-4 w-4" />
              Log ud
            </Button>
          </CardHeader>
          <CardContent className="p-0">
            <Table>
              <TableHeader>
                <TableRow className="bg-slate-50/70">
                  <TableHead className="pl-6">Avatar</TableHead>
                  <TableHead>Brugernavn</TableHead>
                  <TableHead>Niveau</TableHead>
                  <TableHead className="pr-6">Erfaring</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {users.map((currentUser) => (
                  <TableRow key={currentUser.id} className="hover:bg-indigo-50/35">
                    <TableCell className="pl-6">
                      <Avatar>
                        <AvatarImage src={currentUser.avatarUrl} alt={currentUser.username} />
                        <AvatarFallback>{currentUser.username.slice(0, 2).toUpperCase()}</AvatarFallback>
                      </Avatar>
                    </TableCell>
                    <TableCell className="font-medium text-slate-900">{currentUser.username}</TableCell>
                    <TableCell>{currentUser.level}</TableCell>
                    <TableCell className="pr-6">{currentUser.experience}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      </section>
    </Layout_alt>
  )
} 