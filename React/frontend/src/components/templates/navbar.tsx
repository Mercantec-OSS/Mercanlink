import { Link } from "react-router-dom"
import { Button } from "@/components/ui/button"
import { useAuth } from "@/contexts/AuthContext"
import { useState } from "react"
import { FaBars } from "react-icons/fa"
import { ModeToggle } from "@/components/ui/mode-toggle"

export default function Navbar() {
    const { isAuthenticated } = useAuth()
    const [sidebarOpen, setSidebarOpen] = useState(false)

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
                                        Home
                                    </Link>
                                    {isAuthenticated && (
                                        <Link
                                            to="/users"
                                            onClick={() => setSidebarOpen(false)}
                                            className="px-4 py-2 text-lg rounded bg-black text-white hover:bg-gray-600 transition w-30 text-center"
                                        >
                                            Users
                                        </Link>
                                    )}
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
                                </nav>
                            </aside>
                        </div>
                    </>
                )}
            </div>

            {/* Center: Logo */}
            <div className="flex flex-1 justify-center">
                <div className="flex items-center pl-30">
                    <img src="/src/components/images/mercantec-space-logo.png" alt="Mercantec Space Logo" className="h-8 w-8 object-contain" />
                    <span className="ml-1 font-semibold text-lg">Mercantec Hub</span>
                </div>
            </div>

            {/* Right: Auth buttons */}
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