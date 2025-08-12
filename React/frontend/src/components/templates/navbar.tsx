import { Link, useNavigate } from "react-router-dom"
import { Button } from "@/components/ui/button"
import { useAuth } from "@/contexts/AuthContext"
import { useState } from "react"
import { FaBars, FaDiscord } from "react-icons/fa"
import { ModeToggle } from "@/components/ui/mode-toggle"

export default function Navbar() {
    const { isAuthenticated } = useAuth()
    const [sidebarOpen, setSidebarOpen] = useState(false)
    const navigate = useNavigate()
    return (
        <nav className="w-full py-4 bg-white text-black flex items-center px-6">
            {/* Left: Burger menu */}
            <div className="flex items-center justify-start min-w-[60px]">
                <button
                    className="p-2 rounded hover:bg-gray-4 00 focus:outline-none"
                    onClick={() => setSidebarOpen(true)}
                    aria-label="Open sidebar"
                >
                    <FaBars size={24} />
                </button>
                {sidebarOpen && (
                    <>
                        <div className="fixed inset-0 z-40 flex">
                            <div className="fixed inset-0 bg-black opacity-30" onClick={() => setSidebarOpen(false)} />
                            <aside className="relative z-50 w-64 bg-white h-full shadow-lg flex flex-col p-4">
                                <button
                                    className="self-end mb-4 p-2 rounded hover:bg-gray-600"
                                    onClick={() => setSidebarOpen(false)}
                                    aria-label="Close sidebar"
                                >
                                    ✕
                                </button>
                                <nav className="flex flex-col gap-4 items-center justify-start h-full">
                                    <Link
                                        to="/"
                                        onClick={() => setSidebarOpen(false)}
                                        className="px-4 py-2 text-lg rounded bg-black text-white hover:bg-gray-600 transition w-30 text-center"
                                    >
                                        Valgfag
                                    </Link>

                                    {!isAuthenticated && (
                                        <>
                                            <Link
                                                to="/login"
                                                onClick={() => setSidebarOpen(false)}
                                                className="px-4 py-2 text-lg rounded bg-black text-white hover:bg-gray-600 transition w-30 text-center"
                                            >
                                                Login
                                            </Link>
                                            <Link
                                                to="/signup"
                                                onClick={() => setSidebarOpen(false)}
                                                className="px-4 py-2 text-lg rounded bg-black text-white hover:bg-gray-600 transition w-30 text-center"
                                            >
                                                Sign up
                                            </Link>
                                        </>
                                    )}

                                    {isAuthenticated && (
                                        <Link
                                            to="/"
                                            onClick={() => setSidebarOpen(false)}
                                            className="px-4 py-2 text-lg rounded bg-black text-white hover:bg-gray-600 transition w-30 text-center"
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
            <div className="flex flex-1 justify-center">
                <div className="flex items-center pl-86">
                    <img src="/src/components/images/mercantec-space-logo.png" alt="Mercantec Space Logo" className="h-8 w-8 object-contain" />
                    <Button onClick={() => navigate("/")} className="ml-1 font-semibold text-lg hover:text-gray-600 active:scale-95 transition-transform cursor-pointer bg-transparent border-0 shadow-none text-blue">Mercantec Space</Button>
                </div>
            </div>

            {/* Right: Teams and Discord buttons */}
            <div className="flex gap-1 justify-end min-w-[180px] mr-2">
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
            </div>

            <div className="flex gap-2 justify-end min-w-[180px]">
                <Button asChild className="bg-black text-white hover:bg-gray-600 w-24">
                    {isAuthenticated ? (
                        <Link to="/users">View Users</Link>
                    ) : (
                        <Link to="/login">Login</Link>
                    )}
                </Button>
                <Button asChild className="bg-black text-white hover:bg-gray-600 w-24">
                    {isAuthenticated ? (
                        <Link to="/logout">Logout</Link>
                    ) : (
                        <Link to="/signup">Signup</Link>
                    )}
                </Button>
                <ModeToggle />
            </div>
        </nav>
    )
}