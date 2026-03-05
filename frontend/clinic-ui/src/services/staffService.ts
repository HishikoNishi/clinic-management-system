import axios from "axios"

const API_URL = "https://localhost:7235/api/Staffs"

const api = axios.create({
  baseURL: API_URL,
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token")
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

export const getStaffs = () => api.get("")

export const deleteStaff = (id: number) => api.delete(`/${id}`)