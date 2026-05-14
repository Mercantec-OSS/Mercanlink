import { cn } from "@/lib/utils"
import { getEventBannerImageStyle, normalizeBannerFraming } from "@/lib/bannerFraming"

export interface EventBannerFrameProps {
  url: string
  alt: string
  focalX?: number | null
  focalY?: number | null
  zoom?: number | null
  /** Ydre ramme inkl. aspect ratio, fx aspect-[16/7] w-full */
  className?: string
}

export function EventBannerFrame({ url, alt, focalX, focalY, zoom, className }: EventBannerFrameProps) {
  const { focalX: fx, focalY: fy, zoom: z } = normalizeBannerFraming(focalX, focalY, zoom)
  return (
    <div className={cn("overflow-hidden bg-slate-100", className)}>
      <img
        src={url}
        alt={alt}
        draggable={false}
        className="h-full w-full select-none"
        style={getEventBannerImageStyle(fx, fy, z)}
      />
    </div>
  )
}
