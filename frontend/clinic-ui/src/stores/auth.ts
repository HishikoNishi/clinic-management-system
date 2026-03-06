import { defineStore } from 'pinia'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: localStorage.getItem('token') as string | null,
    role: localStorage.getItem('role') as string | null,
    expiresAt: localStorage.getItem('expiresAt') as string | null
  }),

  getters: {
    isLoggedIn: (state) => {
      if (!state.token || !state.expiresAt) return false
      return new Date(state.expiresAt).getTime() > Date.now()
    }
  },

  actions: {
    login(payload: { token: string; role: string; expiresAt: string }) {
      this.token = payload.token
      this.role = payload.role
      this.expiresAt = payload.expiresAt

      localStorage.setItem('token', payload.token)
      localStorage.setItem('role', payload.role)
      localStorage.setItem('expiresAt', payload.expiresAt)
    },

    logout() {
      this.token = null
      this.role = null
      this.expiresAt = null

      localStorage.removeItem('token')
      localStorage.removeItem('role')
      localStorage.removeItem('expiresAt')
    }
  }
})