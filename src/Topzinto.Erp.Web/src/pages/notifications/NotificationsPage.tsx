import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { getNotifications, markNotificationRead, markAllNotificationsRead } from '@/api/notifications'
import styles from '../projects/ProjectsPage.module.css'
import localStyles from './NotificationsPage.module.css'

export function NotificationsPage() {
  const queryClient = useQueryClient()
  const { data, isLoading, isError } = useQuery({
    queryKey: ['notifications-full'],
    queryFn: () => getNotifications(),
    retry: false,
  })

  const markRead = useMutation({
    mutationFn: markNotificationRead,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] })
      queryClient.invalidateQueries({ queryKey: ['notifications-full'] })
      queryClient.invalidateQueries({ queryKey: ['notifications-summary'] })
    },
  })

  const markAll = useMutation({
    mutationFn: markAllNotificationsRead,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] })
      queryClient.invalidateQueries({ queryKey: ['notifications-full'] })
      queryClient.invalidateQueries({ queryKey: ['notifications-summary'] })
    },
  })

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Notifications</nav>
      <header className={styles.header}>
        <h1>Notifications</h1>
        <button type="button" className={styles.newBtn} onClick={() => markAll.mutate()}>
          Mark all read
        </button>
      </header>

      <div className={localStyles.list}>
        {isLoading && <p>Loading...</p>}
        {data?.map((n) => (
          <div key={n.id} className={`${localStyles.card} ${n.isRead ? localStyles.read : ''}`}>
            <div className={localStyles.meta}>
              <span className={localStyles[n.severity.toLowerCase()] || localStyles.info}>{n.severity}</span>
              <span>{n.category}</span>
              <time>{n.createdAt}</time>
            </div>
            <h3>{n.title}</h3>
            <p>{n.message}</p>
            <div className={localStyles.actions}>
              {n.linkPath && <Link to={n.linkPath}>Go to item</Link>}
              {!n.isRead && (
                <button type="button" onClick={() => markRead.mutate(n.id)}>Mark read</button>
              )}
            </div>
          </div>
        ))}
        {isError && <p className={styles.hint}>Start the API to load notifications.</p>}
      </div>
    </div>
  )
}
