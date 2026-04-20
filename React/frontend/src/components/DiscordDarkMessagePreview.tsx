import { Fragment, type ReactNode, useMemo } from "react"

/**
 * Discord-lignende gengivelse af et udklip af subset: # ## ###, **fed**, <@id>, auto-link.
 * Farver inspireret af Discord dark theme.
 */

function parseDiscordInline(text: string): ReactNode[] {
  if (!text) return []
  const re = /\*\*([^*]+)\*\*|<@(\d+)>|(https?:\/\/[^\s<]+)/g
  const out: ReactNode[] = []
  let last = 0
  let m: RegExpExecArray | null
  let k = 0
  while ((m = re.exec(text)) !== null) {
    if (m.index > last) {
      out.push(text.slice(last, m.index))
    }
    if (m[1] !== undefined) {
      out.push(
        <strong key={`b-${k++}`} className="font-semibold text-[#f2f3f5]">
          {m[1]}
        </strong>,
      )
    } else if (m[2] !== undefined) {
      out.push(
        <span
          key={`m-${k++}`}
          className="inline rounded-[3px] bg-[#5865f24d] px-1 py-[1px] font-medium text-[#c9cdfb]"
        >
          @{m[2]}
        </span>,
      )
    } else if (m[3]) {
      const href = m[3]
      out.push(
        <a
          key={`a-${k++}`}
          href={href}
          target="_blank"
          rel="noopener noreferrer"
          className="text-[#00a8fc] hover:underline"
        >
          {href}
        </a>,
      )
    }
    last = re.lastIndex
  }
  if (last < text.length) {
    out.push(text.slice(last))
  }
  return out
}

function renderDiscordLines(content: string): ReactNode[] {
  const lines = content.replace(/\r\n/g, "\n").split("\n")
  const blocks: ReactNode[] = []
  let i = 0
  let blockKey = 0

  while (i < lines.length) {
    const line = lines[i]
    const trimmedEnd = line.trimEnd()

    if (trimmedEnd === "") {
      blocks.push(<div key={`sp-${blockKey++}`} className="h-2 shrink-0" />)
      i++
      continue
    }

    const hm = trimmedEnd.match(/^(#{1,3})\s(.*)$/)
    if (hm) {
      const level = hm[1].length
      const rest = hm[2]
      const size =
        level === 1 ? "text-[22px] leading-7" : level === 2 ? "text-[19px] leading-6" : "text-[17px] leading-6"
      blocks.push(
        <div
          key={`h-${blockKey++}`}
          role="heading"
          aria-level={level + 2}
          className={`${size} font-bold tracking-tight text-[#f2f3f5] mt-3 mb-1 first:mt-0`}
        >
          {parseDiscordInline(rest)}
        </div>,
      )
      i++
      continue
    }

    const paraLines: string[] = []
    while (i < lines.length) {
      const L = lines[i]
      if (L.trim() === "") break
      if (/^#{1,3}\s/.test(L.trim())) break
      paraLines.push(L)
      i++
    }
    blocks.push(
      <p
        key={`p-${blockKey++}`}
        className="text-[15px] leading-[1.375] text-[#dbdee1] whitespace-pre-wrap break-words"
      >
        {paraLines.map((pl, idx) => (
          <Fragment key={idx}>
            {idx > 0 ? "\n" : null}
            {parseDiscordInline(pl)}
          </Fragment>
        ))}
      </p>,
    )
  }

  return blocks
}

type DiscordDarkMessagePreviewProps = {
  content: string
  className?: string
}

export function DiscordDarkMessagePreview({ content, className = "" }: DiscordDarkMessagePreviewProps) {
  const body = useMemo(() => renderDiscordLines(content), [content])

  return (
    <div
      className={`rounded-lg border border-[#1e1f22] bg-[#313338] shadow-inner ${className}`}
      role="region"
      aria-label="Discord forhåndsvisning dark mode"
    >
      <div className="border-b border-[#1e1f22] px-4 py-2">
        <p className="text-[11px] font-semibold uppercase tracking-wide text-[#949ba4]">
          Forhåndsvisning · som i Discord (dark)
        </p>
      </div>
      <div className="p-3 sm:p-4">
        <div className="rounded-md bg-[#2b2d31] px-3 py-2.5 sm:px-4 sm:py-3">
          <div className="font-sans">{body}</div>
        </div>
        <p className="mt-3 text-[11px] leading-relaxed text-[#949ba4]">
          Brug <code className="rounded bg-[#1e1f22] px-1 py-0.5 text-[#dbdee1]">#</code>,{" "}
          <code className="rounded bg-[#1e1f22] px-1 py-0.5 text-[#dbdee1]">##</code> og{" "}
          <code className="rounded bg-[#1e1f22] px-1 py-0.5 text-[#dbdee1]">**tekst**</code> i beskrivelsen for
          at se samme logik som i Discord. Ping vises som{" "}
          <span className="rounded bg-[#5865f24d] px-1 text-[#c9cdfb]">@id</span> her.
        </p>
      </div>
    </div>
  )
}
