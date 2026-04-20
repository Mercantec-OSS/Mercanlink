/** Matcher Backend/Discord/DiscordBotService MapKnowledgeSubmissionTypeParts + BuildPublishedMessageContent */

export type KnowledgePreviewInput = {
  type: string
  title: string
  description: string
  linkToPost: string
  discordId: string
  authorDisplayName: string
}

export function mapKnowledgeSubmissionTypeParts(typeRaw: string): {
  emoji: string
  label: string
  linkIntro: string
} {
  const type = typeRaw.trim().toLowerCase()
  switch (type) {
    case "blog-post":
      return { emoji: "📝", label: "Blogindlæg", linkIntro: "Link til blogindlæg" }
    case "video":
      return { emoji: "🎥", label: "Video", linkIntro: "Link til video" }
    case "artikel":
      return { emoji: "📰", label: "Artikel", linkIntro: "Link til artikel" }
    case "andet":
      return { emoji: "📎", label: "Andet", linkIntro: "Link til materiale" }
    default:
      return {
        emoji: "📝",
        label: typeRaw.trim() || "Ukendt",
        linkIntro: `Link til ${typeRaw.trim() || "materiale"}`,
      }
  }
}

function isValidDiscordSnowflake(s: string): boolean {
  const t = s.trim()
  if (!/^\d+$/.test(t)) return false
  return t.length >= 17 && t.length <= 22
}

export function formatKnowledgeAuthorLine(discordId: string, authorName: string): string {
  if (isValidDiscordSnowflake(discordId)) {
    return `<@${discordId.trim()}>`
  }
  return authorName.trim() || "Ukendt bruger"
}

export function buildKnowledgeCenterDiscordMessage(input: KnowledgePreviewInput): string {
  const { emoji, label, linkIntro } = mapKnowledgeSubmissionTypeParts(input.type)
  const author = formatKnowledgeAuthorLine(input.discordId, input.authorDisplayName)
  const link = input.linkToPost.trim()
  const linkValue = link ? link : "Intet link angivet"

  return (
    "# 🪐 **Nyt materiale udgivet af:** " +
    `${author}, **Tjek det ud!**\n\n` +
    `## **${emoji} Materiale type:** ${label}\n\n` +
    `## **📌 Titel:** ${input.title}\n\n` +
    "## **✉️ Beskrivelse:**\n" +
    `${input.description}\n\n` +
    `## **🔗 ${linkIntro}:** ${linkValue}`
  )
}
