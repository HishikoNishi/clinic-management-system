import api from "./api"

export interface UserDto {
  id: string
  username: string
}

export const getUsers = () => api.get<UserDto[]>("/Admin")