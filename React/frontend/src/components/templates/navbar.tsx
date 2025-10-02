import { Link, useNavigate } from "react-router-dom"
import { Button } from "@/components/ui/button"
import { useAuth } from "@/contexts/AuthContext"
import { useState } from "react"
import { FaBook, FaDiscord, FaSignInAlt } from "react-icons/fa"
import { ModeToggle } from "@/components/ui/mode-toggle"
import mercantecLogo from "@/components/images/mercantec-space-logo.png"
import teamsIcon from "@/components/images/icons8-microsoft-teams-2019-48.png"

export default function Navbar() {
    const { isAuthenticated } = useAuth();
    const [sidebarOpen, setSidebarOpen] = useState(false);
    const navigate = useNavigate();
    return (
        <div className="relative">
            <nav className="w-full py-4 bg-white text-black flex items-center px-6 shadow-md border-b">
                {/* Left: Burger menu */}
                <div className="flex items-center justify-start min-w-[60px]">
                    <button
                        className="p-2 rounded hover:bg-gray-400 focus:outline-none"
                        onClick={() => setSidebarOpen((open) => !open)}
                        aria-label="Toggle sidebar"
                    >
                        <span className="block w-6 h-6 relative">
                            <span
                                className={`absolute left-0 top-1/2 w-6 h-0.5 bg-black transition-all duration-300 ${sidebarOpen ? 'rotate-45 top-3' : '-translate-y-2'}`}
                            ></span>
                            <span
                                className={`absolute left-0 top-1/2 w-6 h-0.5 bg-black transition-all duration-300 ${sidebarOpen ? 'opacity-0' : ''}`}
                            ></span>
                            <span
                                className={`absolute left-0 top-1/2 w-6 h-0.5 bg-black transition-all duration-300 ${sidebarOpen ? '-rotate-45 top-3' : 'translate-y-2'}`}
                            ></span>
                        </span>
                    </button>
                </div>

                {/* Center: Logo */}
                <div className="flex flex-1 justify-center">
                    <div className="flex items-center pl-86">
                        <img src={mercantecLogo} alt="Mercantec Space Logo" className="h-8 w-8 object-contain" />
                        <Button onClick={() => navigate("/")} className="ml-1 font-semibold text-lg hover:text-gray-600 active:scale-95 transition-transform cursor-pointer bg-transparent border-0 shadow-none text-blue">MercanLink </Button>
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
                            src={teamsIcon}
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
                    <Button asChild className="bg-[#202434] text-white hover:bg-gray-600 w-24">
                        {isAuthenticated ? (
                            <Link to="/users">View Users</Link>
                        ) : (
                            <Link to="/login">Login</Link>
                        )}
                    </Button>
                    <Button asChild className="bg-[#202434] text-white hover:bg-gray-600 w-24">
                        {isAuthenticated ? (
                            <Link to="/logout">Logout</Link>
                        ) : (
                            <Link to="/signup">Signup</Link>
                        )}
                    </Button>
                    <ModeToggle />
                </div>
            </nav>
            {/* Sidebar under navbar, left side */}
            {sidebarOpen && (
                <aside className="absolute left-0 top-full w-72 min-h-screen bg-gradient-to-br from-[#1a1f2e] via-[#232a36] to-[#181c22] flex flex-col shadow-2xl z-40 border-r border-gray-600/30">
                    {/* Header section with close button */}
                    <div className="flex items-center justify-between p-6 border-b border-gray-600/30">
                        <div className="flex items-center gap-3">
                            <img src={mercantecLogo} alt="Logo" className="h-8 w-8 object-contain" />
                            <span className="text-white font-semibold text-lg">Menu</span>
                        </div>
                        <button
                            className="p-2 rounded-lg text-gray-300 hover:text-white hover:bg-gray-700/50 transition-all duration-200"
                            onClick={() => setSidebarOpen(false)}
                            aria-label="Close sidebar"
                        >
                            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                            </svg>
                        </button>
                    </div>

                    {/* Navigation section */}
                    <nav className="flex flex-col p-4 space-y-2 flex-1">
                        <Link
                            to="/valgfag"
                            onClick={() => setSidebarOpen(false)}
                            className="flex items-center gap-4 px-4 py-3 text-gray-200 rounded-xl transition-all duration-200 hover:bg-gradient-to-r hover:from-blue-600/20 hover:to-purple-600/20 hover:text-white hover:shadow-lg hover:scale-[1.02] group"
                        >
                            <FaBook className="text-blue-400 group-hover:text-blue-300 transition-colors" size={20} />
                            <span className="font-medium">Valgfag</span>
                        </Link>

                        {!isAuthenticated && (
                            <>
                                <Link
                                    to="/login"
                                    onClick={() => setSidebarOpen(false)}
                                    className="flex items-center gap-4 px-4 py-3 text-gray-200 rounded-xl transition-all duration-200 hover:bg-gradient-to-r hover:from-green-600/20 hover:to-emerald-600/20 hover:text-white hover:shadow-lg hover:scale-[1.02] group"
                                >
                                    <FaSignInAlt className="text-green-400 group-hover:text-green-300 transition-colors" size={20} />
                                    <span className="font-medium">Login</span>
                                </Link>
                                <Link
                                    to="/signup"
                                    onClick={() => setSidebarOpen(false)}
                                    className="flex items-center gap-4 px-4 py-3 text-gray-200 rounded-xl transition-all duration-200 hover:bg-gradient-to-r hover:from-purple-600/20 hover:to-pink-600/20 hover:text-white hover:shadow-lg hover:scale-[1.02] group"
                                >
                                    <FaSignInAlt className="text-purple-400 group-hover:text-purple-300 transition-colors" size={20} />
                                    <span className="font-medium">Sign up</span>
                                </Link>
                            </>
                        )}

                        {isAuthenticated && (
                            <Link
                                to="/"
                                onClick={() => setSidebarOpen(false)}
                                className="flex items-center gap-4 px-4 py-3 text-gray-200 rounded-xl transition-all duration-200 hover:bg-gradient-to-r hover:from-red-600/20 hover:to-orange-600/20 hover:text-white hover:shadow-lg hover:scale-[1.02] group"
                            >
                                <svg className="w-5 h-5 text-red-400 group-hover:text-red-300 transition-colors" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                                </svg>
                                <span className="font-medium">Logout</span>
                            </Link>
                        )}
                    </nav>

                    {/* Footer section */}
                    <div className="p-4 border-t border-gray-600/30">
                        <div className="text-center text-gray-400 text-sm">
                            <span>MercanLink</span>
                        </div>
                    </div>
                </aside>
            )}
        </div>
    );
}