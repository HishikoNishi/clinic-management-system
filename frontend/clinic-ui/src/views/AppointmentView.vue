<template>
  <div class="admin-appointment-page page">
    <div class="page-header mb-4">
      <div>
        <div class="page-eyebrow">Lịch khám</div>
        <h2 class="page-title mb-0">Danh sách lịch khám</h2>
        <p class="page-subtitle">
          Dữ liệu thực từ hệ thống
          <span v-if="auth.role">· Vai trò: {{ auth.role }}</span>
        </p>
      </div>
      <button type="button" class="btn btn-outline-secondary btn-sm" :disabled="loading" @click="loadAppointments">
        <span v-if="loading" class="spinner-border spinner-border-sm me-1" aria-hidden="true"></span>
        <i v-else class="bi bi-arrow-clockwise me-1"></i>
        Làm mới
      </button>
    </div>

    <div v-if="error" class="alert alert-danger" role="alert">{{ error }}</div>

    <div class="row g-3 mb-4">
      <div class="col-sm-6 col-xl-3">
        <div class="card border shadow-sm h-100">
          <div class="card-body py-3">
            <div class="text-muted small">Tổng lịch (sau lọc)</div>
            <div class="fs-4 fw-semibold">{{ filteredAppointments.length }}</div>
          </div>
        </div>
      </div>
      <div class="col-sm-6 col-xl-3">
        <div class="card border shadow-sm h-100">
          <div class="card-body py-3">
            <div class="text-muted small">Hôm nay (theo ngày hẹn)</div>
            <div class="fs-4 fw-semibold text-primary">{{ appointmentsTodayCount }}</div>
          </div>
        </div>
      </div>
      <div class="col-sm-6 col-xl-3">
        <div class="card border shadow-sm h-100">
          <div class="card-body py-3">
            <div class="text-muted small">Đang chờ xử lý</div>
            <div class="fs-4 fw-semibold">{{ statusCount.Pending }}</div>
          </div>
        </div>
      </div>
      <div class="col-sm-6 col-xl-3">
        <div class="card border shadow-sm h-100">
          <div class="card-body py-3">
            <div class="text-muted small">Hoàn thành</div>
            <div class="fs-4 fw-semibold text-success">{{ statusCount.Completed }}</div>
          </div>
        </div>
      </div>
    </div>

    <div class="card border shadow-sm mb-4">
      <div class="card-body">
        <div class="row g-2 align-items-end">
          <div class="col-md-3">
            <label class="form-label small mb-1">Mã lịch</label>
            <input v-model="searchCode" type="text" class="form-control form-control-sm" placeholder="Mã..." />
          </div>
          <div class="col-md-3">
            <label class="form-label small mb-1">Tên bệnh nhân</label>
            <input v-model="searchName" type="text" class="form-control form-control-sm" placeholder="Tên..." />
          </div>
          <div class="col-md-3">
            <label class="form-label small mb-1">Số điện thoại</label>
            <input v-model="searchPhone" type="text" class="form-control form-control-sm" placeholder="SĐT..." />
          </div>
          <div class="col-md-3">
            <label class="form-label small mb-1">Ngày khám</label>
            <input v-model="searchDate" type="date" class="form-control form-control-sm" />
          </div>
        </div>
        <div class="mt-2">
          <button type="button" class="btn btn-link btn-sm p-0" @click="clearFilters">Xóa bộ lọc</button>
        </div>
      </div>
    </div>

    <div class="d-flex flex-wrap gap-2 mb-3">
      <button
        v-for="s in statuses"
        :key="s"
        type="button"
        class="btn btn-sm"
        :class="currentStatus === s ? 'btn-primary' : 'btn-outline-secondary'"
        @click="changeStatus(s)"
      >
        {{ statusLabel(s) }}
      </button>
    </div>

    <div class="card border shadow-sm">
      <div class="table-responsive">
        <table class="table table-hover align-middle mb-0">
        <thead class="table-light">
  <tr>
    <th>Mã</th>
    <th>Bệnh nhân</th>
    <th>SĐT</th>
       <th>Mã BN</th>
    <th>CCCD</th>
    <th>BHYT</th>
    <th>Ngày giờ khám</th>
    <th>Trạng thái</th>
    <th>Bác sĩ / Khoa</th>
    <th class="text-end">Thao tác</th>
  </tr>
</thead>

          <tbody>
            <tr v-if="loading">
              <td colspan="7" class="text-center py-4 text-muted">
                <span class="spinner-border spinner-border-sm me-2"></span>
                Đang tải...
              </td>
            </tr>
            <tr v-else-if="filteredAppointments.length === 0">
              <td colspan="7" class="text-center py-4 text-muted">Không có lịch khám phù hợp.</td>
            </tr>
            <tr v-for="a in filteredAppointments" :key="a.id">
              <td class="text-monospace small">{{ a.appointmentCode }}</td>
              <td class="fw-semibold">{{ a.fullName }}</td>
              <td>{{ a.phone }}</td>
                <td>{{ a.patientCode || '—' }}</td>

             <td>{{ a.citizenId || '—' }}</td>
