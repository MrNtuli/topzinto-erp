import { useState } from 'react'
import { Link } from 'react-router-dom'
import { requestPasswordReset } from '@/api/auth'
import styles from './LoginPage.module.css'

export function ForgotPasswordPage() {
  const [email, setEmail] = useState('')
  const [message, setMessage] = useState('')
  const [devLink, setDevLink] = useState<string | null>(null)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError('')
    setMessage('')
    setDevLink(null)
    setLoading(true)
    try {
      const result = await requestPasswordReset(email)
      setMessage(result.message)
      if (result.devResetLink) setDevLink(result.devResetLink)
    } catch {
      setError('Unable to process request. Try again later.')
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
          <h2>Forgot Password</h2>
          <p className={styles.subtitle}>
            Enter your work email and we will send reset instructions if an account exists.
          </p>

          {error && <div className={styles.error}>{error}</div>}
          {message && <div className={styles.success}>{message}</div>}

          {!message && (
            <>
              <label className={styles.label}>
                Email Address
                <input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="youremail@topzinto.com"
                  required
                  autoComplete="email"
                />
              </label>
              <button type="submit" className={styles.submit} disabled={loading}>
                {loading ? 'Sending...' : 'Send Reset Link'}
              </button>
            </>
          )}

          {devLink && (
            <p className={styles.devLink}>
              Dev mode (SMTP off):{' '}
              <a href={devLink}>Open reset link</a>
            </p>
          )}

          <Link to="/login" className={styles.backLink}>Back to sign in</Link>
        </form>
      </div>
    </div>
  )
}
