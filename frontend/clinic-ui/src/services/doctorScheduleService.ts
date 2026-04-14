import api from './api'

export interface DoctorScheduleSlotPayload {
  shiftCode: string
  slotLabel: string
  startTime: string
  endTime: string
  roomId: string
  isActive: boolean
}

export interface DoctorWorkSummary {
  referenceDate: string
  selectedDateSlots: number
  selectedDateMinutes: number
  todaySlots: number
  todayMinutes: number
  currentMonthSlots: number
  currentMonthMinutes: number
  currentYearSlots: number
  currentYearMinutes: number
}

export interface DoctorScheduleSlot {
  id: string
  shiftCode: string
  slotLabel: string
  startTime: string
  endTime: string
  roomId?: string | null
  roomCode?: string | null
  roomName?: string | null
  isBooked: boolean
}

export interface DoctorScheduleImpactAppointment {
  appointmentId: string
  appointmentCode: string
  patientName: string
  phone?: string | null
  status: string
}

export interface DoctorScheduleSlotImpact {
  doctorId: string
  doctorName: string
  workDate: string
  shiftCode: string
  slotLabel: string
  startTime: string
  endTime: string
  roomId?: string | null
  roomCode?: string | null
  roomName?: string | null
  appointmentCount: number
  hasAppointments: boolean
  appointments: DoctorScheduleImpactAppointment[]
}

export interface AvailableDoctorSlot {
  doctorId: string
  doctorName: string
  doctorCode: string
  departmentId: string
  departmentName: string
  specialtyId: string
  specialtyName: string
}

export const doctorScheduleService = {
  async getDoctorDay(doctorId: string, date: string): Promise<DoctorScheduleSlot[]> {
    const response = await api.get(`/DoctorSchedules/doctors/${doctorId}`, {
      params: { date }
    })
    return response.data ?? []
  },

  async getWeeklyTemplate(doctorId: string, dayOfWeek: number): Promise<DoctorScheduleSlot[]> {
    const response = await api.get(`/DoctorSchedules/doctors/${doctorId}/weekly-template`, {
      params: { dayOfWeek }
    })
    return response.data ?? []
  },

  async getAvailableSlots(doctorId: string, date: string): Promise<DoctorScheduleSlot[]> {
    const response = await api.get(`/DoctorSchedules/doctors/${doctorId}/available-slots`, {
      params: { date }
    })
    return response.data ?? []
  },

  async getWorkSummary(doctorId: string, date: string): Promise<DoctorWorkSummary> {
    const response = await api.get(`/DoctorSchedules/doctors/${doctorId}/work-summary`, {
      params: { date }
    })
    return response.data
  },

  async saveDoctorDay(doctorId: string, workDate: string, slots: DoctorScheduleSlotPayload[]) {
    return api.put(`/DoctorSchedules/doctors/${doctorId}/day`, {
      workDate,
      slots
    })
  },

  async saveWeeklyTemplate(doctorId: string, dayOfWeek: number, slots: DoctorScheduleSlotPayload[]) {
    return api.put(`/DoctorSchedules/doctors/${doctorId}/weekly-template`, {
      dayOfWeek,
      slots
    })
  },

  async getSlotImpact(doctorId: string, date: string, startTime: string): Promise<DoctorScheduleSlotImpact> {
    const response = await api.get(`/DoctorSchedules/doctors/${doctorId}/slot-impact`, {
      params: { date, startTime }
    })
    return response.data
  },

  async getAvailableDoctors(fromDoctorId: string, date: string, startTime: string): Promise<AvailableDoctorSlot[]> {
    const response = await api.get('/DoctorSchedules/available-doctors', {
      params: { fromDoctorId, date, startTime }
    })
    return response.data ?? []
  },

  async reassignSlot(payload: {
    fromDoctorId: string
    toDoctorId: string
    workDate: string
    startTime: string
    moveAppointments: boolean
  }) {
    return api.post('/DoctorSchedules/reassign-slot', payload)
  },

  async deleteSchedule(id: string) {
    return api.delete(`/DoctorSchedules/${id}`)
  }
}
