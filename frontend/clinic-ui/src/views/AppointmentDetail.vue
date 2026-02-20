<template>
  <div class="appointment-container">
    <h2>My Appointment</h2>

    <div v-if="appointment">
      <p><strong>Code:</strong> {{ appointment.appointmentCode }}</p>
      <p><strong>Name:</strong> {{ appointment.fullName }}</p>
      <p><strong>Phone:</strong> {{ appointment.phone }}</p>
      <p><strong>Address:</strong> {{ appointment.address }}</p>
      <p><strong>Status:</strong> {{ appointment.status }}</p>
      <p><strong>Date:</strong> {{ new Date(appointment.appointmentDate).toLocaleDateString('vi-VN') }}</p>
      <p><strong>Time:</strong> {{ appointment.appointmentTime.substring(0,5) }}</p>
      <p><strong>Reason:</strong> {{ appointment.reason }}</p>

      <button @click="goBack">Back</button>
      <button class="cancel" @click="showCancelForm = true">Cancel</button>
    </div>

    <div v-else>
      <p>Please create an appointment to see your appointment!</p>
    </div>

    <div v-if="showCancelForm" class="cancel-form">
      <h3>Cancel Appointment</h3>
      <p>Enter your appointment code to confirm cancellation:</p>
      <input v-model="enteredCode" placeholder="Appointment Code" />
      <button @click="confirmCancel">Confirm Cancel</button>
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
        console.error(err.response?.data || err.message)
        cancelError.value = "Failed to cancel appointment. Please try again."
      }
    }
  } else {
    cancelError.value = "Incorrect code. Please try again."
  }
}


</script>
<style src="@/styles/layouts/appointment.css"></style>
