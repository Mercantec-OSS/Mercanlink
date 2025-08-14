import Footer from "./footer"
import Navbar from "./navbar"

export default function Layout_alt({ children }: { children: React.ReactNode }) {
 return (
    <div className="flex min-h-svh flex-col bg-[#101828] text-white">
      <Navbar />
      <main className="flex flex-1 flex-col items-center justify-center gap-4">
        {children}
      </main>
      <Footer />
    </div>
  )
}