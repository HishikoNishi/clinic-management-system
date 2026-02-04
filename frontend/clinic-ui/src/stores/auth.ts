import { defineStore } from 'pinia'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: localStorage.getItem('token') as string | null,
    role: localStorage.getItem('role') as string | null,
    expiresAt: localStorage.getItem('expiresAt') as string | null
  }),

  actions: {
    login(payload: {
      token: string
      role: string
      expiresAt: string
    }) {
      this.token = payload.token
      this.role = payload.role
      this.expiresAt = payload.expiresAt

      localStorage.setItem('token', payload.token)
      localStorage.setItem('role', payload.role)
      localStorage.setItem('expiresAt', payload.expiresAt)
    },

    isTokenExpired() {
      if (!this.expiresAt) return true
      return new Date(this.expiresAt).getTime() < Date.now()
    },

    logout() {
      this.token = null
      this.role = null
      this.expiresAt = null
      localStorage.clear()
    }
  }
})

