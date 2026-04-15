<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import api from '@/services/api'
import { queueService, type RoomOption } from '@/services/queueService'
import { toLocalDateInputValue } from '@/utils/date'
import { shiftRequestService, type ShiftRequestItem } from '@/services/shiftRequestService'
import {
  doctorScheduleService,
  type AvailableDoctorSlot,
  type DoctorScheduleSlot,
  type DoctorScheduleSlotImpact,
  type DoctorScheduleSlotPayload
} from '@/services/doctorScheduleService'

type SlotTemplate = {
  shiftCode: string
  shiftLabel: string
  slotLabel: string
  startTime: string
  endTime: string
}

const route = useRoute()
const router = useRouter()
const doctorId = String(route.params.id)

const today = toLocalDateInputValue()
const doctor = ref<any>(null)
const roomOptions = ref<RoomOption[]>([])

const selectedWeekday = ref<number>(new Date().getDay())
const previewDate = ref(today)
const templateSlotKeys = ref<string[]>([])
const slotRoomMap = ref<Record<string, string>>({})
const previewSlots = ref<DoctorScheduleSlot[]>([])
const incomingShiftSlots = ref<ShiftRequestItem[]>([])

const loadingTemplate = ref(false)
const loadingPreview = ref(false)
const savingTemplate = ref(false)
const error = ref('')
const success = ref('')
const previewDayOfWeek = new Date(previewDate.value + 'T00:00:00').getDay()
const modalOpen = ref(false)
const modalLoading = ref(false)
const modalSubmitting = ref(false)
const modalError = ref('')
const slotImpact = ref<DoctorScheduleSlotImpact | null>(null)
const availableDoctors = ref<AvailableDoctorSlot[]>([])
const selectedReplacementDoctorId = ref('')
const activeSlot = ref<SlotTemplate | null>(null)

const makeTime = (hour: number, minute: number) =>
  `${String(hour).padStart(2, '0')}:${String(minute).padStart(2, '0')}:00`

const normalizeTimeKey = (value: string) => String(value || '').slice(0, 5)
const normalizeDateKey = (value: string) => String(value || '').slice(0, 10)

const buildSlots = (
  shiftCode: string,
  shiftLabel: string,
  startHour: number,
  startMinute: number,
  endHour: number,
  endMinute: number
): SlotTemplate[] => {
  const slots: SlotTemplate[] = []
  let current = startHour * 60 + startMinute
  const end = endHour * 60 + endMinute

  while (current + 30 <= end) {
    const startH = Math.floor(current / 60)
    const startM = current % 60
    const next = current + 30
    const endH = Math.floor(next / 60)
    const endM = next % 60

    slots.push({
      shiftCode,
      shiftLabel,
      slotLabel: `${String(startH).padStart(2, '0')}:${String(startM).padStart(2, '0')} - ${String(endH).padStart(2, '0')}:${String(endM).padStart(2, '0')}`,
      startTime: makeTime(startH, startM),
      endTime: makeTime(endH, endM)
    })

    current = next
  }

  return slots
}

const slotGroups = computed(() => [
  { code: 'morning', label: 'Ca sáng', slots: buildSlots('morning', 'Ca sáng', 7, 0, 11, 30) },
  { code: 'afternoon', label: 'Ca chiều', slots: buildSlots('afternoon', 'Ca chiều', 13, 0, 17, 30) },
  { code: 'evening', label: 'Ca tối', slots: buildSlots('evening', 'Ca tối', 18, 0, 22, 0) }
])

const weekdays = [
  { value: 1, label: 'Thứ hai' },
  { value: 2, label: 'Thứ ba' },
  { value: 3, label: 'Thứ tư' },
  { value: 4, label: 'Thứ năm' },
  { value: 5, label: 'Thứ sáu' },
  { value: 6, label: 'Thứ bảy' },
  { value: 0, label: 'Chủ nhật' }
]

const buildSlotKey = (slot: Pick<SlotTemplate, 'shiftCode' | 'startTime'>) =>
  `${slot.shiftCode}|${normalizeTimeKey(slot.startTime)}`

const allSlots = computed(() => slotGroups.value.flatMap((group) => group.slots))

const previewSlotMap = computed(() => {
  const map = new Map<string, DoctorScheduleSlot>()
  previewSlots.value.forEach((slot) => {
    map.set(buildSlotKey({ shiftCode: slot.shiftCode, startTime: slot.startTime }), slot)
  })
  return map
})

