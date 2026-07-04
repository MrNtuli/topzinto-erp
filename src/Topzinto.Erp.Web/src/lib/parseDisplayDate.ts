/** Converts API display dates (yyyy-MM-dd or "dd MMM yyyy") to HTML date input value. */
export function parseDisplayDate(value: string | null | undefined): string {
  if (!value) return ''
  if (/^\d{4}-\d{2}-\d{2}/.test(value)) return value.slice(0, 10)
  const d = new Date(value)
  if (!Number.isNaN(d.getTime())) return d.toISOString().slice(0, 10)
  return ''
}
