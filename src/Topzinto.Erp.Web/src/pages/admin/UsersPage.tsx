import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { Plus } from 'lucide-react'
import {
  getUsers,
  getRoles,
  createUser,
  updateUser,
  resetUserPassword,
  type UserAdmin,
} from '@/api/admin'
import { useAuthStore } from '@/stores/authStore'
import styles from '../projects/ProjectsPage.module.css'
import localStyles from './UsersPage.module.css'

const emptyCreateForm = {
  email: '',
  firstName: '',
  lastName: '',
  role: 'Employee',
  password: '',
}

export function UsersPage() {
  const queryClient = useQueryClient()
  const currentUserId = useAuthStore((s) => s.user?.id)
  const [showCreate, setShowCreate] = useState(false)
  const [createForm, setCreateForm] = useState(emptyCreateForm)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [editForm, setEditForm] = useState({ firstName: '', lastName: '', role: 'Employee', isActive: true })
  const [resetId, setResetId] = useState<string | null>(null)
  const [resetPassword, setResetPassword] = useState('')

  const { data: users, isLoading, isError } = useQuery({
    queryKey: ['admin-users'],
    queryFn: getUsers,
    retry: false,
  })

  const { data: roles } = useQuery({
    queryKey: ['admin-roles'],
    queryFn: getRoles,
    retry: false,
  })

  const createMutation = useMutation({
    mutationFn: createUser,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-users'] })
      setCreateForm(emptyCreateForm)
      setShowCreate(false)
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: typeof editForm }) => updateUser(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-users'] })
      setEditingId(null)
    },
  })

  const resetMutation = useMutation({
    mutationFn: ({ id, password }: { id: string; password: string }) => resetUserPassword(id, password),
    onSuccess: () => {
      setResetId(null)
      setResetPassword('')
    },
  })

  function startEdit(user: UserAdmin) {
    setEditingId(user.id)
    setEditForm({
      firstName: user.firstName,
      lastName: user.lastName,
      role: user.systemRole,
      isActive: user.isActive,
    })
    setResetId(null)
  }

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>
        <Link to="/settings">Settings</Link> &gt; User Management
      </nav>
      <header className={styles.header}>
        <h1>User Management</h1>
        <button type="button" className={styles.newBtn} onClick={() => setShowCreate((v) => !v)}>
          <Plus size={18} />
          {showCreate ? 'Cancel' : 'New User'}
        </button>
      </header>

      {showCreate && (
        <form
          className={localStyles.panel}
          onSubmit={(e) => {
            e.preventDefault()
            createMutation.mutate(createForm)
          }}
        >
          <h3>Create User Account</h3>
          <div className={localStyles.formRow}>
            <label>
              Email
              <input
                type="email"
                value={createForm.email}
                onChange={(e) => setCreateForm((f) => ({ ...f, email: e.target.value }))}
                required
              />
            </label>
            <label>
              Role
              <select
                value={createForm.role}
                onChange={(e) => setCreateForm((f) => ({ ...f, role: e.target.value }))}
              >
                {roles?.map((r) => (
                  <option key={r.value} value={r.value}>{r.label}</option>
                ))}
              </select>
            </label>
          </div>
          <div className={localStyles.formRow}>
            <label>
              First name
              <input
                value={createForm.firstName}
                onChange={(e) => setCreateForm((f) => ({ ...f, firstName: e.target.value }))}
                required
              />
            </label>
            <label>
              Last name
              <input
                value={createForm.lastName}
                onChange={(e) => setCreateForm((f) => ({ ...f, lastName: e.target.value }))}
                required
              />
            </label>
          </div>
          <label>
            Initial password
            <input
              type="password"
              value={createForm.password}
              onChange={(e) => setCreateForm((f) => ({ ...f, password: e.target.value }))}
              required
              minLength={8}
            />
          </label>
          <button type="submit" className={localStyles.primaryBtn} disabled={createMutation.isPending}>
            {createMutation.isPending ? 'Creating...' : 'Create User'}
          </button>
          {createMutation.isError && (
            <p className={localStyles.error}>Could not create user. Check email and password rules.</p>
          )}
        </form>
      )}

      {resetId && (
        <form
          className={localStyles.panel}
          onSubmit={(e) => {
            e.preventDefault()
            resetMutation.mutate({ id: resetId, password: resetPassword })
          }}
        >
          <h3>Reset Password</h3>
          <p className={localStyles.note}>
            Set a new password for {users?.find((u) => u.id === resetId)?.email}
          </p>
          <label>
            New password
            <input
              type="password"
              value={resetPassword}
              onChange={(e) => setResetPassword(e.target.value)}
              required
              minLength={8}
            />
          </label>
          <div className={localStyles.actions}>
            <button type="submit" className={localStyles.primaryBtn} disabled={resetMutation.isPending}>
              {resetMutation.isPending ? 'Saving...' : 'Reset Password'}
            </button>
            <button type="button" className={localStyles.secondaryBtn} onClick={() => setResetId(null)}>
              Cancel
            </button>
          </div>
          {resetMutation.isError && (
            <p className={localStyles.error}>
              {resetMutation.error instanceof Error ? resetMutation.error.message : 'Reset failed.'}
            </p>
          )}
          {resetMutation.isSuccess && <p className={localStyles.success}>Password reset successfully.</p>}
        </form>
      )}

      {editingId && (
        <form
          className={localStyles.panel}
          onSubmit={(e) => {
            e.preventDefault()
            updateMutation.mutate({ id: editingId, data: editForm })
          }}
        >
          <h3>Edit User</h3>
          <div className={localStyles.formRow}>
            <label>
              First name
              <input
                value={editForm.firstName}
                onChange={(e) => setEditForm((f) => ({ ...f, firstName: e.target.value }))}
                required
              />
            </label>
            <label>
              Last name
              <input
                value={editForm.lastName}
                onChange={(e) => setEditForm((f) => ({ ...f, lastName: e.target.value }))}
                required
              />
            </label>
          </div>
          <div className={localStyles.formRow}>
            <label>
              Role
              <select
                value={editForm.role}
                onChange={(e) => setEditForm((f) => ({ ...f, role: e.target.value }))}
              >
                {roles?.map((r) => (
                  <option key={r.value} value={r.value}>{r.label}</option>
                ))}
              </select>
            </label>
            <label className={localStyles.checkbox}>
              <input
                type="checkbox"
                checked={editForm.isActive}
                onChange={(e) => setEditForm((f) => ({ ...f, isActive: e.target.checked }))}
                disabled={editingId === currentUserId}
              />
              Active account
            </label>
          </div>
          <div className={localStyles.actions}>
            <button type="submit" className={localStyles.primaryBtn} disabled={updateMutation.isPending}>
              {updateMutation.isPending ? 'Saving...' : 'Save Changes'}
            </button>
            <button type="button" className={localStyles.secondaryBtn} onClick={() => setEditingId(null)}>
              Cancel
            </button>
          </div>
          {updateMutation.isError && <p className={localStyles.error}>Unable to update user.</p>}
        </form>
      )}

      <div className={styles.tableWrap}>
        {isLoading && <p className={styles.loading}>Loading users...</p>}
        {isError && <p className={styles.hint}>Access denied or API unavailable.</p>}
        <table className={styles.table}>
          <thead>
            <tr>
              <th>Name</th>
              <th>Email</th>
              <th>Role</th>
              <th>Status</th>
              <th>Last Login</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {users?.map((user) => (
              <tr key={user.id}>
                <td className={styles.name}>{user.firstName} {user.lastName}</td>
                <td>{user.email}</td>
                <td>{user.role}</td>
                <td>
                  <span className={user.isActive ? styles.badge : localStyles.inactiveBadge}>
                    {user.isActive ? 'Active' : 'Inactive'}
                  </span>
                </td>
                <td>{user.lastLoginAt ?? '—'}</td>
                <td>
                  <div className={localStyles.rowActions}>
                    <button type="button" onClick={() => startEdit(user)}>Edit</button>
                    <button type="button" onClick={() => { setResetId(user.id); setEditingId(null) }}>
                      Reset PW
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
