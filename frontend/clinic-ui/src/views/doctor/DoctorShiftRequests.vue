<template>
  <div class="container py-4 doctor-shift-page">
    <div class="d-flex flex-wrap justify-content-between align-items-center gap-3 mb-4">
      <div>
        <h1 class="h3 mb-1">Yêu cầu nghỉ / chuyển ca</h1>
        <p class="text-muted mb-0">Gửi yêu cầu cho admin và theo dõi phản hồi tự động mỗi 15 giây.</p>
      </div>
      <button class="btn btn-primary" type="button" @click="openForm">
        Tạo yêu cầu mới
      </button>
    </div>

    <div v-if="error" class="alert alert-danger">{{ error }}</div>
    <div v-if="success" class="alert alert-success">{{ success }}</div>

    <div class="row g-4">
      <div class="col-lg-7">
        <div class="card shadow-sm">
          <div class="card-header bg-white d-flex justify-content-between align-items-center">
            <strong>Yêu cầu của tôi</strong>
            <span class="badge bg-light text-dark">{{ requests.length }} yêu cầu</span>
          </div>
          <div class="card-body p-0">
            <div v-if="loadingRequests" class="p-4 text-center text-muted">Đang tải dữ liệu...</div>
            <div v-else-if="!requests.length" class="p-4 text-center text-muted">
              Chưa có yêu cầu nào.
            </div>
            <div v-else class="list-group list-group-flush">
              <div v-for="item in requests" :key="item.id" class="list-group-item">
                <div class="d-flex flex-wrap justify-content-between gap-2">
                  <div>
                    <div class="fw-semibold">
                      {{ requestTypeLabel(item.requestType) }} - {{ formatDate(item.workDate) }} - {{ item.slotLabel }}
                    </div>
                    <div class="text-muted small">
                      {{ item.startTime }} - {{ item.endTime }} | {{ item.appointmentCount }} lịch bị ảnh hưởng
                    </div>
                  </div>
                  <span class="badge" :class="statusBadge(item.status)">
                    {{ statusLabel(item.status) }}
                  </span>
                </div>
                <div class="small mt-2">
                  <div><strong>Lý do:</strong> {{ item.reason }}</div>
                  <div v-if="item.preferredDoctorName"><strong>BS đề xuất:</strong> {{ item.preferredDoctorName }}</div>
                  <div v-if="item.replacementDoctorName"><strong>BS thay thế:</strong> {{ item.replacementDoctorName }}</div>
                  <div v-if="item.adminNote"><strong>Phản hồi admin:</strong> {{ item.adminNote }}</div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="col-lg-5">
        <div class="card shadow-sm">
          <div class="card-header bg-white d-flex justify-content-between align-items-center">
            <strong>Thông báo</strong>
            <span class="badge bg-danger" v-if="unreadCount">{{ unreadCount }}</span>
          </div>
          <div class="card-body p-0">
            <div v-if="loadingNotifications" class="p-4 text-center text-muted">Đang tải thông báo...</div>
            <div v-else-if="!notifications.length" class="p-4 text-center text-muted">
              Chưa có thông báo nào.
            </div>
            <div v-else class="list-group list-group-flush">
              <div
                v-for="item in notifications"
                :key="item.id"
                class="list-group-item d-flex justify-content-between gap-3"
                :class="{ 'bg-light': !item.isRead }"
              >
                <div>
                  <div class="fw-semibold">{{ normalizeNotificationText(item.title) }}</div>
                  <div class="small text-muted">{{ normalizeNotificationText(item.message) }}</div>
                  <div class="small text-muted mt-1">{{ formatDateTime(item.createdAt) }}</div>
                </div>
                <button
                  v-if="!item.isRead"
                  class="btn btn-sm btn-outline-secondary align-self-start"
                  type="button"
                  @click="markAsRead(item.id)"
                >
                  Đã đọc
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div v-if="showForm" class="modal-backdrop-custom">
      <div class="modal-card">
        <div class="d-flex justify-content-between align-items-center mb-3">
          <h2 class="h5 mb-0">Tạo yêu cầu mới</h2>
          <button class="btn-close" type="button" @click="closeForm" />
        </div>

        <div class="row g-3">
          <div class="col-md-6">
            <label class="form-label">Loại yêu cầu


            </label>
            <select v-model="form.requestType" class="form-select">
              <option value="EmergencyLeave">Nghỉ đột xuất</option>
              <option value="ShiftTransfer">Xin chuyển ca</option>
            </select>
          </div>

          <div class="col-md-6">
            <label class="form-label">Ngày làm việc</label>
            <input v-model="form.workDate" type="date" class="form-control" @change="loadOwnSlots" />
          </div>

          <div class="col-12">
            <label class="form-label">Slot</label>
            <select v-model="form.startTime" class="form-select" :disabled="loadingSlots || !slotOptions.length">
              <option value="">Chọn slot</option>
              <option v-for="slot in slotOptions" :key="slot.id" :value="slot.startTime">
                {{ slot.slotLabel }} ({{ slot.startTime }} - {{ slot.endTime }})
              </option>
            </select>
            <div class="form-text" v-if="loadingSlots">Đang tải slot của bác sĩ...</div>
            <div class="form-text" v-else-if="form.workDate && !slotOptions.length">Không có slot hợp lệ cho ngày này.</div>
          </div>

          <div class="col-12">
            <label class="form-label">Lý do</label>
            <textarea v-model="form.reason" class="form-control" rows="4" maxlength="1000" />
          </div>
        </div>

        <div class="d-flex justify-content-end gap-2 mt-4">
          <button class="btn btn-light" type="button" @click="closeForm">Hủy</button>
          <button class="btn btn-primary" type="button" :disabled="submitting" @click="submitRequest">
            <span v-if="submitting" class="spinner-border spinner-border-sm me-2"></span>
            Gửi yêu cầu
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, reactive, ref } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { doctorScheduleService, type DoctorScheduleSlot } from '@/services/doctorScheduleService'
import { toLocalDateInputValue } from '@/utils/date'
import {
  shiftRequestService,
  type DoctorNotification,
  type ShiftRequestItem,
  type ShiftRequestStatus,
  type ShiftRequestType
} from '@/services/shiftRequestService'

