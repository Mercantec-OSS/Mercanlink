import { useEffect, useState } from "react"
import Layout_alt from "@/components/templates/layout"
import { Card } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Label } from "@/components/ui/label"
import {
  approveSubmission,
  getPendingSubmissions,
  rejectSubmission,
  type KnowledgeSubmission,
} from "@/services/knowledgeCenterService"

export function KnowledgeCenterAdminPage() {
  const [submissions, setSubmissions] = useState<KnowledgeSubmission[]>([])
  const [rejectReasons, setRejectReasons] = useState<Record<string, string>>({})
  const [loading, setLoading] = useState(true)
  const [actionMessage, setActionMessage] = useState("")
  const [processingId, setProcessingId] = useState<string | null>(null)

  useEffect(() => {
    void loadPendingSubmissions()
  }, [])

  async function loadPendingSubmissions() {
    setLoading(true)
    setActionMessage("")
    try {
      const data = await getPendingSubmissions()
      setSubmissions(data)
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : "Ukendt fejl under hentning."
      setActionMessage(`Kunne ikke hente moderation-køen: ${errorMessage}`)
    } finally {
      setLoading(false)
    }
  }

  async function handleApprove(submissionId: string) {
    setProcessingId(submissionId)
    setActionMessage("")
    try {
      await approveSubmission(submissionId)
      setSubmissions((previous) => previous.filter((submission) => submission.id !== submissionId))
      setActionMessage("Submission blev godkendt og publiceret til Discord.")
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : "Ukendt fejl under godkendelse."
      setActionMessage(`Kunne ikke godkende submission: ${errorMessage}`)
    } finally {
      setProcessingId(null)
    }
  }

  async function handleReject(submissionId: string) {
    const reason = rejectReasons[submissionId]?.trim() ?? ""
    if (!reason) {
      setActionMessage("Angiv en afvisningsgrund før du afviser.")
      return
    }

    setProcessingId(submissionId)
    setActionMessage("")
    try {
      await rejectSubmission(submissionId, reason)
      setSubmissions((previous) => previous.filter((submission) => submission.id !== submissionId))
      setActionMessage("Submission blev afvist.")
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : "Ukendt fejl under afvisning."
      setActionMessage(`Kunne ikke afvise submission: ${errorMessage}`)
    } finally {
      setProcessingId(null)
    }
  }

  if (loading) {
    return (
      <Layout_alt>
        <section className="container-shell py-16 text-slate-600">Indlæser moderation-kø...</section>
      </Layout_alt>
    )
  }

  return (
    <Layout_alt>
      <section className="container-shell py-12 sm:py-16 lg:py-20">
        <div className="mb-8 flex flex-wrap items-start justify-between gap-4">
          <div>
            <h1 className="text-3xl font-bold tracking-[-0.02em] text-slate-900 sm:text-4xl">
              Knowledge Center moderation
            </h1>
            <p className="mt-3 text-slate-600">
              Godkend eller afvis elevindsendelser. Godkendte indlæg publiceres automatisk i Discord.
            </p>
          </div>
          <Button variant="outline" onClick={() => void loadPendingSubmissions()}>
            Opdater kø
          </Button>
        </div>

        {actionMessage && (
          <div className="mb-6 rounded-lg border border-slate-200 bg-slate-50 px-4 py-3 text-sm text-slate-700">
            {actionMessage}
          </div>
        )}

        {!submissions.length ? (
          <Card className="soft-card border-slate-100 p-6 text-slate-600">
            Ingen pending submissions lige nu.
          </Card>
        ) : (
          <div className="grid gap-5">
            {submissions.map((submission) => (
              <Card key={submission.id} className="soft-card border-slate-100 p-6">
                <div className="space-y-2">
                  <p className="text-xs uppercase tracking-wide text-slate-500">{submission.type}</p>
                  <h2 className="text-xl font-semibold text-slate-900">{submission.title}</h2>
                  <p className="text-sm text-slate-600">Af {submission.authorName}</p>
                  <p className="text-sm leading-6 text-slate-700">{submission.description}</p>
                  {submission.linkToPost ? (
                    <a
                      href={submission.linkToPost}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="inline-flex text-sm font-semibold text-indigo-700 hover:text-indigo-500"
                    >
                      Åbn link
                    </a>
                  ) : (
                    <p className="text-sm text-slate-500">Ingen link angivet</p>
                  )}
                </div>

                <div className="mt-5 grid gap-3">
                  <div className="space-y-2">
                    <Label htmlFor={`reject-${submission.id}`} className="text-sm font-semibold text-slate-700">
                      Afvisningsgrund
                    </Label>
                    <textarea
                      id={`reject-${submission.id}`}
                      rows={3}
                      value={rejectReasons[submission.id] ?? ""}
                      onChange={(event) =>
                        setRejectReasons((previous) => ({
                          ...previous,
                          [submission.id]: event.target.value,
                        }))
                      }
                      className="w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm text-slate-700 outline-none transition focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/30"
                      placeholder="Skriv hvorfor indholdet afvises"
                    />
                  </div>

                  <div className="flex flex-wrap gap-3">
                    <Button
                      onClick={() => void handleApprove(submission.id)}
                      disabled={processingId === submission.id}
                    >
                      Godkend
                    </Button>
                    <Button
                      variant="outline"
                      onClick={() => void handleReject(submission.id)}
                      disabled={processingId === submission.id}
                    >
                      Afvis
                    </Button>
                  </div>
                </div>
              </Card>
            ))}
          </div>
        )}
      </section>
    </Layout_alt>
  )
}
