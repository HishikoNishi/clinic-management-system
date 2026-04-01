import api from "./api"

export interface UserDto {
  id: string
  username: string
  role?: string
}

// 👉 thêm type cho role (optional nhưng nên có)
export type RoleDto = string

// ================= USER =================
export const getUsers = () => api.get<UserDto[]>("/Admin")

export const createUser = (payload: {
  username: string
  password: string
  role: string
}) => api.post("/Admin", payload)

export const updateUser = (
  id: string,
  payload: Partial<{ username: string; password: string; role: string }>
) => api.put(`/Admin/${id}`, payload)

export const deleteUser = (id: string) => api.delete(`/Admin/${id}`)

// ================= ROLE =================
// 👉 thêm cái này
export const getRoles = () => api.get<RoleDto[]>("/Admin/roles")