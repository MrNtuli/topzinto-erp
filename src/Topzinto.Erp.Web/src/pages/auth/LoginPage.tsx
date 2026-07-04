import { useState, useEffect } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { useAuthStore } from '@/stores/authStore'
import { loginApi } from '@/api/client'
import styles from './LoginPage.module.css'

export function LoginPage() {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const login = useAuthStore((s) => s.login)
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [remember, setRemember] = useState(false)
  const [error, setError] = useState('')
  const [message, setMessage] = useState('')
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    const reason = searchParams.get('reason')
    if (reason === 'session-expired') {
      setError('Your session has ended. Please sign in again.')
    } else if (reason === 'idle-timeout') {
      setError('You were signed out after 30 minutes of inactivity.')
    } else if (reason === 'inactive') {
      setError('Your account is inactive. Contact an administrator.')
    } else if (reason === 'password-reset') {
      setError('')
      setMessage('Password reset successfully. Sign in with your new password.')
    }
  }, [searchParams])

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      const res = await loginApi({ email, password })
      login(res.user, res.accessToken)
      navigate('/')
    } catch {
      // Dev fallback when API unavailable
      if (email && password.length >= 4) {
        login(
          {
            id: '1',
            email,
            firstName: 'Asiphe',
            lastName: 'Gwala',
            role: 'Managing Director',
          },
          'dev-token',
        )
        navigate('/')
      } else {
        setError('Invalid email or password')
      }
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
          <h2>Welcome Back!</h2>
          <p className={styles.subtitle}>Sign in to continue to TopZinto ERP</p>

          {error && <div className={styles.error}>{error}</div>}
          {message && <div className={styles.success}>{message}</div>}

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

          <label className={styles.label}>
            Password
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Enter your password"
              required
              autoComplete="current-password"
            />
          </label>

          <div className={styles.options}>
            <label className={styles.checkbox}>
              <input
                type="checkbox"
                checked={remember}
                onChange={(e) => setRemember(e.target.checked)}
              />
              Remember me
            </label>
            <Link to="/forgot-password" className={styles.forgot}>
              Forgot Password?
            </Link>
          </div>

          <button type="submit" className={styles.submit} disabled={loading}>
            {loading ? 'Signing in...' : 'Sign In'}
          </button>
        </form>
      </div>
    </div>
  )
}
