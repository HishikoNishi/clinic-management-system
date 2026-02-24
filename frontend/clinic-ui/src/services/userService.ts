import api from './api'

export interface User {
  id: string
  username: string
  roleId: string
}

export const getUsers = async (): Promise<User[]> => {
  const response = await api.get('/users')
  return response.data
}
