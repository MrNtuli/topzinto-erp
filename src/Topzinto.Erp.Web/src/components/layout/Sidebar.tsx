import { NavLink } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import {
  LayoutDashboard,
  FolderKanban,
  FileText,
  ScrollText,
  Users,
  Calculator,
  ClipboardList,
  Calendar,
  Truck,
  Wrench,
  ShoppingCart,
  Package,
  Warehouse,
  UserCircle,
  Clock,
  MessageSquare,
  Files,
  BarChart3,
  Settings,
  ChevronLeft,
} from 'lucide-react'
import clsx from 'clsx'
import { useAuthStore } from '@/stores/authStore'
import { canAccessNav } from '@/lib/roleAccess'
import { getChatUnreadSummary } from '@/api/chat'
import styles from './Sidebar.module.css'

const navItems = [
  { to: '/', icon: LayoutDashboard, label: 'Dashboard' },
  { to: '/projects', icon: FolderKanban, label: 'Projects' },
  { to: '/tenders', icon: FileText, label: 'Tenders' },
  { to: '/contracts', icon: ScrollText, label: 'Contracts' },
  { to: '/clients', icon: Users, label: 'Clients' },
  { to: '/boq', icon: Calculator, label: 'BOQ & Costing' },
  { to: '/site-reports', icon: ClipboardList, label: 'Site Reports' },
  { to: '/schedule', icon: Calendar, label: 'Schedule' },
  { to: '/fleet', icon: Truck, label: 'Fleet' },
  { to: '/equipment', icon: Wrench, label: 'Equipment' },
  { to: '/procurement', icon: ShoppingCart, label: 'Procurement' },
  { to: '/suppliers', icon: Package, label: 'Suppliers' },
  { to: '/stores', icon: Warehouse, label: 'Stores' },
  { to: '/employees', icon: UserCircle, label: 'Employees' },
  { to: '/timesheets', icon: Clock, label: 'Timesheets' },
  { to: '/messages', icon: MessageSquare, label: 'Messages' },
  { to: '/documents', icon: Files, label: 'Documents' },
  { to: '/reports', icon: BarChart3, label: 'Reports' },
  { to: '/settings', icon: Settings, label: 'Settings' },
]

interface SidebarProps {
  collapsed: boolean
  onToggle: () => void
}

export function Sidebar({ collapsed, onToggle }: SidebarProps) {
  const role = useAuthStore((s) => s.user?.role)
  const visibleItems = navItems.filter(({ to }) => canAccessNav(role, to))
  const showChatBadge = canAccessNav(role, '/messages')

  const { data: chatUnread } = useQuery({
    queryKey: ['chat-unread'],
    queryFn: getChatUnreadSummary,
    enabled: showChatBadge,
    refetchInterval: 30_000,
    retry: false,
  })

  return (
    <aside className={clsx(styles.sidebar, collapsed && styles.collapsed)}>
      <div className={styles.logo}>
        <div className={styles.logoIcon}>T·P·Z</div>
        {!collapsed && (
          <div className={styles.logoText}>
            <span className={styles.logoName}>TOPZINTO</span>
            <span className={styles.logoTagline}>Smart. Proficient. Value.</span>
          </div>
        )}
      </div>

      <nav className={styles.nav}>
        {visibleItems.map(({ to, icon: Icon, label }) => (
          <NavLink
            key={to}
            to={to}
            end={to === '/'}
            className={({ isActive }) => clsx(styles.navItem, isActive && styles.active)}
            title={collapsed ? label : undefined}
          >
            <span className={styles.navIconWrap}>
              <Icon size={20} />
              {to === '/messages' && (chatUnread?.totalUnread ?? 0) > 0 && (
                <span className={styles.navBadge}>
                  {chatUnread!.totalUnread > 99 ? '99+' : chatUnread!.totalUnread}
                </span>
              )}
            </span>
            {!collapsed && <span>{label}</span>}
          </NavLink>
        ))}
      </nav>

      <button className={styles.collapseBtn} onClick={onToggle} aria-label="Toggle sidebar">
        <ChevronLeft size={18} className={clsx(collapsed && styles.rotated)} />
      </button>
    </aside>
  )
}
