export default function Footer() {
  return (
    <footer className="mt-auto border-t border-slate-200 bg-white/90">
      <div className="container-shell flex flex-col items-start justify-between gap-3 py-6 text-sm text-slate-500 sm:flex-row sm:items-center">
        <div className="flex flex-col gap-1">
          <p className="font-medium text-slate-700">MercanLink</p>
          <p>Bygget af MAGS</p>
        </div>
        <div className="flex flex-wrap items-center gap-2 text-sm sm:justify-end">
          <a
            href="https://github.com/MAGS-GH"
            target="_blank"
            rel="noopener noreferrer"
            className="font-semibold text-indigo-600 transition hover:text-violet-600"
          >
            GitHub: MAGS-GH
          </a>
          <span className="text-slate-300">•</span>
          <a
            href="https://www.linkedin.com/in/mathiasgs"
            target="_blank"
            rel="noopener noreferrer"
            className="font-semibold text-indigo-600 transition hover:text-violet-600"
          >
            LinkedIn: Mathiasgs
          </a>
        </div>
      </div>
    </footer>
  )
}