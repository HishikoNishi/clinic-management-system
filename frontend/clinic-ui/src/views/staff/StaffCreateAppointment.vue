<template>
  <div class="staff-booking-page">
    <div class="staff-page-header">
      <div>
        <div class="staff-eyebrow">Staff</div>
        <h2 class="staff-title">Äáº·t lá»‹ch táº¡i quáº§y</h2>
        <p class="staff-subtitle">
          Táº¡o lá»‹ch nhanh cho khách đến trực tiếp, có thể chọn bác sĩ hoặc chỉ giữ chỗ theo khung giờ mong muốn.
        </p>
      </div>
      <button type="button" class="btn btn-outline-primary" @click="router.push('/staff/appointments')">
        <i class="bi bi-arrow-left me-2"></i>Vá» danh sÃ¡ch lá»‹ch
      </button>
    </div>

    <div class="booking-grid">
      <section class="booking-card main-card">
        <div v-if="submitError" class="alert alert-danger mb-3">{{ submitError }}</div>

        <div v-if="bookingSuccess" class="alert alert-success mb-4">
          <div class="fw-semibold mb-2">Äáº·t lá»‹ch thÃ nh cÃ´ng</div>
          <div>MÃ£ lá»‹ch háº¹n: <span class="appointment-code">{{ bookingResponse.appointmentCode }}</span></div>
          <div class="small mt-2">
            {{ bookingResponse.fullName }} Â·
            {{ formatDateTime(bookingResponse.appointmentDate, bookingResponse.appointmentTime) }}
          </div>
          <div class="success-actions">
            <button type="button" class="btn btn-primary" @click="resetForm">
              <i class="bi bi-plus-circle me-2"></i>Táº¡o lá»‹ch má»›i
            </button>
            <button type="button" class="btn btn-outline-success" @click="router.push('/staff/appointments')">
              <i class="bi bi-calendar-check me-2"></i>Xem trong danh sÃ¡ch lá»‹ch
            </button>
          </div>
        </div>

        <div class="lookup-box">
          <div class="lookup-head">
            <div>
              <div class="lookup-title">Bá»‡nh nhÃ¢n cÅ©?</div>
              <div class="lookup-note">Nháº­p sá»‘ Ä‘iá»‡n thoáº¡i hoáº·c email Ä‘á»ƒ Ä‘iá»n nhanh thÃ´ng tin Ä‘Ã£ cÃ³.</div>
            </div>
            <button type="button" class="btn btn-sm btn-outline-secondary" @click="showLookup = !showLookup">
              {{ showLookup ? 'áº¨n' : 'Má»Ÿ tra cá»©u' }}
            </button>
          </div>

          <div v-if="showLookup" class="lookup-form">
            <input
              v-model="lookupPhone"
              type="tel"
              class="form-control"
              placeholder="Sá»‘ Ä‘iá»‡n thoáº¡i"
              inputmode="numeric"
              maxlength="11"
              @input="lookupPhone = normalizePhoneInput(lookupPhone)"
            />
            <input v-model="lookupEmail" type="email" class="form-control" placeholder="Email" />
            <button type="button" class="btn btn-outline-primary" :disabled="lookupLoading" @click="lookupPatient">
              <span v-if="lookupLoading" class="spinner-border spinner-border-sm me-1"></span>
              Äiá»n thÃ´ng tin cÅ©
            </button>
            <button v-if="isReturning" type="button" class="btn btn-outline-dark" @click="clearPrefill">
              Cho phÃ©p sá»­a
            </button>
          </div>

          <div v-if="lookupError" class="text-danger small mt-2">{{ lookupError }}</div>
          <div v-if="isReturning" class="text-success small mt-2">ÄÃ£ Ä‘iá»n thÃ´ng tin tá»« há»“ sÆ¡ cÅ©.</div>
        </div>

        <form class="booking-form" @submit.prevent="submitBooking">
          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Há» vÃ  tÃªn *</label>
              <input ref="fullNameInput" v-model="form.fullName" type="text" class="form-control" :readonly="isReturning" />
              <div v-if="errors.fullName" class="form-error">{{ errors.fullName }}</div>
            </div>
            <div class="form-group">
              <label class="form-label">NgÃ y sinh *</label>
              <input v-model="form.dateOfBirth" type="date" class="form-control" :readonly="isReturning" />
              <div v-if="errors.dateOfBirth" class="form-error">{{ errors.dateOfBirth }}</div>
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Giá»›i tÃ­nh *</label>
              <select v-model="form.gender" class="form-select" :disabled="isReturning">
                <option value="">Chá»n giá»›i tÃ­nh</option>
                <option value="1">Nam</option>
                <option value="2">Ná»¯</option>
              </select>
              <div v-if="errors.gender" class="form-error">{{ errors.gender }}</div>
            </div>
            <div class="form-group">
              <label class="form-label">Äiá»‡n thoáº¡i *</label>
              <input
                v-model="form.phone"
                type="tel"
                class="form-control"
                :readonly="isReturning"
                inputmode="numeric"
                maxlength="11"
                @input="form.phone = normalizePhoneInput(form.phone)"
              />
              <div v-if="errors.phone" class="form-error">{{ errors.phone }}</div>
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Email</label>
              <input v-model="form.email" type="email" class="form-control" placeholder="CÃ³ thá»ƒ Ä‘á»ƒ trá»‘ng" />
              <div v-if="errors.email" class="form-error">{{ errors.email }}</div>
            </div>
            <div class="form-group">
              <label class="form-label">Äá»‹a chá»‰ *</label>
              <input v-model="form.address" type="text" class="form-control" :readonly="isReturning" />
              <div v-if="errors.address" class="form-error">{{ errors.address }}</div>
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Sá»‘ CCCD</label>
              <input v-model="form.citizenId" type="text" class="form-control" placeholder="Nháº­p 12 sá»‘ CCCD" maxlength="12" />
            </div>
            <div class="form-group">
              <label class="form-label">MÃ£ sá»‘ BHYT</label>
              <input v-model="form.insuranceCardNumber" type="text" class="form-control" placeholder="VÃ­ dá»¥: GD479..." />
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label class="form-label">NgÃ y khÃ¡m *</label>
              <input v-model="form.appointmentDate" type="date" class="form-control" :min="todayStr" />
              <div v-if="errors.appointmentDate" class="form-error">{{ errors.appointmentDate }}</div>
            </div>
            <div class="form-group">
              <label class="form-label">Khoa mong muá»‘n (tÃ¹y chá»n)</label>
              <select v-model="selectedDepartmentId" class="form-select">
                <option value="">Táº¥t cáº£ khoa</option>
                <option v-for="department in departments" :key="department.id" :value="department.id">
                  {{ department.name }}
                </option>
              </select>
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Bác sĩ (tùy chọn)</label>
              <select v-model="form.doctorId" class="form-select">
                <option value="">Không chọn trước</option>
                <option v-for="doctor in filteredDoctors" :key="doctor.id" :value="doctor.id">
                  {{ doctor.fullName }} - {{ doctor.departmentName }}
                </option>
              </select>
              <div v-if="errors.doctorId" class="form-error">{{ errors.doctorId }}</div>
            </div>
            <div class="form-group">
              <label class="form-label">Khung giờ khám *</label>
              <select v-model="form.appointmentTime" class="form-select" :disabled="!form.appointmentDate || slotLoading">
                <option value="">{{ slotLoading ? 'Đang tải slot...' : 'Chọn khung giờ' }}</option>
                <option v-for="slot in availableSlots" :key="slot.id || `${slot.shiftCode}-${slot.startTime}`" :value="String(slot.startTime).slice(0, 5)">
                  {{ slot.slotLabel || String(slot.startTime).slice(0, 5) }}
                </option>
              </select>
              <div v-if="errors.appointmentTime" class="form-error">{{ errors.appointmentTime }}</div>
            </div>
          </div>

          <div class="form-group">
            <label class="form-label">LÃ½ do khÃ¡m *</label>
            <textarea
              v-model="form.reason"
              rows="4"
              class="form-control"
              placeholder="MÃ´ táº£ triá»‡u chá»©ng hoáº·c nhu cáº§u khÃ¡m cá»§a bá»‡nh nhÃ¢n"
            ></textarea>
            <div v-if="errors.reason" class="form-error">{{ errors.reason }}</div>
          </div>

          <button type="submit" class="btn btn-primary submit-btn" :disabled="submitting">
            <span v-if="submitting" class="spinner-border spinner-border-sm me-2"></span>
            <i v-else class="bi bi-calendar-plus me-2"></i>
            {{ submitting ? 'Äang táº¡o lá»‹ch...' : 'Táº¡o lá»‹ch khÃ¡m' }}
          </button>
        </form>
      </section>

      <aside class="booking-card side-card">
        <div class="tip-badge">Quy trÃ¬nh gá»£i Ã½</div>
        <ol class="tip-list">
          <li>Tra cá»©u nhanh há»“ sÆ¡ cÅ© náº¿u bá»‡nh nhÃ¢n Ä‘Ã£ tá»«ng khÃ¡m.</li>
          <li>Chọn ngày, khung giờ; bác sĩ là tùy chọn nếu bệnh nhân có yêu cầu cụ thể.</li>
          <li>Tạo lịch xong thì sang danh sách để check-in, chọn phòng và đưa vào hàng chờ.</li>
        </ol>

        <div class="helper-card">
          <div class="helper-title">LÆ°u Ã½</div>
          <ul>
            <li>KhÃ´ng cáº§n OTP vÃ¬ bá»‡nh nhÃ¢n Ä‘Ã£ cÃ³ máº·t táº¡i quáº§y.</li>
            <li>Nếu chọn bác sĩ, slot chỉ hiện khi bác sĩ đó đã được cấu hình lịch làm việc.</li>
            <li>Nếu không chọn bác sĩ, hệ thống dùng khung giờ hành chính để giữ chỗ trước.</li>
            <li>Hệ thống vẫn chặn trùng giờ của bệnh nhân và các slot đã được đặt với bác sĩ.</li>
          </ul>
        </div>
      </aside>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, onMounted, reactive, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import api from '@/services/api'
