<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import api from '@/services/api'
import { toLocalDateInputValue } from '@/utils/date'
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

const selectedWeekday = ref<number>(new Date().getDay())
const previewDate = ref(today)
const templateSlotKeys = ref<string[]>([])
const previewSlots = ref<DoctorScheduleSlot[]>([])

const loadingTemplate = ref(false)
const loadingPreview = ref(false)
const savingTemplate = ref(false)
const error = ref('')
const success = ref('')

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

const loadWeeklyTemplate = async () => {
  error.value = ''
  try {
    loadingTemplate.value = true
    const slots = await doctorScheduleService.getWeeklyTemplate(doctorId, selectedWeekday.value)
    templateSlotKeys.value = slots.map((slot) =>
      buildSlotKey({ shiftCode: slot.shiftCode, startTime: slot.startTime })
    )
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
    previewSlots.value = await doctorScheduleService.getDoctorDay(doctorId, previewDate.value)
  } catch (err: any) {
    console.error(err)
    error.value = err?.response?.data?.message || 'Không tải được lịch theo ngày'
    previewSlots.value = []
  } finally {
    loadingPreview.value = false
  }
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
    const payload: DoctorScheduleSlotPayload[] = allSlots.value
      .filter((slot) => templateSlotKeys.value.includes(buildSlotKey(slot)))
      .map((slot) => ({
        shiftCode: slot.shiftCode,
        slotLabel: slot.slotLabel,
        startTime: slot.startTime,
        endTime: slot.endTime,
        isActive: true
      }))

    const response = await doctorScheduleService.saveWeeklyTemplate(doctorId, selectedWeekday.value, payload)
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
  await Promise.all([loadWeeklyTemplate(), loadPreviewSchedule()])
})
</script>

<template>
  <div class="container-fluid px-4 py-3">
    <div class="d-flex flex-wrap justify-content-between align-items-center gap-3 mb-4">
      <div>
        <button class="btn btn-link px-0 text-decoration-none" @click="router.push('/doctors')">
          <i class="bi bi-arrow-left me-2"></i>Quay lại danh sách bác sĩ
        </button>
        <h3 class="mb-1">Lịch làm cố định theo tuần</h3>
        <div class="text-muted" v-if="doctor">
          {{ doctor.fullName }} · {{ doctor.departmentName || 'Chưa có khoa' }}
        </div>
      </div>
    </div>

    <div v-if="error" class="alert alert-danger">{{ error }}</div>
    <div v-if="success" class="alert alert-success">{{ success }}</div>

    <div class="row g-4">
      <div class="col-xl-7">
        <div class="card border-0 shadow-sm h-100">
          <div class="card-body">
            <div class="d-flex flex-wrap justify-content-between align-items-end gap-3 mb-3">
              <div>
                <h5 class="mb-1">Mẫu lịch cố định</h5>
                <div class="text-muted small">Áp dụng cho mỗi {{ weekdayLabel.toLowerCase() }} trong tuần.</div>
              </div>

              <div class="d-flex gap-2 flex-wrap align-items-end">
                <div>
                  <label class="form-label mb-1">Thứ trong tuần</label>
                  <select v-model="selectedWeekday" class="form-select">
                    <option v-for="item in weekdays" :key="item.value" :value="item.value">
                      {{ item.label }}
                    </option>
                  </select>
                </div>
                <button class="btn btn-primary" :disabled="savingTemplate || loadingTemplate" @click="saveWeeklyTemplate">
                  <span v-if="savingTemplate" class="spinner-border spinner-border-sm me-2"></span>
                  Lưu lịch tuần
                </button>
              </div>
            </div>

            <div class="alert alert-info">
              Lịch này là lịch cố định hàng tuần. Khi bác sĩ xin đổi ca cho một ngày cụ thể, hệ thống sẽ tạo ngoại lệ cho ngày đó thay vì sửa cả tuần.
            </div>

            <div v-if="loadingTemplate" class="text-center py-4">
              <div class="spinner-border text-primary"></div>
            </div>

            <div v-else class="row g-3">
              <div v-for="group in slotGroups" :key="group.code" class="col-lg-4">
                <div class="card h-100 border">
                  <div class="card-body">
                    <div class="d-flex justify-content-between align-items-center mb-3">
                      <div>
                        <h6 class="mb-1">{{ group.label }}</h6>
                        <div class="text-muted small">{{ group.slots.length }} slots</div>
                      </div>
                      <button
                        class="btn btn-sm btn-outline-primary"
                        @click="setShiftSelection(group.code, !isShiftFullySelected(group.code))"
                      >
                        {{ isShiftFullySelected(group.code) ? 'Bỏ chọn' : 'Chọn ca' }}
                      </button>
                    </div>

                    <div class="d-grid gap-2">
                      <label
                        v-for="slot in group.slots"
                        :key="buildSlotKey(slot)"
                        class="border rounded-3 px-3 py-2 d-flex align-items-center gap-2"
                      >
                        <input
                          v-model="templateSlotKeys"
                          class="form-check-input mt-0"
                          type="checkbox"
                          :value="buildSlotKey(slot)"
                        />
                        <span class="fw-medium">{{ slot.slotLabel }}</span>
                      </label>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="col-xl-5">
        <div class="card border-0 shadow-sm h-100">
          <div class="card-body">
            <div class="d-flex flex-wrap justify-content-between align-items-end gap-3 mb-3">
              <div>
                <h5 class="mb-1">Lịch thực tế theo ngày</h5>
                <div class="text-muted small">Xem lịch hiện hành của một ngày và đổi ca nếu cần.</div>
              </div>
              <div>
                <label class="form-label mb-1">Ngày cần xem</label>
                <input v-model="previewDate" type="date" class="form-control" />
              </div>
            </div>

            <div v-if="loadingPreview" class="text-center py-4">
              <div class="spinner-border text-primary"></div>
            </div>

            <div v-else-if="previewSlots.length === 0" class="text-muted small">
              Không có lịch cho ngày nay.
            </div>

            <div v-else class="d-grid gap-2">
              <button
                v-for="slot in previewSlots"
                :key="slot.id + slot.startTime"
                type="button"
                class="btn btn-outline-primary text-start"
                @click="openSlotModal({
                  shiftCode: slot.shiftCode,
                  shiftLabel: slot.shiftCode,
                  slotLabel: slot.slotLabel,
                  startTime: `${slot.startTime}:00`,
                  endTime: `${slot.endTime}:00`
                })"
              >
                <div class="fw-semibold">{{ slot.slotLabel }}</div>
                <div class="small text-muted">{{ slot.startTime }} - {{ slot.endTime }}</div>
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div v-if="modalOpen" class="slot-modal-backdrop" @click.self="closeModal">
      <div class="slot-modal">
        <div class="d-flex justify-content-between align-items-start gap-3 mb-3">
          <div>
            <h4 class="mb-1">Đổi ca làm việc</h4>
            <div class="text-muted small" v-if="activeSlot">
              {{ doctor?.fullName }} · {{ previewDate }} · {{ activeSlot.slotLabel }}
            </div>
          </div>
          <button type="button" class="btn-close" @click="closeModal"></button>
        </div>

        <div v-if="modalError" class="alert alert-danger">{{ modalError }}</div>

        <div v-if="modalLoading" class="text-center py-4">
          <div class="spinner-border text-primary"></div>
        </div>

        <div v-else>
          <div class="border rounded-3 p-3 mb-3" v-if="slotImpact">
