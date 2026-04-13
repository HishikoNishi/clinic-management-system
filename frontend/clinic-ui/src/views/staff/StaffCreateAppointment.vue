<template>
  <div class="staff-booking-page">
    <div class="staff-page-header">
      <div>
        <div class="staff-eyebrow">Staff</div>
        <h2 class="staff-title">Đặt lịch tại quầy</h2>
        <p class="staff-subtitle">
          Tạo lịch nhanh cho khách đến trực tiếp, chọn luôn bác sĩ và slot đang trống.
        </p>
      </div>
      <button type="button" class="btn btn-outline-primary" @click="router.push('/staff/appointments')">
        <i class="bi bi-arrow-left me-2"></i>Về danh sách lịch
      </button>
    </div>

    <div class="booking-grid">
      <section class="booking-card main-card">
        <div v-if="submitError" class="alert alert-danger mb-3">{{ submitError }}</div>

        <div v-if="bookingSuccess" class="alert alert-success mb-4">
          <div class="fw-semibold mb-2">Đặt lịch thành công</div>
          <div>Mã lịch hẹn: <span class="appointment-code">{{ bookingResponse.appointmentCode }}</span></div>
          <div class="small mt-2">
            {{ bookingResponse.fullName }} ·
            {{ formatDateTime(bookingResponse.appointmentDate, bookingResponse.appointmentTime) }}
          </div>
          <div class="success-actions">
            <button type="button" class="btn btn-primary" @click="resetForm">
              <i class="bi bi-plus-circle me-2"></i>Tạo lịch mới
            </button>
            <button type="button" class="btn btn-outline-success" @click="router.push('/staff/appointments')">
              <i class="bi bi-calendar-check me-2"></i>Xem trong danh sách lịch
            </button>
          </div>
        </div>

        <div class="lookup-box">
          <div class="lookup-head">
            <div>
              <div class="lookup-title">Bệnh nhân cũ?</div>
              <div class="lookup-note">Nhập số điện thoại hoặc email để điền nhanh thông tin đã có.</div>
            </div>
            <button type="button" class="btn btn-sm btn-outline-secondary" @click="showLookup = !showLookup">
              {{ showLookup ? 'Ẩn' : 'Mở tra cứu' }}
            </button>
          </div>

          <div v-if="showLookup" class="lookup-form">
            <input
              v-model="lookupPhone"
              type="tel"
              class="form-control"
              placeholder="Số điện thoại"
              inputmode="numeric"
              maxlength="11"
              @input="lookupPhone = normalizePhoneInput(lookupPhone)"
            />
            <input v-model="lookupEmail" type="email" class="form-control" placeholder="Email" />
            <button type="button" class="btn btn-outline-primary" :disabled="lookupLoading" @click="lookupPatient">
              <span v-if="lookupLoading" class="spinner-border spinner-border-sm me-1"></span>
              Điền thông tin cũ
            </button>
            <button v-if="isReturning" type="button" class="btn btn-outline-dark" @click="clearPrefill">
              Cho phép sửa
            </button>
          </div>

          <div v-if="lookupError" class="text-danger small mt-2">{{ lookupError }}</div>
          <div v-if="isReturning" class="text-success small mt-2">Đã điền thông tin từ hồ sơ cũ.</div>
        </div>

        <form class="booking-form" @submit.prevent="submitBooking">
          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Họ và tên *</label>
              <input ref="fullNameInput" v-model="form.fullName" type="text" class="form-control" :readonly="isReturning" />
              <div v-if="errors.fullName" class="form-error">{{ errors.fullName }}</div>
            </div>
            <div class="form-group">
              <label class="form-label">Ngày sinh *</label>
              <input v-model="form.dateOfBirth" type="date" class="form-control" :readonly="isReturning" />
              <div v-if="errors.dateOfBirth" class="form-error">{{ errors.dateOfBirth }}</div>
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Giới tính *</label>
              <select v-model="form.gender" class="form-select" :disabled="isReturning">
                <option value="">Chọn giới tính</option>
                <option value="1">Nam</option>
                <option value="2">Nữ</option>
              </select>
              <div v-if="errors.gender" class="form-error">{{ errors.gender }}</div>
            </div>
            <div class="form-group">
              <label class="form-label">Điện thoại *</label>
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
              <input v-model="form.email" type="email" class="form-control" placeholder="Có thể để trống" />
              <div v-if="errors.email" class="form-error">{{ errors.email }}</div>
            </div>
            <div class="form-group">
              <label class="form-label">Địa chỉ *</label>
              <input v-model="form.address" type="text" class="form-control" :readonly="isReturning" />
              <div v-if="errors.address" class="form-error">{{ errors.address }}</div>
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Số CCCD</label>
              <input v-model="form.citizenId" type="text" class="form-control" placeholder="Nhập 12 số CCCD" maxlength="12" />
            </div>
            <div class="form-group">
              <label class="form-label">Mã số BHYT</label>
              <input v-model="form.insuranceCardNumber" type="text" class="form-control" placeholder="Ví dụ: GD479..." />
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Ngày khám *</label>
              <input v-model="form.appointmentDate" type="date" class="form-control" :min="todayStr" />
              <div v-if="errors.appointmentDate" class="form-error">{{ errors.appointmentDate }}</div>
            </div>
            <div class="form-group">
              <label class="form-label">Khoa mong muốn (tùy chọn)</label>
              <select v-model="selectedDepartmentId" class="form-select">
                <option value="">Tất cả khoa</option>
                <option v-for="department in departments" :key="department.id" :value="department.id">
                  {{ department.name }}
                </option>
              </select>
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Bác sĩ *</label>
              <select v-model="form.doctorId" class="form-select">
                <option value="">Chọn bác sĩ</option>
                <option v-for="doctor in filteredDoctors" :key="doctor.id" :value="doctor.id">
                  {{ doctor.fullName }} - {{ doctor.departmentName }}
                </option>
              </select>
              <div v-if="errors.doctorId" class="form-error">{{ errors.doctorId }}</div>
            </div>
            <div class="form-group">
              <label class="form-label">Slot khám *</label>
              <select v-model="form.appointmentTime" class="form-select" :disabled="!form.doctorId || !form.appointmentDate || slotLoading">
                <option value="">{{ slotLoading ? 'Đang tải slot...' : 'Chọn slot khám' }}</option>
                <option v-for="slot in availableSlots" :key="slot.id || `${slot.shiftCode}-${slot.startTime}`" :value="String(slot.startTime).slice(0, 5)">
                  {{ slot.slotLabel }}
                </option>
              </select>
              <div v-if="errors.appointmentTime" class="form-error">{{ errors.appointmentTime }}</div>
            </div>
          </div>

          <div class="form-group">
            <label class="form-label">Lý do khám *</label>
            <textarea
              v-model="form.reason"
              rows="4"
              class="form-control"
              placeholder="Mô tả triệu chứng hoặc nhu cầu khám của bệnh nhân"
            ></textarea>
            <div v-if="errors.reason" class="form-error">{{ errors.reason }}</div>
          </div>

          <button type="submit" class="btn btn-primary submit-btn" :disabled="submitting">
            <span v-if="submitting" class="spinner-border spinner-border-sm me-2"></span>
            <i v-else class="bi bi-calendar-plus me-2"></i>
            {{ submitting ? 'Đang tạo lịch...' : 'Tạo lịch khám' }}
          </button>
        </form>
      </section>

      <aside class="booking-card side-card">
        <div class="tip-badge">Quy trình gợi ý</div>
        <ol class="tip-list">
          <li>Tra cứu nhanh hồ sơ cũ nếu bệnh nhân đã từng khám.</li>
          <li>Chọn ngày làm việc, bác sĩ và slot còn trống.</li>
          <li>Tạo lịch xong thì sang danh sách để check-in và thu tạm ứng.</li>
        </ol>

        <div class="helper-card">
          <div class="helper-title">Lưu ý</div>
          <ul>
            <li>Không cần OTP vì bệnh nhân đã có mặt tại quầy.</li>
            <li>Slot chỉ hiện khi bác sĩ đã được cấu hình lịch làm việc.</li>
            <li>Hệ thống tự chặn trùng giờ của cả bệnh nhân và bác sĩ.</li>
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
    errors.fullName = 'Họ và tên là bắt buộc'
    ok = false
  }

  if (!form.dateOfBirth) {
    errors.dateOfBirth = 'Ngày sinh là bắt buộc'
    ok = false
  }

  if (!form.gender) {
    errors.gender = 'Giới tính là bắt buộc'
    ok = false
  }

  if (!form.phone.trim()) {
    errors.phone = 'Số điện thoại là bắt buộc'
    ok = false
  } else if (!/^[0-9]{9,11}$/.test(form.phone.trim())) {
    errors.phone = 'Số điện thoại phải có 9-11 chữ số'
    ok = false
  }

  if (form.email.trim() && !/\S+@\S+\.\S+/.test(form.email.trim())) {
    errors.email = 'Email không đúng định dạng'
    ok = false
  }

  if (!form.address.trim()) {
    errors.address = 'Địa chỉ là bắt buộc'
    ok = false
  }

  if (!form.appointmentDate) {
    errors.appointmentDate = 'Ngày khám là bắt buộc'
    ok = false
  } else if (form.appointmentDate < todayStr) {
    errors.appointmentDate = 'Chỉ được đặt từ hôm nay trở đi'
    ok = false
  }

  if (!form.doctorId) {
    errors.doctorId = 'Vui lòng chọn bác sĩ'
    ok = false
  }

  if (!form.appointmentTime.trim()) {
    errors.appointmentTime = 'Slot khám là bắt buộc'
    ok = false
  }

  if (!form.reason.trim()) {
    errors.reason = 'Lý do khám là bắt buộc'
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

  if (!form.doctorId || !form.appointmentDate) {
    form.appointmentTime = ''
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
    lookupError.value = 'Nhập số điện thoại hoặc email để tra cứu'
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
    lookupError.value = getErrorMessage(error, 'Không tìm thấy hồ sơ bệnh nhân')
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
      ? `Khoa yêu cầu: ${departmentName}${form.reason ? ' | ' + form.reason : ''}`
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
      doctorId: form.doctorId,
      appointmentTime: `${form.appointmentTime.trim()}:00`,
      reason: reasonWithDepartment
    })

    bookingResponse.value = response.data
    bookingSuccess.value = true
  } catch (error: any) {
    submitError.value = getErrorMessage(error, 'Không tạo được lịch khám tại quầy')
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
