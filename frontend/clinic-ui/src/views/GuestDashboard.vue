<template>
  <div class="guest-dashboard">
    <!-- Hero Section -->
    <section class="hero">
      <div class="hero-background">
        <div class="shape shape-1"></div>
        <div class="shape shape-2"></div>
        <div class="shape shape-3"></div>
      </div>
      <div class="hero-content">
        <h1 class="hero-title">Chào mừng đến Phòng khám của chúng tôi</h1>
        <p class="hero-subtitle">
          Dịch vụ chăm sóc sức khỏe chất lượng có sẵn cho bạn và gia đình
        </p>
        <button @click="scrollToBooking" class="btn btn-primary">
          <i class="bi bi-calendar-check me-2"></i>Đặt lịch hẹn
        </button>
      </div>
    </section>

    <!-- Services Section -->
    <section id="services" class="services-section">
      <div class="container">
        <h2 class="section-title">Dịch vụ của chúng tôi</h2>
        <p class="section-subtitle">
          Chúng tôi cung cấp các dịch vụ chăm sóc sức khỏe toàn diện
        </p>

        <div class="services-grid">
          <div class="service-card" v-for="service in services" :key="service.id">
            <div class="service-icon" :class="`icon-${service.color}`">
              <i :class="service.icon"></i>
            </div>
            <h3>{{ service.name }}</h3>
            <p>{{ service.description }}</p>
          </div>
        </div>
      </div>
    </section>

    <!-- Booking Section -->
    <section id="booking" class="booking-section">
      <div class="container">
        <h2 class="section-title">Đặt lịch khám của bạn</h2>
        <p class="section-subtitle">
          Điền vào form dưới đây để lên lịch khám
        </p>

        <!-- Success Alert -->
        <div v-if="bookingSuccess" class="alert alert-success">
          <i class="bi bi-check-circle me-2"></i>
          <div>
            <strong>Lịch khám đã được đặt thành công!</strong>
            <p class="mt-2">
              <strong>Mã lịch khám:</strong>
              <span class="appointment-code">{{ bookingResponse.appointmentCode }}</span>
            </p>
            <p>
              <strong>Trạng thái:</strong>
              <span class="badge badge-info">{{ bookingResponse.status }}</span>
            </p>
            <p>
              <strong>Ngày & Giờ:</strong>
              {{ formatDateTime(bookingResponse.appointmentDate, bookingResponse.appointmentTime) }}
            </p>
          </div>
        </div>

        <!-- Error Alert -->
        <div v-if="bookingError" class="alert alert-danger">
          <i class="bi bi-exclamation-circle me-2"></i>
          <div>
            <strong>Lỗi</strong>
            <p class="mt-2">{{ bookingError }}</p>
          </div>
        </div>

        <!-- Booking Form -->
        <form @submit.prevent="submitBooking" class="booking-form" v-if="!bookingSuccess">
          <div class="form-row">
            <!-- Full Name -->
            <div class="form-group">
              <label class="form-label">Họ và tên *</label>
              <input
                v-model="bookingForm.fullName"
                type="text"
                class="form-input"
                required
                placeholder="Nhập họ và tên của bạn"
              />
              <span v-if="bookingErrors.fullName" class="form-error">
                {{ bookingErrors.fullName }}
              </span>
            </div>

            <!-- Date of Birth -->
            <div class="form-group">
              <label class="form-label">Ngày sinh *</label>
              <input
                v-model="bookingForm.dateOfBirth"
                type="date"
                class="form-input"
                required
              />
              <span v-if="bookingErrors.dateOfBirth" class="form-error">
                {{ bookingErrors.dateOfBirth }}
              </span>
            </div>
          </div>

          <div class="form-row">
            <!-- Gender -->
            <div class="form-group">
              <label class="form-label">Giới tính *</label>
              <select v-model="bookingForm.gender" class="form-select" required>
                <option value="">Chọn giới tính</option>
                <option value="1">Nam</option>
                <option value="2">Nữ</option>
              </select>
              <span v-if="bookingErrors.gender" class="form-error">
                {{ bookingErrors.gender }}
              </span>
            </div>

            <!-- Phone -->
            <div class="form-group">
              <label class="form-label">Điện thoại *</label>
              <input
                v-model="bookingForm.phone"
                type="tel"
                class="form-input"
                required
                placeholder="Nhập số điện thoại của bạn"
              />
              <span v-if="bookingErrors.phone" class="form-error">
                {{ bookingErrors.phone }}
              </span>
            </div>
          </div>

          <div class="form-row">
            <!-- Email -->
            <div class="form-group">
              <label class="form-label">Email *</label>
              <input
                v-model="bookingForm.email"
                type="email"
                class="form-input"
                required
                placeholder="Nhập email của bạn"
              />
              <span v-if="bookingErrors.email" class="form-error">
                {{ bookingErrors.email }}
              </span>
            </div>

            <!-- Address -->
            <div class="form-group">
              <label class="form-label">Địa chỉ *</label>
              <input
                v-model="bookingForm.address"
                type="text"
                class="form-input"
                required
                placeholder="Nhập địa chỉ của bạn"
              />
              <span v-if="bookingErrors.address" class="form-error">
                {{ bookingErrors.address }}
              </span>
            </div>
          </div>

          <div class="form-row">
            <!-- Appointment Date -->
            <div class="form-group">
              <label class="form-label">Ngày khám *</label>
              <input
                v-model="bookingForm.appointmentDate"
                type="date"
                class="form-input"
                required
              />
              <span v-if="bookingErrors.appointmentDate" class="form-error">
                {{ bookingErrors.appointmentDate }}
              </span>
            </div>

            <!-- Appointment Time -->
            <div class="form-group">
              <label class="form-label">Thời gian khám *</label>
              <input
                v-model="bookingForm.appointmentTime"
                type="time"
                class="form-input"
                required
              />
              <span v-if="bookingErrors.appointmentTime" class="form-error">
                {{ bookingErrors.appointmentTime }}
              </span>
            </div>
          </div>

          <!-- Reason -->
          <div class="form-group">
            <label class="form-label">Lý do khám *</label>
            <textarea
              v-model="bookingForm.reason"
              class="form-textarea"
              required
              placeholder="Vui lòng mô tả lý do khám của bạn"
              rows="4"
            ></textarea>
            <span v-if="bookingErrors.reason" class="form-error">
              {{ bookingErrors.reason }}
            </span>
          </div>

          <!-- Submit Button -->
          <button
            type="submit"
            class="btn btn-primary w-100"
            :disabled="bookingLoading"
          >
            <i v-if="!bookingLoading" class="bi bi-calendar-check me-2"></i>
            <i v-else class="bi bi-spinner animate-spin me-2"></i>
            {{ bookingLoading ? "Đang đặt..." : "Đặt lịch khám" }}
          </button>
        </form>

        <!-- Reset Button (shown after success) -->
        <button
          v-if="bookingSuccess"
          @click="resetBookingForm"
          class="btn btn-secondary w-100"
        >
          <i class="bi bi-arrow-counterclockwise me-2"></i>
          Đặt lịch khám khác
        </button>
      </div>
    </section>

    <!-- Search Section -->
    <section id="search" class="search-section">
      <div class="container">
        <h2 class="section-title">Tìm lịch khám của bạn</h2>
        <p class="section-subtitle">
          Nhập mã lịch khám và số điện thoại để kiểm tra trạng thái
        </p>

        <!-- Search Error Alert -->
        <div v-if="searchError" class="alert alert-danger">
          <i class="bi bi-exclamation-circle me-2"></i>
          {{ searchError }}
        </div>

        <!-- Search Results -->
        <div v-if="searchResult" class="search-result">
          <div class="result-card">
            <div class="result-header">
              <h3>Chi tiết lịch khám</h3>
              <span class="badge" :class="`badge-${getStatusClass(searchResult.status)}`">
                {{ searchResult.status }}
              </span>
            </div>

            <div class="result-grid">
              <div class="result-item">
                <label>Mã lịch khám</label>
                <p class="result-value">{{ searchResult.appointmentCode }}</p>
              </div>

              <div class="result-item">
                <label>Tên bệnh nhân</label>
                <p class="result-value">{{ searchResult.fullName }}</p>
              </div>

              <div class="result-item">
                <label>Ngày & Giờ</label>
                <p class="result-value">
                  {{ formatDateTime(searchResult.appointmentDate, searchResult.appointmentTime) }}
                </p>
              </div>

              <div class="result-item">
                <label>Điện thoại</label>
                <p class="result-value">{{ searchResult.phone }}</p>
              </div>

              <div class="result-item">
                <label>Email</label>
                <p class="result-value">{{ searchResult.email }}</p>
              </div>

              <div class="result-item">
                <label>Lý do</label>
                <p class="result-value">{{ searchResult.reason }}</p>
              </div>
            </div>

            <div class="result-actions">
              <button @click="cancelAppointment" class="btn btn-danger">
                <i class="bi bi-x-circle me-2"></i>
                Hủy lịch khám
              </button>

              <button @click="resetSearch" class="btn btn-secondary">
                <i class="bi bi-search me-2"></i>
                Tìm kiếm khác
              </button>
            </div>
          </div>
        </div>

        <!-- Search Form -->
        <form @submit.prevent="submitSearch" class="search-form" v-else>
          <div class="form-row">
            <!-- Appointment Code -->
            <div class="form-group">
              <label class="form-label">Mã lịch khám *</label>
              <input
                v-model="searchForm.appointmentCode"
                type="text"
                class="form-input"
                required
                placeholder="Nhập mã lịch khám của bạn"
              />
            </div>

            <!-- Phone -->
            <div class="form-group">
              <label class="form-label">Số điện thoại *</label>
              <input
                v-model="searchForm.phone"
                type="tel"
                class="form-input"
                required
                placeholder="Nhập số điện thoại của bạn" 
              />
            </div>
          </div>

          <!-- Submit Button -->
          <button
            type="submit"
            class="btn btn-primary"
            :disabled="searchLoading"
          >
            <i v-if="!searchLoading" class="bi bi-search me-2"></i>
            <i v-else class="bi bi-spinner animate-spin me-2"></i>
            {{ searchLoading ? "Đang tìm kiếm..." : "Tìm lịch khám" }}
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
const cancelLoading = ref(false)
const cancelError = ref('')
// Services data
const services = reactive([
  {
    id: 1,
    name: 'Khám tổng quát',
    description: 'Kiểm tra sức khỏe toàn diện và tư vấn với các bác sĩ giàu kinh nghiệm',
    icon: 'bi bi-stethoscope',
    color: 'blue'
  },
  {
    id: 2,
    name: 'Chăm sóc răng',
    description: 'Dịch vụ nha khoa chuyên nghiệp bao gồm vệ sinh và điều trị',
    icon: 'bi bi-tooth',
    color: 'green'
  },
  {
    id: 3,
    name: 'Chăm sóc cấp cứu',
    description: 'Dịch vụ y tế cấp cứu 24/7 và hỗ trợ chăm sóc khẩn cấp',
    icon: 'bi bi-exclamation-triangle',
    color: 'red'
  }
])

