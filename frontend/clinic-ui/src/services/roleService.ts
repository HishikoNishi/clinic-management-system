import api from './api'

export interface Role {
  id: string
  name: string
}

export const getRoles = async (): Promise<Role[]> => {
  const response = await api.get('/roles')
  return response.data
}
