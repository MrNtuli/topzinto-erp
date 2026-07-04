import { apiFetch } from './client'

export interface ChatChannel {
  id: string
  name: string
  slug: string
  type: string
  description: string | null
  projectName: string | null
  messageCount: number
  lastMessageAt: string | null
  unreadCount: number
}

export interface ChatMessage {
  id: string
  channelId: string
  senderUserId: string
  senderName: string
  content: string
  sentAt: string
  attachmentFileName: string | null
  attachmentContentType: string | null
  attachmentSizeBytes: number | null
}

export interface ChatUnreadSummary {
  totalUnread: number
}

export function getChatChannels() {
  return apiFetch<ChatChannel[]>('/chat/channels')
}

export function getOrCreateDirectChannel(otherUserId: string) {
  return apiFetch<ChatChannel>(`/chat/direct/${otherUserId}`, { method: 'POST' })
}

export { getMentionableUsers, splitMentionSegments, type ChatMentionUser } from './chatMentions'

export function getChatUnreadSummary() {
  return apiFetch<ChatUnreadSummary>('/chat/unread')
}

export function getChatMessages(channelId: string) {
  return apiFetch<ChatMessage[]>(`/chat/channels/${channelId}/messages`)
}

export function markChatChannelRead(channelId: string) {
  return apiFetch<void>(`/chat/channels/${channelId}/read`, { method: 'POST' })
}

export function sendChatMessage(channelId: string, content: string) {
  return apiFetch<ChatMessage>(`/chat/channels/${channelId}/messages`, {
    method: 'POST',
    body: JSON.stringify({ content }),
  })
}

export async function sendChatMessageWithFile(channelId: string, file: File, content?: string) {
  const token = JSON.parse(localStorage.getItem('topzinto-auth') || '{}')?.state?.accessToken
  const form = new FormData()
  form.append('file', file)
  if (content?.trim()) form.append('content', content.trim())

  const res = await fetch(`/api/chat/channels/${channelId}/messages/upload`, {
    method: 'POST',
    headers: token ? { Authorization: `Bearer ${token}` } : {},
    body: form,
  })
  if (!res.ok) throw new Error('Upload failed')
  return res.json() as Promise<ChatMessage>
}

export function getChatAttachmentUrl(messageId: string) {
  return `/api/chat/messages/${messageId}/attachment`
}

export async function downloadChatAttachment(messageId: string, fileName: string) {
  const token = JSON.parse(localStorage.getItem('topzinto-auth') || '{}')?.state?.accessToken
  const res = await fetch(getChatAttachmentUrl(messageId), {
    headers: token ? { Authorization: `Bearer ${token}` } : {},
  })
  if (!res.ok) throw new Error('Download failed')
  const blob = await res.blob()
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = fileName
  a.click()
  URL.revokeObjectURL(url)
}

export function formatFileSize(bytes: number | null | undefined) {
  if (!bytes) return ''
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}
