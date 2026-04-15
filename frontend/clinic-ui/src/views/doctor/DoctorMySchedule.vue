<template>
  <div class="container py-4 doctor-my-schedule-page">
    <div class="d-flex flex-wrap justify-content-between align-items-center gap-3 mb-4">
      <div>
        <h1 class="h3 mb-1">Lịch làm của tôi</h1>
        <p class="text-muted mb-0">Xem lịch làm theo ngày của chính bác sĩ đang đăng nhập.</p>
      </div>
      <div class="d-flex gap-2 flex-wrap">
        <button class="btn btn-outline-secondary" type="button" @click="setDate(today)">
          Hôm nay
        </button>
        <button class="btn btn-outline-secondary" type="button" @click="setDate(tomorrow)">
          Ngày mai
        </button>
      </div>
    </div>

    <div class="card shadow-sm mb-4">
      <div class="card-body">
        <div class="row g-3 align-items-end">
          <div class="col-md-4">
            <label class="form-label">Ngày làm việc</label>
            <input v-model="selectedDate" type="date" class="form-control" />
          </div>
          <div class="col-md-3">
            <button class="btn btn-primary w-100" type="button" :disabled="loading" @click="loadSchedule">
              Xem lịch
            </button>
          </div>
          <div class="col-md-5 text-md-end">
            <div class="small text-muted">Tổng slot: {{ slots.length }}</div>
          </div>
        </div>
      </div>
    </div>

    <div class="row g-3 mb-4" v-if="summary">
      <div class="col-md-3">
        <div class="card shadow-sm h-100">
          <div class="card-body">
            <div class="text-muted small">Hôm nay</div>
            <div class="h4 mb-1">{{ formatHours(summary.todayMinutes) }}</div>
            <div class="small text-muted">{{ summary.todaySlots }} slot</div>
          </div>
        </div>
      </div>
      <div class="col-md-3">
        <div class="card shadow-sm h-100">
          <div class="card-body">
            <div class="text-muted small">Ngày đang xem</div>
            <div class="h4 mb-1">{{ formatHours(summary.selectedDateMinutes) }}</div>
            <div class="small text-muted">{{ summary.selectedDateSlots }} slot</div>
          </div>
        </div>
      </div>
      <div class="col-md-3">
        <div class="card shadow-sm h-100">
          <div class="card-body">
            <div class="text-muted small">Tháng nay</div>
            <div class="h4 mb-1">{{ formatHours(summary.currentMonthMinutes) }}</div>
            <div class="small text-muted">{{ summary.currentMonthSlots }} slot</div>
          </div>
        </div>
      </div>
      <div class="col-md-3">
        <div class="card shadow-sm h-100">
          <div class="card-body">
            <div class="text-muted small">Năm nay</div>
            <div class="h4 mb-1">{{ formatHours(summary.currentYearMinutes) }}</div>
            <div class="small text-muted">{{ summary.currentYearSlots }} slot</div>
          </div>
        </div>
      </div>
    </div>

    <div v-if="error" class="alert alert-danger">{{ error }}</div>

    <div v-if="loading" class="card shadow-sm">
      <div class="card-body text-center text-muted">Đang tải lịch làm...</div>
    </div>

    <div v-else-if="!slots.length" class="card shadow-sm">
      <div class="card-body text-center text-muted">
        Không có lịch làm cho ngày {{ formatDate(selectedDate) }}.
      </div>
    </div>

    <div v-else class="row g-4">
      <div v-for="group in groupedSlots" :key="group.key" class="col-lg-4">
        <div class="card shadow-sm h-100">
          <div class="card-header bg-white d-flex justify-content-between align-items-center">
            <strong>{{ group.label }}</strong>
            <span class="badge bg-light text-dark">{{ group.slots.length }} slot</span>
          </div>
          <div class="card-body">
            <div v-for="slot in group.slots" :key="slot.id" class="schedule-slot">
              <div class="fw-semibold">{{ slot.slotLabel || `${slot.startTime} - ${slot.endTime}` }}</div>
              <div class="small text-muted" v-if="slot.startTime && slot.endTime">{{ slot.startTime }} - {{ slot.endTime }}</div>
              <div class="small text-muted" v-if="slot.roomName">Phòng: {{ slot.roomName }}</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { addDaysToLocalDateInputValue, toLocalDateInputValue } from '@/utils/date'
import {
  doctorScheduleService,
  type DoctorScheduleSlot,
  type DoctorWorkSummary
} from '@/services/doctorScheduleService'

const authStore = useAuthStore()
const loading = ref(false)
const error = ref('')
const slots = ref<DoctorScheduleSlot[]>([])
const summary = ref<DoctorWorkSummary | null>(null)

const today = toLocalDateInputValue()
const tomorrow = addDaysToLocalDateInputValue(1)
const selectedDate = ref(today)

const doctorId = computed(() => authStore.doctorId || '')

const shiftLabels: Record<string, string> = {
  morning: 'Ca sáng',
  afternoon: 'Ca chiều',
  evening: 'Ca tối'
}

const groupedSlots = computed(() => {
  const groups = new Map<string, { key: string; label: string; slots: DoctorScheduleSlot[] }>()

  slots.value.forEach((slot) => {
    const key = slot.shiftCode || 'other'
    const label = shiftLabels[key] || slot.shiftCode || 'Ca khác'
    if (!groups.has(key)) {
      groups.set(key, { key, label, slots: [] })
    }
    groups.get(key)?.slots.push(slot)
  })

  return Array.from(groups.values())
})

const formatDate = (value: string) => new Date(value).toLocaleDateString('vi-VN')
const formatHours = (minutes: number) => `${(minutes / 60).toFixed(minutes % 60 === 0 ? 0 : 1)} giờ`

const setDate = (value: string) => {
  selectedDate.value = value
}

const loadSchedule = async () => {
  if (!doctorId.value) {
    error.value = 'Không tìm thấy bác sĩ đang đăng nhập.'
    slots.value = []
    return
  }

  loading.value = true
  error.value = ''
  try {
    const [daySlots, workSummary] = await Promise.all([
      doctorScheduleService.getDoctorDay(doctorId.value, selectedDate.value),
      doctorScheduleService.getWorkSummary(doctorId.value, selectedDate.value)
    ])

    slots.value = daySlots
    summary.value = workSummary
  } catch (err: any) {
    console.error(err)
    error.value = err?.response?.data?.message || 'Không tải được lịch làm.'
    slots.value = []
    summary.value = null
  } finally {
    loading.value = false
  }
}

watch(selectedDate, () => {
  loadSchedule()
})

onMounted(loadSchedule)
</script>

<style scoped>
.schedule-slot + .schedule-slot {
  margin-top: 0.75rem;
  padding-top: 0.75rem;
  border-top: 1px solid #eef2f7;
}
</style>