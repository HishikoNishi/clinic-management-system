export const toLocalDateInputValue = (date = new Date()): string => {
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

export const addDaysToLocalDateInputValue = (days: number, baseDate = new Date()): string => {
  const nextDate = new Date(baseDate)
  nextDate.setDate(nextDate.getDate() + days)
  return toLocalDateInputValue(nextDate)
}
