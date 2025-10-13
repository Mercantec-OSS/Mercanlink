import { useTheme } from "@/components/ui/theme-provider"

export default function Footer() {
  const { theme } = useTheme()
  const isDark = theme === "dark" || (theme === "system" && window.matchMedia("(prefers-color-scheme: dark)").matches)
  const bgClass = isDark ? "bg-black text-white" : "bg-gray-600 text-black"

  return (
    <footer className={`w-full py-4 ${bgClass} flex items-center justify-center`}>
      <span>© {new Date().getFullYear()} Mercantec Space</span>
    </footer>
  )
}