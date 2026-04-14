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
          <div class="admin-bar-chart">
            <div v-for="day in stats.appointmentsLast14Days" :key="day.date" class="admin-bar-wrap">
              <span class="admin-bar-value">{{ day.count }}</span>
              <div class="admin-bar" :style="{ height: barHeight(day.count) + 'px' }" />
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
                <div class="admin-status-fill" :class="row.fillClass" :style="{ width: row.pct + '%' }" />
              </div>
              <span class="admin-status-count">{{ row.count }}</span>
            </div>
          </div>
        </div>
      </section>

      <!-- ===================== ACTIONS ===================== -->
      <div>
        <h5 class="fw-bold mb-3">Hành động nhanh</h5>
        <div class="d-flex gap-2 flex-wrap">
          <button class="btn btn-primary" @click="$router.push('/doctors')">Quản lý bác sĩ</button>
          <button class="btn btn-outline-primary" @click="$router.push('/staff')">Quản lý nhân viên</button>
          <button class="btn btn-outline-secondary" @click="$router.push('/admin/users/create')">Thêm người dùng</button>
          <button class="btn btn-outline-secondary" @click="$router.push('/appointment')">Lịch khám</button>
        </div>
      </div>      <section class="mt-4">
        <div class="card p-3">
          <div class="d-flex justify-content-between align-items-center mb-3">
            <div>
              <h5 class="fw-bold mb-1">Yêu cầu đổi ca mới</h5>
              <div class="text-muted small">Danh sách pending từ bác sĩ.</div>
            </div>
            <button class="btn btn-sm btn-outline-primary" @click="$router.push('/admin/shift-requests')">
              Xem tất cả
            </button>
          </div>

          <div v-if="!pendingShiftRequests.length" class="text-muted small">
            Không có yêu cầu pending.
          </div>

          <div v-else class="list-group list-group-flush">
            <button
              v-for="item in pendingShiftRequests"
              :key="item.id"
              type="button"
              class="list-group-item list-group-item-action px-0"
              @click="$router.push('/admin/shift-requests')"
            >
              <div class="d-flex justify-content-between align-items-center gap-2">
                <strong>{{ item.doctorName }}</strong>
                <span class="badge bg-warning-subtle text-warning">Pending</span>
              </div>
              <div class="small">
                {{ item.requestType === 'EmergencyLeave' ? 'Nghỉ đột xuất' : 'Xin chuyển ca' }}
                - {{ new Date(item.workDate).toLocaleDateString('vi-VN') }}
              </div>
              <div class="small text-muted">{{ item.slotLabel }} ({{ item.startTime }} - {{ item.endTime }})</div>
            </button>
          </div>
        </div>
      </section>


      <!-- ===================== REVENUE (NEW) ===================== -->
      <section class="mt-4" v-if="stats">
        <h5 class="fw-bold mb-3">Thống kê doanh thu</h5>

        <!-- FILTER 1 HÀNG -->
        <div class="d-flex gap-2 mb-3">
          <input v-model="filters.year" type="number" placeholder="Năm" class="form-control form-control-sm" />
          <input v-model="filters.month" type="number" placeholder="Tháng" class="form-control form-control-sm" />

          <select v-model="filters.invoiceType" class="form-select form-select-sm">
            <option value="">Tất cả hóa đơn</option>
            <option value="Clinic">Khám bệnh</option>
            <option value="Prescription">Đơn thuốc</option>
          </select>

          <select v-model="filters.departmentId" class="form-select form-select-sm">
            <option value="">Tất cả khoa</option>
            <option v-for="d in departments" :key="d.id" :value="d.id">
              {{ d.name }}
            </option>
          </select>

          <button class="btn btn-primary btn-sm" @click="loadRevenue">
            Lọc
          </button>
        </div>

        <!-- CHARTS -->
        <div class="row g-3">
          <div class="col-md-6">
            <div class="card p-3">
              <h6>Doanh thu theo ngày</h6>
              <LineChart :data="lineChartData" />
            </div>
          </div>

          <div class="col-md-6">
            <div class="card p-3">
              <h6>Doanh thu theo khoa</h6>
              <BarChart :data="barChartData" />
            </div>
          </div>

          <div class="col-md-12">
            <div class="card p-3">
              <h6>Doanh thu theo loại hóa đơn</h6>
              <LineChart :data="invoiceTypeChartData" />
            </div>
          </div>
        </div>
      </section>

    </main>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useAuthStore } from '@/stores/auth'
import api from '@/services/api'
import { shiftRequestService, type ShiftRequestItem } from '@/services/shiftRequestService'
import LineChart from '@/components/charts/LineChart.vue'
import BarChart from '@/components/charts/BarChart.vue'
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
const revenueData = ref<any>(null)
const pendingShiftRequests = ref<ShiftRequestItem[]>([])

const filters = ref({
  year: null as number | null,
  month: null as number | null,
  departmentId: null as string | null,
  invoiceType: null as string | null
})

const departments = ref<any[]>([])

const loadRevenue = async () => {
  const res = await api.get('/admin/AdminDashboard/revenue', {
    params: filters.value
  })
  revenueData.value = res.data
}

watch(filters, () => {
  loadRevenue()
}, { deep: true })

const loadDepartments = async () => {
  try {
    const res = await api.get('/departments')
    departments.value = res.data
  } catch {}
}

const loadPendingShiftRequests = async () => {
  try {
    pendingShiftRequests.value = await shiftRequestService.getAdminRequests('Pending', 5)
  } catch (e) {
    console.error(e)
  }
}

const lineChartData = computed(() => {
  const data = revenueData.value?.byDay || []
  return {
    labels: data.map((x: any) => x.label),
    datasets: [{ label: 'Doanh thu theo ngày', data: data.map((x: any) => x.value) }]
  }
})

const barChartData = computed(() => {
  const data = revenueData.value?.byDepartment || []
  return {
    labels: data.map((x: any) => x.label),
    datasets: [{ label: 'Doanh thu theo khoa', data: data.map((x: any) => x.value) }]
  }
})

const invoiceTypeChartData = computed(() => {
  const data = revenueData.value?.byType || []

  return {
    labels: data.map((x: any) => x.label),
    datasets: [
      {
        label: 'Doanh thu theo loại hóa đơn',
        data: data.map((x: any) => x.value)
      }
    ]
  }
})
onMounted(() => {
  if (authStore.role === 'Admin') {
    loadStats()
    loadRevenue()
    loadDepartments()
    loadPendingShiftRequests()
  } else {
    error.value = 'Chỉ tài khoản Admin mới xem được thống kê tổng quan.'
  }
})
</script>



