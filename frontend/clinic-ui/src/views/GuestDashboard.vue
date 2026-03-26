<template>
  <div class="guest-dashboard">
    <!-- Hero -->
    <section class="hero">
      <div class="hero-background">
        <div class="shape shape-1"></div>
        <div class="shape shape-2"></div>
        <div class="shape shape-3"></div>
      </div>
      <div class="hero-content">
        <h1 class="hero-title">Chào mừng đến Phòng khám</h1>
        <p class="hero-subtitle">
          Đặt lịch nhanh, nhận mã khám qua email sau khi xác thực OTP.
        </p>
        <button @click="scrollToBooking" class="btn btn-primary">
          <i class="bi bi-calendar-check me-2"></i>Đặt lịch hẹn
        </button>
      </div>
    </section>

    <!-- Booking -->
    <section id="booking" class="booking-section">
      <div class="container">
        <h2 class="section-title">Đặt lịch khám</h2>
        <p class="section-subtitle">Nhập thông tin, xác thực email bằng OTP rồi đặt lịch.</p>

        <!-- Success -->
        <div v-if="bookingSuccess" class="alert alert-success">
          <i class="bi bi-check-circle me-2"></i>
          <div>
            <strong>Lịch khám đã được đặt thành công!</strong>
            <p class="mt-2">
              <strong>Mã khám:</strong> <span class="appointment-code">{{ bookingResponse.appointmentCode }}</span>
            </p>
            <p><strong>Trạng thái:</strong> <span class="badge badge-info">{{ bookingResponse.status }}</span></p>
            <p>
              <strong>Ngày & Giờ:</strong>
              {{ formatDateTime(bookingResponse.appointmentDate, bookingResponse.appointmentTime) }}
            </p>
          </div>
        </div>

        <!-- Error -->
        <div v-if="bookingError" class="alert alert-danger">
          <i class="bi bi-exclamation-circle me-2"></i>
          <div>
            <strong>Lỗi</strong>
            <p class="mt-2">{{ bookingError }}</p>
          </div>
        </div>

        <!-- Booking Form -->
        <form v-if="!bookingSuccess" class="booking-form" @submit.prevent="submitBooking">
          <!-- Returning patient -->
          <div class="returning-box">
            <div class="d-flex justify-content-between align-items-start gap-2 flex-wrap">
              <div>
                <button type="button" class="btn btn-link p-0 text-decoration-none" @click="showReturningLookup = !showReturningLookup">
                  <span class="fw-bold text-body">Bệnh nhân cũ?</span>
                  <i class="bi ms-1" :class="showReturningLookup ? 'bi-chevron-up' : 'bi-chevron-down'"></i>
                </button>
                <div v-if="!showReturningLookup" class="text-muted small">
                  Nhập SĐT hoặc email rồi bấm “Điền thông tin cũ”.
                </div>
              </div>
              <span v-if="isReturning" class="badge bg-success-subtle text-success">Đã điền từ hồ sơ trước</span>
            </div>

            <div v-show="showReturningLookup" class="mt-3">
              <div class="form-row">
                <div class="form-group">
                  <input v-model="lookupPhone" type="tel" class="form-input" placeholder="Số điện thoại" />
                </div>
                <div class="form-group">
                  <input v-model="lookupEmail" type="email" class="form-input" placeholder="Email" />
                </div>
                <div class="form-group">
                  <button type="button" class="btn btn-outline-primary w-100" :disabled="lookupLoading" @click="lookupPatient">
                    <span v-if="lookupLoading" class="spinner-border spinner-border-sm me-1"></span>
                    Điền thông tin cũ
                  </button>
                </div>
                <div class="form-group" v-if="isReturning">
                  <button type="button" class="btn btn-secondary w-100" @click="clearPrefill">Chỉnh sửa</button>
                </div>
              </div>
              <div v-if="lookupError" class="alert alert-warning py-2 my-2 mb-0">{{ lookupError }}</div>
            </div>
          </div>

          <!-- Patient info -->
          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Họ và tên *</label>
              <input v-model="bookingForm.fullName" type="text" class="form-input" :readonly="isReturning" required />
              <span v-if="bookingErrors.fullName" class="form-error">{{ bookingErrors.fullName }}</span>
            </div>
            <div class="form-group">
              <label class="form-label">Ngày sinh *</label>
              <input v-model="bookingForm.dateOfBirth" type="date" class="form-input" :readonly="isReturning" required />
              <span v-if="bookingErrors.dateOfBirth" class="form-error">{{ bookingErrors.dateOfBirth }}</span>
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Giới tính *</label>
              <select v-model="bookingForm.gender" class="form-select" :disabled="isReturning" required>
                <option value="">Chọn giới tính</option>
                <option value="1">Nam</option>
                <option value="2">Nữ</option>
              </select>
              <span v-if="bookingErrors.gender" class="form-error">{{ bookingErrors.gender }}</span>
            </div>
            <div class="form-group">
              <label class="form-label">Điện thoại *</label>
              <input v-model="bookingForm.phone" type="tel" class="form-input" :readonly="isReturning" required />
              <span v-if="bookingErrors.phone" class="form-error">{{ bookingErrors.phone }}</span>
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Email *</label>
              <input v-model="bookingForm.email" type="email" class="form-input" required />
              <span v-if="bookingErrors.email" class="form-error">{{ bookingErrors.email }}</span>
            </div>
            <div class="form-group">
              <label class="form-label">Địa chỉ *</label>
              <input v-model="bookingForm.address" type="text" class="form-input" :readonly="isReturning" required />
              <span v-if="bookingErrors.address" class="form-error">{{ bookingErrors.address }}</span>
            </div>
          </div>

          <!-- OTP -->
          <div class="form-row align-items-end otp-row">
            <div class="form-group flex-grow-1">
              <label class="form-label">Mã OTP</label>
              <div class="d-flex flex-wrap gap-2">
                <input v-model="otpCode" type="text" maxlength="6" class="form-input flex-grow-1" placeholder="Nhập mã OTP" />
                <button type="button" class="btn btn-outline-success" :disabled="otpVerifying || otpVerified" @click="verifyOtp">
                  <span v-if="otpVerifying" class="spinner-border spinner-border-sm me-1"></span>
                  {{ otpVerified ? 'Đã xác thực' : 'Xác thực OTP' }}
                </button>
              </div>
              <div class="text-danger small mt-1" v-if="otpError">{{ otpError }}</div>
              <div class="text-success small" v-else-if="otpVerified">Email đã được xác thực.</div>
              <div class="text-muted small" v-else>Bạn cần xác thực email trước khi đặt lịch.</div>
            </div>
            <div class="form-group" style="min-width: 200px;">
              <label class="form-label">&nbsp;</label>
              <button type="button" class="btn btn-primary w-100" :disabled="otpSending || countdown > 0" @click="sendOtp">
                <span v-if="otpSending" class="spinner-border spinner-border-sm me-1"></span>
                {{ countdown > 0 ? `Gửi lại (${countdown}s)` : 'Gửi mã OTP' }}
              </button>
              <div class="text-muted small mt-1" v-if="countdown > 0">Có thể gửi lại sau {{ countdown }}s</div>
            </div>
          </div>

          <!-- Appointment info -->
          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Ngày khám *</label>
              <input v-model="bookingForm.appointmentDate" type="date" class="form-input" required />
              <span v-if="bookingErrors.appointmentDate" class="form-error">{{ bookingErrors.appointmentDate }}</span>
            </div>
            <div class="form-group">
              <label class="form-label">Thời gian khám *</label>
              <input v-model="bookingForm.appointmentTime" type="time" class="form-input" required />
              <span v-if="bookingErrors.appointmentTime" class="form-error">{{ bookingErrors.appointmentTime }}</span>
            </div>
          </div>

          <div class="form-group">
            <label class="form-label">Lý do khám *</label>
            <textarea v-model="bookingForm.reason" class="form-textarea" required rows="4" placeholder="Vui lòng mô tả lý do khám của bạn"></textarea>
            <span v-if="bookingErrors.reason" class="form-error">{{ bookingErrors.reason }}</span>
          </div>

          <button type="submit" class="btn btn-primary w-100" :disabled="bookingLoading">
            <i v-if="!bookingLoading" class="bi bi-calendar-check me-2"></i>
            <i v-else class="bi bi-spinner animate-spin me-2"></i>
            {{ bookingLoading ? 'Đang đặt...' : 'Đặt lịch khám' }}
          </button>
        </form>

        <button v-if="bookingSuccess" @click="resetBookingForm" class="btn btn-secondary w-100 mt-2">
          <i class="bi bi-arrow-counterclockwise me-2"></i>Đặt lịch khám khác
        </button>
      </div>
    </section>

    <!-- Search -->
    <section id="search" class="search-section">
      <div class="container">
        <h2 class="section-title">Tìm lịch khám của bạn</h2>
        <p class="section-subtitle">Nhập mã khám và số điện thoại để kiểm tra trạng thái</p>

        <div v-if="searchError" class="alert alert-danger">
          <i class="bi bi-exclamation-circle me-2"></i>{{ searchError }}
        </div>

        <div v-if="searchResult" class="search-result">
          <div class="result-card">
            <div class="result-header">
              <h3>Chi tiết lịch khám</h3>
              <span class="badge" :class="`badge-${getStatusClass(searchResult.status)}`">{{ searchResult.status }}</span>
            </div>
            <div class="result-grid">
              <div class="result-item"><label>Mã lịch khám</label><p class="result-value">{{ searchResult.appointmentCode }}</p></div>
              <div class="result-item"><label>Tên bệnh nhân</label><p class="result-value">{{ searchResult.fullName }}</p></div>
              <div class="result-item"><label>Ngày & Giờ</label><p class="result-value">{{ formatDateTime(searchResult.appointmentDate, searchResult.appointmentTime) }}</p></div>
              <div class="result-item"><label>Điện thoại</label><p class="result-value">{{ searchResult.phone }}</p></div>
              <div class="result-item"><label>Email</label><p class="result-value">{{ searchResult.email }}</p></div>
              <div class="result-item"><label>Lý do</label><p class="result-value">{{ searchResult.reason }}</p></div>
            </div>
            <div class="result-actions">
              <button @click="cancelAppointment" class="btn btn-danger" :disabled="cancelLoading">
                <span v-if="cancelLoading" class="spinner-border spinner-border-sm me-1"></span>
                <i v-else class="bi bi-x-circle me-2"></i>Huỷ lịch khám
              </button>
              <button @click="resetSearch" class="btn btn-secondary"><i class="bi bi-search me-2"></i>Tìm kiếm khác</button>
            </div>
          </div>
        </div>

        <form v-else @submit.prevent="submitSearch" class="search-form">
          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Mã lịch khám *</label>
              <input v-model="searchForm.appointmentCode" type="text" class="form-input" required />
            </div>
            <div class="form-group">
              <label class="form-label">Số điện thoại *</label>
              <input v-model="searchForm.phone" type="tel" class="form-input" required />
            </div>
          </div>
          <button type="submit" class="btn btn-primary" :disabled="searchLoading">
            <i v-if="!searchLoading" class="bi bi-search me-2"></i>
            <i v-else class="bi bi-spinner animate-spin me-2"></i>
            {{ searchLoading ? 'Đang tìm kiếm...' : 'Tìm lịch khám' }}
          </button>
        </form>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import api from '@/services/api'