import { doctorScheduleService } from '@/services/doctorScheduleService'
import { buildBusinessHourSlots } from '@/utils/appointmentSlots'
import { toLocalDateInputValue } from '@/utils/date'

const router = useRouter()
const fullNameInput = ref<HTMLInputElement | null>(null)

const todayStr = toLocalDateInputValue()

const showLookup = ref(true)
const lookupPhone = ref('')
const lookupEmail = ref('')
const lookupLoading = ref(false)
const lookupError = ref('')
const isReturning = ref(false)

const submitting = ref(false)
const submitError = ref('')
const bookingSuccess = ref(false)
const bookingResponse = ref<any>(null)
const departments = ref<any[]>([])
const doctors = ref<any[]>([])
const selectedDepartmentId = ref('')
const availableSlots = ref<any[]>([])
const slotLoading = ref(false)

const normalizePhoneInput = (value: string) => value.replace(/\D/g, '').slice(0, 11)

const form = reactive({
  fullName: '',
  dateOfBirth: '',
  gender: '',
  phone: '',
  email: '',
  address: '',
  citizenId: '',
  insuranceCardNumber: '',
  appointmentDate: todayStr,
  doctorId: '',
  appointmentTime: '',
  reason: ''
})

const errors = reactive({
  fullName: '',
  dateOfBirth: '',
  gender: '',
  phone: '',
  email: '',
  address: '',
  appointmentDate: '',
  doctorId: '',
  appointmentTime: '',
  reason: ''
})

