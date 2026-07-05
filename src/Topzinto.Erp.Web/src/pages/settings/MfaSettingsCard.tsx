import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  beginMfaSetup,
  disableMfa,
  enableMfa,
  getMfaStatus,
} from '@/api/auth'
import localStyles from '../settings/SettingsPage.module.css'

export function MfaSettingsCard() {
  const queryClient = useQueryClient()
  const [code, setCode] = useState('')
  const [setup, setSetup] = useState<{ sharedKey: string; authenticatorUri: string } | null>(null)

  const { data: status } = useQuery({
    queryKey: ['mfa-status'],
    queryFn: getMfaStatus,
    retry: false,
  })

  const setupMutation = useMutation({
    mutationFn: beginMfaSetup,
    onSuccess: (data) => {
      setSetup(data)
      setCode('')
    },
  })

  const enableMutation = useMutation({
    mutationFn: () => enableMfa(code),
    onSuccess: () => {
      setSetup(null)
      setCode('')
      queryClient.invalidateQueries({ queryKey: ['mfa-status'] })
    },
  })

  const disableMutation = useMutation({
    mutationFn: () => disableMfa(code),
    onSuccess: () => {
      setCode('')
      queryClient.invalidateQueries({ queryKey: ['mfa-status'] })
    },
  })

  const enabled = status?.enabled ?? false

  return (
    <div className={localStyles.card}>
      <h3>Two-Factor Authentication (2FA)</h3>
      <p className={localStyles.note}>
        {enabled
          ? 'Your account is protected with an authenticator app (Google Authenticator, Microsoft Authenticator, etc.).'
          : 'Add an extra layer of security using a TOTP authenticator app.'}
      </p>

      {enabled ? (
        <form
          className={localStyles.form}
          onSubmit={(e) => {
            e.preventDefault()
            disableMutation.mutate()
          }}
        >
          <label>
            Enter current authenticator code to disable 2FA
            <input
              type="text"
              inputMode="numeric"
              maxLength={6}
              value={code}
              onChange={(e) => setCode(e.target.value.replace(/\D/g, ''))}
              required
            />
          </label>
          <button type="submit" className={localStyles.saveBtn} disabled={disableMutation.isPending || code.length < 6}>
            {disableMutation.isPending ? 'Disabling...' : 'Disable 2FA'}
          </button>
          {disableMutation.isSuccess && <p className={localStyles.success}>Two-factor authentication disabled.</p>}
          {disableMutation.isError && (
            <p className={localStyles.error}>
              {disableMutation.error instanceof Error ? disableMutation.error.message : 'Unable to disable 2FA.'}
            </p>
          )}
        </form>
      ) : (
        <>
          {!setup ? (
            <button
              type="button"
              className={localStyles.saveBtn}
              onClick={() => setupMutation.mutate()}
              disabled={setupMutation.isPending}
            >
              {setupMutation.isPending ? 'Preparing...' : 'Set Up Authenticator'}
            </button>
          ) : (
            <form
              className={localStyles.form}
              onSubmit={(e) => {
                e.preventDefault()
                enableMutation.mutate()
              }}
            >
              <img
                src={`https://api.qrserver.com/v1/create-qr-code/?size=180x180&data=${encodeURIComponent(setup.authenticatorUri)}`}
                alt="Authenticator QR code"
                width={180}
                height={180}
              />
              <p className={localStyles.note}>Manual key: <strong>{setup.sharedKey}</strong></p>
              <label>
                Verification code
                <input
                  type="text"
                  inputMode="numeric"
                  maxLength={6}
                  value={code}
                  onChange={(e) => setCode(e.target.value.replace(/\D/g, ''))}
                  required
                />
              </label>
              <button type="submit" className={localStyles.saveBtn} disabled={enableMutation.isPending || code.length < 6}>
                {enableMutation.isPending ? 'Enabling...' : 'Enable 2FA'}
              </button>
              {enableMutation.isSuccess && <p className={localStyles.success}>Two-factor authentication enabled.</p>}
              {enableMutation.isError && (
                <p className={localStyles.error}>
                  {enableMutation.error instanceof Error ? enableMutation.error.message : 'Invalid code.'}
                </p>
              )}
            </form>
          )}
        </>
      )}
    </div>
  )
}