import '@/styles/layouts/guest-dashboard.css'

/* Services display */
const services = reactive([
  { id: 1, name: 'Khám tổng quát', description: 'Kiểm tra sức khỏe và tư vấn', icon: 'bi bi-stethoscope', color: 'blue' },
  { id: 2, name: 'Chăm sóc răng', description: 'Dịch vụ nha khoa', icon: 'bi bi-tooth', color: 'sky' },
  { id: 3, name: 'Chăm sóc cấp cứu', description: 'Hỗ trợ khẩn cấp 24/7', icon: 'bi bi-exclamation-triangle', color: 'red' }
])

/* Booking state */
const bookingForm = reactive({
  fullName: '',
  dateOfBirth: '',
  gender: '',
  phone: '',
  email: '',
  address: '',
  appointmentDate: '',
  appointmentTime: '',
  reason: ''
})

const bookingErrors = reactive<Record<string, string>>({
  fullName: '',
  dateOfBirth: '',
  gender: '',
  phone: '',
  email: '',
  address: '',
  appointmentDate: '',
  appointmentTime: '',
  reason: ''
})

const bookingLoading = ref(false)
const bookingError = ref('')
const bookingSuccess = ref(false)
const bookingResponse = ref({ appointmentCode: '', status: '', appointmentDate: '', appointmentTime: '' })

