import { useState } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { resetPasswordWithToken } from '@/api/auth'
import styles from './LoginPage.module.css'

export function ResetPasswordPage() {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const emailParam = searchParams.get('email') ?? ''
  const tokenParam = searchParams.get('token') ?? ''

  const [email] = useState(emailParam)
  const [token] = useState(tokenParam)
  const [password, setPassword] = useState('')
  const [confirm, setConfirm] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const linkInvalid = !email || !token

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError('')
    if (password !== confirm) {
      setError('Passwords do not match.')
      return
    }
    setLoading(true)
    try {
      await resetPasswordWithToken({ email, token, newPassword: password })
      navigate('/login?reason=password-reset')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unable to reset password.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className={styles.page}>
      <div className={styles.hero}>
        <div className={styles.heroOverlay} />
        <div className={styles.brand}>
          <div className={styles.logoIcon}>T·P·Z</div>
          <div>
            <h1>TOPZINTO</h1>
            <p>Smart. Proficient. Value.</p>
          </div>
        </div>
      </div>

      <div className={styles.formSide}>
        <form className={styles.card} onSubmit={handleSubmit}>
          <h2>Set New Password</h2>
          <p className={styles.subtitle}>
            Choose a strong password for {email || 'your account'}.
          </p>

          {linkInvalid ? (
            <>
              <div className={styles.error}>This reset link is invalid or incomplete.</div>
              <Link to="/forgot-password" className={styles.backLink}>Request a new link</Link>
            </>
          ) : (
            <>
              {error && <div className={styles.error}>{error}</div>}
              <label className={styles.label}>
                New password
                <input
                  type="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                  minLength={8}
                  autoComplete="new-password"
                />
              </label>
              <label className={styles.label}>
                Confirm password
                <input
                  type="password"
                  value={confirm}
                  onChange={(e) => setConfirm(e.target.value)}
                  required
                  minLength={8}
                  autoComplete="new-password"
                />
              </label>
              <p className={styles.hint}>
                Minimum 8 characters with uppercase, lowercase, digit, and special character.
              </p>
              <button
                type="submit"
                className={styles.submit}
                disabled={loading || password !== confirm}
              >
                {loading ? 'Saving...' : 'Reset Password'}
              </button>
              <Link to="/login" className={styles.backLink}>Back to sign in</Link>
            </>
          )}
        </form>
      </div>
    </div>
  )
}
