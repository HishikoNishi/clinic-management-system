<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()
const isNavOpen = ref(false)

// Check if user is logged in and get their role
const isLoggedIn = computed(() => !!authStore.token && !authStore.isTokenExpired())
const userRole = computed(() => authStore.role)

// Guest navigation items (scroll anchors)
const guestNavigationItems = computed(() => [
  { label: 'Services', path: '#services', icon: 'heart', isAnchor: true },
  { label: 'Book Appointment', path: '#booking', icon: 'calendar-check', isAnchor: true },
  { label: 'Search', path: '#search', icon: 'search', isAnchor: true }
])

// Admin/staff/doctor navigation items (route paths)
const adminNavigationItems = computed(() => [
  { label: 'Dashboard', path: '/dashboard', icon: 'speedometer2', isAnchor: false },
  { label: 'Appointments', path: '/appointments', icon: 'calendar-event', isAnchor: false },
  ...(userRole.value === 'Admin' ? [
    { label: 'Patients', path: '/patients', icon: 'people', isAnchor: false },
    { label: 'Doctors', path: '/doctors', icon: 'person-workspace', isAnchor: false }
  ] : [])
])

// Determine which navigation items to show based on role
const navigationItems = computed(() => {
  if (!isLoggedIn.value || userRole.value === 'Guest') {
    return guestNavigationItems.value
  }
  return adminNavigationItems.value
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
    // For anchor links, don't close navbar and let browser handle the scroll
    closeNavbar()
  } else {
    // For route paths, use router
    router.push(path)
    closeNavbar()
  }
}
</script>

<template>
  <nav class="navbar navbar-expand-lg navbar-dark bg-dark fixed-top">
    <div class="container-fluid">
      <!-- Brand -->
      <router-link to="/" class="navbar-brand d-flex align-items-center gap-2">
        <i class="bi bi-hospital"></i>
        <span class="fw-bold">Clinic Management</span>
      </router-link>

      <!-- Toggler Button for Mobile -->
      <button
        class="navbar-toggler"
        type="button"
        @click="isNavOpen = !isNavOpen"
        :aria-expanded="isNavOpen"
        aria-label="Toggle navigation"
      >
        <span class="navbar-toggler-icon"></span>
      </button>

      <!-- Navbar Content -->
      <div class="collapse navbar-collapse" :class="{ show: isNavOpen }">
        <!-- Navigation Links -->
        <ul class="navbar-nav ms-auto mb-2 mb-lg-0">
          <li v-for="item in navigationItems" :key="item.path" class="nav-item">
            <!-- Anchor links for guest navigation -->
            <a
              v-if="item.isAnchor"
              :href="item.path"
              class="nav-link"
              @click="closeNavbar"
            >
              <i :class="`bi bi-${item.icon}`"></i>
              {{ item.label }}
            </a>

            <!-- Router links for authenticated navigation -->
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

        <!-- Divider -->
        <hr v-if="isLoggedIn" class="text-secondary d-lg-none" />

        <!-- Login Button (for guests/not logged in) -->
        <div v-if="!isLoggedIn" class="mt-3 mt-lg-0 ms-lg-2">
          <button
            @click="handleLogin"
            class="btn btn-primary btn-sm"
            type="button"
          >
            <i class="bi bi-box-arrow-in-right me-1"></i>
            Login
          </button>
        </div>

        <!-- User Info and Logout (only for logged-in users) -->
        <div v-if="isLoggedIn && userRole !== 'Guest'" class="d-flex gap-2 align-items-center mt-3 mt-lg-0 ms-lg-3">
          <span class="navbar-text text-light d-none d-lg-inline">
            <i class="bi bi-person-circle me-1"></i>
            <span class="text-capitalize">{{ userRole }}</span>
          </span>
          <button
            @click="handleLogout"
            class="btn btn-outline-light btn-sm"
            type="button"
          >
            <i class="bi bi-box-arrow-right me-1"></i>
            Logout
          </button>
        </div>
      </div>
    </div>
  </nav>
</template>

<style scoped>
.navbar {
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.nav-link {
  transition: color 0.3s ease;
  display: flex;
  align-items: center;
  gap: 8px;
}

.nav-link:hover {
  color: #0d6efd !important;
}

.nav-link.active {
  color: #0d6efd !important;
  border-bottom: 3px solid #0d6efd;
}

.navbar-brand {
  font-size: 1.3rem;
  transition: opacity 0.3s ease;
}

.navbar-brand:hover {
  opacity: 0.8;
}

@media (max-width: 991px) {
  .nav-link.active {
    border-bottom: none;
    background-color: rgba(13, 110, 253, 0.1);
  }
}
</style>