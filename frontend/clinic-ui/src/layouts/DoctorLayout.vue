<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { shiftRequestService } from '@/services/shiftRequestService'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const isSidebarOpen = ref(false)
const pendingShiftRequests = ref(0)
let pendingTimer: ReturnType<typeof setInterval> | undefined

const navItems = computed(() => ([
  { label: 'Lịch khám', icon: 'calendar-heart', path: '/doctor/appointments' },
  { label: 'Khám bệnh', icon: 'heart-pulse', path: '/doctor/appointments' },
  { label: 'Bệnh nhân', icon: 'people', path: '/doctor/patients' },
  { label: 'Yêu cầu ca làm', icon: 'arrow-left-right', path: '/doctor/shift-requests', badgeCount: pendingShiftRequests.value },
  { label: 'Lịch làm', icon: 'calendar3', path: '/doctor/my-schedule' },
  { label: 'Hồ sơ cá nhân', icon: 'person-circle', path: '/doctor/profile' }
]))

const loadPendingShiftRequests = async () => {
  try {
    pendingShiftRequests.value = await shiftRequestService.getMyPendingCount()
  } catch {
    // ignore to keep layout responsive
  }
}

const go = (path: string) => {
  router.push(path)
  isSidebarOpen.value = false
}

const isActive = (path: string) =>
  route.path === path || route.path.startsWith(`${path}/`)

const logout = () => {
  authStore.logout()
  router.push('/login')
}

onMounted(async () => {
  await loadPendingShiftRequests()
  pendingTimer = setInterval(loadPendingShiftRequests, 30000)
})

onUnmounted(() => {
  if (pendingTimer) clearInterval(pendingTimer)
})
</script>

<template>
  <div class="doctor-layout">
    <header class="topbar">
      <div class="brand" @click="go('/doctor/appointments')">
        <i class="bi bi-hospital me-2"></i>
        Quản lý phòng khám
      </div>
      <button class="sidebar-toggle d-lg-none" type="button" @click="isSidebarOpen = !isSidebarOpen">
        <i :class="`bi ${isSidebarOpen ? 'bi-x-lg' : 'bi-list'}`"></i>
      </button>
    </header>

    <div class="layout-shell">
      <aside class="sidebar" :class="{ open: isSidebarOpen }">
        <nav class="nav-list">
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
    <div class="backdrop d-lg-none" :class="{ show: isSidebarOpen }" @click="isSidebarOpen = false" />
  </div>
</template>

<style src="@/styles/layouts/doctor-layout.css"></style>

