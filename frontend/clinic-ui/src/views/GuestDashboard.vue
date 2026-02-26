<template>
  <div class="guest-dashboard">
    <!-- Navbar -->
    <nav class="navbar">
      <div class="navbar-container">
        <div class="navbar-brand">
          <i class="bi bi-hospital"></i>
          <span>Clinic Management</span>
        </div>
        <div class="navbar-links">
          <a href="#services" class="nav-link">Services</a>
          <a href="#booking" class="nav-link">Book Appointment</a>
          <a href="#search" class="nav-link">Search</a>
        </div>
      </div>
    </nav>

    <!-- Hero Section -->
    <section class="hero">
      <div class="hero-background">
        <div class="shape shape-1"></div>
        <div class="shape shape-2"></div>
        <div class="shape shape-3"></div>
      </div>
      <div class="hero-content">
        <h1 class="hero-title">Welcome to Our Clinic</h1>
        <p class="hero-subtitle">
          Quality healthcare services available for you and your family
        </p>
        <button @click="scrollToBooking" class="btn btn-primary">
          <i class="bi bi-calendar-check me-2"></i>Book an Appointment
        </button>
      </div>
    </section>

    <!-- Services Section -->
    <section id="services" class="services-section">
      <div class="container">
        <h2 class="section-title">Our Services</h2>
        <p class="section-subtitle">
          We provide comprehensive healthcare services
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
        <h2 class="section-title">Book Your Appointment</h2>
        <p class="section-subtitle">
          Fill in the form below to schedule your appointment
        </p>

        <!-- Success Alert -->
        <div v-if="bookingSuccess" class="alert alert-success">
          <i class="bi bi-check-circle me-2"></i>
          <div>
            <strong>Appointment Booked Successfully!</strong>
            <p class="mt-2">
              <strong>Appointment Code:</strong>
              <span class="appointment-code">{{ bookingResponse.appointmentCode }}</span>
            </p>
            <p>
              <strong>Status:</strong>
              <span class="badge badge-info">{{ bookingResponse.status }}</span>
            </p>
            <p>
              <strong>Date & Time:</strong>
              {{ formatDateTime(bookingResponse.appointmentDate, bookingResponse.appointmentTime) }}
            </p>
          </div>
        </div>

        <!-- Error Alert -->
        <div v-if="bookingError" class="alert alert-danger">
          <i class="bi bi-exclamation-circle me-2"></i>
          <div>
            <strong>Error</strong>
            <p class="mt-2">{{ bookingError }}</p>
          </div>
        </div>

        <!-- Booking Form -->
        <form @submit.prevent="submitBooking" class="booking-form" v-if="!bookingSuccess">
          <div class="form-row">
            <!-- Full Name -->
            <div class="form-group">
              <label class="form-label">Full Name *</label>
              <input
                v-model="bookingForm.fullName"
                type="text"
                class="form-input"
                required
                placeholder="Enter your full name"
              />
              <span v-if="bookingErrors.fullName" class="form-error">
                {{ bookingErrors.fullName }}
              </span>
            </div>

            <!-- Date of Birth -->
            <div class="form-group">
              <label class="form-label">Date of Birth *</label>
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
              <label class="form-label">Gender *</label>
              <select v-model="bookingForm.gender" class="form-select" required>
                <option value="">Select Gender</option>
                <option value="1">Male</option>
                <option value="2">Female</option>
              </select>
              <span v-if="bookingErrors.gender" class="form-error">
                {{ bookingErrors.gender }}
              </span>
            </div>

            <!-- Phone -->
            <div class="form-group">
              <label class="form-label">Phone *</label>
              <input
                v-model="bookingForm.phone"
                type="tel"
                class="form-input"
                required
                placeholder="Enter your phone number"
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
                placeholder="Enter your email"
              />
              <span v-if="bookingErrors.email" class="form-error">
                {{ bookingErrors.email }}
              </span>
            </div>

            <!-- Address -->
            <div class="form-group">
              <label class="form-label">Address *</label>
              <input
                v-model="bookingForm.address"
                type="text"
                class="form-input"
                required
                placeholder="Enter your address"
              />
              <span v-if="bookingErrors.address" class="form-error">
                {{ bookingErrors.address }}
              </span>
            </div>
          </div>

          <div class="form-row">
            <!-- Appointment Date -->
            <div class="form-group">
              <label class="form-label">Appointment Date *</label>
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
              <label class="form-label">Appointment Time *</label>
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
            <label class="form-label">Reason for Visit *</label>
            <textarea
              v-model="bookingForm.reason"
              class="form-textarea"
              required
              placeholder="Please describe your reason for the appointment"
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
            {{ bookingLoading ? "Booking..." : "Book Appointment" }}
          </button>
        </form>

        <!-- Reset Button (shown after success) -->
        <button
          v-if="bookingSuccess"
          @click="resetBookingForm"
          class="btn btn-secondary w-100"
        >
          <i class="bi bi-arrow-counterclockwise me-2"></i>
          Book Another Appointment
        </button>
      </div>
    </section>

    <!-- Search Section -->
    <section id="search" class="search-section">
      <div class="container">
        <h2 class="section-title">Search Your Appointment</h2>
        <p class="section-subtitle">
          Enter your appointment code and phone number to check status
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
              <h3>Appointment Details</h3>
              <span class="badge" :class="`badge-${getStatusClass(searchResult.status)}`">
                {{ searchResult.status }}
              </span>
            </div>

            <div class="result-grid">
              <div class="result-item">
                <label>Appointment Code</label>
                <p class="result-value">{{ searchResult.appointmentCode }}</p>
              </div>

              <div class="result-item">
                <label>Patient Name</label>
                <p class="result-value">{{ searchResult.fullName }}</p>
              </div>

              <div class="result-item">
                <label>Date & Time</label>
                <p class="result-value">
                  {{ formatDateTime(searchResult.appointmentDate, searchResult.appointmentTime) }}
                </p>
              </div>

              <div class="result-item">
                <label>Phone</label>
                <p class="result-value">{{ searchResult.phone }}</p>
              </div>

              <div class="result-item">
                <label>Email</label>
                <p class="result-value">{{ searchResult.email }}</p>
              </div>

              <div class="result-item">
                <label>Reason</label>
                <p class="result-value">{{ searchResult.reason }}</p>
              </div>
            </div>

            <button @click="resetSearch" class="btn btn-secondary">
              <i class="bi bi-search me-2"></i>
              Search Another
            </button>
          </div>
        </div>

        <!-- Search Form -->
        <form @submit.prevent="submitSearch" class="search-form" v-else>
          <div class="form-row">
            <!-- Appointment Code -->
            <div class="form-group">
              <label class="form-label">Appointment Code *</label>
              <input
                v-model="searchForm.appointmentCode"
                type="text"
                class="form-input"
                required
                placeholder="Enter your appointment code"
              />
            </div>

            <!-- Phone -->
            <div class="form-group">
              <label class="form-label">Phone Number *</label>
              <input
                v-model="searchForm.phone"
                type="tel"
                class="form-input"
                required
                placeholder="Enter your phone number"
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
            {{ searchLoading ? "Searching..." : "Search Appointment" }}
          </button>
        </form>
      </div>
    </section>

    <!-- Footer -->
    <footer class="footer">
      <div class="container">
        <div class="footer-content">
          <div class="footer-section">
            <h4>Clinic Management</h4>
            <p>Your trusted healthcare provider</p>
          </div>

          <div class="footer-section">
            <h4>Contact</h4>
            <p>
              <i class="bi bi-telephone me-2"></i>
              +84 (0) 123 456 789
            </p>
            <p>
              <i class="bi bi-envelope me-2"></i>
              info@clinic.com
            </p>
          </div>

          <div class="footer-section">
            <h4>Hours</h4>
            <p>Monday - Friday: 8:00 AM - 6:00 PM</p>
            <p>Saturday: 9:00 AM - 4:00 PM</p>
            <p>Sunday: Closed</p>
          </div>

          <div class="footer-section">
            <h4>Follow Us</h4>
            <div class="social-links">
              <a href="#" class="social-link">
                <i class="bi bi-facebook"></i>
              </a>
              <a href="#" class="social-link">
                <i class="bi bi-twitter"></i>
              </a>
              <a href="#" class="social-link">
                <i class="bi bi-instagram"></i>
              </a>
            </div>
          </div>
        </div>

        <div class="footer-bottom">
          <p>&copy; 2026 Clinic Management. All rights reserved.</p>
        </div>
      </div>
    </footer>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import api from '@/services/api'
