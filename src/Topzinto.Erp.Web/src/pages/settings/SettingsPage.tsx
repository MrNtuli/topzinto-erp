import { useState, useEffect } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { getCurrentUser, changePassword, updateProfile } from '@/api/auth'
import { getAuditLogs, getBackupHub, createBackup, downloadBackupFile, scanSystemAlerts } from '@/api/admin'
import { getCompanySettings, updateCompanySettings, type CompanySettings } from '@/api/company'
import { useAuthStore } from '@/stores/authStore'
import { canAccessNav } from '@/lib/roleAccess'
import styles from '../projects/ProjectsPage.module.css'
import localStyles from './SettingsPage.module.css'

export function SettingsPage() {
  const queryClient = useQueryClient()
  const updateUser = useAuthStore((s) => s.updateUser)
  const role = useAuthStore((s) => s.user?.role)
  const isAdmin = canAccessNav(role, '/admin/users')

  const { data: user, isLoading } = useQuery({
    queryKey: ['current-user'],
    queryFn: getCurrentUser,
    retry: false,
  })

  const [profileForm, setProfileForm] = useState({ firstName: '', lastName: '' })
  useEffect(() => {
    if (user) setProfileForm({ firstName: user.firstName, lastName: user.lastName })
  }, [user])

  const [passwordForm, setPasswordForm] = useState({
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
  })

  const { data: company } = useQuery({
    queryKey: ['company-settings'],
    queryFn: getCompanySettings,
    retry: false,
  })

  const [form, setForm] = useState<CompanySettings | null>(null)
  useEffect(() => {
    if (company) setForm(company)
  }, [company])

  const { data: auditLogs } = useQuery({
    queryKey: ['audit-logs'],
    queryFn: () => getAuditLogs(50),
    retry: false,
  })

  const { data: backupHub } = useQuery({
    queryKey: ['backups'],
    queryFn: getBackupHub,
    retry: false,
  })

  const backupMutation = useMutation({
    mutationFn: createBackup,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['backups'] }),
  })

  const companyMutation = useMutation({
    mutationFn: updateCompanySettings,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['company-settings'] }),
  })

  const profileMutation = useMutation({
    mutationFn: updateProfile,
    onSuccess: (updated) => {
      updateUser(updated)
      queryClient.setQueryData(['current-user'], updated)
    },
  })

  const passwordMutation = useMutation({
    mutationFn: changePassword,
    onSuccess: () => {
      setPasswordForm({ currentPassword: '', newPassword: '', confirmPassword: '' })
    },
  })

  const alertScanMutation = useMutation({
    mutationFn: scanSystemAlerts,
  })

  function updateField<K extends keyof CompanySettings>(key: K, value: CompanySettings[K]) {
    setForm((f) => f ? { ...f, [key]: value } : f)
  }

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>Home &gt; Settings</nav>
      <header className={styles.header}><h1>Settings</h1></header>

      <div className={localStyles.grid}>
        <div className={localStyles.card}>
          <h3>My Profile</h3>
          {isLoading ? <p>Loading...</p> : user ? (
            <>
              <dl className={localStyles.dl}>
                <dt>Email</dt><dd>{user.email}</dd>
                <dt>Role</dt><dd><span className={localStyles.roleBadge}>{user.role}</span></dd>
              </dl>
              <form
                className={localStyles.form}
                onSubmit={(e) => {
                  e.preventDefault()
                  profileMutation.mutate(profileForm)
                }}
              >
                <div className={localStyles.formRow}>
                  <label>
                    First name
                    <input
                      value={profileForm.firstName}
                      onChange={(e) => setProfileForm((f) => ({ ...f, firstName: e.target.value }))}
                      required
                    />
                  </label>
                  <label>
                    Last name
                    <input
                      value={profileForm.lastName}
                      onChange={(e) => setProfileForm((f) => ({ ...f, lastName: e.target.value }))}
                      required
                    />
                  </label>
                </div>
                <button type="submit" className={localStyles.saveBtn} disabled={profileMutation.isPending}>
                  {profileMutation.isPending ? 'Saving...' : 'Save Profile'}
                </button>
                {profileMutation.isSuccess && <p className={localStyles.success}>Profile updated.</p>}
                {profileMutation.isError && <p className={localStyles.error}>Unable to update profile.</p>}
              </form>
            </>
          ) : <p>Unable to load profile.</p>}
        </div>

        <div className={localStyles.card}>
          <h3>Change Password</h3>
          <p className={localStyles.note}>
            Minimum 8 characters with uppercase, lowercase, digit, and special character.
          </p>
          <form
            className={localStyles.form}
            onSubmit={(e) => {
              e.preventDefault()
              if (passwordForm.newPassword !== passwordForm.confirmPassword) return
              passwordMutation.mutate({
                currentPassword: passwordForm.currentPassword,
                newPassword: passwordForm.newPassword,
              })
            }}
          >
            <label>
              Current password
              <input
                type="password"
                value={passwordForm.currentPassword}
                onChange={(e) => setPasswordForm((f) => ({ ...f, currentPassword: e.target.value }))}
                required
                autoComplete="current-password"
              />
            </label>
            <div className={localStyles.formRow}>
              <label>
                New password
                <input
                  type="password"
                  value={passwordForm.newPassword}
                  onChange={(e) => setPasswordForm((f) => ({ ...f, newPassword: e.target.value }))}
                  required
                  minLength={8}
                  autoComplete="new-password"
                />
              </label>
              <label>
                Confirm new password
                <input
                  type="password"
                  value={passwordForm.confirmPassword}
                  onChange={(e) => setPasswordForm((f) => ({ ...f, confirmPassword: e.target.value }))}
                  required
                  minLength={8}
                  autoComplete="new-password"
                />
              </label>
            </div>
            {passwordForm.confirmPassword && passwordForm.newPassword !== passwordForm.confirmPassword && (
              <p className={localStyles.error}>Passwords do not match.</p>
            )}
            <button
              type="submit"
              className={localStyles.saveBtn}
              disabled={
                passwordMutation.isPending
                || passwordForm.newPassword !== passwordForm.confirmPassword
              }
            >
              {passwordMutation.isPending ? 'Updating...' : 'Update Password'}
            </button>
            {passwordMutation.isSuccess && <p className={localStyles.success}>Password updated successfully.</p>}
            {passwordMutation.isError && (
              <p className={localStyles.error}>
                {passwordMutation.error instanceof Error ? passwordMutation.error.message : 'Password change failed.'}
              </p>
            )}
          </form>
        </div>

        <div className={localStyles.card}>
          <h3>Database Backup</h3>
          <p className={localStyles.note}>
            Engine: <strong>{backupHub?.engine ?? '—'}</strong>
            {backupHub?.scheduleEnabled && (
              <> · Auto backup every {backupHub.intervalHours}h (keeps last {backupHub.retentionCount})</>
            )}
          </p>
          <button
            type="button"
            className={localStyles.backupBtn}
            onClick={() => backupMutation.mutate()}
            disabled={backupMutation.isPending}
          >
            {backupMutation.isPending ? 'Creating...' : 'Create Backup Now'}
          </button>
          {backupMutation.isSuccess && (
            <p className={localStyles.success}>Created: {backupMutation.data.fileName}</p>
          )}
          {backupMutation.isError && (
            <p className={localStyles.error}>Backup failed — ensure pg_dump is installed for PostgreSQL.</p>
          )}
          {backupHub && backupHub.files.length > 0 && (
            <ul className={localStyles.backupList}>
              {backupHub.files.slice(0, 5).map((b) => (
                <li key={b.fileName}>
                  <button type="button" onClick={() => downloadBackupFile(b.fileName).catch(() => {})}>
                    {b.fileName}
                  </button>
                  <span className={localStyles.backupMeta}>
                    {b.createdAt} · {(b.sizeBytes / 1024).toFixed(0)} KB
                  </span>
                </li>
              ))}
            </ul>
          )}
        </div>

        {form && (
          <div className={`${localStyles.card} ${localStyles.wide}`}>
            <h3>Company Settings</h3>
            <form
              className={localStyles.form}
              onSubmit={(e) => {
                e.preventDefault()
                companyMutation.mutate(form)
              }}
            >
              <div className={localStyles.formRow}>
                <label>Company name<input value={form.companyName} onChange={(e) => updateField('companyName', e.target.value)} /></label>
                <label>Tagline<input value={form.tagline} onChange={(e) => updateField('tagline', e.target.value)} /></label>
              </div>
              <div className={localStyles.formRow}>
                <label>Phone<input value={form.phone ?? ''} onChange={(e) => updateField('phone', e.target.value)} /></label>
                <label>Email<input value={form.email ?? ''} onChange={(e) => updateField('email', e.target.value)} /></label>
              </div>
              <div className={localStyles.formRow}>
                <label>VAT number<input value={form.vatNumber ?? ''} onChange={(e) => updateField('vatNumber', e.target.value)} /></label>
                <label>CIDB number<input value={form.cidbNumber ?? ''} onChange={(e) => updateField('cidbNumber', e.target.value)} /></label>
              </div>
              <label>Address<input value={form.address ?? ''} onChange={(e) => updateField('address', e.target.value)} /></label>
              <div className={localStyles.formRow}>
                <label>City<input value={form.city ?? ''} onChange={(e) => updateField('city', e.target.value)} /></label>
                <label>Province<input value={form.province ?? ''} onChange={(e) => updateField('province', e.target.value)} /></label>
              </div>
              <button type="submit" className={localStyles.saveBtn} disabled={companyMutation.isPending}>
                {companyMutation.isPending ? 'Saving...' : 'Save Company Settings'}
              </button>
              {companyMutation.isSuccess && <p className={localStyles.success}>Company settings saved.</p>}
            </form>
          </div>
        )}

        {isAdmin && (
          <div className={localStyles.card}>
            <h3>Administration</h3>
            <p className={localStyles.note}>Director-only tools for managing the platform.</p>
            <div className={localStyles.adminLinks}>
              <Link to="/admin/users" className={localStyles.link}>Manage Users</Link>
              <Link to="/admin/audit" className={localStyles.link}>Full Audit Log</Link>
              <button
                type="button"
                className={localStyles.scanBtn}
                onClick={() => alertScanMutation.mutate()}
                disabled={alertScanMutation.isPending}
              >
                {alertScanMutation.isPending ? 'Scanning...' : 'Scan System Alerts'}
              </button>
              {alertScanMutation.isSuccess && (
                <p className={localStyles.success}>{alertScanMutation.data.message}</p>
              )}
            </div>
          </div>
        )}

        <div className={`${localStyles.card} ${localStyles.wide}`}>
          <h3>Recent Audit Log</h3>
          <div className={styles.tableWrap}>
            <table className={styles.table}>
              <thead>
                <tr>
                  <th>Time</th>
                  <th>User</th>
                  <th>Action</th>
                  <th>Module</th>
                  <th>Entity</th>
                </tr>
              </thead>
              <tbody>
                {auditLogs?.map((log) => (
                  <tr key={log.id}>
                    <td>{log.createdAt}</td>
                    <td>{log.userEmail || '—'}</td>
                    <td>{log.action}</td>
                    <td>{log.module}</td>
                    <td>{log.entityType} {log.newValues ? `· ${log.newValues}` : ''}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <Link to="/admin/audit" className={localStyles.link}>View full audit log</Link>
        </div>
      </div>
    </div>
  )
}
