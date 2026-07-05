import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getMyProfile, updateMyProfile } from '@/api/users'
import { useAuthStore } from '@/stores/authStore'
import styles from '../projects/ProjectsPage.module.css'
import localStyles from './ProfilePage.module.css'

export function ProfilePage() {
  const queryClient = useQueryClient()
  const updateUser = useAuthStore((s) => s.updateUser)

  const { data: profile, isLoading, isError } = useQuery({
    queryKey: ['my-profile'],
    queryFn: getMyProfile,
    retry: false,
  })

  const [form, setForm] = useState({ firstName: '', lastName: '', phone: '' })

  useEffect(() => {
    if (profile) {
      setForm({
        firstName: profile.firstName,
        lastName: profile.lastName,
        phone: profile.phone ?? '',
      })
    }
  }, [profile])

  const mutation = useMutation({
    mutationFn: updateMyProfile,
    onSuccess: (updated) => {
      updateUser({
        id: updated.id,
        email: updated.email,
        firstName: updated.firstName,
        lastName: updated.lastName,
        role: updated.role,
      })
      queryClient.setQueryData(['my-profile'], updated)
    },
  })

  function updateField<K extends keyof typeof form>(key: K, value: string) {
    if (mutation.isSuccess || mutation.isError) mutation.reset()
    setForm((f) => ({ ...f, [key]: value }))
  }

  return (
    <div className={styles.page}>
      <nav className={styles.breadcrumb}>
        <Link to="/">Home</Link> &gt; My Profile
      </nav>
      <header className={styles.header}>
        <h1>My Profile</h1>
        <p className={localStyles.subtitle}>View and update your account details.</p>
      </header>

      <div className={localStyles.card}>
        {isLoading ? (
          <p className={localStyles.muted}>Loading profile...</p>
        ) : isError || !profile ? (
          <p className={localStyles.error}>Unable to load profile. Please try again.</p>
        ) : (
          <>
            <dl className={localStyles.dl}>
              <dt>Email</dt>
              <dd>{profile.email}</dd>
              <dt>Role</dt>
              <dd>
                <span className={localStyles.roleBadge}>{profile.role}</span>
              </dd>
              {profile.lastLoginAt && (
                <>
                  <dt>Last login</dt>
                  <dd>{profile.lastLoginAt}</dd>
                </>
              )}
            </dl>

            <form
              className={localStyles.form}
              onSubmit={(e) => {
                e.preventDefault()
                mutation.mutate({
                  firstName: form.firstName.trim(),
                  lastName: form.lastName.trim(),
                  phone: form.phone.trim() || null,
                })
              }}
            >
              <div className={localStyles.formRow}>
                <label>
                  First name
                  <input
                    value={form.firstName}
                    onChange={(e) => updateField('firstName', e.target.value)}
                    required
                    autoComplete="given-name"
                  />
                </label>
                <label>
                  Last name
                  <input
                    value={form.lastName}
                    onChange={(e) => updateField('lastName', e.target.value)}
                    required
                    autoComplete="family-name"
                  />
                </label>
              </div>
              <label>
                Phone
                <input
                  type="tel"
                  value={form.phone}
                  onChange={(e) => updateField('phone', e.target.value)}
                  placeholder="+27 ..."
                  autoComplete="tel"
                />
              </label>
              <div className={localStyles.actions}>
                <button type="submit" className={localStyles.saveBtn} disabled={mutation.isPending}>
                  {mutation.isPending ? 'Saving...' : 'Save Changes'}
                </button>
                <Link to="/settings" className={localStyles.link}>
                  Change password in Settings
                </Link>
              </div>
              {mutation.isSuccess && <p className={localStyles.success}>Profile updated successfully.</p>}
              {mutation.isError && <p className={localStyles.error}>Unable to update profile.</p>}
            </form>
          </>
        )}
      </div>
    </div>
  )
}
