<template>
  <div class="container py-4">
    <div class="d-flex flex-wrap justify-content-between align-items-center gap-3 mb-4">
      <div>
          <h1 class="h3 mb-1">Yêu cầu đổi ca của bác sĩ</h1>
          <p class="text-muted mb-0">Admin duyệt hoặc từ chối, sau đó hệ thống thông báo lại cho bác sĩ.</p>
        </div>
        <button class="btn btn-outline-secondary" type="button" @click="loadRequests">
          Tải lại
        </button>
    </div>

    <div v-if="error" class="alert alert-danger">{{ error }}</div>
    <div v-if="success" class="alert alert-success">{{ success }}</div>

    <div class="row g-4">
      <div class="col-lg-5">
        <div class="card shadow-sm">
          <div class="card-header bg-white">
            <strong>Danh sách yêu cầu</strong>
          </div>
          <div class="list-group list-group-flush request-list">
            <button
              v-for="item in requests"
              :key="item.id"
              type="button"
              class="list-group-item list-group-item-action text-start"
              :class="{ active: selectedRequest?.id === item.id }"
              @click="selectRequest(item.id)"
            >
              <div class="d-flex justify-content-between align-items-center gap-2">
                <div class="fw-semibold">{{ item.doctorName }}</div>
                <span class="badge" :class="statusBadge(item.status)">{{ statusLabel(item.status) }}</span>
              </div>
              <div class="small">{{ requestTypeLabel(item.requestType) }} - {{ formatDate(item.workDate) }}</div>
              <div class="small text-muted">{{ item.slotLabel }} ({{ item.startTime }} - {{ item.endTime }})</div>
            </button>
            <div v-if="!requests.length" class="p-4 text-center text-muted">
              Chưa có yêu cầu nào.
            </div>
          </div>
        </div>
      </div>

      <div class="col-lg-7">
        <div class="card shadow-sm">
          <div class="card-header bg-white">
            <strong>Chi tiết yêu cầu</strong>
          </div>
          <div class="card-body" v-if="selectedRequest">
            <div class="row g-3 mb-3">
              <div class="col-md-6">
                <div class="text-muted small">Bác sĩ</div>
                <div class="fw-semibold">{{ selectedRequest.doctorName }}</div>
              </div>
              <div class="col-md-6">
                <div class="text-muted small">Loại yêu cầu</div>
                <div class="fw-semibold">{{ requestTypeLabel(selectedRequest.requestType) }}</div>
              </div>
              <div class="col-md-6">
                <div class="text-muted small">Ngày / slot</div>
                <div class="fw-semibold">
                  {{ formatDate(selectedRequest.workDate) }} - {{ selectedRequest.slotLabel }}
                </div>
                <div class="small text-muted">{{ selectedRequest.startTime }} - {{ selectedRequest.endTime }}</div>
              </div>
              <div class="col-md-6">
                <div class="text-muted small">Trạng thái</div>
                <span class="badge" :class="statusBadge(selectedRequest.status)">
                  {{ statusLabel(selectedRequest.status) }}
                </span>
              </div>
              <div class="col-12">
                <div class="text-muted small">Lý do</div>
                <div>{{ selectedRequest.reason }}</div>
              </div>
              <div v-if="selectedRequest.adminNote" class="col-12">
                <div class="text-muted small">Ghi chú admin</div>
                <div>{{ selectedRequest.adminNote }}</div>
              </div>
            </div>

            <div class="mb-4">
              <h2 class="h6">Lịch hẹn bị ảnh hưởng</h2>
              <div v-if="!selectedRequest.appointments.length" class="text-muted small">
                Slot này hiện không có bệnh nhân.
              </div>
              <div v-else class="table-responsive">
                <table class="table table-sm align-middle">
                  <thead>
                    <tr>
                      <th>Mã lịch</th>
                      <th>Bệnh nhân</th>
                      <th>Điện thoại</th>
                      <th>Trạng thái</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="appointment in selectedRequest.appointments" :key="appointment.appointmentId">
                      <td>{{ appointment.appointmentCode }}</td>
                      <td>{{ appointment.patientName }}</td>
                      <td>{{ appointment.phone || '-' }}</td>
                      <td>{{ appointment.status }}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>

            <template v-if="selectedRequest.status === 'Pending'">
              <div class="mb-3">
                <label class="form-label">Bác sĩ thay thế</label>
                <select v-model="decision.replacementDoctorId" class="form-select">
                  <option value="">Chọn bác sĩ thay thế</option>
                  <option
                    v-for="doctor in selectedRequest.availableDoctors"
                    :key="doctor.doctorId"
                    :value="doctor.doctorId"
                  >
                    {{ doctor.doctorName }} - {{ doctor.specialtyName }}
                  </option>
                </select>
                <div class="form-text" v-if="!selectedRequest.availableDoctors.length">
                  Không có bác sĩ đang rảnh cho slot này.
                </div>
              </div>

              <div class="mb-3">
                <label class="form-label">Ghi chú Admin</label>
                <textarea v-model="decision.adminNote" class="form-control" rows="3" />
              </div>

              <div class="d-flex gap-2">
                <button
                  class="btn btn-success"
                  type="button"
                  :disabled="decisionBusy || !decision.replacementDoctorId"
                  @click="approveSelected"
                >
                  <span v-if="decisionBusy" class="spinner-border spinner-border-sm me-2"></span>
                  Duyệt và đổi ca
                </button>
                <button
                  class="btn btn-outline-danger"
                  type="button"
                  :disabled="decisionBusy || !decision.adminNote.trim()"
                  @click="rejectSelected"
                >
                  Từ chối
                </button>
              </div>
            </template>
          </div>
          <div v-else class="card-body text-center text-muted">
            Chọn một yêu cầu để xem chi tiết.
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { shiftRequestService, type ShiftRequestItem, type ShiftRequestStatus, type ShiftRequestType } from '@/services/shiftRequestService'

