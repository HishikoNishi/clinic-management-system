const DEPARTMENT_NAME_MAP: Record<string, string> = {
  'general medicine': 'Nội tổng quát',
  surgery: 'Ngoại khoa',
  pediatrics: 'Nhi khoa',
  ent: 'Tai Mũi Họng',
  obstetrics: 'Sản phụ khoa',
  diagnostics: 'Cận lâm sàng'
}

export function toVietnameseDepartmentName(name: string | null | undefined): string {
  const raw = (name || '').trim()
  if (!raw) return ''

  const mapped = DEPARTMENT_NAME_MAP[raw.toLowerCase()]
  return mapped || raw
}