const weekdayLabel = computed(() => {
  return weekdays.find((item) => item.value === selectedWeekday.value)?.label || 'Không rõ'
})

const loadDoctor = async () => {
  const response = await api.get(`/Doctor/${doctorId}`)
  doctor.value = response.data
}

const loadRooms = async () => {
  const departmentId = doctor.value?.departmentId || doctor.value?.DepartmentId
  roomOptions.value = await queueService.getRooms(departmentId)
}

const loadWeeklyTemplate = async () => {
  error.value = ''
  try {
    loadingTemplate.value = true
    const slots = await doctorScheduleService.getWeeklyTemplate(doctorId, selectedWeekday.value)
    templateSlotKeys.value = slots.map((slot) =>
      buildSlotKey({ shiftCode: slot.shiftCode, startTime: slot.startTime })
    )
    slotRoomMap.value = slots.reduce<Record<string, string>>((acc, slot) => {
      const key = buildSlotKey({ shiftCode: slot.shiftCode, startTime: slot.startTime })
      acc[key] = slot.roomId || ''
      return acc
    }, {})
  } catch (err: any) {
    console.error(err)
    error.value = err?.response?.data?.message || 'Không tải được lịch tuần cố định'
    templateSlotKeys.value = []
  } finally {
    loadingTemplate.value = false
  }
}

const loadPreviewSchedule = async () => {
  error.value = ''
  try {
    loadingPreview.value = true
    const [slots, approvedRequests] = await Promise.all([
      doctorScheduleService.getDoctorDay(doctorId, previewDate.value),
      shiftRequestService.getAdminRequests('Approved', 200)
    ])

    previewSlots.value = slots
    incomingShiftSlots.value = (approvedRequests ?? [])
      .filter((request) =>
        request.replacementDoctorId === doctorId &&
        normalizeDateKey(request.workDate) === previewDate.value
      )
      .sort((a, b) => normalizeTimeKey(a.startTime).localeCompare(normalizeTimeKey(b.startTime)))
  } catch (err: any) {
    console.error(err)
    error.value = err?.response?.data?.message || 'Không tải được lịch theo ngày'
    previewSlots.value = []
    incomingShiftSlots.value = []
  } finally {
    loadingPreview.value = false
  }
}

const incomingSlotKeySet = computed(() => {
  return new Set(incomingShiftSlots.value.map((item) => `${item.shiftCode}|${normalizeTimeKey(item.startTime)}`))
})

const baseDaySlots = computed(() => {
  return previewSlots.value.filter((slot) => !incomingSlotKeySet.value.has(`${slot.shiftCode}|${normalizeTimeKey(slot.startTime)}`))
})

const openPreviewSlot = (slot: DoctorScheduleSlot) => {
  openSlotModal({
    shiftCode: slot.shiftCode,
    shiftLabel: slot.shiftCode,
    slotLabel: slot.slotLabel,
    startTime: `${normalizeTimeKey(slot.startTime)}:00`,
    endTime: `${normalizeTimeKey(slot.endTime)}:00`
  })
}

const findPreviewSlotForShift = (item: ShiftRequestItem) => {
  return previewSlots.value.find((slot) =>
    slot.shiftCode === item.shiftCode &&
    normalizeTimeKey(slot.startTime) === normalizeTimeKey(item.startTime)
  )
}

const setShiftSelection = (shiftCode: string, checked: boolean) => {
  const shiftKeys = allSlots.value
    .filter((slot) => slot.shiftCode === shiftCode)
    .map((slot) => buildSlotKey(slot))

  if (checked) {
    templateSlotKeys.value = Array.from(new Set([...templateSlotKeys.value, ...shiftKeys]))
    return
  }

  templateSlotKeys.value = templateSlotKeys.value.filter((key) => !shiftKeys.includes(key))
}

const isShiftFullySelected = (shiftCode: string) => {
  const shiftKeys = allSlots.value
    .filter((slot) => slot.shiftCode === shiftCode)
    .map((slot) => buildSlotKey(slot))

  return shiftKeys.length > 0 && shiftKeys.every((key) => templateSlotKeys.value.includes(key))
}

const isPreviewSlot = (slot: SlotTemplate) => previewSlotMap.value.has(buildSlotKey(slot))