const requests = ref<ShiftRequestItem[]>([])
const selectedRequest = ref<ShiftRequestItem | null>(null)
const loading = ref(false)
const decisionBusy = ref(false)
const error = ref('')
const success = ref('')

const decision = reactive({
  replacementDoctorId: '',
  adminNote: ''
})

const requestTypeLabel = (type: ShiftRequestType) => {
  return type === 'EmergencyLeave' ? 'Nghỉ đột xuất' : 'Xin chuyển ca'
}

const statusLabel = (status: ShiftRequestStatus) => {
  if (status === 'Approved') return 'Đã duyệt'
  if (status === 'Rejected') return 'Bị từ chối'
  return 'Đang chờ'
}

const statusBadge = (status: ShiftRequestStatus) => {
  if (status === 'Approved') return 'bg-success-subtle text-success'
  if (status === 'Rejected') return 'bg-danger-subtle text-danger'
  return 'bg-warning-subtle text-warning'
}

const formatDate = (value: string) => new Date(value).toLocaleDateString('vi-VN')

const loadRequests = async () => {
  loading.value = true
  error.value = ''
  try {
    requests.value = await shiftRequestService.getAdminRequests(undefined, 100)
    if (selectedRequest.value) {
      const stillExists = requests.value.find((item) => item.id === selectedRequest.value?.id)
      if (stillExists) {
        await selectRequest(stillExists.id)
      } else {
        selectedRequest.value = requests.value[0] || null
        if (selectedRequest.value) {
          await selectRequest(selectedRequest.value.id)
        }
      }
    } else if (requests.value.length) {
      const firstRequest = requests.value[0]
      if (firstRequest) {
        await selectRequest(firstRequest.id)
      }
    }
  } catch (err: any) {
    console.error(err)
    error.value = err?.response?.data?.message || 'Không tải được yêu cầu.'
  } finally {
    loading.value = false
  }
}

const selectRequest = async (id: string) => {
  error.value = ''
  success.value = ''
  try {
    selectedRequest.value = await shiftRequestService.getAdminRequestDetail(id)
    decision.replacementDoctorId = selectedRequest.value.preferredDoctorId || ''
    decision.adminNote = selectedRequest.value.adminNote || ''
  } catch (err: any) {
    console.error(err)
    error.value = err?.response?.data?.message || 'Không tải được chi tiết yêu cầu.'
  }
}

const approveSelected = async () => {
  if (!selectedRequest.value || !decision.replacementDoctorId) {
    return
  }

  decisionBusy.value = true
  error.value = ''
  success.value = ''
  try {
    await shiftRequestService.approveRequest(selectedRequest.value.id, {
      replacementDoctorId: decision.replacementDoctorId,
      adminNote: decision.adminNote.trim() || undefined
    })
    success.value = 'Đã duyệt yêu cầu và đổi ca thành công.'
    await loadRequests()
  } catch (err: any) {
    console.error(err)
    error.value = err?.response?.data?.message || 'Không thể duyệt yêu cầu.'
  } finally {
    decisionBusy.value = false
  }
}

const rejectSelected = async () => {
  if (!selectedRequest.value || !decision.adminNote.trim()) {
    return
  }

  decisionBusy.value = true
  error.value = ''
  success.value = ''
  try {
    await shiftRequestService.rejectRequest(selectedRequest.value.id, decision.adminNote.trim())
    success.value = 'Đã từ chối yêu cầu.'
    await loadRequests()
  } catch (err: any) {
    console.error(err)
    error.value = err?.response?.data?.message || 'Không thể từ chối yêu cầu.'
  } finally {
    decisionBusy.value = false
  }
}

onMounted(loadRequests)
</script>

<style scoped>
.request-list {
  max-height: 70vh;
  overflow: auto;
}
</style>
