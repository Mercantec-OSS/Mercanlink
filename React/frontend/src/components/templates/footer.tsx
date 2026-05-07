export default function Footer() {
  return (
    <footer className="relative mt-auto border-t border-slate-200/70 bg-white/60 backdrop-blur supports-[backdrop-filter]:bg-white/40">
      <div className="container-shell py-5">
        <div className="flex flex-col items-start justify-between gap-1.5 text-xs text-slate-500 sm:flex-row sm:items-center">
          <p>© 2026 MAGS Solutions. Alle rettigheder forbeholdes.</p>
          <p>
            Mercanlink er udviklet og drevet af{" "}
            <a
              href="https://mags.dk"
              target="_blank"
              rel="noopener noreferrer"
              className="font-semibold text-slate-700 transition hover:text-indigo-600"
            >
              MAGS Solutions
            </a>
            .
          </p>
        </div>
        <p className="mt-1 text-[11px] text-slate-400">
          CVR 46436490 · Fuldt ansvarlig deltager: Mathias Gaardsdal Steenberg
        </p>
      </div>
    </footer>
  )
}