const saveWeeklyTemplate = async () => {
  error.value = ''
  success.value = ''

  try {
    savingTemplate.value = true
    const selectedKeys = new Set(templateSlotKeys.value)
    const missingRoom = allSlots.value.find((slot) => {
      const key = buildSlotKey(slot)
      return selectedKeys.has(key) && !slotRoomMap.value[key]
    })

    if (missingRoom) {
      error.value = `Vui lòng chọn phòng cho slot ${missingRoom.slotLabel}.`
      return
    }

    const payload: DoctorScheduleSlotPayload[] = allSlots.value
      .filter((slot) => selectedKeys.has(buildSlotKey(slot)))
      .map((slot) => ({
        shiftCode: slot.shiftCode,
        slotLabel: slot.slotLabel,
        startTime: slot.startTime,
        endTime: slot.endTime,
        roomId: slotRoomMap.value[buildSlotKey(slot)] || '',
        isActive: true
      }))

    const response = await doctorScheduleService.saveWeeklyTemplate(doctorId, selectedWeekday.value, payload)
        if (previewDayOfWeek === selectedWeekday.value) {
          try {
            await doctorScheduleService.deleteDayOverride(doctorId, previewDate.value)
          } catch {
            // Bỏ qua nếu ngày đó có appointments — override vẫn giữ nguyên
          }
        }
    await Promise.all([loadWeeklyTemplate(), loadPreviewSchedule()])
    const slotsSaved = Number(response?.data?.slotsSaved ?? payload.length)
    success.value = slotsSaved > 0
      ? `Đã cập nhật lịch cố định cho ${weekdayLabel.value}: ${slotsSaved} slot`
      : `Đã xóa lịch cố định cho ${weekdayLabel.value}`
  } catch (err: any) {
    console.error(err)
    error.value = err?.response?.data?.message || 'Không lưu được lịch cố định'
  } finally {
    savingTemplate.value = false
  }
}

const closeModal = () => {
  modalOpen.value = false
  modalLoading.value = false
  modalSubmitting.value = false
  modalError.value = ''
  slotImpact.value = null
  availableDoctors.value = []
  selectedReplacementDoctorId.value = ''
  activeSlot.value = null
}

const openSlotModal = async (slot: SlotTemplate) => {
  if (!isPreviewSlot(slot)) {
    return
  }

  activeSlot.value = slot
  modalOpen.value = true
  modalError.value = ''
  slotImpact.value = null
  availableDoctors.value = []
  selectedReplacementDoctorId.value = ''

  try {
    modalLoading.value = true
    const [impact, doctors] = await Promise.all([
      doctorScheduleService.getSlotImpact(doctorId, previewDate.value, slot.startTime),
      doctorScheduleService.getAvailableDoctors(doctorId, previewDate.value, slot.startTime)
    ])

    slotImpact.value = impact
    availableDoctors.value = doctors
    selectedReplacementDoctorId.value = doctors[0]?.doctorId ?? ''
  } catch (err: any) {
    console.error(err)
    modalError.value = err?.response?.data?.message || 'Không tải được thông tin đổi ca'
  } finally {
    modalLoading.value = false
  }
}

const submitReassign = async () => {
  if (!activeSlot.value || !selectedReplacementDoctorId.value) {
    modalError.value = 'Vui lòng chọn bác sĩ thay thế.'
    return
  }

  modalError.value = ''

  try {
    modalSubmitting.value = true
    const response = await doctorScheduleService.reassignSlot({
      fromDoctorId: doctorId,
      toDoctorId: selectedReplacementDoctorId.value,
      workDate: previewDate.value,
      startTime: activeSlot.value.startTime,
      moveAppointments: true
    })

    await loadPreviewSchedule()
    const movedAppointments = Number(response?.data?.movedAppointments ?? 0)
    success.value = movedAppointments > 0
      ? `Đã đổi ca slot ${normalizeTimeKey(activeSlot.value.startTime)} và chuyển ${movedAppointments} lịch hẹn`
      : `Đã đổi ca slot ${normalizeTimeKey(activeSlot.value.startTime)}`
    closeModal()
  } catch (err: any) {
    console.error(err)
    modalError.value = err?.response?.data?.message || 'Không đổi được ca'
  } finally {
    modalSubmitting.value = false
  }
}

watch(selectedWeekday, async () => {
  await loadWeeklyTemplate()
})

watch(previewDate, async () => {
  closeModal()
  await loadPreviewSchedule()
})

onMounted(async () => {
  await loadDoctor()
  await loadRooms()
  await Promise.all([loadWeeklyTemplate(), loadPreviewSchedule()])
})
</script>

