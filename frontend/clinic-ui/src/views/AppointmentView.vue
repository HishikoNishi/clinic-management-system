<template>
  <div class="appointment-container">
    <h2>Create Appointment</h2>

    <form class="appointment-form" @submit.prevent="submit">
      <label>Full name</label>
      <input v-model="form.fullName" required />

      <label>Date of birth</label>
      <input type="date" v-model="form.dateOfBirth" required />

      <label>Gender</label>
      <select v-model="form.gender" required>
        <option value="">Select gender</option>
        <option value="Male">Male</option>
        <option value="Female">Female</option>
      </select>

      <label>Phone number</label>
      <input type="text" v-model="form.phone" @input="handlePhoneInput" @blur="validatePhone" maxlength="10" required />
      <span v-if="errors.phone" class="error">{{ errors.phone }}</span>

      <label>Email</label>
      <input type="email" v-model="form.email" @input="validateEmail" required />
      <span v-if="errors.email" class="error">{{ errors.email }}</span>

      <label>Address</label>
      <input v-model="form.address" required />

      <label>Appointment date</label>
      <input type="date" v-model="form.date" required />

      <label>Appointment time</label>
      <input type="time" v-model="form.time" required />

      <label>Reason / Note</label>
      <textarea v-model="form.note"></textarea>

      <div class="actions">
        <button type="submit">Create Appointment</button>
        <button type="button" @click="goToMyAppointment">My Appointment</button>
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import { reactive } from "vue"
import { useRouter } from "vue-router"
import axios from "axios"

const router = useRouter()

const form = reactive({
  fullName: "",
  dateOfBirth: "",
  gender: "",
  phone: "",
  email: "",
  address: "",
  date: "",
  time: "",
  note: ""
})

const errors = reactive({ phone: "", email: "" })

const handlePhoneInput = () => {
  form.phone = form.phone.replace(/\D/g, '')
  if (form.phone.length > 10) form.phone = form.phone.slice(0, 10)
  errors.phone = ""
}

const validatePhone = () => {
  errors.phone = form.phone.length !== 10 ? "Please enter right your phone number" : ""
}

const validateEmail = () => {
  const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  errors.email = !regex.test(form.email) ? "Please enter a valid email address" : ""
}

const submit = async () => {
  validatePhone()
  validateEmail()
  if (!errors.phone && !errors.email) {
    const payload = {
      fullName: form.fullName,
      dateOfBirth: form.dateOfBirth,
      gender: form.gender,
      phone: form.phone,
      email: form.email,
      address: form.address,
      appointmentDate: form.date,
      appointmentTime: form.time + ":00",
      reason: form.note
    }

    try {
      const res = await axios.post("https://localhost:7235/api/Appointments", payload)
      const appointmentData = res.data
      localStorage.setItem("appointmentCode", appointmentData.appointmentCode)

      // Hiện thông báo và chuyển trang
      if (window.confirm("Appointment created successfully! Go to My Appointment?")) {
        router.push("/appointmentdetail")
      }
    } catch (err: any) {
      alert("Failed: " + (err.response?.data || err.message))
    }
  }
}


const goToMyAppointment = () => {
  const appointmentCode = localStorage.getItem("appointmentCode")
  if (appointmentCode) router.push("/appointmentdetail")
  else alert("Please create an appointment to see your appointment!")
}
</script>
<style src="@/styles/layouts/appointment.css"></style>
