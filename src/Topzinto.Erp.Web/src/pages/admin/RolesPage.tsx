import { useMemo, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { Check, X } from 'lucide-react'
import clsx from 'clsx'
import { getRoleMatrix, getUsers } from '@/api/admin'
import styles from '../projects/ProjectsPage.module.css'
import localStyles from './RolesPage.module.css'

const MODULE_LABELS: Record<string, string> = {
  Dashboard: 'Dashboard',
  Projects: 'Projects',
  SiteReports: 'Site Reports',
  Schedule: 'Schedule',
  Documents: 'Documents',
  Notifications: 'Notifications',
  Chat: 'Messages',
  Timesheets: 'Timesheets',
  Employees: 'Employees',
  Fleet: 'Fleet',
  Equipment: 'Equipment',
  Procurement: 'Procurement',
  Suppliers: 'Suppliers',
  Stores: 'Stores / Inventory',
  Boq: 'BOQ & Costing',
  Reports: 'Reports',
  Tenders: 'Tenders',
  Contracts: 'Contracts',
  Clients: 'Clients',
  Search: 'Global Search',
  Settings: 'Settings',
  Safety: 'Safety',
  Compliance: 'Compliance',
}

const MODULE_ORDER = [
  'Dashboard', 'Projects', 'Clients', 'Tenders', 'Contracts', 'Boq', 'SiteReports', 'Schedule',
  'Fleet', 'Equipment', 'Procurement', 'Suppliers', 'Stores', 'Employees', 'Timesheets',
  'Documents', 'Reports', 'Safety', 'Compliance', 'Notifications', 'Chat', 'Search', 'Settings',
]

function moduleLabel(key: string) {
  return MODULE_LABELS[key] ?? key.replace(/([A-Z])/g, ' $1').trim()
}

export function RolesPage() {
  const [selectedRole, setSelectedRole] = useState<string | null>(null)

  const { data: matrixData, isLoading, isError } = useQuery({
    queryKey: ['admin-role-matrix'],
    queryFn: getRoleMatrix,
    retry: false,
  })

  const { data: users } = useQuery({
    queryKey: ['admin-users'],
    queryFn: getUsers,
    retry: false,
  })

  const userCounts = useMemo(() => {
    const counts: Record<string, number> = {}
    for (const user of users ?? []) {
      counts[user.systemRole] = (counts[user.systemRole] ?? 0) + 1
    }
    return counts
  }, [users])

  const roles = matrixData?.roles ?? []
  const activeRole = selectedRole ?? roles[0]?.value ?? null
  const activeRoleLabel = roles.find((r) => r.value === activeRole)?.label

  const moduleAccess = useMemo(() => {
    if (!matrixData?.matrix || !activeRole) return []
    return MODULE_ORDER.map((module) => ({
      module,
      label: moduleLabel(module),
      hasAccess: (matrixData.matrix[module] ?? []).includes(activeRole),
    }))
  }, [matrixData, activeRole])

  const roleUsers = useMemo(
    () => (users ?? []).filter((u) => u.systemRole === activeRole),
    [users, activeRole],
  )

  const accessCount = moduleAccess.filter((m) => m.hasAccess).length

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>
        <Link to="/settings">Settings</Link> &gt; Roles &amp; Permissions
      </nav>
      <header className={styles.header}>
        <h1>Roles &amp; Permissions</h1>
      </header>

      {isLoading && <p className={styles.loading}>Loading roles...</p>}
      {isError && <p className={styles.hint}>Access denied or API unavailable.</p>}

      {!isLoading && !isError && roles.length === 0 && (
        <p className={styles.hint}>No system roles configured.</p>
      )}

      {roles.length > 0 && (
        <div className={localStyles.layout}>
          <section className={localStyles.panel}>
            <div className={localStyles.panelHeader}>
              <h2>System Roles</h2>
              <p>{roles.length} roles · click to view module access</p>
            </div>
            <div className={styles.tableWrap}>
              <table className={styles.table}>
                <thead>
                  <tr>
                    <th>Role</th>
                    <th>Users</th>
                  </tr>
                </thead>
                <tbody>
                  {roles.map((role) => {
                    const count = userCounts[role.value] ?? 0
                    const isSelected = activeRole === role.value
                    return (
                      <tr
                        key={role.value}
                        className={clsx(localStyles.roleRow, isSelected && localStyles.roleRowSelected)}
                        onClick={() => setSelectedRole(role.value)}
                      >
                        <td className={styles.name}>{role.label}</td>
                        <td>
                          <span
                            className={clsx(
                              localStyles.countBadge,
                              count === 0 && localStyles.countZero,
                            )}
                          >
                            {count}
                          </span>
                        </td>
                      </tr>
                    )
                  })}
                </tbody>
              </table>
            </div>
          </section>

          <section className={localStyles.panel}>
            {activeRole ? (
              <>
                <div className={localStyles.panelHeader}>
                  <h2>{activeRoleLabel}</h2>
                  <p>
                    {accessCount} of {moduleAccess.length} modules · {roleUsers.length} user
                    {roleUsers.length === 1 ? '' : 's'}
                  </p>
                </div>
                <table className={localStyles.matrixTable}>
                  <thead>
                    <tr>
                      <th>Module</th>
                      <th>Access</th>
                    </tr>
                  </thead>
                  <tbody>
                    {moduleAccess.map(({ module, label, hasAccess }) => (
                      <tr key={module}>
                        <td data-label="Module">{label}</td>
                        <td data-label="Access">
                          <span className={hasAccess ? localStyles.accessYes : localStyles.accessNo}>
                            {hasAccess ? <Check size={16} /> : <X size={16} />}
                            {hasAccess ? 'Allowed' : 'Denied'}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
                {roleUsers.length > 0 && (
                  <div className={localStyles.userList}>
                    <h3>Users with this role</h3>
                    <ul>
                      {roleUsers.map((user) => (
                        <li key={user.id}>
                          {user.firstName} {user.lastName}
                          <span>{user.email}</span>
                        </li>
                      ))}
                    </ul>
                  </div>
                )}
                <p className={localStyles.note}>
                  Module permissions are defined in the server RBAC matrix and are read-only here.
                  Assign roles to users from{' '}
                  <Link to="/admin/users">User Management</Link>.
                </p>
              </>
            ) : (
              <p className={localStyles.detailEmpty}>Select a role to view its module permissions.</p>
            )}
          </section>
        </div>
      )}
    </div>
  )
}