import '@/styles/layouts/guest-dashboard.css'

// Services data
const services = reactive([
  {
    id: 1,
    name: 'General Consultation',
    description: 'Comprehensive medical checkups and consultations with experienced doctors',
    icon: 'bi bi-stethoscope',
    color: 'blue'
  },
  {
    id: 2,
    name: 'Dental Care',
    description: 'Professional dental services including cleanings and treatments',
    icon: 'bi bi-tooth',
    color: 'green'
  },
  {
    id: 3,
    name: 'Emergency Care',
    description: '24/7 emergency medical services and urgent care support',
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
    bookingErrors.fullName = 'Full name is required'
    isValid = false
  }

  if (!bookingForm.dateOfBirth) {
    bookingErrors.dateOfBirth = 'Date of birth is required'
    isValid = false
  }

  if (!bookingForm.gender) {
    bookingErrors.gender = 'Gender is required'
    isValid = false
  }

  if (!bookingForm.phone.trim()) {
    bookingErrors.phone = 'Phone number is required'
    isValid = false
  } else if (!/^[0-9]{9,11}$/.test(bookingForm.phone)) {
    bookingErrors.phone = 'Phone must be 9-11 digits'
    isValid = false
  }

  if (!bookingForm.email.trim()) {
    bookingErrors.email = 'Email is required'
    isValid = false
  } else if (!/\S+@\S+\.\S+/.test(bookingForm.email)) {
    bookingErrors.email = 'Invalid email format'
    isValid = false
  }

  if (!bookingForm.address.trim()) {
    bookingErrors.address = 'Address is required'
    isValid = false
  }

  if (!bookingForm.appointmentDate) {
    bookingErrors.appointmentDate = 'Appointment date is required'
    isValid = false
  }

  if (!bookingForm.appointmentTime) {
    bookingErrors.appointmentTime = 'Appointment time is required'
    isValid = false
  }

  if (!bookingForm.reason.trim()) {
    bookingErrors.reason = 'Reason for visit is required'
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
    bookingError.value = error.response?.data?.message || 'Failed to book appointment. Please try again.'
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
    searchError.value = 'Please enter appointment code and phone number'
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
    searchError.value = error.response?.data?.message || 'Appointment not found. Please check your details.'
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
</script>
