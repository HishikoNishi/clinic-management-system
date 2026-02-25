<template>
  <div class="login-container">

    <!-- Background -->
    <div class="background-shapes">
      <div class="shape shape-1"></div>
      <div class="shape shape-2"></div>
      <div class="shape shape-3"></div>
    </div>

    <div class="login-card">

      <!-- LEFT -->
      <div class="login-brand">
        <div class="brand-content">
          <div class="logo-container">
            <i class="bi bi-hospital logo-icon"></i>
          </div>

          <h1 class="brand-title">Clinic Management</h1>
          <p class="brand-subtitle">Professional Healthcare System</p>

          <div class="features">
            <div class="feature-item">
              <i class="bi bi-shield-check feature-icon"></i>
              <span class="px-2">Secure & Reliable</span>
            </div>
            <div class="feature-item">
              <i class="bi bi-clock feature-icon"></i>
              <span class="px-2">24/7 Access</span>
            </div>
            <div class="feature-item">
              <i class="bi bi-people feature-icon"></i>
              <span class="px-2">Patient Focused</span>
            </div>
          </div>
        </div>
      </div>

      <!-- RIGHT -->
      <div class="login-form-section">
        <div class="form-container">

          <div class="form-header">
            <h2 class="form-title">Welcome Back</h2>
            <p class="form-subtitle">Sign in to your account</p>
          </div>

          <div v-if="errorMessage" class="alert alert-error">
            {{ errorMessage }}
          </div>

          <form @submit.prevent="handleLogin" class="login-form">

            <div class="form-group">
              <label class="form-label">Username</label>
              <input
                v-model="credentials.username"
                class="form-input"
                type="text"
                required
              />
            </div>

            <div class="form-group">
              <label class="form-label">Password</label>
              <input
                v-model="credentials.password"
                class="form-input"
                type="password"
                required
              />
            </div>

            <button class="btn-login" :disabled="loading">
              <span v-if="!loading">Login</span>
              <span v-else>Logging in...</span>
            </button>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth.ts'
import api from '@/services/api.ts'
import '@/styles/layouts/login.css'

const router = useRouter()
const authStore = useAuthStore()

const credentials = ref({
  username: '',
  password: ''
})

const showPassword = ref(false)
const rememberMe = ref(false)
const loading = ref(false)
const errorMessage = ref('')

const handleLogin = async () => {
  try {
    loading.value = true
    errorMessage.value = ''

    // Call the login API
    const response = await api.post('/Auth/login', {
      username: credentials.value.username,
      password: credentials.value.password
    })

    authStore.login({
  token: response.data.token,
  role: response.data.role,
  expiresAt: response.data.expiresAt
})

// ✅ redirect theo role
const role = response.data.role

if (role === 'Staff') {
  router.push('/staff')
}
else if (role === 'Doctor') {
  router.push('/dashboard')
}
else if (role === 'Admin') {
  router.push('/dashboard')
}
else {
  router.push('/login')
}
  } catch (error) {
    console.error('Login error:', error)
    errorMessage.value = error.response?.data?.message || 'Invalid username or password. Please try again.'
  } finally {
    loading.value = false
  }
}
</script>