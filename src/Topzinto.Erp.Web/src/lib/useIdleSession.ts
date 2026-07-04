import { useEffect, useRef, useState } from 'react'
import { clearSessionAndRedirect } from '@/api/client'

const IDLE_MS = 30 * 60 * 1000
const WARN_BEFORE_MS = 2 * 60 * 1000

const ACTIVITY_EVENTS = ['mousemove', 'mousedown', 'keydown', 'scroll', 'touchstart', 'click'] as const

export function useIdleSession(enabled: boolean) {
  const [showWarning, setShowWarning] = useState(false)
  const idleTimer = useRef<ReturnType<typeof setTimeout> | undefined>(undefined)
  const warnTimer = useRef<ReturnType<typeof setTimeout> | undefined>(undefined)

  useEffect(() => {
    if (!enabled) return

    function clearTimers() {
      if (idleTimer.current) clearTimeout(idleTimer.current)
      if (warnTimer.current) clearTimeout(warnTimer.current)
    }

    function resetTimers() {
      clearTimers()
      setShowWarning(false)

      warnTimer.current = setTimeout(() => setShowWarning(true), IDLE_MS - WARN_BEFORE_MS)
      idleTimer.current = setTimeout(() => clearSessionAndRedirect('idle-timeout'), IDLE_MS)
    }

    for (const event of ACTIVITY_EVENTS) {
      window.addEventListener(event, resetTimers, { passive: true })
    }
    resetTimers()

    return () => {
      for (const event of ACTIVITY_EVENTS) {
        window.removeEventListener(event, resetTimers)
      }
      clearTimers()
    }
  }, [enabled])

  return { showWarning, warnMinutes: WARN_BEFORE_MS / 60_000 }
}
