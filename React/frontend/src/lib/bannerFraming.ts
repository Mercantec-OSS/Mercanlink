import type { CSSProperties } from "react"

export const DEFAULT_BANNER_FOCAL_X = 50
export const DEFAULT_BANNER_FOCAL_Y = 50
export const DEFAULT_BANNER_ZOOM = 1
export const BANNER_ZOOM_MIN = 1
export const BANNER_ZOOM_MAX = 2.5

export function clampBannerFocal(value: number): number {
  return Math.min(100, Math.max(0, value))
}

export function clampBannerZoom(value: number): number {
  return Math.min(BANNER_ZOOM_MAX, Math.max(BANNER_ZOOM_MIN, value))
}

export function normalizeBannerFraming(
  focalX?: number | null,
  focalY?: number | null,
  zoom?: number | null,
): { focalX: number; focalY: number; zoom: number } {
  return {
    focalX: clampBannerFocal(focalX ?? DEFAULT_BANNER_FOCAL_X),
    focalY: clampBannerFocal(focalY ?? DEFAULT_BANNER_FOCAL_Y),
    zoom: clampBannerZoom(zoom ?? DEFAULT_BANNER_ZOOM),
  }
}

export function getEventBannerImageStyle(focalX: number, focalY: number, zoom: number): CSSProperties {
  const { focalX: fx, focalY: fy, zoom: z } = normalizeBannerFraming(focalX, focalY, zoom)
  return {
    width: "100%",
    height: "100%",
    objectFit: "cover",
    objectPosition: `${fx}% ${fy}%`,
    transform: `scale(${z})`,
    transformOrigin: `${fx}% ${fy}%`,
  }
}
