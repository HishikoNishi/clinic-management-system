<template>
  <div class="container-fluid py-3">
    <div class="d-flex flex-wrap justify-content-between align-items-start gap-3 mb-4">
      <div>
        <h2 class="mb-1">Hàng chờ khám</h2>
        <p class="text-muted mb-0">Bác sĩ gọi bệnh nhân theo phòng và theo hàng chờ thực tế.</p>
      </div>

      <div class="d-flex flex-wrap gap-2 align-items-end">
        <div>
          <label class="form-label small fw-semibold mb-1">Trạng thái</label>
          <select v-model="doctorStatus" @change="updateDoctorStatus" class="form-select form-select-sm" style="min-width: 170px;">
            <option value="Active">Hoạt động</option>
            <option value="Busy">Đang khám</option>
            <option value="Inactive">Không hoạt động</option>
          </select>
        </div>
        <div>
          <label class="form-label small fw-semibold mb-1">Lọc lịch</label>
          <select v-model="currentStatus" class="form-select form-select-sm" style="min-width: 220px;" @change="loadAppointments">
            <option value="CheckedIn,Completed">Đã check-in + đã khám</option>
            <option value="CheckedIn">Đã check-in</option>
            <option value="Confirmed">Đã phân công (chưa check-in)</option>
            <option value="Completed">Đã khám xong</option>
            <option value="All">Tất cả</option>
          </select>
        </div>
        <button class="btn btn-outline-secondary btn-sm mt-4" @click="refreshAll">
          <i class="bi bi-arrow-clockwise me-1"></i>Làm mới
        </button>
      </div>
    </div>

    <div v-if="error" class="alert alert-danger py-2">{{ error }}</div>

    <div class="row g-3 mb-4">
      <div class="col-lg-4">
        <div class="card shadow-sm h-100">
          <div class="card-body">
            <div class="d-flex justify-content-between align-items-center mb-3">
              <h5 class="mb-0">Phòng của tôi</h5>
              <span class="badge bg-primary-subtle text-primary">{{ roomSummaries.length }}</span>
            </div>

            <div v-if="roomLoading" class="text-muted small">
              <span class="spinner-border spinner-border-sm me-2"></span>Đang tải phòng...
            </div>

            <div v-else class="d-grid gap-2">
              <button
                v-for="room in roomSummaries"
                :key="room.roomId"
                class="btn text-start room-btn"
                :class="selectedRoomId === room.roomId ? 'btn-primary' : 'btn-outline-primary'"
                @click="selectedRoomId = room.roomId"
              >
                <div class="fw-semibold">{{ room.roomName }}</div>
                <div class="small opacity-75">{{ room.roomCode }} · {{ room.departmentName }}</div>
                <div class="small mt-1">
                  Chờ: {{ room.waitingCount }} · Đang khám: {{ room.inProgressCount }}
                </div>
              </button>
              <div v-if="roomSummaries.length === 0" class="text-muted small">
                Chưa có phòng nào trong khoa của bác sĩ.
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="col-lg-8">
        <div class="card shadow-sm h-100">
          <div class="card-body">
            <div class="d-flex flex-wrap justify-content-between align-items-start gap-2 mb-3">
              <div>
                <h5 class="mb-1">Điều phối phòng</h5>
                <p class="text-muted small mb-0" v-if="selectedRoomState">
                  {{ selectedRoomState.roomName }} · {{ selectedRoomState.departmentName }}
                </p>
              </div>
              <div class="d-flex gap-2">
                <button class="btn btn-success btn-sm" :disabled="!selectedRoomId" @click="callNextPatient">
                  <i class="bi bi-megaphone me-1"></i>Gọi bệnh nhân tiếp theo
                </button>
                <button class="btn btn-outline-secondary btn-sm" :disabled="!selectedRoomId" @click="loadSelectedRoomQueue">
                  <i class="bi bi-arrow-clockwise"></i>
                </button>
              </div>
            </div>

            <div v-if="queueLoading" class="text-muted small">
              <span class="spinner-border spinner-border-sm me-2"></span>Đang tải hàng chờ...
            </div>

            <template v-else-if="selectedRoomState">
              <div class="card border-warning-subtle bg-warning-subtle mb-3" v-if="selectedRoomState.currentCalling">
                <div class="card-body">
                  <div class="d-flex flex-wrap justify-content-between align-items-start gap-3">
                    <div>
                      <div class="small text-uppercase text-muted">Đang gọi</div>
                      <h4 class="mb-1">#{{ selectedRoomState.currentCalling.queueNumber }} - {{ selectedRoomState.currentCalling.fullName }}</h4>
                      <div class="text-muted small">
                        {{ selectedRoomState.currentCalling.appointmentCode }} · {{ selectedRoomState.currentCalling.phone || '—' }}
                      </div>
                    </div>
                    <div class="d-flex flex-wrap gap-2">
                      <button class="btn btn-primary btn-sm" @click="goExamine(selectedRoomState.currentCalling.appointmentId)">
                        <i class="bi bi-stethoscope me-1"></i>Mở màn khám
                      </button>
                      <button class="btn btn-outline-success btn-sm" @click="markCurrentDone">
                        Hoàn tất lượt
                      </button>
                      <button class="btn btn-outline-danger btn-sm" @click="skipCurrent">
                        Bỏ lượt
                      </button>
                    </div>
                  </div>
                </div>
              </div>

              <div v-else class="alert alert-light border mb-3">
                Chưa có bệnh nhân nào đang được gọi ở phòng này.
              </div>

              <div class="table-responsive">
                <table class="table table-sm align-middle mb-0">
                  <thead>
                    <tr>
                      <th>STT</th>
                      <th>Bệnh nhân</th>
                      <th>Mã lịch</th>
                      <th>Ưu tiên</th>
                      <th>Thao tác</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="item in waitingItems" :key="item.id">
                      <td class="fw-semibold">#{{ item.queueNumber }}</td>
                      <td>
                        <div class="fw-semibold">{{ item.fullName }}</div>
                        <div class="text-muted small">{{ item.phone || '—' }}</div>
                      </td>
                      <td class="text-monospace">{{ item.appointmentCode }}</td>
                      <td>
                        <span class="badge" :class="item.isPriority ? 'bg-success-subtle text-success' : 'bg-secondary-subtle text-secondary'">
                          {{ item.isPriority ? 'Đặt trước' : 'Tại quầy' }}
                        </span>
                      </td>
                      <td>
                        <button class="btn btn-outline-primary btn-sm" @click="goExamine(item.appointmentId)">
                          Xem hồ sơ
                        </button>
                      </td>
                    </tr>
                    <tr v-if="waitingItems.length === 0">
                      <td colspan="5" class="text-center text-muted py-3">Không còn bệnh nhân đang chờ trong phòng này.</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </template>

            <div v-else class="text-muted small">
              Chọn một phòng để xem hàng chờ.
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="card shadow-sm">
      <div class="card-body p-0">
        <div class="d-flex justify-content-between align-items-center px-3 pt-3">
          <div>
            <h5 class="mb-1">Danh sách lịch được phân công</h5>
            <p class="text-muted small mb-0">Giữ lại danh sách lịch cũ để bác sĩ tra cứu nhanh các ca đã/đang xử lý.</p>
          </div>
        </div>

        <div class="table-responsive">
          <table class="table mb-0 align-middle">
            <thead>
              <tr>
                <th>Bệnh nhân</th>
                <th>Điện thoại</th>
                <th>Mã bệnh nhân</th>
                <th>CCCD</th>
                <th>BHYT</th>
                <th>Ngày khám</th>
                <th>Trạng thái</th>
                <th class="text-end">Thao tác</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="loading">
                <td colspan="8" class="text-center py-4 text-muted">
                  <span class="spinner-border spinner-border-sm me-2"></span>Đang tải...
                </td>
              </tr>
              <tr v-else-if="filteredAppointments.length === 0">
                <td colspan="8" class="text-center py-4 text-muted">Không có lịch khám</td>
              </tr>
              <tr v-else v-for="a in filteredAppointments" :key="a.id">
                <td class="fw-semibold">{{ a.fullName }}</td>
                <td>{{ a.phone }}</td>
                <td>{{ a.patientCode || '—' }}</td>
                <td>{{ a.citizenId || '—' }}</td>
                <td>{{ a.insuranceCardNumber || '—' }}</td>
                <td>{{ formatDateTime(a.appointmentDate, a.appointmentTime) }}</td>
                <td>
                  <span :class="['badge', badgeClass(a.status)]">{{ statusLabel(a.status) }}</span>
                </td>
                <td class="text-end">
                  <button class="btn btn-primary btn-sm" @click="goExamine(a.id)">
                    <i class="bi bi-stethoscope me-1"></i>
                    {{ a.status === 'Completed' ? 'Xem hồ sơ khám' : 'Khám / tiếp tục' }}
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
// Mặc định chỉ hiển thị ca đã check-in hoặc đã khám xong.
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

onMounted(async () => {
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
.room-btn {
  white-space: normal;
}
</style>