const authStore = useAuthStore()

const requests = ref<ShiftRequestItem[]>([])
const notifications = ref<DoctorNotification[]>([])
const unreadCount = ref(0)
const slotOptions = ref<DoctorScheduleSlot[]>([])

const loadingRequests = ref(false)
const loadingNotifications = ref(false)
const loadingSlots = ref(false)
const submitting = ref(false)
const showForm = ref(false)
const error = ref('')
const success = ref('')
let pollTimer: ReturnType<typeof setInterval> | null = null

const getToday = () => toLocalDateInputValue()

const form = reactive<{
  requestType: ShiftRequestType
  workDate: string
  startTime: string
  reason: string
}>({
  requestType: 'EmergencyLeave',
  workDate: getToday(),
  startTime: '',
  reason: ''
})

const doctorId = computed(() => authStore.doctorId || '')

const openForm = async () => {
  error.value = ''
  success.value = ''
  form.workDate = getToday()
  form.startTime = ''
  showForm.value = true
  await loadOwnSlots()
}

const closeForm = () => {
  showForm.value = false
}

const loadOwnSlots = async () => {
  if (!doctorId.value || !form.workDate) {
    slotOptions.value = []
    return
  }

  loadingSlots.value = true
  try {
    const slots = await doctorScheduleService.getDoctorDay(doctorId.value, form.workDate)
    const now = new Date()
    slotOptions.value = slots.filter((slot) => {
      if (form.workDate !== getToday()) return true
      const [hour = 0, minute = 0] = slot.startTime.split(':').map(Number)
      const slotDate = new Date(now)
      slotDate.setHours(hour, minute, 0, 0)
      return slotDate.getTime() > now.getTime()
    })

    if (!slotOptions.value.some((slot) => slot.startTime === form.startTime)) {
      form.startTime = ''
    }
  } catch (err: any) {
    console.error(err)
    error.value = err?.response?.data?.message || 'Không tải được danh sách slot.'
    slotOptions.value = []
  } finally {
    loadingSlots.value = false
  }
}

