import Layout_alt from "@/components/templates/layout"
import { useState, useRef } from "react"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Card } from "@/components/ui/card"
import { Button } from "@/components/ui/button"

export default function FormPage() {
    const [isSubmitting, setIsSubmitting] = useState(false)
    const [message, setMessage] = useState("")
    const formRef = useRef<HTMLFormElement>(null)

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault()
        setIsSubmitting(true)
        setMessage("")

        const formData = new FormData(e.currentTarget)

        const data = {
            type: formData.get('materialType') as string,
            author: "User",
            discordId: formData.get('DiscordId') as string,
            title: formData.get('title') as string,
            description: formData.get('description') as string,
            linkToPost: formData.get('link') as string || "",
        }

        try {
            const response = await fetch('/api/KnowledgeCenter', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(data),
            })

            if (response.ok) {
                setMessage("Materiale blev indsendt.")
                formRef.current?.reset()
            } else {
                const errorText = await response.text()
                setMessage(`Kunne ikke indsende: ${errorText}`)
            }
        } catch (error) {
            setMessage(`Netværksfejl: ${String(error)}`)
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
                        Del links, noter eller videoer med holdet. Formularen sender direkte til Knowledge Center endpoint.
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

                        <div className="space-y-2">
                            <Label htmlFor="DiscordId" className="text-sm font-semibold text-slate-700">Discord ID</Label>
                            <Input id="DiscordId" name="DiscordId" required className="h-11" placeholder="fx User#1234" />
                        </div>

                        <div className="space-y-2">
                            <Label htmlFor="link" className="text-sm font-semibold text-slate-700">Link (valgfri)</Label>
                            <Input id="link" name="link" className="h-11" placeholder="https://..." />
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