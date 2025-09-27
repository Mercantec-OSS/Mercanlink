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
                <aside className="absolute left-0 top-full w-64 h-189 bg-gradient-to-br from-[#232a36] to-[#181c22] flex flex-col p-4 z-40 ">
                    <button
                        className="self-end mb-4 p-2 rounded text-white hover:bg-gray-600"
                        onClick={() => setSidebarOpen(false)}
                        aria-label="Close sidebar"
                    >
                        ✕
                    </button>
                    <div className="flex items-center left-0 justify-center border-b-4 mb-4 bg-red-300">


                    </div>
                    <nav className="flex flex-col gap-2 items-start justify-start h-full w-full">
                        <Link
                            to="/valgfag"
                            onClick={() => setSidebarOpen(false)}
                            className="flex flex-row items-center px-4 mb-5 py-2 text-lg rounded-none transition-all duration-300 border-b border-white text-white hover:rounded-2xl hover:border-b-white hover:bg-gray-700 hover:text-gray-200 w-full"
                        >
                            <FaBook className="mr-4 min-w-[24px]" />
                            <span>Valgfag</span>
                        </Link>
                        {!isAuthenticated && (
                            <>
                                <Link
                                    to="/login"
                                    onClick={() => setSidebarOpen(false)}
                                    className="flex flex-row items-center px-4 mb-5 py-2 text-lg rounded-none transition-all duration-300 border-b border-white text-white hover:rounded-2xl hover:border-b-white hover:bg-gray-700 hover:text-gray-200 w-full"
                                >
                                    <FaSignInAlt className="mr-4 min-w-[24px]" />
                                    <span>Login</span>
                                </Link>
                                <Link
                                    to="/signup"
                                    onClick={() => setSidebarOpen(false)}
                                    className="flex flex-row items-center px-4 py-2 text-lg rounded-none transition-all duration-300 border-b border-white text-white hover:rounded-2xl hover:border-b-white hover:bg-gray-700 hover:text-gray-200 w-full"
                                >
                                    <FaSignInAlt className="mr-4 min-w-[24px]" />
                                    <span>Sign up</span>
                                </Link>
                            </>
                        )}
                        {isAuthenticated && (
                            <Link
                                to="/"
                                onClick={() => setSidebarOpen(false)}
                                className="flex flex-row items-center px-4 py-2 text-lg rounded bg-black text-white hover:bg-gray-600 transition w-full"
                            >
                                <span className="min-w-[24px]" />
                                <span>Logout</span>
                            </Link>
                        )}
                    </nav>
                </aside>
            )}
        </div>
    );
}