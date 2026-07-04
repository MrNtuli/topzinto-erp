import { useEffect, useMemo, useRef, useState } from 'react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import clsx from 'clsx'
import { Paperclip, Plus, X } from 'lucide-react'
import type { HubConnection } from '@microsoft/signalr'
import {
  downloadChatAttachment,
  formatFileSize,
  getChatChannels,
  getChatMessages,
  getMentionableUsers,
  getOrCreateDirectChannel,
  markChatChannelRead,
  sendChatMessage,
  sendChatMessageWithFile,
  splitMentionSegments,
  type ChatMessage,
  type ChatMentionUser,
} from '@/api/chat'
import { useAuthStore } from '@/stores/authStore'
import { createChatConnection } from '@/lib/chatHub'
import styles from './MessagesPage.module.css'
import pageStyles from '../projects/ProjectsPage.module.css'

const MAX_FILE_MB = 10

function MessageContent({ text, displayNames }: { text: string; displayNames: string[] }) {
  const segments = splitMentionSegments(text, displayNames)
  return (
    <>
      {segments.map((seg, i) =>
        seg.type === 'mention' ? (
          <span key={i} className={styles.mention}>{seg.value}</span>
        ) : (
          <span key={i}>{seg.value}</span>
        ),
      )}
    </>
  )
}