/* OTP */
const otpCode = ref('')
const otpSent = ref(false)
const otpVerified = ref(false)
const otpSending = ref(false)
const otpVerifying = ref(false)
const otpError = ref('')
const countdown = ref(0)
let countdownTimer: any = null

/* Returning patient lookup */
const lookupPhone = ref('')
const lookupEmail = ref('')
const lookupLoading = ref(false)
const lookupError = ref('')
const isReturning = ref(false)
const showReturningLookup = ref(false)

/* Search */
const searchForm = reactive({ appointmentCode: '', phone: '' })
const searchLoading = ref(false)
const searchError = ref('')
const searchResult = ref<any>(null)
const cancelLoading = ref(false)
const cancelError = ref('')

/* Helpers */
const scrollToBooking = () => document.getElementById('booking')?.scrollIntoView({ behavior: 'smooth' })

const formatDateTime = (dateStr: string, timeStr: string) => {
  if (!dateStr || !timeStr) return ''
  const date = new Date(dateStr)
  const [h, m] = timeStr.split(':')
  return `${date.toLocaleDateString()} ${h}:${m}`
}

const validateBookingForm = () => {
  Object.keys(bookingErrors).forEach((k) => (bookingErrors[k] = ''))
  let ok = true
  if (!bookingForm.fullName.trim()) { bookingErrors.fullName = 'Họ và tên là bắt buộc'; ok = false }
  if (!bookingForm.dateOfBirth) { bookingErrors.dateOfBirth = 'Ngày sinh là bắt buộc'; ok = false }
  if (!bookingForm.gender) { bookingErrors.gender = 'Giới tính là bắt buộc'; ok = false }
  if (!bookingForm.phone.trim()) { bookingErrors.phone = 'Số điện thoại là bắt buộc'; ok = false }
  else if (!/^[0-9]{9,11}$/.test(bookingForm.phone)) { bookingErrors.phone = 'Số điện thoại phải có 9-11 chữ số'; ok = false }
  if (!bookingForm.email.trim()) { bookingErrors.email = 'Email là bắt buộc'; ok = false }
  else if (!/\S+@\S+\.\S+/.test(bookingForm.email)) { bookingErrors.email = 'Định dạng email không hợp lệ'; ok = false }
  if (!bookingForm.address.trim()) { bookingErrors.address = 'Địa chỉ là bắt buộc'; ok = false }
  if (!bookingForm.appointmentDate) { bookingErrors.appointmentDate = 'Ngày khám là bắt buộc'; ok = false }
  if (!bookingForm.appointmentTime) { bookingErrors.appointmentTime = 'Thời gian khám là bắt buộc'; ok = false }
  if (!bookingForm.reason.trim()) { bookingErrors.reason = 'Lý do khám là bắt buộc'; ok = false }
  return ok
}

