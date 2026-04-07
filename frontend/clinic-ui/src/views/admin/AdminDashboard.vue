<template>
  <div class="dashboard min-vh-100" :class="{ 'admin-dash-loading': loading }">
    <main class="dashboard-main">
      <section class="welcome-section">
        <h2>Chào mừng trở lại!</h2>
        <p>Dưới đây là tổng quan thực tế từ hệ thống (cập nhật khi tải trang).</p>
      </section>

      <div v-if="error" class="alert alert-danger mb-4" role="alert">
        <i class="bi bi-exclamation-triangle me-2"></i>{{ error }}
      </div>

      <section class="stats-grid">
        <div v-for="stat in displayStats" :key="stat.label" class="stat-card">
          <div class="stat-icon" :class="stat.iconClass">
            <i :class="stat.icon"></i>
          </div>
          <div class="stat-info">
            <div class="stat-value">{{ stat.value }}</div>
            <div class="stat-label">{{ stat.label }}</div>
          </div>
        </div>
      </section>

      <section v-if="stats" class="admin-dash-charts">
        <div class="admin-chart-card">
          <h3 class="admin-chart-title">
            <i class="bi bi-bar-chart-line me-2 text-primary"></i>
            Lịch khám 14 ngày gần nhất
          </h3>
          <div class="admin-bar-chart" role="img" :aria-label="'Biểu đồ cột số lịch khám theo ngày'">
            <div v-for="day in stats.appointmentsLast14Days" :key="day.date" class="admin-bar-wrap">
              <span class="admin-bar-value">{{ day.count }}</span>
              <div
                class="admin-bar"
                :style="{ height: barHeight(day.count) + 'px' }"
                :title="`${day.date}: ${day.count} lịch`"
              />
              <span class="admin-bar-label">{{ day.label }}</span>
            </div>
          </div>
        </div>

        <div class="admin-chart-card">
          <h3 class="admin-chart-title">
            <i class="bi bi-pie-chart me-2 text-primary"></i>
            Trạng thái lịch khám (toàn hệ thống)
          </h3>
          <div class="admin-status-chart">
            <div v-for="row in statusRows" :key="row.key" class="admin-status-row">
              <span class="admin-status-name">{{ row.label }}</span>
              <div class="admin-status-track">
                <div
                  class="admin-status-fill"
                  :class="row.fillClass"
                  :style="{ width: row.pct + '%' }"
                />
              </div>
              <span class="admin-status-count">{{ row.count }}</span>
            </div>
          </div>
        </div>
      </section>

      <div>
        <h5 class="fw-bold mb-3">Hành động nhanh</h5>
        <div class="d-flex gap-2 flex-wrap">
          <button class="btn btn-primary d-flex align-items-center gap-2" @click="$router.push('/doctors')">
            <i class="bi bi-people"></i>
            Quản lý bác sĩ
          </button>
          <button class="btn btn-outline-primary d-flex align-items-center gap-2" @click="$router.push('/staff')">
            <i class="bi bi-people"></i>
            Quản lý nhân viên
          </button>
          <button class="btn btn-outline-secondary d-flex align-items-center gap-2" @click="$router.push('/admin/users/create')">
            <i class="bi bi-person-plus"></i>
            Thêm người dùng
          </button>
          <button class="btn btn-outline-secondary d-flex align-items-center gap-2" @click="$router.push('/appointment')">
            <i class="bi bi-calendar-event"></i>
            Lịch khám
          </button>
        </div>
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useAuthStore } from '@/stores/auth'
import api from '@/services/api'
import '@/styles/layouts/dashboard.css'
import '@/styles/layouts/admin-dashboard.css'

const authStore = useAuthStore()

interface DailyPoint {
  date: string
  label: string
  count: number
}

interface StatsPayload {
  patientCount: number
  doctorActiveCount: number
  appointmentsToday: number
  appointmentsByStatus: Record<string, number>
  revenueThisMonth: number
  appointmentsLast14Days: DailyPoint[]
}

