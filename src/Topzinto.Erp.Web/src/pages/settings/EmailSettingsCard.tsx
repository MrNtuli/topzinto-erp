import { useEffect, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  getEmailSettings,
  sendTestEmail,
  updateEmailSettings,
  type EmailSettings,
  type UpdateEmailSettingsRequest,
} from '@/api/admin'
import localStyles from './SettingsPage.module.css'

const defaultForm: UpdateEmailSettingsRequest = {
  smtpEnabled: false,
  smtpHost: '',
  smtpPort: 587,
  smtpUseSsl: true,
  smtpUsername: '',
  smtpPassword: '',
  emailFromAddress: '',
  emailFromName: 'TopZinto ERP',
}

function toForm(settings: EmailSettings): UpdateEmailSettingsRequest {
  return {
    smtpEnabled: settings.smtpEnabled,
    smtpHost: settings.smtpHost ?? '',
    smtpPort: settings.smtpPort ?? 587,
    smtpUseSsl: settings.smtpUseSsl,
    smtpUsername: settings.smtpUsername ?? '',
    smtpPassword: '',
    emailFromAddress: settings.emailFromAddress ?? '',
    emailFromName: settings.emailFromName ?? 'TopZinto ERP',
  }
}

export function EmailSettingsCard() {
  const queryClient = useQueryClient()
  const [form, setForm] = useState<UpdateEmailSettingsRequest>(defaultForm)
  const [testEmail, setTestEmail] = useState('')

  const { data: settings, isLoading } = useQuery({
    queryKey: ['email-settings'],
    queryFn: getEmailSettings,
    retry: false,
  })

  useEffect(() => {
    if (settings) setForm(toForm(settings))
  }, [settings])

  const saveMutation = useMutation({
    mutationFn: () => updateEmailSettings({
      ...form,
      smtpHost: form.smtpHost?.trim() || null,
      smtpUsername: form.smtpUsername?.trim() || null,
      smtpPassword: form.smtpPassword?.trim() ? form.smtpPassword : null,
      emailFromAddress: form.emailFromAddress?.trim() || null,
      emailFromName: form.emailFromName?.trim() || null,
    }),
    onSuccess: (data) => {
      setForm(toForm(data))
      queryClient.invalidateQueries({ queryKey: ['email-settings'] })
    },
  })

  const testMutation = useMutation({
    mutationFn: () => sendTestEmail(testEmail.trim()),
  })

  function updateField<K extends keyof UpdateEmailSettingsRequest>(
    key: K,
    value: UpdateEmailSettingsRequest[K],
  ) {
    if (saveMutation.isSuccess || saveMutation.isError) saveMutation.reset()
    setForm((f) => ({ ...f, [key]: value }))
  }

  if (isLoading) {
    return (
      <div className={`${localStyles.card} ${localStyles.wide}`}>
        <h3>Email / SMTP Configuration</h3>
        <p className={localStyles.note}>Loading email settings...</p>
      </div>
    )
  }

  return (
    <div className={`${localStyles.card} ${localStyles.wide}`}>
      <h3>Email / SMTP Configuration</h3>
      <p className={localStyles.note}>
        Configure outbound email for password resets, notifications, and @mentions.
        When disabled, dev mode shows reset links in the API response and logs.
      </p>

      <form
        className={localStyles.form}
        onSubmit={(e) => {
          e.preventDefault()
          saveMutation.mutate()
        }}
      >
        <label className={localStyles.checkboxLabel}>
          <input
            type="checkbox"
            checked={form.smtpEnabled}
            onChange={(e) => updateField('smtpEnabled', e.target.checked)}
          />
          Enable SMTP email delivery
        </label>

        <div className={localStyles.formRow}>
          <label>
            SMTP host
            <input
              value={form.smtpHost ?? ''}
              onChange={(e) => updateField('smtpHost', e.target.value)}
              placeholder="smtp.example.com"
              disabled={!form.smtpEnabled}
            />
          </label>
          <label>
            Port
            <input
              type="number"
              min={1}
              max={65535}
              value={form.smtpPort}
              onChange={(e) => updateField('smtpPort', Number(e.target.value) || 587)}
              disabled={!form.smtpEnabled}
            />
          </label>
        </div>

        <label className={localStyles.checkboxLabel}>
          <input
            type="checkbox"
            checked={form.smtpUseSsl}
            onChange={(e) => updateField('smtpUseSsl', e.target.checked)}
            disabled={!form.smtpEnabled}
          />
          Use SSL/TLS
        </label>

        <div className={localStyles.formRow}>
          <label>
            Username
            <input
              value={form.smtpUsername ?? ''}
              onChange={(e) => updateField('smtpUsername', e.target.value)}
              autoComplete="off"
              disabled={!form.smtpEnabled}
            />
          </label>
          <label>
            Password
            <input
              type="password"
              value={form.smtpPassword ?? ''}
              onChange={(e) => updateField('smtpPassword', e.target.value)}
              placeholder={settings?.hasPassword ? 'Leave blank to keep current password' : 'SMTP password'}
              autoComplete="new-password"
              disabled={!form.smtpEnabled}
            />
          </label>
        </div>

        <div className={localStyles.formRow}>
          <label>
            From address
            <input
              type="email"
              value={form.emailFromAddress ?? ''}
              onChange={(e) => updateField('emailFromAddress', e.target.value)}
              placeholder="noreply@topzinto.com"
              disabled={!form.smtpEnabled}
            />
          </label>
          <label>
            From name
            <input
              value={form.emailFromName ?? ''}
              onChange={(e) => updateField('emailFromName', e.target.value)}
              disabled={!form.smtpEnabled}
            />
          </label>
        </div>

        <button type="submit" className={localStyles.saveBtn} disabled={saveMutation.isPending}>
          {saveMutation.isPending ? 'Saving...' : 'Save Email Settings'}
        </button>
        {saveMutation.isSuccess && <p className={localStyles.success}>Email settings saved.</p>}
        {saveMutation.isError && (
          <p className={localStyles.error}>
            {saveMutation.error instanceof Error ? saveMutation.error.message : 'Unable to save email settings.'}
          </p>
        )}
      </form>

      <form
        className={localStyles.form}
        onSubmit={(e) => {
          e.preventDefault()
          testMutation.mutate()
        }}
      >
        <label>
          Send test email to
          <input
            type="email"
            value={testEmail}
            onChange={(e) => setTestEmail(e.target.value)}
            placeholder="admin@topzinto.com"
            required
          />
        </label>
        <button type="submit" className={localStyles.scanBtn} disabled={testMutation.isPending || !testEmail.trim()}>
          {testMutation.isPending ? 'Sending...' : 'Send Test Email'}
        </button>
        {testMutation.isSuccess && <p className={localStyles.success}>{testMutation.data.message}</p>}
        {testMutation.isError && (
          <p className={localStyles.error}>
            {testMutation.error instanceof Error ? testMutation.error.message : 'Test email failed.'}
          </p>
        )}
      </form>
    </div>
  )
}