const applyPrefill = (data: any) => {
  bookingForm.fullName = data.fullName || ''
  bookingForm.dateOfBirth = data.dateOfBirth?.slice(0, 10) || ''
  bookingForm.gender = data.gender != null ? String(data.gender) : ''
  bookingForm.phone = data.phone || ''
  bookingForm.email = data.email || ''
  bookingForm.address = data.address || ''
  isReturning.value = true
}

const clearPrefill = () => {
  isReturning.value = false
  lookupError.value = ''
}

const lookupPatient = async () => {
  lookupError.value = ''
  if (!lookupPhone.value.trim() && !lookupEmail.value.trim()) {
    lookupError.value = 'Nhập SĐT hoặc email để tra cứu'
    return
  }
  try {
    lookupLoading.value = true
    const res = await api.get('/appointments/patient-lookup', {
      params: { phone: lookupPhone.value || undefined, email: lookupEmail.value || undefined }
    })
    applyPrefill(res.data)
  } catch (err: any) {
    console.error(err)
    lookupError.value = err?.response?.data?.message || 'Không tìm thấy bệnh nhân phù hợp'
  } finally {
    lookupLoading.value = false
  }
}

/* OTP */
const startCountdown = () => {
  if (countdownTimer) clearInterval(countdownTimer)
  countdown.value = 60
  countdownTimer = setInterval(() => {
    countdown.value -= 1
    if (countdown.value <= 0 && countdownTimer) {
      clearInterval(countdownTimer)
      countdownTimer = null
    }
  }, 1000)
}

