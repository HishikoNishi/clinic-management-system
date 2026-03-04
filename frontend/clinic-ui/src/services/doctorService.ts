import api from "./api"
export interface Doctor {
  id: string
  code: string
  specialty: string
  licenseNumber?: string
  status: number
  userId: string
}

export interface CreateDoctorDto {
  username: string
  password: string
  code: string
  specialty: string
  licenseNumber?: string
}

export interface UpdateDoctorDto {
  code: string
  specialty: string
  licenseNumber?: string
  status: number
}

/* ========================= */

export const doctorService = {

  async getAll(): Promise<Doctor[]> {
    const res = await api.get("/Doctor")
    return res.data
  },

  async create(data: CreateDoctorDto) {
    return api.post("/Doctor", data)
  },

  async update(id: string, data: UpdateDoctorDto) {
    return api.put(`/Doctor/${id}`, data)
  },

  async delete(id: string) {
    return api.delete(`/Doctor/${id}`)
  }
}