<div class="fw-semibold mb-2">Ảnh hưởng hiện tại</div>
              <div class="small text-muted mb-2">
                {{ slotImpact.appointmentCount > 0 ? `Slot này đang có ${slotImpact.appointmentCount} lịch hẹn.` : 'Slot này đang trống, chưa có bệnh nhân.' }}
            </div>

            <div v-if="slotImpact.appointments.length" class="table-responsive">
              <table class="table table-sm align-middle mb-0">
                <thead>
                  <tr>
                    <th>Mã lịch</th>
                    <th>Bệnh nhân</th>
                    <th>Điện thoại</th>
                    <th>Trạng thái</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="item in slotImpact.appointments" :key="item.appointmentId">
                    <td>{{ item.appointmentCode }}</td>
                    <td>{{ item.patientName }}</td>
                    <td>{{ item.phone || '-' }}</td>
                    <td>{{ item.status }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          <div class="border rounded-3 p-3 mb-3">
            <div class="fw-semibold mb-2">Bác sĩ thay thế đang không có ca khám nào.</div>
            <div v-if="availableDoctors.length === 0" class="text-muted small">
              Không có bác sĩ nào rảnh cùng giờ trong khoa này.
            </div>
            <div v-else class="d-grid gap-2">
              <label
                v-for="candidate in availableDoctors"
                :key="candidate.doctorId"
                class="border rounded-3 px-3 py-2 d-flex gap-2 align-items-start"
              >
                <input
                  v-model="selectedReplacementDoctorId"
                  class="form-check-input mt-1"
                  type="radio"
                  :value="candidate.doctorId"
                />
                <div>
                  <div class="fw-semibold">{{ candidate.doctorName }} ({{ candidate.doctorCode }})</div>
                  <div class="small text-muted">{{ candidate.departmentName }} · {{ candidate.specialtyName }}</div>
                </div>
              </label>
            </div>
          </div>

          <div class="d-flex justify-content-end gap-2">
            <button type="button" class="btn btn-light" @click="closeModal">Đóng</button>
            <button
              type="button"
              class="btn btn-primary"
              :disabled="modalSubmitting || availableDoctors.length === 0"
              @click="submitReassign"
            >
              <span v-if="modalSubmitting" class="spinner-border spinner-border-sm me-2"></span>
              Chuyển slot sang bác sĩ khác
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.slot-modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 24px;
  z-index: 1050;
}

.slot-modal {
  width: min(860px, 100%);
  max-height: calc(100vh - 48px);
  overflow: auto;
  background: #fff;
  border-radius: 20px;
  padding: 24px;
  box-shadow: 0 24px 80px rgba(15, 23, 42, 0.25);
}
</style>
