import api from '@/services/api'

export interface RoomOption {
  id: string
  code: string
  name: string
  departmentId: string
  departmentName: string
  doctorId?: string | null
  doctorName?: string | null
}

export interface QueueItem {
  id: string
  appointmentId: string
  appointmentCode: string
  patientCode?: string | null
  fullName: string
  phone?: string | null
  doctorId?: string | null
  doctorName?: string | null
  roomId: string
  roomCode: string
  roomName: string
  departmentName: string
  queueNumber: number
  status: string
  isPriority: boolean
  queuedAt: string
  calledAt?: string | null
  appointmentDate: string
  appointmentTime: string
}

export interface RoomQueueState {
  roomId: string
  roomCode: string
  roomName: string
  departmentId: string
  departmentName: string
  waitingCount: number
  inProgressCount: number
  currentCalling?: QueueItem | null
  items: QueueItem[]
}

export interface DoctorRoomSummary {
  roomId: string
  roomCode: string
  roomName: string
  departmentId: string
  departmentName: string
  waitingCount: number
  inProgressCount: number
  totalToday: number
}

export interface PatientQueueStatus {
  appointmentCode: string
  appointmentStatus: string
  ownQueue?: QueueItem | null
  currentCalling?: QueueItem | null
  waitingAhead: number
  message?: string | null
}

export const queueService = {
  async getRooms(departmentId?: string) {
    const { data } = await api.get<RoomOption[]>('/RoomQueues/rooms', {
      params: departmentId ? { departmentId } : undefined
    })
    return data ?? []
  },

  async checkIn(appointmentId: string, roomId?: string | null) {
    const { data } = await api.post<QueueItem>('/RoomQueues/check-in', {
      appointmentId,
      roomId: roomId || null
    })
    return data
  },

  async getRoomQueue(roomId: string) {
    const { data } = await api.get<RoomQueueState>(`/RoomQueues/rooms/${roomId}`)
    return data
  },

  async getDoctorRooms() {
    const { data } = await api.get<DoctorRoomSummary[]>('/RoomQueues/doctor/rooms')
    return data ?? []
  },

  async callNext(roomId: string) {
    const { data } = await api.post<QueueItem>(`/RoomQueues/rooms/${roomId}/next`)
    return data
  },

  async markDone(queueId: string) {
    const { data } = await api.post<QueueItem>(`/RoomQueues/${queueId}/done`)
    return data
  },

  async markSkipped(queueId: string) {
    const { data } = await api.post<QueueItem>(`/RoomQueues/${queueId}/skip`)
    return data
  },

  async getPatientStatus(appointmentCode: string) {
    const { data } = await api.get<PatientQueueStatus>(`/RoomQueues/patient/${appointmentCode}`)
    return data
  }
}
