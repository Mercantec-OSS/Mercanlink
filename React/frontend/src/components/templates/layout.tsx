import Footer from "./footer"
import Navbar from "./navbar"

export default function Layout_alt({ children }: { children: React.ReactNode }) {
  return (
    <div className="relative flex min-h-svh flex-col overflow-hidden bg-slate-50 text-slate-900">
      <div className="orb left-[-120px] top-[-100px] h-[340px] w-[340px] bg-indigo-300/50" />
      <div className="orb bottom-[-180px] right-[-140px] h-[420px] w-[420px] bg-violet-300/45" />
      <Navbar />
      <main className="relative z-10 flex flex-1 flex-col">{children}</main>
      <Footer />
    </div>
  )
}