const filteredDoctors = computed(() =>
  doctors.value.filter((doctor) =>
    doctor.status === 'Active' &&
    (!selectedDepartmentId.value || doctor.departmentId === selectedDepartmentId.value)
  )
)

const clearErrors = () => {
  errors.fullName = ''
  errors.dateOfBirth = ''
  errors.gender = ''
  errors.phone = ''
  errors.email = ''
  errors.address = ''
  errors.appointmentDate = ''
  errors.doctorId = ''
  errors.appointmentTime = ''
  errors.reason = ''
}

const getErrorMessage = (error: any, fallback: string) => {
  const data = error?.response?.data
  if (typeof data === 'string' && data.trim()) return data
  if (typeof data?.message === 'string' && data.message.trim()) return data.message
  return fallback
}

const mapGenderToOption = (gender: string | number | null | undefined) => {
  const normalized = String(gender ?? '').toLowerCase()
  if (normalized === 'male' || normalized === '1') return '1'
  if (normalized === 'female' || normalized === '2') return '2'
  return ''
}

const validateForm = () => {
  clearErrors()
  let ok = true

  if (!form.fullName.trim()) {
    errors.fullName = 'Há» vÃ  tÃªn lÃ  báº¯t buá»™c'
    ok = false
  }

  if (!form.dateOfBirth) {
    errors.dateOfBirth = 'NgÃ y sinh lÃ  báº¯t buá»™c'
    ok = false
  }

  if (!form.gender) {
    errors.gender = 'Giá»›i tÃ­nh lÃ  báº¯t buá»™c'
    ok = false
  }

  if (!form.phone.trim()) {
    errors.phone = 'Sá»‘ Ä‘iá»‡n thoáº¡i lÃ  báº¯t buá»™c'
    ok = false
  } else if (!/^[0-9]{9,11}$/.test(form.phone.trim())) {
    errors.phone = 'Sá»‘ Ä‘iá»‡n thoáº¡i pháº£i cÃ³ 9-11 chá»¯ sá»‘'
    ok = false
  }

  if (form.email.trim() && !/\S+@\S+\.\S+/.test(form.email.trim())) {
    errors.email = 'Email khÃ´ng Ä‘Ãºng Ä‘á»‹nh dáº¡ng'
    ok = false
  }

  if (!form.address.trim()) {
    errors.address = 'Äá»‹a chá»‰ lÃ  báº¯t buá»™c'
    ok = false
  }

  if (!form.appointmentDate) {
    errors.appointmentDate = 'NgÃ y khÃ¡m lÃ  báº¯t buá»™c'
    ok = false
  } else if (form.appointmentDate < todayStr) {
    errors.appointmentDate = 'Chá»‰ Ä‘Æ°á»£c Ä‘áº·t tá»« hÃ´m nay trá»Ÿ Ä‘i'
    ok = false
  }

  if (!form.appointmentTime.trim()) {
    errors.appointmentTime = 'Slot khÃ¡m lÃ  báº¯t buá»™c'
    ok = false
  }

  if (!form.reason.trim()) {
    errors.reason = 'LÃ½ do khÃ¡m lÃ  báº¯t buá»™c'
    ok = false
  }

  return ok
}