const stats = ref<StatsPayload | null>(null)
const loading = ref(false)
const error = ref<string | null>(null)

const formatVnd = (n: number) =>
  `${n.toLocaleString('vi-VN')} đ`

const displayStats = computed(() => {
  const s = stats.value
  if (!s) {
    return [
      { label: 'Tổng số bệnh nhân', value: '—', icon: 'bi bi-people', iconClass: 'bg-primary-subtle text-primary' },
      { label: 'Lịch khám hôm nay', value: '—', icon: 'bi bi-calendar-event', iconClass: 'bg-success-subtle text-success' },
      { label: 'Bác sĩ đang hoạt động', value: '—', icon: 'bi bi-heart-pulse', iconClass: 'bg-warning-subtle text-warning' },
      { label: 'Thu trong tháng (thanh toán)', value: '—', icon: 'bi bi-cash-stack', iconClass: 'bg-info-subtle text-info' }
    ]
  }
  return [
    {
      label: 'Tổng số bệnh nhân',
      value: String(s.patientCount),
      icon: 'bi bi-people',
      iconClass: 'bg-primary-subtle text-primary'
    },
    {
      label: 'Lịch khám hôm nay',
      value: String(s.appointmentsToday),
      icon: 'bi bi-calendar-event',
      iconClass: 'bg-success-subtle text-success'
    },
    {
      label: 'Bác sĩ đang hoạt động',
      value: String(s.doctorActiveCount),
      icon: 'bi bi-heart-pulse',
      iconClass: 'bg-warning-subtle text-warning'
    },
    {
      label: 'Thu trong tháng (thanh toán)',
      value: formatVnd(Number(s.revenueThisMonth) || 0),
      icon: 'bi bi-cash-stack',
      iconClass: 'bg-info-subtle text-info'
    }
  ]
})

const maxDayCount = computed(() => {
  const days = stats.value?.appointmentsLast14Days ?? []
  const m = Math.max(1, ...days.map((d) => d.count))
  return m
})

const barHeight = (count: number) => {
  const max = maxDayCount.value
  const h = max <= 0 ? 0 : Math.round((count / max) * 140)
  return Math.max(count > 0 ? 6 : 0, h)
}

const statusOrder: { key: string; label: string; fillClass: string }[] = [
  { key: 'Pending', label: 'Chờ xử lý', fillClass: 'admin-status-fill--pending' },
  { key: 'Confirmed', label: 'Đã xác nhận', fillClass: 'admin-status-fill--confirmed' },
  { key: 'Assigned', label: 'Đã phân bác sĩ', fillClass: 'admin-status-fill--assigned' },
  { key: 'CheckedIn', label: 'Đã check-in', fillClass: 'admin-status-fill--checkedin' },
  { key: 'Completed', label: 'Hoàn thành', fillClass: 'admin-status-fill--completed' },
  { key: 'Cancelled', label: 'Đã hủy', fillClass: 'admin-status-fill--cancelled' }
]

const statusRows = computed(() => {
  const by = stats.value?.appointmentsByStatus ?? {}
  const rows = statusOrder.map((o) => ({
    key: o.key,
    label: o.label,
    fillClass: o.fillClass,
    count: by[o.key] ?? 0
  }))
  const total = rows.reduce((s, r) => s + r.count, 0) || 1
  return rows.map((r) => ({
    ...r,
    pct: Math.round((r.count / total) * 100)
  }))
})

const loadStats = async () => {
  loading.value = true
  error.value = null
  try {
    const res = await api.get<StatsPayload>('/admin/AdminDashboard/stats')
    stats.value = res.data
  } catch (e: any) {
    console.error(e)
    error.value = e?.response?.data?.message || 'Không tải được thống kê. Kiểm tra quyền Admin và kết nối API.'
    stats.value = null
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  if (authStore.role === 'Admin') {
    loadStats()
  } else {
    error.value = 'Chỉ tài khoản Admin mới xem được thống kê tổng quan.'
  }
})
</script>
