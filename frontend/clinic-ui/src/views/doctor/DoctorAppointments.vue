<template>
  <div class="exam-queue-container bg-light min-vh-100 pb-5">
    <div class="glass-header sticky-top bg-white border-bottom shadow-sm py-3 mb-4">
      <div class="container-fluid px-4">
        <div class="d-flex flex-wrap justify-content-between align-items-center gap-3">
          
          <div class="flex-shrink-0">
            <h3 class="fw-bold mb-0 text-primary"><i class="bi bi-person-lines-fill me-2"></i>Hàng chờ khám</h3>
            <p class="text-muted small mb-0">Hệ thống điều phối phòng khám</p>
          </div>

          <div class="d-flex flex-column align-items-center flex-grow-1">
            <div class="status-box" style="min-width: 280px;">
              <span class="x-small fw-bold text-uppercase text-primary mb-1 d-block text-center">
                <i class="bi bi-person-badge-fill me-1"></i>Trạng thái của bạn
              </span>
              <div class="status-control p-1 bg-white rounded-pill border border-primary shadow-sm">
                <select v-model="doctorStatus" @change="updateDoctorStatus" 
                  class="form-select form-select-sm border-0 fw-bold text-center" 
                  style="border-radius: 50px; background-position: right 15px center;">
                  <option value="Active">🟢 Sẵn sàng làm việc</option>
                  <option value="Busy">🟡 Đang bận khám</option>
                  <option value="Inactive">🔴 Tạm nghỉ / Vắng mặt</option>
                </select>
              </div>
              <span class="x-small text-muted mt-1 d-block text-center italic">Chỉnh trạng thái hoạt động của bác sĩ</span>
            </div>
          </div>

          <div class="d-flex flex-wrap gap-3 align-items-end flex-shrink-0">
            <div class="d-flex flex-column">
              <span class="x-small fw-bold text-uppercase text-muted mb-1">
                <i class="bi bi-funnel-fill me-1"></i>Lọc danh sách
              </span>
              <div class="filter-control p-1 bg-white rounded-3 border">
                <select v-model="currentStatus" class="form-select form-select-sm border-0" 
                  style="min-width: 200px;" @change="loadAppointments">
                  <option value="CheckedIn,Completed">Đã check-in + đã khám</option>
                  <option value="CheckedIn">Đã check-in</option>
                  <option value="Confirmed">Đã phân công (chưa check-in)</option>
                  <option value="Completed">Đã khám xong</option>
                  <option value="All">Tất cả</option>
                </select>
              </div>
              <span class="x-small text-muted mt-1 d-block italic">Lọc hiển thị bảng bên dưới</span>
            </div>

            <button class="btn btn-outline-secondary btn-sm px-3 rounded-pill shadow-sm mb-4" @click="refreshAll" style="height: 38px;">
              <i class="bi bi-arrow-clockwise"></i>
            </button>
          </div>
        </div>
      </div>
    </div>

    <div class="container-fluid px-4">
      <div v-if="error" class="alert alert-danger border-0 shadow-sm mb-4 animate__animated animate__shakeX">
        <i class="bi bi-exclamation-triangle me-2"></i>{{ error }}
      </div>

      <div class="row g-4">
        <div class="col-xl-4 col-lg-5">
          <div class="card border-0 shadow-sm rounded-4 overflow-hidden h-100">
            <div class="card-header bg-white border-0 py-3 px-4 border-bottom">
              <div class="d-flex justify-content-between align-items-center">
                <h6 class="mb-0 fw-bold text-dark"><i class="bi bi-door-open me-2 text-primary"></i>Phòng của tôi</h6>
                <span class="badge rounded-pill bg-primary px-3">{{ roomSummaries.length }} phòng</span>
              </div>
            </div>
            <div class="card-body px-3 pb-4">
              <div v-if="roomLoading" class="text-center py-5">
                <div class="spinner-border spinner-border-sm text-primary"></div>
              </div>
              <div v-else class="d-grid gap-3">
                <div
                  v-for="room in roomSummaries"
                  :key="room.roomId"
                  class="room-card p-3 rounded-4 border-2 transition-all cursor-pointer shadow-sm"
                  :class="selectedRoomId === room.roomId ? 'border-primary bg-primary-subtle' : 'border-light bg-white'"
                  @click="selectedRoomId = room.roomId"
                >
                  <div class="d-flex justify-content-between align-items-start mb-2">
                    <span class="fw-bold fs-5" :class="selectedRoomId === room.roomId ? 'text-primary' : 'text-dark'">{{ room.roomName }}</span>
                    <span class="badge bg-white text-dark shadow-sm border">{{ room.roomCode }}</span>
                  </div>
                  <div class="small text-muted mb-3">{{ room.departmentName }}</div>
                  <div class="d-flex gap-2">
                    <div class="flex-fill bg-white p-2 rounded-3 text-center border shadow-sm">
                      <div class="x-small text-muted fw-bold text-uppercase">Chờ</div>
                      <div class="fw-bold text-primary fs-5">{{ room.waitingCount }}</div>
                    </div>
                    <div class="flex-fill bg-white p-2 rounded-3 text-center border shadow-sm">
                      <div class="x-small text-muted fw-bold text-uppercase">Khám</div>
                      <div class="fw-bold text-success fs-5">{{ room.inProgressCount }}</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="col-xl-8 col-lg-7">
          <div class="card border-0 shadow-sm rounded-4 h-100">
            <div class="card-header bg-white border-0 py-3 px-4 d-flex justify-content-between align-items-center border-bottom">
              <div>
                <h6 class="mb-0 fw-bold text-dark">
                  <i class="bi bi-cpu me-2 text-primary"></i>Điều phối: 
                  <span class="text-primary fw-bold" v-if="selectedRoomState">{{ selectedRoomState.roomName }}</span>
                </h6>
              </div>
              <div class="d-flex gap-2">
                <button class="btn btn-success btn-sm rounded-pill px-4 shadow-sm fw-bold" :disabled="!selectedRoomId" @click="callNextPatient">
                  <i class="bi bi-megaphone-fill me-1"></i> Gọi tiếp theo
                </button>
                <button class="btn btn-light btn-sm rounded-circle border shadow-sm" :disabled="!selectedRoomId" @click="loadSelectedRoomQueue">
                  <i class="bi bi-arrow-clockwise"></i>
                </button>
              </div>
            </div>

            <div class="card-body px-4">
              <div v-if="queueLoading" class="text-center py-5">
                <div class="spinner-border text-primary"></div>
              </div>

              <template v-else-if="selectedRoomState">
                <div class="calling-card p-4 rounded-4 mb-4 animate__animated animate__fadeInDown" v-if="selectedRoomState.currentCalling">
                   <div class="row align-items-center">
                      <div class="col-md-7 d-flex align-items-center gap-3">
                          <div class="queue-number-lg shadow">#{{ selectedRoomState.currentCalling.queueNumber }}</div>
                          <div>
                            <div class="small text-uppercase text-primary fw-bold">Bệnh nhân đang gọi</div>
                            <h3 class="fw-bold mb-1 text-dark">{{ selectedRoomState.currentCalling.fullName }}</h3>
                            <div class="text-muted small">Mã: {{ selectedRoomState.currentCalling.appointmentCode }}</div>
                          </div>
                      </div>
                      <div class="col-md-5 d-flex justify-content-md-end gap-2">
                        <button class="btn btn-primary rounded-pill px-4 fw-bold shadow-sm" @click="goExamine(selectedRoomState.currentCalling.appointmentId)">
                          <i class="bi bi-stethoscope me-1"></i> Khám bệnh
                        </button>
                        <div class="dropdown">
                          <button class="btn btn-outline-secondary rounded-pill shadow-sm px-3" data-bs-toggle="dropdown">Xử lý</button>
                          <ul class="dropdown-menu dropdown-menu-end shadow border-0 p-2 rounded-3">
                            <li><a class="dropdown-item rounded-2 text-success fw-medium" @click="markCurrentDone"><i class="bi bi-check-circle me-2"></i>Hoàn tất</a></li>
                            <li><a class="dropdown-item rounded-2 text-danger fw-medium" @click="skipCurrent"><i class="bi bi-slash-circle me-2"></i>Bỏ lượt</a></li>
                          </ul>
                        </div>
                      </div>
                   </div>
                </div>

                <div class="table-responsive custom-scrollbar" style="max-height: 400px;">
                  <table class="table table-hover align-middle">
                    <thead class="sticky-top bg-white shadow-sm">
                      <tr class="x-small text-uppercase text-muted fw-bold">
                        <th class="border-0 px-3">STT</th>
                        <th class="border-0">Bệnh nhân</th>
                        <th class="border-0">Mã lịch</th>
                        <th class="border-0">Ưu tiên</th>
                        <th class="border-0 text-end px-3">Thao tác</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="item in waitingItems" :key="item.id">
                        <td class="fw-bold text-primary fs-5 px-3">#{{ item.queueNumber }}</td>
                        <td>
                          <div class="fw-bold text-dark">{{ item.fullName }}</div>
                          <div class="text-muted small">{{ item.phone || '—' }}</div>
                        </td>
                        <td class="small font-monospace">{{ item.appointmentCode }}</td>
                        <td>
                          <span :class="['badge rounded-pill px-3', item.isPriority ? 'bg-success-subtle text-success' : 'bg-secondary-subtle text-secondary']">
                            {{ item.isPriority ? 'Ưu tiên' : 'Tại quầy' }}
                          </span>
                        </td>
                        <td class="text-end px-3">
                          <button class="btn btn-outline-primary btn-sm rounded-pill px-3 shadow-sm" @click="goExamine(item.appointmentId)">Khám</button>
                        </td>
                      </tr>
                      <tr v-if="waitingItems.length === 0">
                        <td colspan="5" class="text-center py-5 text-muted">Hàng chờ trống.</td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </template>
            </div>
          </div>
        </div>
      </div>

      <div class="card border-0 shadow-sm rounded-4 mt-5 overflow-hidden">
        <div class="card-header bg-white border-0 py-3 px-4 d-flex justify-content-between align-items-center border-bottom">
            <h5 class="mb-0 fw-bold text-dark"><i class="bi bi-list-stars me-2 text-primary"></i>Toàn bộ danh sách ca khám</h5>
            <span class="small text-muted italic">Dữ liệu theo bộ lọc phía trên</span>
        </div>
        <div class="table-responsive">
          <table class="table table-hover mb-0 align-middle">
            <thead class="table-light">
              <tr class="x-small text-uppercase text-muted">
                <th class="px-4">Bệnh nhân</th>
                <th>Liên hệ</th>
                <th>Mã BN</th>
                <th>CCCD / BHYT</th>
                <th>Ngày khám</th>
                <th>Trạng thái</th>
                <th class="text-end px-4">Thao tác</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="a in filteredAppointments" :key="a.id">
                <td class="px-4 fw-bold text-dark">{{ a.fullName }}</td>
                <td>{{ a.phone }}</td>
                <td><span class="badge bg-light text-dark border">{{ a.patientCode || '—' }}</span></td>
                <td>
                   <div class="x-small fw-bold">BHYT: {{ a.insuranceCardNumber || '—' }}</div>
                   <div class="x-small text-muted">CCCD: {{ a.citizenId || '—' }}</div>
                </td>
                <td class="small">{{ formatDateTime(a.appointmentDate, a.appointmentTime) }}</td>
                <td>
                  <span :class="['badge rounded-pill px-3 py-2', badgeClass(a.status)]">{{ statusLabel(a.status) }}</span>
                </td>
                <td class="text-end px-4">
                  <button class="btn btn-primary btn-sm rounded-pill px-3 shadow-sm" @click="goExamine(a.id)">
                    <i class="bi bi-stethoscope me-1"></i> {{ a.status === 'Completed' ? 'Xem lại' : 'Tiếp tục' }}
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>



