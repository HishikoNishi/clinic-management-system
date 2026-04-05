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

          <h1 class="brand-title">Quản Lý Phòng Khám</h1>
          <p class="brand-subtitle">Hệ thống quản lý chăm sóc sức khỏe chuyên nghiệp</p>

          <div class="features">
            <div class="feature-item">
              <i class="bi bi-shield-check feature-icon"></i>
              <span class="px-2">An toàn & Đáng tin cậy</span>
            </div>
            <div class="feature-item">
              <i class="bi bi-clock feature-icon"></i>
              <span class="px-2">Truy cập 24/7</span>
            </div>
            <div class="feature-item">
              <i class="bi bi-people feature-icon"></i>
              <span class="px-2">Tập trung vào bệnh nhân</span>
            </div>
          </div>
        </div>
      </div>

      <!-- RIGHT -->
      <div class="login-form-section">
        <div class="form-container">

          <div class="form-header">
            <h2 class="form-title">Chào mừng trở lại</h2>
            <p class="form-subtitle">Đăng nhập vào tài khoản của bạn</p>
          </div>

          <div v-if="errorMessage" class="alert alert-error text-danger">
            {{ errorMessage }}
          </div>

          <form @submit.prevent="handleLogin" class="login-form">

            <div class="form-group">
              <label class="form-label">Tên đăng nhập</label>
              <input
                v-model="credentials.username"
                class="form-input"
                type="text"
                required
              />
            </div>

            <div class="form-group">
              <label class="form-label">Mật khẩu</label>
              <input
                v-model="credentials.password"
                class="form-input"
                type="password"
                required
              />
            </div>

            <button class="btn-login" :disabled="loading">
              <span v-if="!loading">Đăng nhập</span>
              <span v-else>Đang đăng nhập...</span>
            </button>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth.ts'
import api from '@/services/api.ts'
import '@/styles/layouts/login.css'
import type { AxiosError } from 'axios'
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
      expiresAt: response.data.expiresAt,
      refreshToken: response.data.refreshToken,
      refreshExpiresAt: response.data.refreshExpiresAt,
      doctorId: response.data.doctorId
    })

// ✅ redirect theo role
const role = response.data.role?.trim()

const roleRedirect: Record<string, string> = {
  Admin: '/dashboard',
  Staff: '/staff/appointments',
  Doctor: '/doctor/appointments',
  Cashier: '/cashier/invoices',
  Technician: '/technician/tests'
}

router.push(roleRedirect[role] || '/home')


  } catch (error) {
    const err = error as AxiosError<any>
    errorMessage.value =
      err.response?.data?.message ||
      'Invalid username or password. Please try again.'
  }
}
</script>
