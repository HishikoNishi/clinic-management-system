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
   status: 'Active' | 'Busy' | 'Inactive' | 'Deleted'   // 🔥 đổi từ number sang string union
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
    status: 'Active' | 'Busy' | 'Inactive' | 'Deleted'   // 🔥 đổi từ number sang string union
  avatarUrl?: string
}

/* ========================= */


export const doctorService = {

  async getAll(): Promise<Doctor[]> {
    const res = await api.get("/Doctor")
    return res.data
  },

  async create(data: CreateDoctorDto) {
  return api.post("/Doctor", {
    userId: data.userId,
    fullName: data.fullName,
    code: data.code,
    specialtyId: data.specialtyId,
    licenseNumber: data.licenseNumber,
    departmentId: data.departmentId,
    avatarUrl: data.avatarUrl
  })
},

  async update(id: string, data: UpdateDoctorDto) {
  return api.put(`/Doctor/${id}`, {
    code: data.code,
    fullName: data.fullName,
    specialtyId: data.specialtyId,
    licenseNumber: data.licenseNumber,
    departmentId: data.departmentId,
    avatarUrl: data.avatarUrl

  })
},

  async delete(id: string) {
    return api.delete(`/Doctor/${id}`)
  }
}