<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from "vue"
import { useRouter } from "vue-router"
import api from "@/services/api"
import { queueService, type DoctorRoomSummary, type RoomQueueState } from "@/services/queueService"

const router = useRouter()
const appointments = ref<any[]>([])
const loading = ref(false)
const error = ref<string | null>(null)
const currentStatus = ref("CheckedIn,Completed")
const doctorStatus = ref("Active")
const doctorId = localStorage.getItem("doctorId")

const roomSummaries = ref<DoctorRoomSummary[]>([])
const selectedRoomId = ref("")
const selectedRoomState = ref<RoomQueueState | null>(null)
const roomLoading = ref(false)
const queueLoading = ref(false)
let refreshTimer: ReturnType<typeof setInterval> | null = null

const filteredAppointments = computed(() => {
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  return appointments.value.filter((a) => {
    if (!a.appointmentDate) return true
    const d = new Date(a.appointmentDate)
    d.setHours(0, 0, 0, 0)
    return d.getTime() >= today.getTime() || a.status === "Completed"
  })
})

const waitingItems = computed(() =>
  (selectedRoomState.value?.items || []).filter((item) => item.status === "Waiting")
)

async function updateDoctorStatus() {
  if (!doctorId) {
    error.value = "Không tìm thấy ID bác sĩ"
    return
  }
  try {
    await api.put(`/doctor/${doctorId}/status`, {
      Status: doctorStatus.value
    })
  } catch (err: any) {
    console.error("Update doctor status error:", err.response?.data || err)
    error.value = "Không thể cập nhật trạng thái bác sĩ"
  }
}

