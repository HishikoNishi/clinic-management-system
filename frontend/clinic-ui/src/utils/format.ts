export const formatCurrency = (value?: number | null): string =>
  value !== undefined && value !== null
    ? value.toLocaleString('vi-VN')
    : '—'

export const formatDateTime = (value?: string | null): string =>
  value ? new Date(value).toLocaleString() : '—'
