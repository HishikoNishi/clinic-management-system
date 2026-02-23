<template>
  <div class="dashboard min-vh-100">

    <!-- HEADER -->
    <header class="dashboard-header">
      <span class="dashboard-logo">Clinic Management</span>

      <div class="dashboard-header-right">
        <span class="dashboard-user-name">
          {{ authStore.user?.name || 'User' }}
        </span>
        <button class="btn btn-outline-danger btn-sm" @click="handleLogout">
          <i class="bi bi-box-arrow-right me-1"></i>
          Logout
        </button>
      </div>
    </header>

    <!-- MAIN -->
    <main class="dashboard-main">

      <!-- Welcome -->
    <section class="welcome-section">
      <h2>Welcome back!</h2>
      <p>Here's an overview of your clinic today.</p>
    </section>

      <!-- STATS -->
<section class="stats-grid">
      <div
        class="stat-card"
        v-for="stat in stats"
        :key="stat.label"
      >
        <div class="stat-icon" :class="stat.iconClass">
          <i :class="stat.icon"></i>
        </div>

        <div class="stat-info">
          <div class="stat-value">{{ stat.value }}</div>
          <div class="stat-label">{{ stat.label }}</div>
        </div>
      </div>
    </section>

      <!-- QUICK ACTIONS -->
      <div>
        <h5 class="fw-bold mb-3">Quick Actions</h5>
        <div class="d-flex gap-3 flex-wrap">
          <button class="btn btn-primary d-flex align-items-center gap-2">
            <i class="bi bi-plus-circle"></i>
            New Patient
          </button>

          <button class="btn btn-warning d-flex align-items-center gap-2">
            <i class="bi bi-calendar-event"></i>
            Schedule Appointment
          </button>

          <button class="btn btn-success d-flex align-items-center gap-2">
            <i class="bi bi-file-earmark-text"></i>
            Medical Records
          </button>
        </div>
      </div>
    </main>
  </div>
</template>


<script setup>
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth.ts'
import '@/styles/layouts/dashboard.css'

const router = useRouter()
const authStore = useAuthStore()

const stats = [
  {
    label: 'Total Patients',
    value: 124,
    icon: 'bi bi-people',
    bg: 'bg-primary-subtle text-primary'
  },
  {
    label: "Today's Appointments",
    value: 18,
    icon: 'bi bi-calendar-event',
    bg: 'bg-success-subtle text-success'
  },
  {
    label: 'Active Doctors',
    value: 8,
    icon: 'bi bi-heart-pulse',
    bg: 'bg-warning-subtle text-warning'
  },
  {
    label: 'Monthly Revenue',
    value: '$12,450',
    icon: 'bi bi-currency-dollar',
    bg: 'bg-info-subtle text-info'
  }
]

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
}
</script>