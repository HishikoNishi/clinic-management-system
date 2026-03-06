<template>
  <div class="appointment-container">
    <h2>My Appointment</h2>

    <!-- Link để mở form tìm kiếm -->
    <p class="find-link" @click="showSearchForm = true">
      🔎 Find Your Appointment
    </p>

   <div v-if="appointment && !showSearchForm" class="appointment-detail">
  <p><strong>Code:</strong> {{ appointment.appointmentCode }}</p>
  <p><strong>Name:</strong> {{ appointment.fullName }}</p>
  <p><strong>Phone:</strong> {{ appointment.phone }}</p>
  <p><strong>Email:</strong> {{ appointment.email }}</p>
  <p><strong>Date of Birth:</strong> {{ new Date(appointment.dateOfBirth).toLocaleDateString('vi-VN') }}</p>
  <p><strong>Gender:</strong> {{ appointment.gender }}</p>
  <p><strong>Address:</strong> {{ appointment.address }}</p>
  <p><strong>Status:</strong> {{ appointment.status }}</p>
  <p><strong>Appointment Date:</strong> {{ new Date(appointment.appointmentDate).toLocaleDateString('vi-VN') }}</p>
  <p><strong>Appointment Time:</strong> {{ appointment.appointmentTime.substring(0,5) }}</p>
  <p><strong>Reason:</strong> {{ appointment.reason }}</p>
  <p><strong>Created At:</strong> {{ new Date(appointment.createdAt).toLocaleString('vi-VN') }}</p>

  <div class="actions">
    <button @click="goBack">Back</button>
    <button class="cancel" @click="showCancelForm = true">Cancel</button>
  </div>
</div>


    <!-- Nếu chưa có appointment -->
    <div v-else-if="!appointment && !showSearchForm">
      <p>
        Please create an appointment to see your appointment or if you already have appointment, lets find your appointment!
     
      </p>
    </div>

    <!-- Form tìm kiếm -->
    <div v-if="showSearchForm" class="search-form">
      <h3>Find Your Appointment</h3>
      <label>Appointment Code</label>
      <input v-model="searchCode" placeholder="Enter code" />

      <label>Phone number</label>
      <input v-model="searchPhone" placeholder="Enter phone" />

      <div class="actions">
        <button @click="searchAppointment">Search</button>
        <button type="button" @click="closeSearch">Close</button>
      </div>
      <span v-if="searchError" class="error">{{ searchError }}</span>
    </div>

    <!-- Form cancel -->
    <div v-if="showCancelForm" class="cancel-form">
      <h3>Cancel Appointment</h3>
      <p>Enter your appointment code to confirm cancellation:</p>
      <input v-model="enteredCode" placeholder="Appointment Code" />
      <div class="actions">
        <button @click="confirmCancel">Confirm Cancel</button>
        <button type="button" @click="showCancelForm = false">Close</button>
      </div>
      <span v-if="cancelError" class="error">{{ cancelError }}</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue"
import { useRouter } from "vue-router"
import axios from "axios"

const router = useRouter()
const appointment = ref<any>(null)
const showCancelForm = ref(false)
const enteredCode = ref("")
const cancelError = ref("")

const appointmentCode = localStorage.getItem("appointmentCode")

onMounted(async () => {
  if (appointmentCode) {
    const res = await axios.get(`https://localhost:7235/api/Appointments/${appointmentCode}`)
    appointment.value = res.data
  }
})

const goBack = () => router.push("/appointment")

// Search form
const showSearchForm = ref(false)
const searchCode = ref("")
const searchPhone = ref("")
const searchError = ref("")

const searchAppointment = async () => {
  try {
    const res = await axios.get(`https://localhost:7235/api/Appointments/${searchCode.value}`)
    if (res.data.phone === searchPhone.value) {
      appointment.value = res.data
      showSearchForm.value = false
      searchError.value = ""
    } else {
      searchError.value = "Phone number does not match."
    }
  } catch (err: any) {
    searchError.value = "Appointment not found."
  }
}

const closeSearch = () => {
  showSearchForm.value = false
  searchCode.value = ""
  searchPhone.value = ""
  searchError.value = ""
}

// Cancel form
const confirmCancel = async () => {
  if (enteredCode.value === appointment.value.appointmentCode) {
    if (window.confirm("Are you sure to cancel your appointment?")) {
      try {
        const payload = {
          appointmentCode: appointment.value.appointmentCode,
          fullName: appointment.value.fullName,
          phone: appointment.value.phone
        }
        await axios.post("https://localhost:7235/api/Appointments/cancel", payload)
        appointment.value.status = "Cancelled"
        showCancelForm.value = false
        cancelError.value = ""
        alert("Your appointment has been cancelled.")
      } catch (err: any) {
        cancelError.value = "Failed to cancel appointment. Please try again."
      }
    }
  } else {
    cancelError.value = "Incorrect code. Please try again."
  }
}
</script>