const loadAppointments = async () => {
  loading.value = true
  error.value = null
  appointments.value = []
  try {
    const params = currentStatus.value === "All" ? {} : { status: currentStatus.value }
    const res = await api.get("/doctor/DoctorAppointments", { params })
    appointments.value = res.data ?? []
  } catch (err) {
    console.error(err)
    error.value = "Không tải được dữ liệu lịch khám."
  } finally {
    loading.value = false
  }
}

const loadDoctorRooms = async () => {
  try {
    roomLoading.value = true
    roomSummaries.value = await queueService.getDoctorRooms()

    if (!selectedRoomId.value && roomSummaries.value.length > 0) {
      const preferredRoom = roomSummaries.value.find((room) => room.totalToday > 0)
      selectedRoomId.value = preferredRoom?.roomId || roomSummaries.value[0]?.roomId || ""
    }
  } catch (err) {
    console.error(err)
    error.value = "Không tải được danh sách phòng."
  } finally {
    roomLoading.value = false
  }
}

const loadSelectedRoomQueue = async () => {
  if (!selectedRoomId.value) {
    selectedRoomState.value = null
    return
  }

  try {
    queueLoading.value = true
    selectedRoomState.value = await queueService.getRoomQueue(selectedRoomId.value)
  } catch (err: any) {
    if (err?.response?.status === 404) {
      selectedRoomState.value = null
      return
    }
    console.error(err)
    error.value = "Không tải được hàng chờ của phòng."
  } finally {
    queueLoading.value = false
  }
}

