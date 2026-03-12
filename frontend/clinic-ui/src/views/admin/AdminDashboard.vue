<template>
  <div class="dashboard min-vh-100">
    <!-- MAIN -->
    <main class="dashboard-main">

      <!-- Welcome -->
    <section class="welcome-section">
      <h2>Chào mừng trở lại!</h2>
      <p>Dưới đây là tổng quan về phòng khám của bạn ngày hôm nay.</p>
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
        <h5 class="fw-bold mb-3">Hành động nhanh</h5>
        <div class="d-flex gap-3 flex-wrap">

          <button class="btn btn-success d-flex align-items-center gap-2" @click="$router.push('/doctors')">
            <i class="bi bi-people"></i>
            Quản lý bác sĩ
          </button>
          <button class="btn btn-warning d-flex align-items-center gap-2" @click="$router.push('/staff')">
          <i class="bi bi-people"></i>
            Quản lý nhân viên
          </button>
          <button class="btn btn-info d-flex align-items-center gap-2" @click="$router.push('/admin/users/create')">
             <i class="bi bi-person-plus"></i>
            Thêm người dùng
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
    label: 'Tổng số bệnh nhân',
    value: 124,
    icon: 'bi bi-people',
    bg: 'bg-primary-subtle text-primary'
  },
  {
    label: "Lịch khám hôm nay",
    value: 18,
    icon: 'bi bi-calendar-event',
    bg: 'bg-success-subtle text-success'
  },
  {
    label: 'Bác sĩ hoạt động',
    value: 8,
    icon: 'bi bi-heart-pulse',
    bg: 'bg-warning-subtle text-warning'
  },
  {
    label: 'Doanh thu hàng tháng',
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