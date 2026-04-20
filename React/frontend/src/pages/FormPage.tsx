import Layout_alt from "@/components/templates/layout"
import { useMemo, useState, useRef } from "react"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Card } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { apiClient } from "@/services/apiClient"
import { useAuth } from "@/contexts/AuthContext"
import { DiscordDarkMessagePreview } from "@/components/DiscordDarkMessagePreview"
import { buildKnowledgeCenterDiscordMessage } from "@/lib/knowledgeCenterDiscordMessage"

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
    const [materialType, setMaterialType] = useState("")
    const [discordId, setDiscordId] = useState("")
    const [title, setTitle] = useState("")
    const [description, setDescription] = useState("")
    const formRef = useRef<HTMLFormElement>(null)
    const { user } = useAuth()

    const parsedLink = useMemo(() => tryParseUrl(linkDraft), [linkDraft])
    const linkTags = useMemo(() => (parsedLink ? classifyLink(parsedLink) : []), [parsedLink])
    const faviconUrl = useMemo(() => {
        if (!parsedLink) return ""
        return `${parsedLink.origin}/favicon.ico`
    }, [parsedLink])

    const builtDiscordMessage = useMemo(
        () =>
            buildKnowledgeCenterDiscordMessage({
                type: materialType,
                title,
                description,
                linkToPost: normalizeUrl(linkDraft || ""),
                discordId,
                authorDisplayName: user?.username ?? "Ukendt bruger",
            }),
        [materialType, title, description, linkDraft, discordId, user?.username],
    )

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault()
        setIsSubmitting(true)
        setMessage("")

        const discordIdRaw = discordId.trim()
        const data = {
            type: materialType,
            title: title.trim(),
            description: description.trim(),
            linkToPost: normalizeUrl(linkDraft || ""),
            ...(discordIdRaw ? { discordId: discordIdRaw } : {}),
        }

        try {
            await apiClient('/KnowledgeCenter', {
                method: 'POST',
                body: JSON.stringify(data),
            })

            setMessage("Materiale blev indsendt til godkendelse.")
            formRef.current?.reset()
            setLinkDraft("")
            setMaterialType("")
            setDiscordId("")
            setTitle("")
            setDescription("")
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
                <div className="mb-8 w-full">
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

                <div className="grid gap-8 lg:grid-cols-2 lg:items-start">
                <Card className="soft-card w-full min-w-0 border-slate-100 p-6 sm:p-8">
                    <form ref={formRef} className="grid w-full min-w-0 gap-5" onSubmit={handleSubmit}>
                        {message && (
                            <div className={`rounded-lg border px-4 py-3 text-sm ${message.startsWith("Materiale blev")
                                ? "border-emerald-200 bg-emerald-50 text-emerald-700"
                                : "border-red-200 bg-red-50 text-red-600"
                                }`}>
                                {message}
                            </div>
                        )}

                        <div className="min-w-0 space-y-2">
                            <Label htmlFor="materialType" className="text-sm font-semibold text-slate-700">Materialetype</Label>
                            <select
                                name="materialType"
                                id="materialType"
                                value={materialType}
                                onChange={(e) => setMaterialType(e.target.value)}
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

                        <div className="min-w-0 space-y-2">
                            <Label htmlFor="discordId" className="text-sm font-semibold text-slate-700">
                                Discord bruger-ID <span className="font-normal text-slate-500">(valgfrit)</span>
                            </Label>
                            <Input
                                id="discordId"
                                name="discordId"
                                value={discordId}
                                onChange={(e) => setDiscordId(e.target.value)}
                                className="h-11 w-full min-w-0 font-mono text-sm"
                                placeholder="Fx 123456789012345678"
                                inputMode="numeric"
                                autoComplete="off"
                                pattern="\d*"
                                title="Kun cifre — dit numeriske Discord-ID (snowflake)"
                            />
                            <p className="text-xs text-slate-500">
                                Bruges til @-omtale i Knowledge Center på Discord. Find ID under Discord → Indstillinger → Avanceret →
                                udviklertilstand → højreklik på profil → Kopier bruger-ID. Tomt felt = dit linkede Discord, hvis du har et.
                            </p>
                        </div>

                        <div className="min-w-0 space-y-2">
                            <Label htmlFor="title" className="text-sm font-semibold text-slate-700">Titel</Label>
                            <Input
                                id="title"
                                name="title"
                                value={title}
                                onChange={(e) => setTitle(e.target.value)}
                                required
                                className="h-11 w-full min-w-0"
                                placeholder="Skriv en titel"
                            />
                        </div>

                        <div className="min-w-0 space-y-2">
                            <Label htmlFor="description" className="text-sm font-semibold text-slate-700">
                                Beskrivelse
                            </Label>
                            <p className="text-xs text-slate-500">
                                Du kan bruge Discord-markdown i beskrivelsen (fed, kursiv, understregning, lister, citater, spoilers, kode,
                                m.m.) — se den levende forhåndsvisning og listen nederst i preview-panelet.
                            </p>
                            <textarea
                                id="description"
                                name="description"
                                value={description}
                                onChange={(e) => setDescription(e.target.value)}
                                required
                                minLength={10}
                                rows={5}
                                placeholder="Beskriv materialet kort"
                                className="w-full min-w-0 rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-700 outline-none transition focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/30"
                            />
                        </div>

                        <div className="min-w-0 space-y-2">
                            <Label htmlFor="link" className="text-sm font-semibold text-slate-700">
                                Link (valgfri)
                            </Label>
                            <div className="grid gap-3 rounded-xl border border-slate-200 bg-slate-50/60 p-4">
                                <Input
                                    id="link"
                                    name="link"
                                    value={linkDraft}
                                    onChange={(event) => setLinkDraft(event.target.value)}
                                    className="h-12 w-full min-w-0 bg-white text-base"
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

                        <div className="min-w-0">
                            <Button type="submit" disabled={isSubmitting} className="h-11 w-full sm:w-auto">
                                {isSubmitting ? "Indsender..." : "Indsend materiale"}
                            </Button>
                        </div>
                    </form>
                </Card>

                <div className="min-w-0 lg:sticky lg:top-4">
                    <DiscordDarkMessagePreview content={builtDiscordMessage} className="w-full" />
                </div>
                </div>
            </section>
        </Layout_alt>
    )
}