const callNextPatient = async () => {
  if (!selectedRoomId.value) return
  try {
    const called = await queueService.callNext(selectedRoomId.value)
    await refreshAll()
    alert(`Đang gọi bệnh nhân #${called.queueNumber} - ${called.fullName}`)
  } catch (err: any) {
    const msg = err?.response?.data?.message || "Không thể gọi bệnh nhân tiếp theo"
    alert(msg)
  }
}

const markCurrentDone = async () => {
  const current = selectedRoomState.value?.currentCalling
  if (!current) return
  try {
    await queueService.markDone(current.id)
    await refreshAll()
  } catch (err: any) {
    alert(err?.response?.data?.message || "Không thể hoàn tất lượt hiện tại")
  }
}

const skipCurrent = async () => {
  const current = selectedRoomState.value?.currentCalling
  if (!current) return
  try {
    await queueService.markSkipped(current.id)
    await refreshAll()
  } catch (err: any) {
    alert(err?.response?.data?.message || "Không thể bỏ lượt hiện tại")
  }
}

const refreshAll = async () => {
  await Promise.all([loadAppointments(), loadDoctorRooms(), loadSelectedRoomQueue()])
}

const goExamine = (id: string) => {
  router.push(`/doctor/examination/${id}`)
}

const statusLabel = (status: string) => {
  const labels: Record<string, string> = {
    CheckedIn: "Đã check-in",
    Confirmed: "Chờ khám",
    Completed: "Đã khám xong"
  }
  return labels[status] || status
}

const badgeClass = (status: string) => {
  if (status === "CheckedIn") return "bg-info-subtle text-info"
  if (status === "Confirmed") return "bg-warning-subtle text-warning"
  if (status === "Completed") return "bg-success-subtle text-success"
  return "bg-secondary-subtle text-secondary"
}

const formatDateTime = (dateStr: string, timeStr: string) => {
  if (!dateStr || !timeStr) return ""
  const date = new Date(dateStr)
  const [hours, minutes] = timeStr.split(":")
  const day = String(date.getDate()).padStart(2, "0")
  const month = String(date.getMonth() + 1).padStart(2, "0")
  const year = date.getFullYear()
  return `${day}/${month}/${year} ${hours}:${minutes}`
}

