<script setup lang="ts">
import { computed, onUnmounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { shiftRequestService } from '@/services/shiftRequestService'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const isSidebarOpen = ref(false)
const pendingAdminShiftRequests = ref(0)
let pendingAdminTimer: ReturnType<typeof setInterval> | undefined

const role = computed(() => authStore.role || 'Guest')

const navItems = computed(() => {
  if (role.value === 'Admin') {
    return [
      { label: 'Tổng quan', icon: 'speedometer2', path: '/dashboard' },
      { label: 'Lịch khám', icon: 'calendar-event', path: '/appointment' },
      { label: 'Bác sĩ', icon: 'person-workspace', path: '/doctors' },
      { label: 'Yêu cầu đổi ca', icon: 'arrow-left-right', path: '/admin/shift-requests', badgeCount: pendingAdminShiftRequests.value },
      { label: 'Khoa', icon: 'building', path: '/departments' },
      { label: 'Chuyên khoa', icon: 'layers', path: '/specialties' },
      { label: 'Thuốc', icon: 'capsule-pill', path: '/medicines' },
      { label: 'Nhân viên', icon: 'people', path: '/staff' },
      { label: 'Tạo tài khoản', icon: 'person-plus', path: '/admin/users/create' }
    ]
  }

  if (role.value === 'Staff') {
    return [
      { label: 'Lich kham', icon: 'calendar-check', path: '/staff/appointments' },
      { label: 'Đặt lịch tại quầy', icon: 'calendar-plus', path: '/staff/create-appointment' },
      { label: 'Tài khoản của tôi', icon: 'person-circle', path: '/staff/profile' }
    ]
  }

  if (role.value === 'Doctor') {
    return [
      { label: 'Lich kham', icon: 'calendar-heart', path: '/doctor/appointments' }
    ]
  }

  if (role.value === 'Technician') {
    return [
      { label: 'Xet nghiem cho', icon: 'list-check', path: '/technician/tests' },
      { label: 'Lịch sử xét nghiệm', icon: 'clock-history', path: '/technician/tests/history' }
    ]
  }

  return []
})

const homePath = computed(() => navItems.value[0]?.path ?? '/home')

const go = (path: string) => {
  router.push(path)
  isSidebarOpen.value = false
}

const isActive = (path: string) =>
  route.path === path || route.path.startsWith(`${path}/`)

const toggleSidebar = () => {
  isSidebarOpen.value = !isSidebarOpen.value
}

const logout = () => {
  authStore.logout()
  router.push('/login')
}

const stopPendingAdminPolling = () => {
  if (pendingAdminTimer) clearInterval(pendingAdminTimer)
  pendingAdminTimer = undefined
}

const loadPendingAdminShiftRequests = async () => {
  try {
    pendingAdminShiftRequests.value = await shiftRequestService.getAdminPendingCount()
  } catch {
    // ignore to keep layout responsive
  }
}

watch(
  role,
  async (value) => {
    stopPendingAdminPolling()
    pendingAdminShiftRequests.value = 0

    if (value !== 'Admin') return

    await loadPendingAdminShiftRequests()
    pendingAdminTimer = setInterval(loadPendingAdminShiftRequests, 30000)
  },
  { immediate: true }
)

onUnmounted(() => {
  stopPendingAdminPolling()
})
</script>

<template>
  <div class="dashboard-layout">
    <header class="topbar">
      <div class="brand" @click="go(homePath)">
        <i class="bi bi-hospital me-2"></i>
        Quản lý phòng khám
      </div>
      <button class="sidebar-toggle d-lg-none" type="button" @click="toggleSidebar">
        <i :class="`bi ${isSidebarOpen ? 'bi-x-lg' : 'bi-list'}`"></i>
      </button>
    </header>

    <div class="layout-shell">
      <aside class="sidebar" :class="{ open: isSidebarOpen }">
        <div class="sidebar-header">
          <span class="role-pill">{{ role }}</span>
        </div>

        <nav class="nav-list" v-if="navItems.length">
          <button
            v-for="item in navItems"
            :key="item.path"
            class="nav-link-btn"
            :class="{ active: isActive(item.path) }"
            type="button"
            @click="go(item.path)"
          >
            <i :class="`bi bi-${item.icon}`"></i>
            <span class="nav-label">{{ item.label }}</span>
            <span v-if="item.badgeCount" class="nav-badge">{{ item.badgeCount }}</span>
          </button>
        </nav>

        <div class="sidebar-footer">
          <button class="logout-btn" type="button" @click="logout">
            <i class="bi bi-box-arrow-right"></i>
            <span>Đăng xuất</span>
          </button>
        </div>
      </aside>

      <main class="content">
        <div class="content-inner">
          <router-view />
        </div>
      </main>
    </div>

    <div class="backdrop d-lg-none" :class="{ show: isSidebarOpen }" @click="toggleSidebar" />
  </div>
</template>

<style src="@/styles/layouts/admin-layout.css" scoped></style>