const loadDepartments = async () => {
  const response = await api.get('/Departments')
  departments.value = response.data ?? []
}

const loadDoctors = async () => {
  const response = await api.get('/Doctor')
  doctors.value = Array.isArray(response.data) ? response.data : []
}

const loadAvailableSlots = async () => {
  availableSlots.value = []

  if (!form.appointmentDate) {
    form.appointmentTime = ''
    return
  }

  if (!form.doctorId) {
    const slots = buildBusinessHourSlots(form.appointmentDate)
    availableSlots.value = slots

    const hasSelectedSlot = slots.some((slot) => slot.startTime?.startsWith(form.appointmentTime))
    if (!hasSelectedSlot) {
      form.appointmentTime = ''
    }
    return
  }

  try {
    slotLoading.value = true
    const slots = await doctorScheduleService.getAvailableSlots(form.doctorId, form.appointmentDate)
    availableSlots.value = slots

    const hasSelectedSlot = slots.some((slot) => slot.startTime?.startsWith(form.appointmentTime))
    if (!hasSelectedSlot) {
      form.appointmentTime = ''
    }
  } finally {
    slotLoading.value = false
  }
}

const lookupPatient = async () => {
  lookupError.value = ''
  if (!lookupPhone.value.trim() && !lookupEmail.value.trim()) {
    lookupError.value = 'Nháº­p sá»‘ Ä‘iá»‡n thoáº¡i hoáº·c email Ä‘á»ƒ tra cá»©u'
    return
  }

  try {
    lookupLoading.value = true
    const { data } = await api.get('/appointments/patient-lookup', {
      params: {
        phone: lookupPhone.value.trim() || undefined,
        email: lookupEmail.value.trim() || undefined
      }
    })

    form.fullName = data.fullName || ''
    form.dateOfBirth = data.dateOfBirth?.slice(0, 10) || ''
    form.gender = mapGenderToOption(data.gender)
    form.phone = normalizePhoneInput(data.phone || '')
    form.email = data.email || ''
    form.address = data.address || ''
    form.citizenId = data.citizenId || ''
    form.insuranceCardNumber = data.insuranceCardNumber || ''
    isReturning.value = true
  } catch (error: any) {
    lookupError.value = getErrorMessage(error, 'KhÃ´ng tÃ¬m tháº¥y há»“ sÆ¡ bá»‡nh nhÃ¢n')
    isReturning.value = false
  } finally {
    lookupLoading.value = false
  }
}

