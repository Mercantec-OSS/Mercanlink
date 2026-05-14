import { useCallback, useRef } from "react"
import { Button } from "@/components/ui/button"
import { cn } from "@/lib/utils"
import {
  BANNER_ZOOM_MAX,
  BANNER_ZOOM_MIN,
  clampBannerFocal,
  clampBannerZoom,
  DEFAULT_BANNER_FOCAL_X,
  DEFAULT_BANNER_FOCAL_Y,
  DEFAULT_BANNER_ZOOM,
  getEventBannerImageStyle,
} from "@/lib/bannerFraming"

export interface BannerFramingValue {
  focalX: number
  focalY: number
  zoom: number
}

interface BannerFocalEditorProps {
  imageUrl: string
  value: BannerFramingValue
  onChange: (next: BannerFramingValue) => void
  /** CSS aspect for preview (matcher typisk eventsiden) */
  aspectClassName?: string
  className?: string
}

export function BannerFocalEditor({
  imageUrl,
  value,
  onChange,
  aspectClassName = "aspect-[16/7]",
  className,
}: BannerFocalEditorProps) {
  const frameRef = useRef<HTMLDivElement>(null)
  const dragRef = useRef<{ startX: number; startY: number; focalX: number; focalY: number } | null>(null)

  const { focalX, focalY, zoom } = value

  const applyPan = useCallback(
    (clientX: number, clientY: number) => {
      const start = dragRef.current
      const el = frameRef.current
      if (!start || !el) return
      const rect = el.getBoundingClientRect()
      if (rect.width < 1 || rect.height < 1) return
      const dx = clientX - start.startX
      const dy = clientY - start.startY
      const panStrength = Math.max(0.35, zoom * 0.45)
      const nextX = clampBannerFocal(start.focalX - (dx / rect.width) * 100 * panStrength)
      const nextY = clampBannerFocal(start.focalY - (dy / rect.height) * 100 * panStrength)
      onChange({ focalX: nextX, focalY: nextY, zoom })
    },
    [onChange, zoom],
  )

  const onPointerDown = (e: React.PointerEvent<HTMLDivElement>) => {
    e.preventDefault()
    e.currentTarget.setPointerCapture(e.pointerId)
    dragRef.current = { startX: e.clientX, startY: e.clientY, focalX, focalY }
  }

  const onPointerMove = (e: React.PointerEvent<HTMLDivElement>) => {
    if (!dragRef.current) return
    applyPan(e.clientX, e.clientY)
  }

  const onPointerUp = (e: React.PointerEvent<HTMLDivElement>) => {
    if (dragRef.current) {
      try {
        e.currentTarget.releasePointerCapture(e.pointerId)
      } catch {
        /* ignore */
      }
      dragRef.current = null
    }
  }

  const reset = () => {
    dragRef.current = null
    onChange({
      focalX: DEFAULT_BANNER_FOCAL_X,
      focalY: DEFAULT_BANNER_FOCAL_Y,
      zoom: DEFAULT_BANNER_ZOOM,
    })
  }

  return (
    <div className={cn("flex flex-col gap-2", className)}>
      <div
        ref={frameRef}
        className={cn(
          "relative w-full max-w-md overflow-hidden rounded-lg border border-slate-200 bg-slate-100 shadow-sm",
          aspectClassName,
        )}
      >
        <img
          src={imageUrl}
          alt=""
          draggable={false}
          className="pointer-events-none h-full w-full select-none"
          style={getEventBannerImageStyle(focalX, focalY, zoom)}
        />
        <div
          role="application"
          aria-label="Træk for at flytte udsnit af banneret"
          className="absolute inset-0 cursor-grab touch-none active:cursor-grabbing"
          onPointerDown={onPointerDown}
          onPointerMove={onPointerMove}
          onPointerUp={onPointerUp}
          onPointerCancel={onPointerUp}
        />
      </div>
      <div className="flex max-w-md flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
        <label className="flex min-w-0 flex-1 items-center gap-2 text-xs text-slate-600">
          <span className="shrink-0 font-medium text-slate-700">Zoom</span>
          <input
            type="range"
            min={BANNER_ZOOM_MIN}
            max={BANNER_ZOOM_MAX}
            step={0.05}
            value={zoom}
            onChange={(e) =>
              onChange({
                focalX,
                focalY,
                zoom: clampBannerZoom(Number(e.target.value)),
              })
            }
            className="h-2 w-full min-w-0 accent-indigo-600"
          />
          <span className="w-10 shrink-0 tabular-nums text-slate-500">{zoom.toFixed(2)}×</span>
        </label>
        <Button type="button" variant="outline" size="sm" className="shrink-0 self-start sm:self-auto" onClick={reset}>
          Nulstil ramme
        </Button>
      </div>
      <p className="max-w-md text-xs text-slate-500">
        Træk i forhåndsvisningen for at vælge hvilken del af billedet der vises. Zoom beskærer tættere på
        fokuspunktet (samme udsnit som på eventsiden).
      </p>
    </div>
  )
}
