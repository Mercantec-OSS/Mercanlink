import Footer from "./footer"
import Navbar from "./navbar"
import { useTheme } from "@/components/ui/theme-provider"

interface LayoutProps {
  children: React.ReactNode
}

export default function Layout({ children }: LayoutProps) {
  const { theme } = useTheme()
  const isDark = theme === "dark" || (theme === "system" && window.matchMedia("(prefers-color-scheme: dark)").matches)

  return (
    <div className={`flex min-h-svh flex-col ${isDark ? "bg-white text-black" : "bg-black text-white"}`}>
      <Navbar />
      <main className="flex flex-1 flex-col items-center justify-center gap-4">
        {children}
      </main>
      <Footer />
    </div>
  )
}