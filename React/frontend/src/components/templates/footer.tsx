export default function Footer() {
  return (
    <footer className="relative mt-auto">
      <div className="container-shell flex items-center justify-center gap-2 py-4 text-[11px] text-slate-400">
        <span>© 2026</span>
        <span aria-hidden="true" className="text-slate-300">·</span>

        <span className="group relative inline-flex">
          <a
            href="https://mags.dk"
            target="_blank"
            rel="noopener noreferrer"
            className="font-medium tracking-wide text-slate-500 transition hover:text-indigo-600"
          >
            MAGS Solutions
          </a>

          <span
            role="tooltip"
            className="pointer-events-none absolute bottom-full left-1/2 z-20 mb-2 w-max max-w-[260px] -translate-x-1/2 translate-y-1 rounded-lg border border-slate-200 bg-white/95 px-3 py-2 text-left text-[11px] leading-snug text-slate-600 opacity-0 shadow-[0_8px_24px_-6px_rgba(15,23,42,0.18)] backdrop-blur transition-all duration-150 group-hover:translate-y-0 group-hover:opacity-100"
          >
            <span className="block font-semibold text-slate-700">MAGS Solutions</span>
            <span className="block text-slate-500">CVR 46436490</span>
            <span className="block text-slate-500">
              Fuldt ansvarlig deltager: Mathias Gaardsdal Steenberg
            </span>
            <span className="mt-1 block text-indigo-500">mags.dk</span>
          </span>
        </span>
      </div>
    </footer>
  )
}
