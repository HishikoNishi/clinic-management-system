
import api from "./api"

export const getStaffs = () => api.get("/Staffs")

export const getStaffById = (id: string) =>
  api.get(`/Staffs/${id}`)

export const createStaff = (data: {
  userId: string
  code: string
  fullName: string
  role: string
}) =>
  api.post("/Staffs", data)

export const updateStaff = (
  id: string,
  data: {
    userId: string
    code: string
    fullName: string
    role: string
    isActive: boolean
  }
) =>
  api.put(`/Staffs/${id}`, data)

export const deleteStaff = (id: string) =>
  api.delete(`/Staffs/${id}`)
