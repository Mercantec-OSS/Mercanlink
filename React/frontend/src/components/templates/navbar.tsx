import { Link, useLocation } from "react-router-dom"
import { Button } from "@/components/ui/button"
import { useAuth } from "@/contexts/AuthContext"
import { useState } from "react"
import { Menu, X, LogOut, Users, Layers, FileText, UserCircle2 } from "lucide-react"

const navItems = [
  { to: "/valgfag", label: "Valgfag", icon: Layers },
  { to: "/form", label: "Indsend materiale", icon: FileText },
]

export default function Navbar() {
  const { isAuthenticated, logout } = useAuth()
  const location = useLocation()
  const [menuOpen, setMenuOpen] = useState(false)

  const closeMenu = () => setMenuOpen(false)

  return (
    <header className="sticky top-0 z-50 border-b border-slate-200/70 bg-white/90 backdrop-blur supports-[backdrop-filter]:bg-white/75">
      <div className="container-shell">
        <nav className="flex min-h-[76px] items-center justify-between gap-4">
          <Link to="/" className="flex items-center gap-3" onClick={closeMenu}>
            <img src="/Mercanlink-Logo.png" alt="Mercanlink logo" className="h-10 w-auto object-contain" />
            <span className="hidden text-sm font-medium text-slate-500 sm:inline">Mercanlink Platform</span>
          </Link>

          <div className="hidden items-center gap-1 lg:flex">
            {navItems.map((item) => {
              const Icon = item.icon
              const isActive = location.pathname === item.to
              return (
                <Link
                  key={item.to}
                  to={item.to}
                  className={`inline-flex items-center gap-2 rounded-lg px-3 py-2 text-sm font-semibold transition ${
                    isActive ? "bg-indigo-50 text-indigo-700" : "text-slate-600 hover:bg-slate-50 hover:text-slate-900"
                  }`}
                >
                  <Icon className="h-4 w-4" />
                  {item.label}
                </Link>
              )
            })}
          </div>

          <div className="hidden items-center gap-2 lg:flex">
            {isAuthenticated ? (
              <>
                <Button asChild variant="outline">
                  <Link to="/profile" className="inline-flex items-center gap-2">
                    <UserCircle2 className="h-4 w-4" />
                    Profil
                  </Link>
                </Button>
                <Button asChild variant="outline">
                  <Link to="/users" className="inline-flex items-center gap-2">
                    <Users className="h-4 w-4" />
                    Brugere
                  </Link>
                </Button>
                <Button variant="outline" onClick={logout} className="inline-flex items-center gap-2">
                  <LogOut className="h-4 w-4" />
                  Log ud
                </Button>
              </>
            ) : (
              <Button asChild variant="outline">
                <Link to="/login">Log ind</Link>
              </Button>
            )}
          </div>

          <button
            type="button"
            className="inline-flex items-center justify-center rounded-lg border border-slate-200 p-2 text-slate-700 lg:hidden"
            aria-label="Åbn menu"
            onClick={() => setMenuOpen((prev) => !prev)}
          >
            {menuOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
          </button>
        </nav>
      </div>

      {menuOpen && (
        <div className="border-t border-slate-200 bg-white lg:hidden">
          <div className="container-shell py-4">
            <div className="flex flex-col gap-2">
              {navItems.map((item) => {
                const Icon = item.icon
                return (
                  <Link
                    key={item.to}
                    to={item.to}
                    onClick={closeMenu}
                    className="inline-flex items-center gap-2 rounded-lg px-3 py-2 text-sm font-semibold text-slate-700 hover:bg-indigo-50 hover:text-indigo-700"
                  >
                    <Icon className="h-4 w-4" />
                    {item.label}
                  </Link>
                )
              })}

              <div className="mt-2 grid grid-cols-2 gap-2">
                {isAuthenticated ? (
                  <>
                    <Button asChild variant="outline">
                      <Link to="/profile" onClick={closeMenu}>Profil</Link>
                    </Button>
                    <Button asChild variant="outline">
                      <Link to="/users" onClick={closeMenu}>Brugere</Link>
                    </Button>
                    <Button variant="outline" onClick={() => { closeMenu(); logout() }}>
                      Log ud
                    </Button>
                  </>
                ) : (
                  <Button asChild variant="outline" className="col-span-2">
                    <Link to="/login" onClick={closeMenu}>Log ind</Link>
                  </Button>
                )}
              </div>
            </div>
          </div>
        </div>
      )}
    </header>
  )
}