<template>
  <div class="container-fluid px-4 py-4 bg-light min-vh-100">
    <div class="d-flex justify-content-between align-items-center mb-4">
      <div>
        <button class="btn btn-link p-0 text-decoration-none text-muted mb-1" @click="router.push('/doctors')">
          <i class="bi bi-chevron-left me-1"></i>Danh sách bác sĩ
        </button>
        <h3 class="fw-bold m-0 text-dark">Quản lý lịch bác sĩ</h3>
        <div v-if="doctor" class="badge bg-white text-primary border shadow-sm mt-2 px-3 py-2 rounded-pill">
          <i class="bi bi-person-badge me-2"></i>{{ doctor.fullName }} | {{ doctor.departmentName || 'Chưa có khoa' }}
        </div>
      </div>
      <div class="d-flex gap-2">
        <div v-if="success" class="alert alert-success py-2 px-3 m-0 shadow-sm border-0 animate__animated animate__fadeIn">
          <i class="bi bi-check-circle-fill me-2"></i>{{ success }}
        </div>
      </div>
    </div>

    <div v-if="error" class="alert alert-danger shadow-sm border-0 mb-4 animate__animated animate__shakeX">
      <i class="bi bi-exclamation-triangle-fill me-2"></i>{{ error }}
    </div>

    <div class="row g-4">
      <div class="col-xl-7">
        <div class="card border-0 shadow-sm rounded-4 overflow-hidden h-100">
          <div class="card-header bg-white py-3 px-4 border-bottom">
            <div class="d-flex justify-content-between align-items-center flex-wrap gap-3">
              <div>
                <h5 class="fw-bold m-0 text-primary">Mẫu lịch cố định hàng tuần</h5>
                <p class="text-muted small m-0">Thiết lập các khung giờ khám mặc định</p>
              </div>
              <div class="d-flex gap-2 align-items-center">
                <select v-model="selectedWeekday" class="form-select border-2 shadow-none rounded-3" style="min-width: 140px;">
                  <option v-for="item in weekdays" :key="item.value" :value="item.value">{{ item.label }}</option>
                </select>
                <button class="btn btn-primary px-4 fw-bold rounded-3 shadow-sm" :disabled="savingTemplate || loadingTemplate" @click="saveWeeklyTemplate">
                  <span v-if="savingTemplate" class="spinner-border spinner-border-sm me-1"></span>
                  Lưu lịch tuần
                </button>
              </div>
            </div>
          </div>
          <div class="card-body p-4">
            <div v-if="loadingTemplate" class="text-center py-5"><div class="spinner-border text-primary"></div></div>
            <div v-else class="row g-3">
              <div v-for="group in slotGroups" :key="group.code" class="col-lg-4">
                <div class="card h-100 border shadow-none rounded-3 bg-white">
                  <div class="card-header bg-light d-flex justify-content-between align-items-center py-2 border-0">
                    <span class="fw-bold text-dark small">{{ group.label }}</span>
                    <button class="btn btn-link btn-sm p-0 text-decoration-none small" @click="setShiftSelection(group.code, !isShiftFullySelected(group.code))">
                      {{ isShiftFullySelected(group.code) ? 'Bỏ' : 'Tất cả' }}
                    </button>
                  </div>
                  <div class="card-body p-2 scroll-area">
                    <div v-for="slot in group.slots" :key="buildSlotKey(slot)" 
                      class="p-2 rounded-3 border-2 mb-2 transition-all"
                      :class="templateSlotKeys.includes(buildSlotKey(slot)) ? 'border-primary bg-primary-subtle shadow-sm' : 'border-light bg-white'">
                      <div class="form-check m-0">
                        <input v-model="templateSlotKeys" class="form-check-input shadow-none" type="checkbox" :value="buildSlotKey(slot)" />
                        <label class="form-check-label small fw-bold text-dark">{{ slot.slotLabel }}</label>
                      </div>
                      <select v-if="templateSlotKeys.includes(buildSlotKey(slot))" v-model="slotRoomMap[buildSlotKey(slot)]" class="form-select form-select-sm border-0 mt-2 shadow-none bg-white rounded-2">
                        <option value="">Chọn phòng...</option>
                        <option v-for="room in roomOptions" :key="room.id" :value="room.id">{{ room.name }}</option>
                      </select>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="col-xl-5">
        <div class="card border-0 shadow-sm rounded-4 h-100 bg-white">
          <div class="card-header bg-white py-3 px-4 border-bottom d-flex justify-content-between align-items-center">
            <div>
              <h5 class="fw-bold m-0 text-dark">Lịch thực tế</h5>
              <p class="text-muted small m-0">Theo ngày cụ thể</p>
            </div>
            <input v-model="previewDate" type="date" class="form-control border-2 shadow-none rounded-3 w-auto" />
          </div>
          <div class="card-body p-4">
            <div v-if="loadingPreview" class="text-center py-5"><div class="spinner-border text-primary"></div></div>
            <div v-else>
              <div class="mb-5">
                <div class="d-flex justify-content-between align-items-center mb-3">
                  <span class="fw-bold text-secondary small text-uppercase ls-1">Lịch trong ngày</span>
                  <span class="badge bg-primary rounded-pill shadow-sm">{{ baseDaySlots.length }} slot</span>
                </div>
                <div v-if="baseDaySlots.length === 0" class="empty-state">Trống lịch</div>
                <div class="d-grid gap-2">
                  <button v-for="slot in baseDaySlots" :key="slot.id + slot.startTime" class="btn btn-schedule btn-schedule-primary" @click="openPreviewSlot(slot)">
                    <div class="d-flex justify-content-between align-items-center">
                      <div>
                        <div class="fw-bold">{{ slot.slotLabel }}</div>
                        <div class="small opacity-75"><i class="bi bi-geo-alt-fill me-1"></i>{{ slot.roomName || 'N/A' }}</div>
                      </div>
                      <i class="bi bi-arrow-right-short fs-4"></i>
                    </div>
                  </button>
                </div>
              </div>

              <div>
                <div class="d-flex justify-content-between align-items-center mb-3">
                  <span class="fw-bold text-warning-emphasis small text-uppercase ls-1">Được chuyển ca tới</span>
                  <span class="badge bg-warning text-dark rounded-pill shadow-sm">{{ incomingShiftSlots.length }} slot</span>
                </div>
                <div v-if="incomingShiftSlots.length === 0" class="empty-state">Không có ca chuyển tới</div>
                <div class="d-grid gap-2">
                  <button v-for="item in incomingShiftSlots" :key="item.id" 
                    class="btn btn-schedule btn-schedule-warning" 
                    :disabled="!findPreviewSlotForShift(item)"
                    @click="() => { const slot = findPreviewSlotForShift(item); if (slot) openPreviewSlot(slot) }">
                    <div class="d-flex justify-content-between align-items-center">
                      <div>
                        <div class="fw-bold">{{ item.slotLabel }}</div>
                        <div class="small fw-medium"><i class="bi bi-person-fill me-1"></i>Từ: {{ item.doctorName }}</div>
                        <div class="small opacity-75" v-if="item.roomName"><i class="bi bi-geo-alt-fill me-1"></i>{{ item.roomName }}</div>
                      </div>
                      <i class="bi bi-box-arrow-in-down-right fs-4"></i>
                    </div>
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div v-if="modalOpen" class="modal-custom-backdrop" @click.self="closeModal">
      <div class="modal-custom-content animate__animated animate__zoomIn">
        <div class="modal-custom-header">
          <div>
            <h4 class="fw-bold m-0 text-primary">Đổi ca làm việc</h4>
            <p class="text-muted small m-0" v-if="activeSlot">
              {{ doctor?.fullName }} • {{ previewDate }} • {{ activeSlot.slotLabel }}
            </p>
          </div>
          <button class="btn-close shadow-none" @click="closeModal"></button>
        </div>

        <div class="modal-custom-body">
          <div v-if="modalError" class="alert alert-danger border-0 shadow-sm mb-4">{{ modalError }}</div>
          <div v-if="modalLoading" class="text-center py-5"><div class="spinner-border text-primary"></div></div>
          
          <div v-else>
            <div class="impact-card mb-4" v-if="slotImpact">
              <div class="d-flex align-items-center gap-2 mb-3">
                <div class="impact-dot" :class="slotImpact.appointmentCount > 0 ? 'bg-danger' : 'bg-success'"></div>
                <h6 class="fw-bold m-0">Ảnh hưởng lịch hẹn ({{ slotImpact.appointmentCount }})</h6>
              </div>
              
              <div v-if="slotImpact.appointments.length" class="table-responsive rounded-3 border">
                <table class="table table-sm table-hover mb-0 small">
                  <thead class="bg-light">
                    <tr><th>Mã</th><th>Bệnh nhân</th><th>Trạng thái</th></tr>
                  </thead>
                  <tbody>
                    <tr v-for="item in slotImpact.appointments" :key="item.appointmentId">
                      <td class="fw-bold">{{ item.appointmentCode }}</td>
                      <td>{{ item.patientName }}</td>
                      <td><span class="badge bg-light text-dark border">{{ item.status }}</span></td>
                    </tr>
                  </tbody>
                </table>
              </div>
              <div v-else class="text-center py-3 text-muted bg-light rounded-3 small">Slot hiện chưa có lịch hẹn nào.</div>
            </div>

            <div class="mb-4">
              <h6 class="fw-bold mb-3">Bác sĩ thay thế rảnh giờ này:</h6>
              <div v-if="!availableDoctors.length" class="empty-state py-4">Không tìm thấy bác sĩ rảnh</div>
              <div class="doctor-list scroll-area">
                <label v-for="cand in availableDoctors" :key="cand.doctorId" 
                  class="doctor-item transition-all" 
                  :class="selectedReplacementDoctorId === cand.doctorId ? 'selected' : ''">
                  <input type="radio" v-model="selectedReplacementDoctorId" :value="cand.doctorId" class="d-none" />
                  <div class="d-flex justify-content-between align-items-center w-100">
                    <div>
                      <div class="fw-bold text-dark">{{ cand.doctorName }}</div>
                      <div class="text-muted x-small">{{ cand.specialtyName }}</div>
                    </div>
                    <i v-if="selectedReplacementDoctorId === cand.doctorId" class="bi bi-check-circle-fill text-primary"></i>
                  </div>
                </label>
              </div>
            </div>
          </div>
        </div>

        <div class="modal-custom-footer">
          <button class="btn btn-light px-4 fw-bold rounded-3" @click="closeModal">Đóng</button>
          <button class="btn btn-primary px-4 fw-bold rounded-3" 
            :disabled="modalSubmitting || !availableDoctors.length" @click="submitReassign">
            <span v-if="modalSubmitting" class="spinner-border spinner-border-sm me-2"></span>
            Xác nhận chuyển ca
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Tổng thể */
.ls-1 { letter-spacing: 0.5px; }
.transition-all { transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1); }
.scroll-area { max-height: 380px; overflow-y: auto; padding-right: 4px; }
.x-small { font-size: 0.75rem; }

