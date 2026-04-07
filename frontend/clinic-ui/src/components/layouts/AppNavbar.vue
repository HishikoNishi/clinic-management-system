<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import api from '@/services/api'

const router = useRouter()
const authStore = useAuthStore()
const isNavOpen = ref(false)

const isLoggedIn = computed(() => {
  if (!authStore.token) return false
  if (!authStore.expiresAt) return true
  return new Date(authStore.expiresAt) > new Date()
})

const userRole = computed(() => authStore.role)

const guestNavigationItems = [
  { label: 'Dịch vụ', path: '#services', icon: 'heart-pulse', isAnchor: true },
  { label: 'Bác sĩ', path: '#doctors', icon: 'people', isAnchor: true },
  { label: 'Đặt lịch hẹn', path: '#booking', icon: 'calendar-check', isAnchor: true },
  { label: 'Tìm kiếm', path: '#search', icon: 'search', isAnchor: true }
]

const authNavigationItems = [
  { label: 'Bảng điều khiển', path: '/dashboard', icon: 'speedometer2', isAnchor: false },
  { label: 'Lịch khám', path: '/appointment', icon: 'calendar-event', isAnchor: false },
  { label: 'Bác sĩ', path: '/doctors', icon: 'person-workspace', isAnchor: false },
  { label: 'Nhân viên', path: '/staff', icon: 'people', isAnchor: false }
]

const navigationItems = computed(() => {
  if (!isLoggedIn.value || userRole.value === 'Guest') {
    return guestNavigationItems
  }
  return authNavigationItems
})

const handleLogout = () => {
  authStore.logout()
  router.push('/')
}

const closeNavbar = () => { isNavOpen.value = false }

const navigateTo = (path: string, isAnchor: boolean) => {
  if (isAnchor) closeNavbar()
  else { router.push(path); closeNavbar() }
}

// PIN modal for internal login
const showPinModal = ref(false)
const pinCode = ref('')
const pinError = ref('')
const pinLoading = ref(false)

const openPinModal = () => {
  showPinModal.value = true
  pinCode.value = ''
  pinError.value = ''
}

const submitPin = async () => {
  pinError.value = ''
  if (!pinCode.value || pinCode.value.length !== 6) {
    pinError.value = 'Nhập mã PIN 6 chữ số'
    return
  }
  try {
    pinLoading.value = true
    await api.post('/auth/login-pin', { pin: pinCode.value })
    localStorage.setItem('pinAuthOk', 'true')
    showPinModal.value = false
    router.push('/login')
  } catch (err: any) {
    pinError.value = err?.response?.data?.message || 'Mã PIN không đúng'
  } finally {
    pinLoading.value = false
  }
}
</script>

<template>
  <nav class="navbar navbar-expand-lg fixed-top">
    <div class="container-fluid">
      <router-link to="/" class="navbar-brand d-flex align-items-center gap-2">
        <i class="bi bi-hospital"></i>
        <span class="fw-bold">Quản lý phòng khám</span>
      </router-link>

      <button
        class="navbar-toggler"
        type="button"
        @click="isNavOpen = !isNavOpen"
        :aria-expanded="isNavOpen"
        aria-label="Toggle navigation"
      >
        <span class="navbar-toggler-icon"></span>
      </button>

      <div class="collapse navbar-collapse" :class="{ show: isNavOpen }">
        <ul class="navbar-nav ms-auto mb-2 mb-lg-0">
          <li v-for="item in navigationItems" :key="item.path" class="nav-item">
            <a
              v-if="item.isAnchor"
              :href="item.path"
              class="nav-link"
              @click="closeNavbar"
            >
              <i :class="`bi bi-${item.icon}`"></i>
              {{ item.label }}
            </a>

            <router-link
              v-else
              :to="item.path"
              class="nav-link"
              :class="{ active: $route.path === item.path }"
              @click="closeNavbar"
            >
              <i :class="`bi bi-${item.icon}`"></i>
              {{ item.label }}
            </router-link>
          </li>
        </ul>

        <hr v-if="isLoggedIn" class="text-secondary d-lg-none" />

        <div v-if="!isLoggedIn" class="mt-3 mt-lg-0 ms-lg-3 d-flex align-items-center">
          <button
            @click="openPinModal"
            class="btn btn-outline-light btn-icon"
            type="button"
            title="Khu vực nội bộ"
          >
            <i class="bi bi-shield-lock"></i>
          </button>
        </div>

        <div v-if="isLoggedIn" class="d-flex gap-2 align-items-center mt-3 mt-lg-0 ms-lg-3">
          <span class="navbar-text text-light d-none d-lg-inline">
            <i class="bi bi-person-circle me-1"></i>
            {{ userRole || 'Người dùng' }}
          </span>

          <button
            @click="handleLogout"
            class="btn btn-outline-light btn-sm"
            type="button"
          >
            <i class="bi bi-box-arrow-right me-1"></i>
            Đăng xuất
          </button>
        </div>
      </div>
    </div>
  </nav>

  <div v-if="showPinModal" class="modal-backdrop" @click.self="showPinModal = false">
    <div class="modal-box">
      <h5>Truy cập nội bộ</h5>
      <p class="text-muted small mb-2">Nhập mã PIN 6 chữ số để vào.</p>
      <input
        v-model="pinCode"
        type="password"
        maxlength="6"
        class="form-control"
        placeholder="******"
        inputmode="numeric"
      />
      <div v-if="pinError" class="form-error mt-1">{{ pinError }}</div>
      <div class="modal-actions">
        <button class="btn btn-outline-secondary w-50" @click="showPinModal = false">Đóng</button>
        <button class="btn btn-primary w-50" :disabled="pinLoading" @click="submitPin">
          <span v-if="pinLoading" class="spinner-border spinner-border-sm me-1"></span>
          Xác nhận
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.navbar {
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  background: linear-gradient(90deg, var(--color-primary-dark), var(--color-primary));
}

.nav-link {
  transition: color 0.3s ease;
  display: flex;
  align-items: center;
  gap: 8px;
}

.nav-link:hover {
  color: #e8f0ff !important;
}

.nav-link.active {
  color: #e8f0ff !important;
  border-bottom: 3px solid #e8f0ff;
}

.navbar-brand {
  font-size: 1.3rem;
  transition: opacity 0.3s ease;
  color: #fff !important;
}

.navbar-brand:hover { opacity: 0.85; }

.navbar-toggler { border-color: rgba(255, 255, 255, 0.6); }
.navbar-toggler-icon { filter: brightness(0) invert(1); }

@media (max-width: 991px) {
  .nav-link.active {
    border-bottom: none;
    background-color: rgba(232, 240, 255, 0.12);
  }
}

.btn-icon {
  width: 34px;
  height: 34px;
  padding: 0;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
}
.btn-icon i { font-size: 16px; }

.modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.55);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 2000;
}
.modal-box {
  background: #fff;
  border-radius: 10px;
  padding: 20px;
  width: 320px;
  box-shadow: 0 10px 30px rgba(0,0,0,0.25);
}
.modal-box h5 { margin: 0 0 12px 0; }
.modal-actions { display: flex; gap: 8px; margin-top: 12px; }
.form-error { color: #c0392b; font-size: 12px; }
</style>
