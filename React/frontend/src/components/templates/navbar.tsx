import { Link } from "react-router-dom"
import { Button } from "@/components/ui/button"
import { useAuth } from "@/contexts/AuthContext"
import { useState } from "react"
import { FaBars, FaDiscord, FaSun, FaMoon } from "react-icons/fa"
import { useTheme } from "@/components/ui/theme-provider"

export default function Navbar() {
  const { isAuthenticated } = useAuth()
  const [sidebarOpen, setSidebarOpen] = useState(false)
  const { theme, setTheme } = useTheme()
  const isDark = theme === "dark" || (theme === "system" && window.matchMedia("(prefers-color-scheme: dark)").matches)
  const bgClass = isDark ? "bg-black text-white" : "bg-gray-600 text-black"

  return (
    <nav className={`relative w-full py-4 ${bgClass} flex items-center px-6`}>
      {/* Left: Burger menu */}
      <div className="flex items-center min-w-[60px]">
        <button
          className="p-2 rounded hover:bg-gray-400 focus:outline-none"
          onClick={() => setSidebarOpen(true)}
          aria-label="Open sidebar"
        >
          <FaBars size={24} />
        </button>
        {sidebarOpen && (
          <>
            <div className="fixed inset-0 z-40 flex">
              <div className="fixed inset-0 bg-black opacity-30" onClick={() => setSidebarOpen(false)} />
              <aside className={`relative z-50 w-64 ${bgClass} h-full shadow-lg flex flex-col p-4`}>
                <button
                  className="self-end mb-4 p-2 rounded hover:bg-gray-400"
                  onClick={() => setSidebarOpen(false)}
                  aria-label="Close sidebar"
                >
                  ✕
                </button>
                <nav className="flex flex-col gap-4 items-center justify-start h-full">
                  <Link
                    to="/"
                    onClick={() => setSidebarOpen(false)}
                    className={`px-4 py-2 text-lg rounded ${isDark ? "bg-white text-black" : "bg-black text-white"} hover:bg-gray-400 transition w-30 text-center`}
                  >
                    Valgfag
                  </Link>
                  {!isAuthenticated && (
                    <>
                      <Link
                        to="/login"
                        onClick={() => setSidebarOpen(false)}
                        className={`px-4 py-2 text-lg rounded ${isDark ? "bg-white text-black" : "bg-black text-white"} hover:bg-gray-400 transition w-30 text-center`}
                      >
                        Login
                      </Link>
                      <Link
                        to="/signup"
                        onClick={() => setSidebarOpen(false)}
                        className={`px-4 py-2 text-lg rounded ${isDark ? "bg-white text-black" : "bg-black text-white"} hover:bg-gray-400 transition w-30 text-center`}
                      >
                        Sign up
                      </Link>
                    </>
                  )}
                  {isAuthenticated && (
                    <Link
                      to="/"
                      onClick={() => setSidebarOpen(false)}
                      className={`px-4 py-2 text-lg rounded ${isDark ? "bg-white text-black" : "bg-black text-white"} hover:bg-gray-400 transition w-30 text-center`}
                    >
                      Logout
                    </Link>
                  )}
                </nav>
              </aside>
            </div>
          </>
        )}
      </div>

      {/* Center: Logo */}
      <div className="absolute left-0 right-0 flex justify-center items-center pointer-events-none">
        <div className="flex items-center pointer-events-auto">
          <img src="/src/components/images/mercantec-space-logo.png" alt="Mercantec Space Logo" className="h-8 w-8 object-contain" />
          <span className="ml-1 font-semibold text-lg">Mercantec Space</span>
        </div>
      </div>

      {/* Right: Teams, Discord, Auth buttons, Theme toggle */}
      <div className="flex gap-2 items-center min-w-[180px] justify-end ml-auto">
        <a  
          href="https://teams.microsoft.com/l/team/19%3AILE0F1oc9fGJMPT6mFVjcQ7N10W1SE8XA9r1X4N6Pko1%40thread.tacv2/conversations?groupId=a014d18d-572a-4621-bf7d-682f06ec1fea&tenantId=17aab4ce-4b26-487e-9bea-1e2a70348bf0"
          target="_blank"
          rel="noopener noreferrer"
          title="Open Teams"
          className="flex items-center justify-center"
          style={{ height: "48px", width: "48px" }}
        >
          <img
            src="/src/components/images/icons8-microsoft-teams-2019-48.png"
            alt="Microsoft Teams"
            className="h-10 w-10 object-contain"
          />
        </a>
        <a
          href="https://discord.gg/uHkYDgsKcm"
          target="_blank"
          rel="noopener noreferrer"
          title="Open Discord"
          className="flex items-center justify-center"
          style={{ height: "48px", width: "48px" }}
        >
          <FaDiscord size={40} color="#5865F2" />
        </a>
        <Button asChild className={`${isDark ? "bg-white text-black" : "bg-black text-white"} hover:bg-gray-400 w-24`}>
          {isAuthenticated ? (
            <Link to="/users">View Users</Link>
          ) : (
            <Link to="/login">Login</Link>
          )}
        </Button>
        <Button asChild className={`${isDark ? "bg-white text-black" : "bg-black text-white"} hover:bg-gray-400 w-24`}>
          {isAuthenticated ? (
            <Link to="/logout">Logout</Link>
          ) : (
            <Link to="/signup">Signup</Link>
          )}
        </Button>
        {/* Theme toggle button */}
        <Button onClick={() => setTheme(isDark ? "light" : "dark")} className={`${isDark ? "bg-white text-black" : "bg-black text-white"} hover:bg-gray-400 w-10 flex items-center justify-center`}>
          {isDark ? <FaSun color="black" size={20} /> : <FaMoon color="white" size={20} />}
        </Button>
      </div>
    </nav>
  )
}