export function MessagesPage() {
  const user = useAuthStore((s) => s.user)
  const accessToken = useAuthStore((s) => s.accessToken)
  const queryClient = useQueryClient()
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [draft, setDraft] = useState('')
  const [pendingFile, setPendingFile] = useState<File | null>(null)
  const [messages, setMessages] = useState<ChatMessage[]>([])
  const [connected, setConnected] = useState(false)
  const [sending, setSending] = useState(false)
  const [mentionOpen, setMentionOpen] = useState(false)
  const [mentionFilter, setMentionFilter] = useState('')
  const [newDmOpen, setNewDmOpen] = useState(false)
  const [startingDm, setStartingDm] = useState(false)
  const messagesEndRef = useRef<HTMLDivElement>(null)
  const fileInputRef = useRef<HTMLInputElement>(null)
  const draftInputRef = useRef<HTMLInputElement>(null)
  const connectionRef = useRef<HubConnection | null>(null)
  const joinedChannelRef = useRef<string | null>(null)

  const { data: channels, isLoading } = useQuery({
    queryKey: ['chat-channels'],
    queryFn: getChatChannels,
    retry: false,
  })

  const { data: mentionUsers = [] } = useQuery({
    queryKey: ['chat-mentionable-users'],
    queryFn: getMentionableUsers,
    retry: false,
  })

  const displayNames = useMemo(() => mentionUsers.map((u) => u.displayName), [mentionUsers])

  const filteredMentionUsers = useMemo(() => {
    const q = mentionFilter.toLowerCase()
    return mentionUsers.filter(
      (u) =>
        u.id !== user?.id &&
        (u.displayName.toLowerCase().includes(q) ||
          u.email.toLowerCase().includes(q) ||
          u.firstName.toLowerCase().startsWith(q) ||
          u.lastName.toLowerCase().startsWith(q)),
    )
  }, [mentionUsers, mentionFilter, user?.id])

  const selected = channels?.find((c) => c.id === selectedId)
  const canSend = Boolean(selectedId && !sending && (draft.trim() || pendingFile))

  useEffect(() => {
    if (!selectedId && channels?.length) setSelectedId(channels[0].id)
  }, [channels, selectedId])

  useEffect(() => {
    if (!selectedId) return
    getChatMessages(selectedId).then(setMessages).catch(() => setMessages([]))
    markChatChannelRead(selectedId)
      .then(() => {
        queryClient.invalidateQueries({ queryKey: ['chat-channels'] })
        queryClient.invalidateQueries({ queryKey: ['chat-unread'] })
      })
      .catch(() => {})
  }, [selectedId, queryClient])

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages])

  useEffect(() => {
    if (!accessToken) return

    const connection = createChatConnection(() => accessToken)
    connectionRef.current = connection

    connection.on('ReceiveMessage', (msg: ChatMessage) => {
      const isActive = msg.channelId === joinedChannelRef.current
      setMessages((prev) => {
        if (!isActive) return prev
        return prev.some((m) => m.id === msg.id) ? prev : [...prev, msg]
      })
      if (isActive) {
        markChatChannelRead(msg.channelId)
          .then(() => {
            queryClient.invalidateQueries({ queryKey: ['chat-channels'] })
            queryClient.invalidateQueries({ queryKey: ['chat-unread'] })
          })
          .catch(() => {})
      } else {
        queryClient.invalidateQueries({ queryKey: ['chat-channels'] })
        queryClient.invalidateQueries({ queryKey: ['chat-unread'] })
      }
    })

    connection.onreconnected(() => setConnected(true))
    connection.onclose(() => setConnected(false))

    connection.start()
      .then(() => setConnected(true))
      .catch(() => setConnected(false))

    return () => {
      connection.stop()
      connectionRef.current = null
    }
  }, [accessToken, queryClient])

  useEffect(() => {
    const connection = connectionRef.current
    if (!connection || !connected || !selectedId) return

    async function switchChannel() {
      if (joinedChannelRef.current)
        await connection!.invoke('LeaveChannel', joinedChannelRef.current)
      await connection!.invoke('JoinChannel', selectedId)
      joinedChannelRef.current = selectedId
    }

    switchChannel().catch(() => {})
  }, [selectedId, connected])

  function handleDraftChange(e: React.ChangeEvent<HTMLInputElement>) {
    const val = e.target.value
    setDraft(val)
    const cursor = e.target.selectionStart ?? val.length
    const before = val.slice(0, cursor)
    const atMatch = before.match(/@([^\s@]*)$/)
    if (atMatch) {
      setMentionOpen(true)
      setMentionFilter(atMatch[1])
    } else {
      setMentionOpen(false)
      setMentionFilter('')
    }
  }

  function insertMention(mentionUser: ChatMentionUser) {
    const input = draftInputRef.current
    const cursor = input?.selectionStart ?? draft.length
    const before = draft.slice(0, cursor)
    const after = draft.slice(cursor)
    const atMatch = before.match(/@([^\s@]*)$/)
    if (!atMatch) return

    const prefix = before.slice(0, before.length - atMatch[0].length)
    const mention = `@${mentionUser.displayName} `
    const next = prefix + mention + after
    setDraft(next)
    setMentionOpen(false)
    setMentionFilter('')
    requestAnimationFrame(() => {
      input?.focus()
      const pos = prefix.length + mention.length
      input?.setSelectionRange(pos, pos)
    })
  }

  function handleFileSelect(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    if (!file) return
    if (file.size > MAX_FILE_MB * 1024 * 1024) {
      alert(`File too large. Maximum size is ${MAX_FILE_MB} MB.`)
      e.target.value = ''
      return
    }
    setPendingFile(file)
    e.target.value = ''
  }

  async function handleSend(e: React.FormEvent) {
    e.preventDefault()
    if (!selectedId || !canSend) return
    setSending(true)
    setMentionOpen(false)
    try {
      if (pendingFile) {
        const msg = await sendChatMessageWithFile(selectedId, pendingFile, draft.trim() || undefined)
        setMessages((prev) => (prev.some((m) => m.id === msg.id) ? prev : [...prev, msg]))
        setDraft('')
        setPendingFile(null)
      } else if (draft.trim()) {
        const connection = connectionRef.current
        if (connection && connected) {
          await connection.invoke('SendMessage', selectedId, draft.trim())
          setDraft('')
        } else {
          const msg = await sendChatMessage(selectedId, draft.trim())
          setMessages((prev) => (prev.some((m) => m.id === msg.id) ? prev : [...prev, msg]))
          setDraft('')
        }
      }
      queryClient.invalidateQueries({ queryKey: ['chat-channels'] })
      queryClient.invalidateQueries({ queryKey: ['chat-unread'] })
    } finally {
      setSending(false)
    }
  }

  async function startDirectMessage(otherUser: ChatMentionUser) {
    setStartingDm(true)
    try {
      const channel = await getOrCreateDirectChannel(otherUser.id)
      setNewDmOpen(false)
      await queryClient.invalidateQueries({ queryKey: ['chat-channels'] })
      setSelectedId(channel.id)
    } finally {
      setStartingDm(false)
    }
  }

  const grouped = {
    General: channels?.filter((c) => c.type === 'General') ?? [],
    Department: channels?.filter((c) => c.type === 'Department') ?? [],
    Project: channels?.filter((c) => c.type === 'Project') ?? [],
    Direct: channels?.filter((c) => c.type === 'Direct') ?? [],
  }

  const dmCandidates = mentionUsers.filter((u) => u.id !== user?.id)

  return (
    <div className={pageStyles.page}>
      <nav className={pageStyles.breadcrumb}>Home &gt; Messages</nav>
      <header className={pageStyles.header}>
        <h1>Team Chat</h1>
      </header>
      <p className={pageStyles.hint} style={{ marginBottom: 16 }}>
        Type @ to mention a colleague. Attach files up to {MAX_FILE_MB} MB.
      </p>

      {isLoading && <p className={pageStyles.loading}>Loading channels...</p>}

      <div className={styles.layout}>
        <aside className={styles.channelList}>
          <div className={styles.channelListHeader}>
            <h2>Channels</h2>
            <span className={clsx(styles.status, connected && styles.statusLive)}>
              {connected ? '● Live' : 'Connecting...'}
            </span>
          </div>
          {(['General', 'Department', 'Project'] as const).map((group) =>
            grouped[group].length > 0 ? (
              <div key={group} className={styles.channelGroup}>
                <div className={styles.groupLabel}>{group}</div>
                {grouped[group].map((ch) => (
                  <button
                    key={ch.id}
                    type="button"
                    className={clsx(styles.channelBtn, selectedId === ch.id && styles.channelActive)}
                    onClick={() => setSelectedId(ch.id)}
                  >
                    <span className={styles.channelRow}>
                      <span>#{ch.slug}</span>
                      {ch.unreadCount > 0 && (
                        <span className={styles.unreadBadge}>{ch.unreadCount}</span>
                      )}
                    </span>
                    <span className={styles.channelMeta}>{ch.name}</span>
                  </button>
                ))}
              </div>
            ) : null,
          )}
          <div className={styles.channelGroup}>
            <div className={styles.groupLabelRow}>
              <div className={styles.groupLabel}>Direct Messages</div>
              <button
                type="button"
                className={styles.newDmBtn}
                title="New direct message"
                onClick={() => setNewDmOpen((v) => !v)}
                disabled={startingDm}
              >
                <Plus size={14} />
              </button>
            </div>
            {newDmOpen && (
              <ul className={styles.newDmPicker}>
                {dmCandidates.map((u) => (
                  <li key={u.id}>
                    <button type="button" onClick={() => startDirectMessage(u)} disabled={startingDm}>
                      {u.displayName}
                      <span>{u.email}</span>
                    </button>
                  </li>
                ))}
              </ul>
            )}
            {grouped.Direct.length === 0 && !newDmOpen && (
              <p className={styles.emptyDm}>No direct messages yet — click + to start one.</p>
            )}
            {grouped.Direct.map((ch) => (
              <button
                key={ch.id}
                type="button"
                className={clsx(styles.channelBtn, selectedId === ch.id && styles.channelActive)}
                onClick={() => setSelectedId(ch.id)}
              >
                <span className={styles.channelRow}>
                  <span>{ch.name}</span>
                  {ch.unreadCount > 0 && (
                    <span className={styles.unreadBadge}>{ch.unreadCount}</span>
                  )}
                </span>
              </button>
            ))}
          </div>
        </aside>

        <section className={styles.chatPanel}>
          {!selected ? (
            <div className={styles.empty}>Select a channel to start chatting</div>
          ) : (
            <>
              <div className={styles.chatHeader}>
                <h2>{selected.type === 'Direct' ? selected.name : `#${selected.slug}`}</h2>
                <p>{selected.type === 'Direct' ? 'Private direct message' : (selected.description ?? selected.name)}</p>
              </div>
              <div className={styles.messages}>
                {messages.map((m) => {
                  const isOwn = m.senderUserId === user?.id
                  const hasAttachment = Boolean(m.attachmentFileName)
                  return (
                    <div key={m.id} className={clsx(styles.message, isOwn && styles.messageOwn)}>
                      {!isOwn && <div className={styles.messageSender}>{m.senderName}</div>}
                      {m.content && (
                        <div className={styles.messageText}>
                          <MessageContent text={m.content} displayNames={displayNames} />
                        </div>
                      )}
                      {hasAttachment && (
                        <button
                          type="button"
                          className={styles.attachmentLink}
                          onClick={() => downloadChatAttachment(m.id, m.attachmentFileName!)}
                        >
                          <Paperclip size={14} />
                          <span>{m.attachmentFileName}</span>
                          {m.attachmentSizeBytes ? (
                            <span className={styles.attachmentSize}>
                              {formatFileSize(m.attachmentSizeBytes)}
                            </span>
                          ) : null}
                        </button>
                      )}
                      <div className={styles.messageTime}>{m.sentAt}</div>
                    </div>
                  )
                })}
                <div ref={messagesEndRef} />
              </div>
              {pendingFile && (
                <div className={styles.pendingFile}>
                  <Paperclip size={14} />
                  <span>{pendingFile.name}</span>
                  <span className={styles.attachmentSize}>{formatFileSize(pendingFile.size)}</span>
                  <button type="button" onClick={() => setPendingFile(null)} aria-label="Remove file">
                    <X size={14} />
                  </button>
                </div>
              )}
              <form className={styles.compose} onSubmit={handleSend}>
                <input
                  ref={fileInputRef}
                  type="file"
                  className={styles.fileInput}
                  onChange={handleFileSelect}
                />
                <button
                  type="button"
                  className={styles.attachBtn}
                  onClick={() => fileInputRef.current?.click()}
                  title={`Attach file (max ${MAX_FILE_MB} MB)`}
                  disabled={sending}
                >
                  <Paperclip size={18} />
                </button>
                <div className={styles.composeInputWrap}>
                  {mentionOpen && filteredMentionUsers.length > 0 && (
                    <ul className={styles.mentionDropdown} role="listbox">
                      {filteredMentionUsers.slice(0, 6).map((u) => (
                        <li key={u.id}>
                          <button type="button" className={styles.mentionOption} onClick={() => insertMention(u)}>
                            <span className={styles.mentionOptionName}>{u.displayName}</span>
                            <span className={styles.mentionOptionEmail}>{u.email}</span>
                          </button>
                        </li>
                      ))}
                    </ul>
                  )}
                  <input
                    ref={draftInputRef}
                    value={draft}
                    onChange={handleDraftChange}
                    placeholder="Type a message... use @ to mention"
                    maxLength={4000}
                  />
                </div>
                <button type="submit" disabled={!canSend}>Send</button>
              </form>
            </>
          )}
        </section>
      </div>
    </div>
  )
}