const clearPrefill = async () => {
  isReturning.value = false
  await nextTick()
  fullNameInput.value?.focus()
}

const resetForm = async () => {
  form.fullName = ''
  form.dateOfBirth = ''
  form.gender = ''
  form.phone = ''
  form.email = ''
  form.address = ''
  form.citizenId = ''
  form.insuranceCardNumber = ''
  form.appointmentDate = todayStr
  form.doctorId = ''
  form.appointmentTime = ''
  form.reason = ''
  selectedDepartmentId.value = ''
  availableSlots.value = []
  submitError.value = ''
  bookingSuccess.value = false
  bookingResponse.value = null
  lookupPhone.value = ''
  lookupEmail.value = ''
  lookupError.value = ''
  isReturning.value = false
  clearErrors()
  await nextTick()
  fullNameInput.value?.focus()
}

const formatDateTime = (dateStr: string, timeStr: string) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  const [hours = '00', minutes = '00'] = String(timeStr || '').split(':')
  const day = String(date.getDate()).padStart(2, '0')
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const year = date.getFullYear()
  return `${day}/${month}/${year} ${hours}:${minutes}`
}

const submitBooking = async () => {
  submitError.value = ''
  if (!validateForm()) return

  try {
    submitting.value = true
    const departmentName = departments.value.find((item: any) => item.id === selectedDepartmentId.value)?.name
    const reasonWithDepartment = departmentName
      ? `Khoa yÃªu cáº§u: ${departmentName}${form.reason ? ' | ' + form.reason : ''}`
      : form.reason

    const response = await api.post('/staff/StaffAppointments/walk-in', {
      fullName: form.fullName.trim(),
      dateOfBirth: form.dateOfBirth,
      gender: Number(form.gender),
      phone: form.phone.trim(),
      email: form.email.trim() || null,
      address: form.address.trim(),
      citizenId: form.citizenId.trim() || null,
      insuranceCardNumber: form.insuranceCardNumber.trim() || null,
      appointmentDate: form.appointmentDate,
      doctorId: form.doctorId || null,
      appointmentTime: `${form.appointmentTime.trim()}:00`,
      reason: reasonWithDepartment
    })

    bookingResponse.value = response.data
    bookingSuccess.value = true
  } catch (error: any) {
    submitError.value = getErrorMessage(error, 'KhÃ´ng táº¡o Ä‘Æ°á»£c lá»‹ch khÃ¡m táº¡i quáº§y')
  } finally {
    submitting.value = false
  }
}