const loadRequests = async () => {
  loadingRequests.value = true
  try {
    requests.value = await shiftRequestService.getMyRequests()
  } catch (err: any) {
    console.error(err)
    error.value = err?.response?.data?.message || 'Không tải được danh sách yêu cầu.'
  } finally {
    loadingRequests.value = false
  }
}

const loadNotifications = async () => {
  loadingNotifications.value = true
  try {
    const response = await shiftRequestService.getDoctorNotifications(10)
    notifications.value = response.items
    unreadCount.value = response.unreadCount
  } catch (err: any) {
    console.error(err)
    error.value = err?.response?.data?.message || 'Không tải được thông báo.'
  } finally {
    loadingNotifications.value = false
  }
}

const submitRequest = async () => {
  error.value = ''
  success.value = ''

  if (!form.workDate || !form.startTime || !form.reason.trim()) {
    error.value = 'Vui lòng chọn ngày, slot và nhập lý do.'
    return
  }

  submitting.value = true
  try {
    await shiftRequestService.createRequest({
      requestType: form.requestType,
      workDate: form.workDate,
      startTime: form.startTime,
      reason: form.reason.trim()
    })

    success.value = 'Đã gửi yêu cầu cho Admin.'
    form.startTime = ''
    form.reason = ''
    showForm.value = false
    await Promise.all([loadRequests(), loadNotifications()])
  } catch (err: any) {
    console.error(err)
    error.value = err?.response?.data?.message || 'Gửi yêu cầu thất bại.'
  } finally {
    submitting.value = false
  }
}

const markAsRead = async (id: string) => {
  try {
    await shiftRequestService.markNotificationAsRead(id)
    await loadNotifications()
  } catch (err) {
    console.error(err)
  }
}

const requestTypeLabel = (type: ShiftRequestType) => {
  return type === 'EmergencyLeave' ? 'Nghỉ đột xuất' : 'Xin chuyển ca'
}

const statusLabel = (status: ShiftRequestStatus) => {
  if (status === 'Approved') return 'Đã duyệt'
  if (status === 'Rejected') return 'Đã từ chối'
  return 'Đang chờ'
}

const statusBadge = (status: ShiftRequestStatus) => {
  if (status === 'Approved') return 'bg-success-subtle text-success'
  if (status === 'Rejected') return 'bg-danger-subtle text-danger'
  return 'bg-warning-subtle text-warning'
}

const formatDate = (value: string) => {
  return new Date(value).toLocaleDateString('vi-VN')
}

const normalizeNotificationText = (text = '') => {
  return String(text)
    .replace(/Yeu\s*cau/gi, 'Yêu cầu')
    .replace(/doi\s*ca\s*duoc/gi, 'đổi ca được')
    .replace(/da\s*duoc/gi, 'đã được')
    .replace(/chap\s*thuan/gi, 'chấp thuận')
    .replace(/bi\s*tu\s*choi/gi, 'bị từ chối')
    .replace(/Bac\s*si\s*thay\s*the/gi, 'Bác sĩ thay thế')
    .replace(/Bac\s*si/gi, 'Bác sĩ')
    .replace(/ngay/gi, 'ngày')
    .replace(/Da\s*doc/gi, 'Đã đọc')
}

const formatDateTime = (value: string) => {
  return new Date(value).toLocaleString('vi-VN')
}

onMounted(async () => {
  await Promise.all([loadRequests(), loadNotifications()])
  pollTimer = setInterval(() => {
    loadRequests()
    loadNotifications()
  }, 15000)
})

onUnmounted(() => {
  if (pollTimer) {
    clearInterval(pollTimer)
  }
})
</script>

<style scoped>
.doctor-shift-page .card {
  border: 1px solid #e9ecef;
}

.modal-backdrop-custom {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1050;
  padding: 1rem;
}

.modal-card {
  width: min(720px, 100%);
  background: #fff;
  border-radius: 1rem;
  padding: 1.5rem;
  box-shadow: 0 24px 60px rgba(15, 23, 42, 0.2);
}
</style>