/* Empty state */
.empty-state {
  padding: 20px;
  background: #f8fafc;
  border-radius: 12px;
  text-align: center;
  color: #94a3b8;
  font-size: 0.85rem;
  border: 2px dashed #e2e8f0;
}

.btn-schedule {
  border-radius: 14px;
  padding: 14px 18px;
  text-align: left;
  border: 2px solid transparent;
  transition: all 0.2s ease;
}
.btn-schedule-primary {
  background: #f0f7ff;
  border-color: #e0efff;
  color: #0056b3;
}
.btn-schedule-primary:hover {
  background: #0056b3;
  color: white;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 86, 179, 0.2);
}
.btn-schedule-warning {
  background: #fff9eb;
  border-color: #ffecb3;
  color: #856404;
}
.btn-schedule-warning:hover:not(:disabled) {
  background: #ffc107;
  color: #000;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(255, 193, 7, 0.2);
}

/* Modal Custom */
.modal-custom-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.65);
  backdrop-filter: blur(8px);
  z-index: 2000;
  display: flex;
  align-items: center;
  justify-content: center;
}
.modal-custom-content {
  background: white;
  width: min(700px, 95vw);
  max-height: 90vh;
  border-radius: 28px;
  display: flex;
  flex-direction: column;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
}
.modal-custom-header { padding: 24px 32px; border-bottom: 1px solid #f1f5f9; display: flex; justify-content: space-between; align-items: center; }
.modal-custom-body { padding: 24px 32px; overflow-y: auto; }
.modal-custom-footer { padding: 20px 32px; border-top: 1px solid #f1f5f9; display: flex; justify-content: flex-end; gap: 12px; }

/* Doctor Selector */
.doctor-item {
  display: flex;
  padding: 14px 20px;
  border: 2px solid #f1f5f9;
  border-radius: 16px;
  margin-bottom: 10px;
  cursor: pointer;
}
.doctor-item:hover { border-color: #cbd5e1; background: #f8fafc; }
.doctor-item.selected { border-color: #0d6efd; background: #f0f7ff; }

/* Impact Card */
.impact-card { background: #f8fafc; padding: 16px; border-radius: 16px; }
.impact-dot { width: 10px; height: 10px; border-radius: 50%; }

.scroll-area::-webkit-scrollbar { width: 4px; }
.scroll-area::-webkit-scrollbar-thumb { background: #e2e8f0; border-radius: 10px; }
</style>