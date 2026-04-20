import { Fragment, type ReactNode, useMemo } from "react"

/**
 * Discord-lignende gengivelse (dark theme) med udvidet markdown:
 * overskrifter, subtext (-#), lister, blockquotes, code fences,
 * samt inline: fed/kursiv/understreget, gennemstreget, spoiler, links, kode, pings.
 */

let inlineKey = 0
function nextKey(prefix: string) {
  return `${prefix}-${inlineKey++}`
}

/** Første match vinder i denne rækkefølge (Discord-lignende prioritering). */
function parseInlineAll(s: string): ReactNode[] {
  if (!s) return []
  const out: ReactNode[] = []
  let pos = 0

  const patterns: Array<{
    re: RegExp
    render: (m: RegExpExecArray) => ReactNode
  }> = [
    { re: /^`([^`]+)`/, render: (m) => (
      <code
        key={nextKey("c")}
        className="rounded bg-[#1e1f22] px-1 py-0.5 font-mono text-[13px] text-[#e8b86d]"
      >
        {m[1]}
      </code>
    ) },
    {
      re: /^\|\|([\s\S]+?)\|\|/,
      render: (m) => (
        <span
          key={nextKey("sp")}
          className="cursor-pointer rounded bg-[#2b2d31] px-0.5 text-[#dbdee1] [filter:blur(4px)] hover:[filter:blur(0)] transition-[filter]"
          title="Spoiler — hold musen over for at vise"
        >
          {parseInlineAll(m[1])}
        </span>
      ),
    },
    {
      re: /^\[([^\]]*)\]\((https?:\/\/[^)\s]+)\)/,
      render: (m) => (
        <a
          key={nextKey("a")}
          href={m[2]}
          target="_blank"
          rel="noopener noreferrer"
          className="text-[#00a8fc] hover:underline"
        >
          {m[1] || m[2]}
        </a>
      ),
    },
    {
      re: /^<@(\d+)>/,
      render: (m) => (
        <span
          key={nextKey("m")}
          className="inline rounded-[3px] bg-[#5865f24d] px-1 py-[1px] font-medium text-[#c9cdfb]"
        >
          @{m[1]}
        </span>
      ),
    },
    {
      re: /^https?:\/\/[^\s<]+/,
      render: (m) => (
        <a
          key={nextKey("al")}
          href={m[0]}
          target="_blank"
          rel="noopener noreferrer"
          className="break-all text-[#00a8fc] hover:underline"
        >
          {m[0]}
        </a>
      ),
    },
    {
      re: /^~~(.+?)~~/,
      render: (m) => (
        <s key={nextKey("s")} className="text-[#949ba4]">
          {parseInlineAll(m[1])}
        </s>
      ),
    },
    {
      re: /^\*\*\*([^*]+)\*\*\*/,
      render: (m) => (
        <strong key={nextKey("bi")} className="font-bold italic text-[#f2f3f5]">
          {parseInlineAll(m[1])}
        </strong>
      ),
    },
    {
      re: /^\*\*([^*]+)\*\*/,
      render: (m) => (
        <strong key={nextKey("b")} className="font-semibold text-[#f2f3f5]">
          {parseInlineAll(m[1])}
        </strong>
      ),
    },
    {
      re: /^__([\s\S]+?)__/,
      render: (m) => (
        <u key={nextKey("u")} className="decoration-[#dbdee1] underline underline-offset-2">
          {parseInlineAll(m[1])}
        </u>
      ),
    },
    {
      re: /^\*(?!\*)([^*]+)\*(?!\*)/,
      render: (m) => (
        <em key={nextKey("is")} className="italic text-[#dbdee1]">
          {parseInlineAll(m[1])}
        </em>
      ),
    },
    {
      re: /^_(?!_)([^_]+)_(?!_)/,
      render: (m) => (
        <em key={nextKey("iu")} className="italic text-[#dbdee1]">
          {parseInlineAll(m[1])}
        </em>
      ),
    },
  ]

  while (pos < s.length) {
    const rest = s.slice(pos)
    let matched = false
    for (const { re, render } of patterns) {
      const m = re.exec(rest)
      if (m && m.index === 0) {
        out.push(render(m))
        pos += m[0].length
        matched = true
        break
      }
    }
    if (!matched) {
      out.push(rest[0]!)
      pos += 1
    }
  }
  return out
}

type FencePart = { type: "md" | "code"; text: string }

function splitFencedCodeBlocks(text: string): FencePart[] {
  const normalized = text.replace(/\r\n/g, "\n")
  const parts: FencePart[] = []
  let i = 0
  while (i < normalized.length) {
    const start = normalized.indexOf("```", i)
    if (start === -1) {
      if (i < normalized.length) parts.push({ type: "md", text: normalized.slice(i) })
      break
    }
    if (start > i) parts.push({ type: "md", text: normalized.slice(i, start) })
    const afterTicks = start + 3
    const nl = normalized.indexOf("\n", afterTicks)
    let codeStart: number
    if (nl === -1) {
      parts.push({ type: "md", text: normalized.slice(start) })
      break
    }
    codeStart = nl + 1
    const close = normalized.indexOf("```", codeStart)
    if (close === -1) {
      parts.push({ type: "md", text: normalized.slice(start) })
      break
    }
    const code = normalized.slice(codeStart, close)
    parts.push({ type: "code", text: code })
    i = close + 3
    if (normalized[i] === "\n") i++
  }
  return parts
}

