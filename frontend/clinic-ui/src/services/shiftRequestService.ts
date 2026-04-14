import api from './api'

export type ShiftRequestType = 'EmergencyLeave' | 'ShiftTransfer'
export type ShiftRequestStatus = 'Pending' | 'Approved' | 'Rejected'

export interface ShiftRequestAppointment {
  appointmentId: string
  appointmentCode: string
  patientName: string
  phone?: string | null
  status: string
}

export interface ShiftRequestAvailableDoctor {
  doctorId: string
  doctorName: string
  doctorCode: string
  specialtyName: string
  departmentName: string
}

export interface ShiftRequestItem {
  id: string
  doctorId: string
  doctorName: string
  requestType: ShiftRequestType
  status: ShiftRequestStatus
  workDate: string
  shiftCode: string
  slotLabel: string
  startTime: string
  endTime: string
  roomId?: string | null
  roomCode?: string | null
  roomName?: string | null
  reason: string
  preferredDoctorId?: string | null
  preferredDoctorName?: string | null
  replacementDoctorId?: string | null
  replacementDoctorName?: string | null
  adminNote?: string | null
  reviewedAt?: string | null
  createdAt: string
  appointmentCount: number
  appointments: ShiftRequestAppointment[]
  availableDoctors: ShiftRequestAvailableDoctor[]
}

export interface DoctorNotification {
  id: string
  title: string
  message: string
  type: string
  isRead: boolean
  link?: string | null
  createdAt: string
}

export interface DoctorNotificationsResponse {
  unreadCount: number
  items: DoctorNotification[]
}

const toApiTimeSpan = (value: string) => {
  const normalized = String(value || '').trim()
  if (!normalized) return normalized
  if (/^\d{2}:\d{2}:\d{2}$/.test(normalized)) return normalized
  if (/^\d{2}:\d{2}$/.test(normalized)) return `${normalized}:00`
  return normalized
}

export const shiftRequestService = {
  async getMyRequests(status?: ShiftRequestStatus): Promise<ShiftRequestItem[]> {
    const response = await api.get('/doctor/shift-requests/my', {
      params: status ? { status } : undefined
    })
    return response.data ?? []
  },

  async getMyPendingCount(): Promise<number> {
    const response = await api.get('/doctor/shift-requests/pending-count')
    return response.data?.count ?? 0
  },

  async createRequest(payload: {
    requestType: ShiftRequestType
    workDate: string
    startTime: string
    reason: string
    preferredDoctorId?: string | null
  }) {
    return api.post('/doctor/shift-requests', {
      ...payload,
      startTime: toApiTimeSpan(payload.startTime)
    })
  },

  async getDoctorNotifications(take = 10): Promise<DoctorNotificationsResponse> {
    const response = await api.get('/doctor/notifications', {
      params: { take }
    })
    return response.data
  },

  async markNotificationAsRead(id: string) {
    return api.post(`/doctor/notifications/${id}/read`)
  },

  async getAdminRequests(status?: ShiftRequestStatus, take?: number): Promise<ShiftRequestItem[]> {
    const response = await api.get('/admin/shift-requests', {
      params: {
        ...(status ? { status } : {}),
        ...(take ? { take } : {})
      }
    })
    return response.data ?? []
  },

  async getAdminPendingCount(): Promise<number> {
    const response = await api.get('/admin/shift-requests/pending-count')
    return response.data?.count ?? 0
  },

  async getAdminRequestDetail(id: string): Promise<ShiftRequestItem> {
    const response = await api.get(`/admin/shift-requests/${id}`)
    return response.data
  },

  async approveRequest(id: string, payload: { replacementDoctorId: string; adminNote?: string }) {
    return api.post(`/admin/shift-requests/${id}/approve`, payload)
  },

  async rejectRequest(id: string, adminNote: string) {
    return api.post(`/admin/shift-requests/${id}/reject`, { adminNote })
  }
}
