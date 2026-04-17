import Layout_alt from "@/components/templates/layout"
import { useMemo, useState, useRef } from "react"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Card } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { apiClient } from "@/services/apiClient"
import { useAuth } from "@/contexts/AuthContext"

function normalizeUrl(input: string): string {
    const raw = input.trim()
    if (!raw) return ""
    if (/^https?:\/\//i.test(raw)) return raw
    return `https://${raw}`
}

function tryParseUrl(input: string): URL | null {
    try {
        const url = new URL(normalizeUrl(input))
        if (url.protocol !== "http:" && url.protocol !== "https:") return null
        return url
    } catch {
        return null
    }
}

function classifyLink(url: URL): { label: string; tone: string }[] {
    const host = url.hostname.toLowerCase()
    const path = url.pathname.toLowerCase()

    const tags: { label: string; tone: string }[] = []

    if (host.includes("notion.site") || host.includes("notion.so")) tags.push({ label: "Notion", tone: "bg-slate-100 text-slate-700 border-slate-200" })
    if (host.includes("youtube.com") || host === "youtu.be") tags.push({ label: "YouTube", tone: "bg-red-50 text-red-700 border-red-200" })
    if (host.includes("github.com")) tags.push({ label: "GitHub", tone: "bg-zinc-100 text-zinc-800 border-zinc-200" })
    if (host.includes("docs.google.com")) tags.push({ label: "Google Docs", tone: "bg-blue-50 text-blue-700 border-blue-200" })
    if (host.includes("teams.microsoft.com")) tags.push({ label: "Teams", tone: "bg-indigo-50 text-indigo-700 border-indigo-200" })
    if (host.includes("discord.com") || host.includes("discord.gg")) tags.push({ label: "Discord", tone: "bg-violet-50 text-violet-700 border-violet-200" })

    if (path.endsWith(".pdf")) tags.push({ label: "PDF", tone: "bg-amber-50 text-amber-800 border-amber-200" })

    return tags
}

export default function FormPage() {
    const [isSubmitting, setIsSubmitting] = useState(false)
    const [message, setMessage] = useState("")
    const [linkDraft, setLinkDraft] = useState("")
    const formRef = useRef<HTMLFormElement>(null)
    const { user } = useAuth()

    const parsedLink = useMemo(() => tryParseUrl(linkDraft), [linkDraft])
    const linkTags = useMemo(() => (parsedLink ? classifyLink(parsedLink) : []), [parsedLink])
    const faviconUrl = useMemo(() => {
        if (!parsedLink) return ""
        return `${parsedLink.origin}/favicon.ico`
    }, [parsedLink])

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault()
        setIsSubmitting(true)
        setMessage("")

        const formData = new FormData(e.currentTarget)

        const data = {
            type: formData.get('materialType') as string,
            title: formData.get('title') as string,
            description: formData.get('description') as string,
            linkToPost: normalizeUrl((formData.get('link') as string) || ""),
        }

        try {
            await apiClient('/KnowledgeCenter', {
                method: 'POST',
                body: JSON.stringify(data),
            })

            setMessage("Materiale blev indsendt til godkendelse.")
            formRef.current?.reset()
            setLinkDraft("")
        } catch (error) {
            const errorMessage = error instanceof Error
                ? error.message
                : "Ukendt fejl under indsendelse."
            setMessage(`Kunne ikke indsende: ${errorMessage}`)
        } finally {
            setIsSubmitting(false)
        }
    }
    return (
        <Layout_alt>
            <section className="container-shell py-12 sm:py-16 lg:py-20">
                <div className="mb-8 max-w-2xl">
                    <h1 className="text-4xl font-extrabold tracking-[-0.02em] text-slate-900 sm:text-5xl">
                        Indsend nyt <span className="brand-gradient-text">materiale</span>
                    </h1>
                    <p className="mt-4 text-base leading-7 text-slate-600">
                        Del links og læringsressourcer med holdet. Din indsendelse går først til moderation før publicering.
                    </p>
                    <p className="mt-2 text-sm text-slate-500">
                        Indsendt som: <span className="font-medium text-slate-700">{user?.username ?? "Ukendt bruger"}</span>
                    </p>
                </div>

                <Card className="soft-card border-slate-100 p-6 sm:p-8">
                    <form ref={formRef} className="grid gap-5 md:grid-cols-2" onSubmit={handleSubmit}>
                        {message && (
                            <div className={`md:col-span-2 rounded-lg border px-4 py-3 text-sm ${message.startsWith("Materiale blev")
                                ? "border-emerald-200 bg-emerald-50 text-emerald-700"
                                : "border-red-200 bg-red-50 text-red-600"
                                }`}>
                                {message}
                            </div>
                        )}

                        <div className="space-y-2 md:col-span-2">
                            <Label htmlFor="materialType" className="text-sm font-semibold text-slate-700">Materialetype</Label>
                            <select
                                name="materialType"
                                id="materialType"
                                className="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm text-slate-700 outline-none transition focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/30"
                                required
                            >
                                <option value="">Vælg materialetype</option>
                                <option value="blog-post">Blog post</option>
                                <option value="video">Video</option>
                                <option value="artikel">Artikel</option>
                                <option value="andet">Andet</option>
                            </select>
                        </div>

                        <div className="space-y-2 md:col-span-2">
                            <Label htmlFor="title" className="text-sm font-semibold text-slate-700">Titel</Label>
                            <Input id="title" name="title" required className="h-11" placeholder="Skriv en titel" />
                        </div>

                        <div className="space-y-2 md:col-span-2">
                            <Label htmlFor="description" className="text-sm font-semibold text-slate-700">Beskrivelse</Label>
                            <textarea
                                id="description"
                                name="description"
                                required
                                rows={5}
                                placeholder="Beskriv materialet kort"
                                className="w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-700 outline-none transition focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/30"
                            />
                        </div>

                        <div className="space-y-2 md:col-span-2">
                            <Label htmlFor="link" className="text-sm font-semibold text-slate-700">
                                Link (valgfri)
                            </Label>
                            <div className="grid gap-3 rounded-xl border border-slate-200 bg-slate-50/60 p-4">
                                <Input
                                    id="link"
                                    name="link"
                                    value={linkDraft}
                                    onChange={(event) => setLinkDraft(event.target.value)}
                                    className="h-12 bg-white text-base"
                                    placeholder="Indsæt et link (fx Notion, YouTube, GitHub, Google Docs...)"
                                    inputMode="url"
                                    autoComplete="url"
                                />

                                {linkDraft.trim() && !parsedLink && (
                                    <p className="text-sm text-amber-700">
                                        Linket ser ikke ud til at være en gyldig URL endnu. Prøv fx at starte med <span className="font-mono">https://</span>
                                    </p>
                                )}

                                {parsedLink && (
                                    <div className="grid gap-3 sm:grid-cols-[1fr_auto] sm:items-start">
                                        <div className="rounded-xl border border-slate-200 bg-white p-4">
                                            <div className="flex items-start gap-3">
                                                <div className="mt-0.5 flex h-10 w-10 items-center justify-center overflow-hidden rounded-lg border border-slate-200 bg-slate-50">
                                                    {faviconUrl ? (
                                                        <img
                                                            src={faviconUrl}
                                                            alt=""
                                                            className="h-6 w-6"
                                                            onError={(event) => {
                                                                ;(event.currentTarget as HTMLImageElement).style.display = "none"
                                                            }}
                                                        />
                                                    ) : null}
                                                </div>
                                                <div className="min-w-0 flex-1">
                                                    <p className="truncate text-sm font-semibold text-slate-900">
                                                        {parsedLink.hostname}
                                                    </p>
                                                    <p className="mt-1 break-all text-xs text-slate-500">
                                                        {parsedLink.href}
                                                    </p>
                                                    {!!linkTags.length && (
                                                        <div className="mt-3 flex flex-wrap gap-2">
                                                            {linkTags.map((tag) => (
                                                                <span
                                                                    key={tag.label}
                                                                    className={`inline-flex items-center rounded-full border px-2.5 py-1 text-xs font-semibold ${tag.tone}`}
                                                                >
                                                                    {tag.label}
                                                                </span>
                                                            ))}
                                                        </div>
                                                    )}
                                                </div>
                                            </div>
                                        </div>

                                        <div className="flex flex-wrap gap-2 sm:flex-col sm:items-stretch">
                                            <Button
                                                type="button"
                                                variant="outline"
                                                className="h-11"
                                                onClick={() => window.open(parsedLink.href, "_blank", "noopener,noreferrer")}
                                            >
                                                Åbn preview
                                            </Button>
                                            <Button
                                                type="button"
                                                variant="outline"
                                                className="h-11"
                                                onClick={async () => {
                                                    try {
                                                        await navigator.clipboard.writeText(parsedLink.href)
                                                        setMessage("Link kopieret til udklipsholder.")
                                                    } catch {
                                                        setMessage("Kunne ikke kopiere link automatisk.")
                                                    }
                                                }}
                                            >
                                                Kopiér link
                                            </Button>
                                        </div>
                                    </div>
                                )}

                                <p className="text-xs text-slate-500">
                                    Tip: Vi normaliserer automatisk linket (tilføjer <span className="font-mono">https://</span> hvis du glemmer det).
                                </p>
                            </div>
                        </div>

                        <div className="md:col-span-2">
                            <Button type="submit" disabled={isSubmitting} className="h-11 w-full sm:w-auto">
                                {isSubmitting ? "Indsender..." : "Indsend materiale"}
                            </Button>
                        </div>
                    </form>
                </Card>
            </section>
        </Layout_alt>
    )
}