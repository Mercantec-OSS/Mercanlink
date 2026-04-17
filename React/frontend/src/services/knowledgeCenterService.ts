import { apiClient } from "@/services/apiClient"

export type KnowledgeSubmissionStatus = "Pending" | "Approved" | "Rejected"

export interface KnowledgeSubmission {
  id: string
  type: string
  title: string
  description: string
  linkToPost: string
  authorName: string
  discordId: string
  status: KnowledgeSubmissionStatus
  reviewedByUserId?: string
  reviewedAt?: string
  rejectionReason?: string
  modMessageId?: number
  publishedMessageId?: number
  publishedToDiscordAt?: string
  createdAt: string
  updatedAt: string
}

export async function getPendingSubmissions(): Promise<KnowledgeSubmission[]> {
  return apiClient<KnowledgeSubmission[]>("/KnowledgeCenter/pending")
}

export async function approveSubmission(submissionId: string): Promise<KnowledgeSubmission> {
  return apiClient<KnowledgeSubmission>(`/KnowledgeCenter/${submissionId}/approve`, {
    method: "PATCH",
    body: JSON.stringify({}),
  })
}

export async function rejectSubmission(submissionId: string, reason: string): Promise<KnowledgeSubmission> {
  return apiClient<KnowledgeSubmission>(`/KnowledgeCenter/${submissionId}/reject`, {
    method: "PATCH",
    body: JSON.stringify({ reason }),
  })
}