// Booking Form State
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

const bookingErrors = reactive({
  fullName: '',
  dateOfBirth: '',
  gender: '',
  phone: '',
  email: '',
  address: '',
  appointmentDate: '',
  appointmentTime: '',
  reason: ''
} as Record<string, string>)

const bookingLoading = ref(false)
const bookingError = ref('')
const bookingSuccess = ref(false)
const bookingResponse = ref({
  appointmentCode: '',
  status: '',
  appointmentDate: '',
  appointmentTime: ''
})

// Search Form State
const searchForm = reactive({
  appointmentCode: '',
  phone: ''
})

const searchLoading = ref(false)
const searchError = ref('')
const searchResult = ref<any>(null)

// Validation function
const validateBookingForm = (): boolean => {
  bookingErrors.fullName = ''
  bookingErrors.dateOfBirth = ''
  bookingErrors.gender = ''
  bookingErrors.phone = ''
  bookingErrors.email = ''
  bookingErrors.address = ''
  bookingErrors.appointmentDate = ''
  bookingErrors.appointmentTime = ''
  bookingErrors.reason = ''

  let isValid = true

  if (!bookingForm.fullName.trim()) {
    bookingErrors.fullName = 'Họ và tên là bắt buộc'
    isValid = false
  }

  if (!bookingForm.dateOfBirth) {
    bookingErrors.dateOfBirth = 'Ngày sinh là bắt buộc'
    isValid = false
  }

  if (!bookingForm.gender) {
    bookingErrors.gender = 'Giới tính là bắt buộc'
    isValid = false
  }

  if (!bookingForm.phone.trim()) {
    bookingErrors.phone = 'Số điện thoại là bắt buộc'
    isValid = false
  } else if (!/^[0-9]{9,11}$/.test(bookingForm.phone)) {
    bookingErrors.phone = 'Số điện thoại phải có 9-11 chữ số'
    isValid = false
  }

  if (!bookingForm.email.trim()) {
    bookingErrors.email = 'Email là bắt buộc'
    isValid = false
  } else if (!/\S+@\S+\.\S+/.test(bookingForm.email)) {
    bookingErrors.email = 'Định dạng email không hợp lệ'
    isValid = false
  }

  if (!bookingForm.address.trim()) {
    bookingErrors.address = 'Địa chỉ là bắt buộc'
    isValid = false
  }

  if (!bookingForm.appointmentDate) {
    bookingErrors.appointmentDate = 'Ngày khám là bắt buộc'
    isValid = false
  }

  if (!bookingForm.appointmentTime) {
    bookingErrors.appointmentTime = 'Thời gian khám là bắt buộc'
    isValid = false
  }

  if (!bookingForm.reason.trim()) {
    bookingErrors.reason = 'Lý do khám là bắt buộc'
    isValid = false
  }

  return isValid
}

