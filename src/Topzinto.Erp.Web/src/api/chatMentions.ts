import { apiFetch } from './client'

export interface ChatMentionUser {
  id: string
  displayName: string
  firstName: string
  lastName: string
  email: string
}

export function getMentionableUsers() {
  return apiFetch<ChatMentionUser[]>('/chat/mentionable-users')
}

/** Split message text into plain segments and @mention highlights. */
export function splitMentionSegments(
  text: string,
  displayNames: string[],
): Array<{ type: 'text' | 'mention'; value: string }> {
  if (!text || displayNames.length === 0) return [{ type: 'text', value: text }]

  const sorted = [...displayNames].sort((a, b) => b.length - a.length)
  const pattern = new RegExp(`@(${sorted.map(escapeRegex).join('|')})`, 'gi')
  const segments: Array<{ type: 'text' | 'mention'; value: string }> = []
  let lastIndex = 0

  for (const match of text.matchAll(pattern)) {
    const index = match.index ?? 0
    if (index > lastIndex) {
      segments.push({ type: 'text', value: text.slice(lastIndex, index) })
    }
    segments.push({ type: 'mention', value: match[0] })
    lastIndex = index + match[0].length
  }

  if (lastIndex < text.length) {
    segments.push({ type: 'text', value: text.slice(lastIndex) })
  }

  return segments.length > 0 ? segments : [{ type: 'text', value: text }]
}

function escapeRegex(value: string) {
  return value.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')
}
