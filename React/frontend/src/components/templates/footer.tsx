export default function Footer() {
  return (
    <footer className="mt-auto border-t border-slate-200 bg-white/90">
      <div className="container-shell flex flex-col items-start justify-between gap-3 py-6 text-sm text-slate-500 sm:flex-row sm:items-center">
        <div className="flex flex-col gap-1">
          <p className="font-medium text-slate-700">MercanLink</p>
          <div className="flex items-center gap-3">
            <p>Bygget af MAGS</p>
            <a
              href="https://github.com/MAGS-GH"
              target="_blank"
              rel="noopener noreferrer"
              aria-label="GitHub profil for MAGS"
              className="text-slate-500 transition hover:text-indigo-600"
            >
              <svg viewBox="0 0 24 24" aria-hidden="true" className="h-5 w-5 fill-current">
                <path d="M12 .5C5.65.5.5 5.65.5 12c0 5.08 3.29 9.39 7.86 10.9.58.1.79-.25.79-.56 0-.27-.01-1.16-.02-2.1-3.2.69-3.88-1.36-3.88-1.36-.52-1.33-1.28-1.68-1.28-1.68-1.04-.71.08-.69.08-.69 1.15.08 1.75 1.18 1.75 1.18 1.02 1.75 2.67 1.25 3.32.96.1-.74.4-1.25.72-1.54-2.55-.29-5.23-1.27-5.23-5.67 0-1.25.45-2.28 1.18-3.08-.12-.29-.51-1.46.11-3.05 0 0 .97-.31 3.17 1.18a10.9 10.9 0 0 1 5.77 0c2.2-1.49 3.17-1.18 3.17-1.18.62 1.59.23 2.76.11 3.05.73.8 1.18 1.83 1.18 3.08 0 4.41-2.68 5.37-5.24 5.66.41.35.77 1.04.77 2.1 0 1.52-.01 2.74-.01 3.11 0 .31.21.67.8.56A11.52 11.52 0 0 0 23.5 12C23.5 5.65 18.35.5 12 .5z" />
              </svg>
            </a>
            <a
              href="https://www.linkedin.com/in/mathiasgs"
              target="_blank"
              rel="noopener noreferrer"
              aria-label="LinkedIn profil for Mathiasgs"
              className="text-slate-500 transition hover:text-indigo-600"
            >
              <svg viewBox="0 0 24 24" aria-hidden="true" className="h-5 w-5 fill-current">
                <path d="M4.98 3.5C4.98 4.88 3.87 6 2.49 6S0 4.88 0 3.5 1.12 1 2.49 1s2.49 1.12 2.49 2.5zM.5 8h4V23h-4V8zm7.5 0h3.83v2.05h.05c.53-1 1.84-2.05 3.78-2.05 4.05 0 4.8 2.66 4.8 6.12V23h-4v-7.73c0-1.84-.03-4.2-2.56-4.2-2.56 0-2.95 2-2.95 4.06V23H8V8z" />
              </svg>
            </a>
          </div>
        </div>
        <p className="text-sm">© 2026 Mercantec Space. Alle rettigheder forbeholdes.</p>
      </div>
    </footer>
  )
}