const sendOtp = async () => {
  otpError.value = ''
  if (!bookingForm.email?.trim()) {
    otpError.value = 'Vui lòng nhập email trước khi gửi OTP'
    return
  }
  try {
    otpSending.value = true
    await api.post('/email/send-otp', { email: bookingForm.email })
    otpSent.value = true
    otpVerified.value = false
    startCountdown()
  } catch (err: any) {
    console.error(err)
    otpError.value = err?.response?.data?.message || 'Không gửi được OTP, thử lại sau'
  } finally {
    otpSending.value = false
  }
}

const verifyOtp = async () => {
  otpError.value = ''
  if (!otpCode.value.trim()) {
    otpError.value = 'Nhập mã OTP'
    return
  }
  try {
    otpVerifying.value = true
    await api.post('/email/verify-otp', { email: bookingForm.email, code: otpCode.value })
    otpVerified.value = true
    otpError.value = ''
  } catch (err: any) {
    console.error(err)
    otpError.value = err?.response?.data?.message || 'OTP sai hoặc đã hết hạn'
    otpVerified.value = false
  } finally {
    otpVerifying.value = false
  }
}

/* Booking submit */
const submitBooking = async () => {
  if (!validateBookingForm()) return
  if (!otpVerified.value) { bookingError.value = 'Vui lòng gửi và xác thực OTP email trước khi đặt lịch'; return }

  try {
    bookingLoading.value = true
    bookingError.value = ''
    const response = await api.post('/appointments', {
      fullName: bookingForm.fullName,
      dateOfBirth: bookingForm.dateOfBirth,
      gender: parseInt(bookingForm.gender),
      phone: bookingForm.phone,
      email: bookingForm.email,
      address: bookingForm.address,
      appointmentDate: bookingForm.appointmentDate,
      appointmentTime: bookingForm.appointmentTime + ':00',
      reason: bookingForm.reason
    })
    bookingResponse.value = response.data
    bookingSuccess.value = true
  } catch (error: any) {
    bookingError.value = error.response?.data?.message || 'Không thể đặt lịch khám. Vui lòng thử lại.'
    console.error('Booking error:', error)
  } finally {
    bookingLoading.value = false
  }
}

const resetBookingForm = () => {
  Object.keys(bookingForm).forEach((k) => (bookingForm as any)[k] = '')
  bookingSuccess.value = false
  bookingError.value = ''
  otpCode.value = ''
  otpSent.value = false
  otpVerified.value = false
  otpError.value = ''
  countdown.value = 0
}

/* Search */
const submitSearch = async () => {
  if (!searchForm.appointmentCode.trim() || !searchForm.phone.trim()) {
    searchError.value = 'Vui lòng nhập mã lịch khám và số điện thoại'
    return
  }
  try {
    searchLoading.value = true
    searchError.value = ''
    const response = await api.post('/appointments/search', searchForm)
    searchResult.value = response.data
  } catch (error: any) {
    searchError.value = error.response?.data?.message || 'Không tìm thấy lịch khám'
  } finally {
    searchLoading.value = false
  }
}

const cancelAppointment = async () => {
  if (!searchResult.value) return
  try {
    cancelLoading.value = true
    await api.post('/appointments/cancel', {
      appointmentCode: searchResult.value.appointmentCode,
      fullName: searchResult.value.fullName,
      phone: searchResult.value.phone
    })
    resetSearch()
  } catch (error: any) {
    cancelError.value = error.response?.data?.message || 'Không thể huỷ lịch'
  } finally {
    cancelLoading.value = false
  }
}

const resetSearch = () => {
  searchResult.value = null
  searchError.value = ''
  searchForm.appointmentCode = ''
  searchForm.phone = ''
}

const getStatusClass = (status: string) => {
  switch (status?.toLowerCase()) {
    case 'pending': return 'warning'
    case 'confirmed': return 'info'
    case 'completed': return 'success'
    case 'cancelled': return 'danger'
    default: return 'secondary'
  }
}
</script>