watch(selectedDepartmentId, () => {
  if (form.doctorId && !filteredDoctors.value.some((doctor) => doctor.id === form.doctorId)) {
    form.doctorId = ''
    form.appointmentTime = ''
    availableSlots.value = []
  }
})

watch([() => form.doctorId, () => form.appointmentDate], () => {
  loadAvailableSlots()
})

onMounted(async () => {
  await loadDepartments()
  await loadDoctors()
  fullNameInput.value?.focus()
})
</script>

<style scoped>
.staff-booking-page {
  max-width: 1240px;
  margin: 0 auto;
  padding: 8px 0 24px;
}

.staff-page-header {
  display: flex;
  justify-content: space-between;
  gap: 16px;
  align-items: flex-start;
  margin-bottom: 24px;
}

.staff-eyebrow {
  text-transform: uppercase;
  letter-spacing: 0.12em;
  font-size: 12px;
  font-weight: 700;
  color: #3b82f6;
  margin-bottom: 8px;
}

.staff-title {
  margin: 0 0 8px;
  font-size: 30px;
  font-weight: 700;
  color: #0f172a;
}

.staff-subtitle {
  margin: 0;
  color: #64748b;
  max-width: 720px;
}

.booking-grid {
  display: grid;
  grid-template-columns: minmax(0, 1.7fr) minmax(280px, 0.9fr);
  gap: 20px;
}

.booking-card {
  background: #fff;
  border: 1px solid #dbe3f0;
  border-radius: 22px;
  box-shadow: 0 18px 40px rgba(15, 23, 42, 0.08);
}

.main-card {
  padding: 24px;
}

.side-card {
  padding: 24px;
  background: linear-gradient(180deg, #f8fbff 0%, #eef5ff 100%);
}

.lookup-box {
  border: 1px dashed #bfd5ff;
  border-radius: 16px;
  padding: 16px;
  background: #f8fbff;
  margin-bottom: 20px;
}

.lookup-head {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  align-items: flex-start;
}

.lookup-title {
  font-weight: 700;
  color: #0f172a;
}

.lookup-note {
  font-size: 13px;
  color: #64748b;
}

.lookup-form {
  display: grid;
  grid-template-columns: 1fr 1fr auto auto;
  gap: 10px;
  margin-top: 14px;
}

.booking-form {
  display: grid;
  gap: 16px;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.form-group {
  display: grid;
  gap: 8px;
}

.form-label {
  font-weight: 600;
  color: #0f172a;
}

.form-error {
  color: #dc2626;
  font-size: 13px;
}

.submit-btn {
  min-height: 48px;
}

.appointment-code {
  font-weight: 800;
  letter-spacing: 0.08em;
  color: #1d4ed8;
}

.success-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  margin-top: 14px;
}

.tip-badge {
  display: inline-flex;
  align-items: center;
  padding: 6px 12px;
  border-radius: 999px;
  background: #dbeafe;
  color: #1d4ed8;
  font-size: 12px;
  font-weight: 700;
  margin-bottom: 16px;
}

.tip-list {
  margin: 0;
  padding-left: 18px;
  color: #334155;
  display: grid;
  gap: 12px;
}

.helper-card {
  margin-top: 20px;
  border-radius: 18px;
  padding: 18px;
  background: #fff;
  border: 1px solid #dbe3f0;
}

.helper-title {
  font-weight: 700;
  color: #0f172a;
  margin-bottom: 10px;
}

.helper-card ul {
  padding-left: 18px;
  margin: 0;
  color: #475569;
  display: grid;
  gap: 8px;
}

@media (max-width: 992px) {
  .booking-grid {
    grid-template-columns: 1fr;
  }

  .lookup-form {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 768px) {
  .staff-page-header,
  .form-row {
    grid-template-columns: 1fr;
    display: grid;
  }
}
</style>