<td>{{ a.insuranceCardNumber || '—' }}</td>

              <td>{{ formatDateTime(a.appointmentDate, a.appointmentTime) }}</td>
              <td>
                <span class="badge rounded-pill" :class="statusBadgeClass(a.status)">{{ statusLabelVi(a.status) }}</span>
              </td>
              <td class="small">
                <template v-if="a.statusDetail?.doctorName">
                  {{ a.statusDetail.doctorName }}
                  <span v-if="a.statusDetail.doctorDepartmentName" class="text-muted"> · {{ a.statusDetail.doctorDepartmentName }}</span>
                </template>
                <span v-else class="text-muted">—</span>
              </td>
              <td class="text-end">
                <button type="button" class="btn btn-sm btn-primary" @click="goDetail(a)">
                  {{ auth.role === 'Doctor' ? 'Khám / Chi tiết' : 'Chi tiết' }}
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import api from '@/services/api'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const router = useRouter()

const searchCode = ref('')
const searchName = ref('')
const searchPhone = ref('')
const searchDate = ref('')

const appointments = ref<any[]>([])
const loading = ref(false)
const error = ref<string | null>(null)

const statuses = ['All', 'Pending', 'Confirmed', 'Assigned', 'CheckedIn', 'Completed', 'Cancelled']
const currentStatus = ref('All')

const todayPrefix = () => new Date().toISOString().slice(0, 10)

const appointmentsTodayCount = computed(() => {
  const t = todayPrefix()
  return appointments.value.filter((a: any) => a.appointmentDate && String(a.appointmentDate).startsWith(t)).length
})

const statusCount = computed(() => {
  const m: Record<string, number> = {
    Pending: 0,
    Confirmed: 0,
    Assigned: 0,
    CheckedIn: 0,
    Completed: 0,
    Cancelled: 0
  }
  for (const a of appointments.value) {
    const s = a.status as string
    if (s && m[s] !== undefined) m[s]++
  }
  return m
})

const filteredAppointments = computed(() => {
  return appointments.value.filter((a: any) => {
    const matchStatus = currentStatus.value === 'All' || a.status === currentStatus.value
    const matchCode =
      !searchCode.value || a.appointmentCode?.toLowerCase().includes(searchCode.value.toLowerCase())
    const matchName =
      !searchName.value || a.fullName?.toLowerCase().includes(searchName.value.toLowerCase())
    const matchPhone = !searchPhone.value || a.phone?.includes(searchPhone.value)
    const matchDate = !searchDate.value || String(a.appointmentDate).startsWith(searchDate.value)
    return matchStatus && matchCode && matchName && matchPhone && matchDate
  })
})

const changeStatus = (s: string) => {
  currentStatus.value = s
}

const statusLabel = (status: string) => {
  const labels: Record<string, string> = {
    All: 'Tất cả',
    Pending: 'Chờ xử lý',
    Confirmed: 'Đã xác nhận',
    Assigned: 'Đã phân BS',
    CheckedIn: 'Đã check-in',
    Completed: 'Hoàn thành',
    Cancelled: 'Đã hủy'
  }
  return labels[status] || status
}

const statusLabelVi = (status: string) => statusLabel(status)

const statusBadgeClass = (status: string) => {
  switch (status) {
    case 'Pending':
      return 'text-bg-warning'
    case 'Confirmed':
      return 'text-bg-primary'
    case 'Assigned':
      return 'text-bg-info'
    case 'CheckedIn':
      return 'text-bg-info'
    case 'Completed':
      return 'text-bg-success'
    case 'Cancelled':
      return 'text-bg-danger'
    default:
      return 'text-bg-secondary'
  }
}

const clearFilters = () => {
  searchCode.value = ''
  searchName.value = ''
  searchPhone.value = ''
  searchDate.value = ''
}

const formatDateTime = (dateStr: string, timeStr: string) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  const parts = (timeStr || '').split(':')
  const hours = parts[0] ?? '00'
  const minutes = parts[1] ?? '00'
  const day = String(date.getDate()).padStart(2, '0')
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const year = date.getFullYear()
  return `${day}/${month}/${year} ${hours}:${minutes}`
}

const loadAppointments = async () => {
  loading.value = true
  error.value = null
  try {
    const role = auth.role
    if (role === 'Admin' || role === 'Staff') {
      const res = await api.get('/staff/StaffAppointments')
      appointments.value = res.data || []
    } else if (role === 'Doctor') {
      const res = await api.get('/doctor/DoctorAppointments')
      appointments.value = res.data || []
    } else {
      appointments.value = []
      error.value = 'Không có quyền xem danh sách lịch khám.'
    }
  } catch (err: any) {
    console.error('Load appointments error:', err)
    error.value = err?.response?.data?.message || 'Không tải được danh sách lịch khám.'
  } finally {
    loading.value = false
  }
}

const goDetail = (a: any) => {
  if (auth.role === 'Doctor') {
    router.push(`/doctor/examination/${a.id}`)
  } else {
    router.push(`/staff/appointments/${a.id}`)
  }
}

onMounted(() => {
  loadAppointments()
})
</script>

<style scoped>
.admin-appointment-page {
  max-width: 1280px;
  margin: 0 auto;
}
.text-monospace {
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
}
</style>