// Submit booking
const submitBooking = async () => {
  if (!validateBookingForm()) {
    return
  }

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
      appointmentTime: bookingForm.appointmentTime + ":00",
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

// Reset booking form
const resetBookingForm = () => {
  bookingForm.fullName = ''
  bookingForm.dateOfBirth = ''
  bookingForm.gender = ''
  bookingForm.phone = ''
  bookingForm.email = ''
  bookingForm.address = ''
  bookingForm.appointmentDate = ''
  bookingForm.appointmentTime = ''
  bookingForm.reason = ''
  bookingSuccess.value = false
  bookingError.value = ''
}

// Submit search
const submitSearch = async () => {
  if (!searchForm.appointmentCode.trim() || !searchForm.phone.trim()) {
    searchError.value = 'Vui lòng nhập mã lịch khám và số điện thoại'
    return
  }

  try {
    searchLoading.value = true
    searchError.value = ''

    const response = await api.post('/appointments/search', {
      appointmentCode: searchForm.appointmentCode,
      phone: searchForm.phone
    })

    searchResult.value = response.data
  } catch (error: any) {
    searchError.value = error.response?.data?.message || 'Lịch khám không tìm thấy. Vui lòng kiểm tra thông tin của bạn.'
    console.error('Search error:', error)
  } finally {
    searchLoading.value = false
  }
}

// Reset search
const resetSearch = () => {
  searchForm.appointmentCode = ''
  searchForm.phone = ''
  searchResult.value = null
  searchError.value = ''
}

// Format date and time
const formatDateTime = (date: string, time: string): string => {
  if (!date || !time) return ''
  const dateObj = new Date(date)
  const dateStr = dateObj.toLocaleDateString('en-US', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
  return `${dateStr} at ${time}`
}

// Get status badge class
const getStatusClass = (status: string): string => {
  const statusLower = status.toLowerCase()
  if (statusLower === 'pending') return 'warning'
  if (statusLower === 'confirmed') return 'success'
  if (statusLower === 'cancelled') return 'danger'
  return 'info'
}

// Scroll to booking section
const scrollToBooking = () => {
  const element = document.getElementById('booking')
  element?.scrollIntoView({ behavior: 'smooth' })
}

const cancelAppointment = async () => {
  if (!searchResult.value) return

  const confirmCancel = confirm("Bạn có chắc chắn muốn hủy lịch khám này?")
  if (!confirmCancel) return

  try {
    cancelLoading.value = true
    cancelError.value = ''

    await api.post('/appointments/cancel', {
      appointmentCode: searchResult.value.appointmentCode,
      fullName: searchResult.value.fullName,
      phone: searchResult.value.phone
    })

    searchResult.value.status = "Cancelled"

    alert("Lịch khám đã bị hủy thành công")

  } catch (error: any) {
    cancelError.value =
      error.response?.data || "Không thể hủy lịch khám"

    console.error(error)
  } finally {
    cancelLoading.value = false
  }
}
</script>
