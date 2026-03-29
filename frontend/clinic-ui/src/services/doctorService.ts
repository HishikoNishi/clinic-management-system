import api from "./api"

/* ========================= */

export interface Doctor {
  id: string
  userId: string
  code: string
  fullName: string
  specialtyId: string
  specialtyName: string
  departmentId: string
  departmentName: string
  licenseNumber?: string
  status: number
  avatarUrl?: string
  username?: string
}

export interface CreateDoctorDto {
  userId: string
  fullName: string
  code: string
  specialtyId: string
  licenseNumber?: string
  departmentId: string
  avatarUrl?: string
}

export interface UpdateDoctorDto {
  code: string
  fullName: string
  specialtyId: string
  licenseNumber?: string
  departmentId: string
  status: string
  avatarUrl?: string
}

/* ========================= */

// 🔥 đặt ngoài
const statusMap: any = {
  Active: 0,
  Busy: 1,
  Inactive: 2,
  Deleted: 3
}

export const doctorService = {

  async getAll(): Promise<Doctor[]> {
    const res = await api.get("/Doctor")
    return res.data
  },

  async create(data: CreateDoctorDto & { status: string }) {
    return api.post("/Doctor", {
      userId: data.userId,
      fullName: data.fullName,
      code: data.code,
      specialtyId: data.specialtyId,
      licenseNumber: data.licenseNumber,
      departmentId: data.departmentId,
      avatarUrl: data.avatarUrl,
      status: statusMap[data.status] // ✅ FIX
    })
  },

  async update(id: string, data: UpdateDoctorDto) {
    return api.put(`/Doctor/${id}`, {
      ...data,
      status: statusMap[data.status] // ✅ FIX
    })
  },

  async delete(id: string) {
    return api.delete(`/Doctor/${id}`)
  }
}