function renderMarkdownSegment(segment: string, segmentId: number): ReactNode[] {
  const lines = segment.split("\n")
  const blocks: ReactNode[] = []
  let i = 0
  let blockKey = 0
  const k = (suffix: string) => `s${segmentId}-${suffix}-${blockKey++}`

  const isStructuralLine = (trimmed: string) =>
    /^#{1,3}\s/.test(trimmed) ||
    /^-#\s/.test(trimmed) ||
    /^>{1,3}\s?/.test(trimmed) ||
    /^(\s{0,2})([-*]|\d+\.)\s+/.test(trimmed)

  while (i < lines.length) {
    const rawLine = lines[i]
    const trimmedEnd = rawLine.trimEnd()
    const trimmed = rawLine.trim()

    if (trimmedEnd === "") {
      blocks.push(<div key={k("sp")} className="h-2 shrink-0" />)
      i++
      continue
    }

    // Multilinje block quote: >>> (Discord)
    if (trimmed.startsWith(">>>")) {
      const firstContent = trimmed.slice(3).trimStart()
      const quoteLines: string[] = firstContent ? [firstContent] : []
      i++
      while (i < lines.length && lines[i].trim() !== "") {
        quoteLines.push(lines[i])
        i++
      }
      blocks.push(
        <blockquote
          key={k("bq3")}
          className="border-l-4 border-[#4e5058] pl-3 text-[15px] leading-[1.375] text-[#dbdee1]"
        >
          {quoteLines.map((ql, idx) => (
            <Fragment key={idx}>
              {idx > 0 ? <br /> : null}
              {parseInlineAll(ql)}
            </Fragment>
          ))}
        </blockquote>,
      )
      continue
    }

    // Enkelt linje block quote: >
    if (/^>\s/.test(trimmedEnd) && !trimmed.startsWith(">>")) {
      const qLines: string[] = []
      while (i < lines.length) {
        const L = lines[i]
        if (L.trim() === "") break
        const te = L.trimEnd()
        if (!/^>\s?/.test(te)) break
        qLines.push(te.replace(/^>\s?/, ""))
        i++
      }
      blocks.push(
        <blockquote
          key={k("bq1")}
          className="my-1 border-l-4 border-[#4e5058] pl-3 text-[15px] text-[#dbdee1]"
        >
          {qLines.map((ql, idx) => (
            <Fragment key={idx}>
              {idx > 0 ? <br /> : null}
              {parseInlineAll(ql)}
            </Fragment>
          ))}
        </blockquote>,
      )
      continue
    }

    // Subtext: -# (Discord)
    const sub = trimmedEnd.match(/^-#\s+(.*)$/)
    if (sub) {
      blocks.push(
        <div
          key={k("sub")}
          className="text-xs font-medium leading-5 text-[#949ba4] mt-1 mb-0.5"
        >
          {parseInlineAll(sub[1])}
        </div>,
      )
      i++
      continue
    }

    // Overskrifter # ## ###
    const hm = trimmedEnd.match(/^(#{1,3})\s(.*)$/)
    if (hm) {
      const level = hm[1].length
      const rest = hm[2]
      const size =
        level === 1 ? "text-[22px] leading-7" : level === 2 ? "text-[19px] leading-6" : "text-[17px] leading-6"
      blocks.push(
        <div
          key={k("h")}
          role="heading"
          aria-level={level + 2}
          className={`${size} font-bold tracking-tight text-[#f2f3f5] mt-3 mb-1 first:mt-0`}
        >
          {parseInlineAll(rest)}
        </div>,
      )
      i++
      continue
    }

    // Lister (på hinanden følgende linjer med - * eller 1.)
    if (/^(\s{0,6})([-*]|\d+\.)\s+/.test(trimmedEnd)) {
      const isOrdered = /^\s{0,6}\d+\.\s+/.test(trimmedEnd)
      const items: string[] = []
      while (i < lines.length) {
        const L = lines[i]
        if (L.trim() === "") break
        const m = L.match(/^(\s{0,6})([-*]|\d+\.)\s+(.*)$/)
        if (!m) break
        const ordered = /^\d+\.$/.test(m[2]!)
        if (ordered !== isOrdered) break
        items.push(m[3]!)
        i++
      }
      const listClass = `my-1 pl-6 text-[15px] leading-[1.5] text-[#dbdee1] ${isOrdered ? "list-decimal" : "list-disc"}`
      blocks.push(
        isOrdered ? (
          <ol key={k("ol")} className={listClass}>
            {items.map((item, idx) => (
              <li key={idx} className="[&>p]:inline">
                {parseInlineAll(item)}
              </li>
            ))}
          </ol>
        ) : (
          <ul key={k("ul")} className={listClass}>
            {items.map((item, idx) => (
              <li key={idx} className="[&>p]:inline">
                {parseInlineAll(item)}
              </li>
            ))}
          </ul>
        ),
      )
      continue
    }

    const paraLines: string[] = []
    while (i < lines.length) {
      const L = lines[i]
      if (L.trim() === "") break
      const t = L.trim()
      if (isStructuralLine(t)) break
      paraLines.push(L)
      i++
    }
    blocks.push(
      <p
        key={k("p")}
        className="text-[15px] leading-[1.375] text-[#dbdee1] whitespace-pre-wrap break-words"
      >
        {paraLines.map((pl, idx) => (
          <Fragment key={idx}>
            {idx > 0 ? "\n" : null}
            {parseInlineAll(pl)}
          </Fragment>
        ))}
      </p>,
    )
  }

  return blocks
}

function renderDiscordDocument(content: string): ReactNode[] {
  inlineKey = 0
  const pieces = splitFencedCodeBlocks(content)
  const out: ReactNode[] = []
  let pieceIdx = 0
  for (const piece of pieces) {
    if (piece.type === "code") {
      out.push(
        <pre
          key={`pre-${pieceIdx}`}
          className="my-2 overflow-x-auto rounded-md border border-[#1e1f22] bg-[#1e1f22] p-3 font-mono text-[13px] leading-relaxed text-[#d6d8db]"
        >
          {piece.text.replace(/\n$/, "")}
        </pre>,
      )
    } else {
      out.push(...renderMarkdownSegment(piece.text, pieceIdx))
    }
    pieceIdx++
  }
  return out
}

type DiscordDarkMessagePreviewProps = {
  content: string
  className?: string
}

export function DiscordDarkMessagePreview({ content, className = "" }: DiscordDarkMessagePreviewProps) {
  const body = useMemo(() => {
    inlineKey = 0
    return renderDiscordDocument(content)
  }, [content])

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
          Understøtter bl.a. <code className="rounded bg-[#1e1f22] px-1 py-0.5 text-[#dbdee1]">**fed**</code>,{" "}
          <code className="rounded bg-[#1e1f22] px-1 py-0.5 text-[#dbdee1]">*kursiv*</code>,{" "}
          <code className="rounded bg-[#1e1f22] px-1 py-0.5 text-[#dbdee1]">__underline__</code>,{" "}
          <code className="rounded bg-[#1e1f22] px-1 py-0.5 text-[#dbdee1]">~~gennemstreget~~</code>,{" "}
          <code className="rounded bg-[#1e1f22] px-1 py-0.5 text-[#dbdee1]">||spoiler||</code>,{" "}
          <code className="rounded bg-[#1e1f22] px-1 py-0.5 text-[#dbdee1]">`kode`</code>,{" "}
          <code className="rounded bg-[#1e1f22] px-1 py-0.5 text-[#dbdee1]">```blok```</code>,{" "}
          <code className="rounded bg-[#1e1f22] px-1 py-0.5 text-[#dbdee1]">#</code> /{" "}
          <code className="rounded bg-[#1e1f22] px-1 py-0.5 text-[#dbdee1]">-#</code>,{" "}
          <code className="rounded bg-[#1e1f22] px-1 py-0.5 text-[#dbdee1]">&gt;</code> /{" "}
          <code className="rounded bg-[#1e1f22] px-1 py-0.5 text-[#dbdee1]">&gt;&gt;&gt;</code>, lister,{" "}
          <code className="rounded bg-[#1e1f22] px-1 py-0.5 text-[#dbdee1]">[tekst](url)</code> og{" "}
          <code className="rounded bg-[#1e1f22] px-1 py-0.5 text-[#dbdee1]">&lt;@id&gt;</code>.
        </p>
      </div>
    </div>
  )
}
