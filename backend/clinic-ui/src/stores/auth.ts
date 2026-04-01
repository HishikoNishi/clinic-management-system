import { defineStore } from 'pinia'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: localStorage.getItem('token') as string | null,
    role: localStorage.getItem('role') as string | null,
    expiresAt: localStorage.getItem('expiresAt') as string | null,
    refreshToken: localStorage.getItem('refreshToken') as string | null,
    refreshExpiresAt: localStorage.getItem('refreshExpiresAt') as string | null
  }),

  getters: {
    isLoggedIn: (state) => {
      if (!state.token || !state.expiresAt) return false
      return new Date(state.expiresAt).getTime() > Date.now()
    },

    isAdmin: (state) => state.role === 'Admin',
    isDoctor: (state) => state.role === 'Doctor',
    isStaff: (state) => state.role === 'Staff'
  },

  actions: {
    login(payload: { token: string; role: string; expiresAt: string; refreshToken?: string; refreshExpiresAt?: string }) {
      this.token = payload.token
      this.role = payload.role
      this.expiresAt = payload.expiresAt
      this.refreshToken = payload.refreshToken || null
      this.refreshExpiresAt = payload.refreshExpiresAt || null

      localStorage.setItem('token', payload.token)
      localStorage.setItem('role', payload.role)
      localStorage.setItem('expiresAt', payload.expiresAt)
      if (payload.refreshToken) localStorage.setItem('refreshToken', payload.refreshToken)
      if (payload.refreshExpiresAt) localStorage.setItem('refreshExpiresAt', payload.refreshExpiresAt)
    },

    logout() {
      this.token = null
      this.role = null
      this.expiresAt = null
      this.refreshToken = null
      this.refreshExpiresAt = null

      localStorage.removeItem('token')
      localStorage.removeItem('role')
      localStorage.removeItem('expiresAt')
      localStorage.removeItem('refreshToken')
      localStorage.removeItem('refreshExpiresAt')
    },

    isTokenExpired(): boolean {
      if (!this.expiresAt) return true
      return new Date(this.expiresAt).getTime() < Date.now()
    },

    isRefreshValid(): boolean {
      if (!this.refreshToken || !this.refreshExpiresAt) return false
      return new Date(this.refreshExpiresAt).getTime() > Date.now()
    },

    async refresh(): Promise<boolean> {
      if (!this.isRefreshValid()) {
        this.logout()
        return false
      }
      try {
        const res = await fetch(`${import.meta.env.VITE_API_BASE ?? 'https://localhost:7235/api'}/auth/refresh`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ refreshToken: this.refreshToken })
        })
        if (!res.ok) throw new Error('refresh failed')
        const data = await res.json()
        this.login({
          token: data.token,
          role: data.role,
          expiresAt: data.expiresAt,
          refreshToken: data.refreshToken,
          refreshExpiresAt: data.refreshExpiresAt
        })
        return true
      } catch {
        this.logout()
        return false
      }
    }
  }
})
