<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

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
  { label: 'Dịch vụ', path: '#services', icon: 'heart', isAnchor: true },
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

const handleLogin = () => {
  router.push('/login')
}

const closeNavbar = () => {
  isNavOpen.value = false
}

const navigateTo = (path: string, isAnchor: boolean) => {
  if (isAnchor) {
    closeNavbar()
  } else {
    router.push(path)
    closeNavbar()
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

        <div v-if="!isLoggedIn" class="mt-3 mt-lg-0 ms-lg-2">
          <button
            @click="handleLogin"
            class="btn btn-primary btn-sm"
            type="button"
          >
            <i class="bi bi-box-arrow-in-right me-1"></i>
            Đăng nhập
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

.navbar-brand:hover {
  opacity: 0.85;
}

.navbar-toggler {
  border-color: rgba(255, 255, 255, 0.6);
}

.navbar-toggler-icon {
  filter: brightness(0) invert(1);
}

@media (max-width: 991px) {
  .nav-link.active {
    border-bottom: none;
    background-color: rgba(232, 240, 255, 0.12);
  }
}
</style>
