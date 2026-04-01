import axios from 'axios'
import { useAuthStore } from '@/stores/auth'

const api = axios.create({
  baseURL: 'https://localhost:7235/api',
})

api.interceptors.request.use(config => {
  const authStore = useAuthStore()


  if (authStore.token) {
    config.headers.Authorization = `Bearer ${authStore.token}`
  }


  return config
})

let isRefreshing = false
let refreshQueue: { resolve: (value: any) => void; reject: (reason?: any) => void; config: any }[] = []

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const authStore = useAuthStore()
    const originalRequest = error.config

    if (error.response?.status === 401 && authStore.isRefreshValid?.() && !originalRequest._retry) {
      originalRequest._retry = true

      if (!isRefreshing) {
        isRefreshing = true
        const refreshed = await authStore.refresh()
        isRefreshing = false
        refreshQueue.forEach(({ resolve, reject, config }) => {
          if (refreshed) {
            config.headers.Authorization = `Bearer ${authStore.token}`
            resolve(api(config))
          } else {
            reject(error)
          }
        })
        refreshQueue = []

        if (refreshed) {
          originalRequest.headers.Authorization = `Bearer ${authStore.token}`
          return api(originalRequest)
        }
      }

      return new Promise((resolve, reject) => {
        refreshQueue.push({ resolve, reject, config: originalRequest })
      })
    }

    return Promise.reject(error)
  }
)

export default api    
