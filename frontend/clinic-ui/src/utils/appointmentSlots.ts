export interface BasicAppointmentSlot {
  id: string
  startTime: string
  slotLabel: string
  isBooked?: boolean
}

export function buildBusinessHourSlots(date?: string) {
  const slots: BasicAppointmentSlot[] = []
  const now = new Date()
  const isToday = !!date && new Date(date).toDateString() === now.toDateString()

  for (let hour = 7; hour <= 22; hour += 1) {
    for (const minute of [0, 30]) {
      if (hour === 22 && minute > 0) continue

      const hh = String(hour).padStart(2, '0')
      const mm = String(minute).padStart(2, '0')
      const startTime = `${hh}:${mm}:00`

      if (isToday) {
        const slotDate = new Date(now)
        slotDate.setHours(hour, minute, 0, 0)
        if (slotDate <= now) continue
      }

      slots.push({
        id: `${hh}${mm}`,
        startTime,
        slotLabel: `${hh}:${mm}`,
        isBooked: false
      })
    }
  }

  return slots
}