watch(selectedRoomId, () => {
  loadSelectedRoomQueue()
})

const loadCurrentDoctorStatus = async () => {
  if (!doctorId) return
  try {
    const res = await api.get(`/doctor/${doctorId}`) 
    if (res.data && res.data.status) {
      doctorStatus.value = res.data.status
    }
  } catch (err) {
    console.error("Không thể lấy trạng thái hiện tại của bác sĩ")
  }
}

onMounted(async () => {
  await loadCurrentDoctorStatus() 
  await refreshAll()
  refreshTimer = setInterval(() => {
    loadDoctorRooms()
    loadSelectedRoomQueue()
  }, 10000)
})

onBeforeUnmount(() => {
  if (refreshTimer) {
    clearInterval(refreshTimer)
  }
})
</script>
<style scoped>
.exam-queue-container {
  font-family: 'Inter', system-ui, -apple-system, sans-serif;
  background-color: #f8fafc; /* Màu nền nhẹ nhàng hơn */
}

.x-small { 
  font-size: 0.68rem; 
  text-transform: uppercase; 
  font-weight: 700; 
  letter-spacing: 0.8px; 
  opacity: 0.8;
}

.transition-all { transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1); }
.cursor-pointer { cursor: pointer; }
.border-dashed { border: 2px dashed #cbd5e1 !important; background: transparent; }

.glass-header {
  backdrop-filter: blur(12px) saturate(180%);
  -webkit-backdrop-filter: blur(12px) saturate(180%);
  background-color: rgba(255, 255, 255, 0.8) !important;
  border-bottom: 1px solid rgba(226, 232, 240, 0.8) !important;
  z-index: 1020;
}

.room-card {
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  border: 2px solid transparent !important;
  position: relative;
  overflow: hidden;
}

.room-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 10px 20px rgba(0, 0, 0, 0.05) !important;
  border-color: #e2e8f0 !important;
}

.room-card.border-primary {
  background: white !important;
  border-color: #3b82f6 !important;
  box-shadow: 0 8px 25px rgba(59, 130, 246, 0.12) !important;
}

.room-card.border-primary::after {
  content: '';
  position: absolute;
  top: 0; right: 0;
  width: 40px; height: 40px;
  background: linear-gradient(135deg, transparent 50%, rgba(59, 130, 246, 0.1) 50%);
}

.calling-card {
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-left: 6px solid #3b82f6;
  border-radius: 1.25rem !important;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.03);
  position: relative;
}

.queue-number-lg {
  width: 80px;
  height: 80px;
  background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
  color: white;
  border-radius: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2rem;
  font-weight: 800;
  box-shadow: 0 8px 16px rgba(59, 130, 246, 0.25);
  text-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.status-control, .filter-control {
  transition: border-color 0.2s;
}

.status-control:focus-within, .filter-control:focus-within {
  border-color: #3b82f6 !important;
}

.form-select-sm {
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;
  box-shadow: none !important;
  color: #334155;
}

.badge {
  font-weight: 700;
  padding: 0.5em 1em;
  letter-spacing: 0.3px;
}

.bg-success-subtle { background-color: #ecfdf5 !important; color: #059669 !important; border: 1px solid #d1fae5; }
.bg-info-subtle { background-color: #f0f9ff !important; color: #0284c7 !important; border: 1px solid #e0f2fe; }
.bg-warning-subtle { background-color: #fffbeb !important; color: #d97706 !important; border: 1px solid #fef3c7; }
.bg-primary-subtle { background-color: #eff6ff !important; color: #2563eb !important; }

.table thead th {
  background-color: #f8fafc;
  color: #64748b;
  font-weight: 700;
  border-top: none;
  padding: 1rem 0.75rem;
}

.table tbody tr {
  transition: background-color 0.2s;
}

.table tbody tr:hover {
  background-color: #fcfdfe !important;
}

.custom-scrollbar::-webkit-scrollbar { width: 5px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { 
  background: #cbd5e1; 
  border-radius: 10px; 
}
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #94a3b8; }

/* Animation */
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

.card {
  animation: fadeIn 0.4s ease-out;
}
</style>