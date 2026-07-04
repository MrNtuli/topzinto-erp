import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useState, useRef, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { Bell } from 'lucide-react'
import {
  getNotificationSummary,
  getNotifications,
  markNotificationRead,
  markAllNotificationsRead,
} from '@/api/notifications'
import styles from './NotificationBell.module.css'

export function NotificationBell() {
  const [open, setOpen] = useState(false)
  const ref = useRef<HTMLDivElement>(null)
  const queryClient = useQueryClient()

  const { data: summary } = useQuery({
    queryKey: ['notifications-summary'],
    queryFn: getNotificationSummary,
    refetchInterval: 60_000,
    retry: false,
  })

  const { data: notifications } = useQuery({
    queryKey: ['notifications'],
    queryFn: () => getNotifications(),
    enabled: open,
    retry: false,
  })

  const markRead = useMutation({
    mutationFn: markNotificationRead,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] })
      queryClient.invalidateQueries({ queryKey: ['notifications-summary'] })
    },
  })

  const markAll = useMutation({
    mutationFn: markAllNotificationsRead,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] })
      queryClient.invalidateQueries({ queryKey: ['notifications-summary'] })
    },
  })

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false)
    }
    document.addEventListener('mousedown', handler)
    return () => document.removeEventListener('mousedown', handler)
  }, [])

  const unread = summary?.unreadCount ?? 0

  return (
    <div className={styles.wrap} ref={ref}>
      <button
        type="button"
        className={styles.btn}
        aria-label="Notifications"
        onClick={() => setOpen((o) => !o)}
      >
        <Bell size={20} />
        {unread > 0 && <span className={styles.badge}>{unread > 9 ? '9+' : unread}</span>}
      </button>

      {open && (
        <div className={styles.dropdown}>
          <div className={styles.header}>
            <strong>Notifications</strong>
            {unread > 0 && (
              <button type="button" onClick={() => markAll.mutate()}>Mark all read</button>
            )}
          </div>
          <ul className={styles.list}>
            {notifications?.length === 0 && <li className={styles.empty}>No notifications</li>}
            {notifications?.map((n) => (
              <li key={n.id} className={n.isRead ? styles.read : styles.unread}>
                <div className={styles.itemHead}>
                  <span className={styles[n.severity.toLowerCase()] || styles.info}>{n.category}</span>
                  <time>{n.createdAt}</time>
                </div>
                <strong>{n.title}</strong>
                <p>{n.message}</p>
                <div className={styles.actions}>
                  {n.linkPath && (
                    <Link to={n.linkPath} onClick={() => { if (!n.isRead) markRead.mutate(n.id); setOpen(false) }}>
                      View
                    </Link>
                  )}
                  {!n.isRead && (
                    <button type="button" onClick={() => markRead.mutate(n.id)}>Mark read</button>
                  )}
                </div>
              </li>
            ))}
          </ul>
          <Link to="/notifications" className={styles.viewAll} onClick={() => setOpen(false)}>
            View all notifications
          </Link>
        </div>
      )}
    </div>
  )
}
