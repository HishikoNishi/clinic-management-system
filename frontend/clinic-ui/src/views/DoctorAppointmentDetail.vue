<template>
  <div class="appointment-detail">
    <h2>Appointment Detail</h2>
    <div v-if="appointment" class="detail-card">
      <div class="detail-row"><span class="label">Code:</span> {{ appointment.appointmentCode }}</div>
      <div class="detail-row"><span class="label">Patient:</span> {{ appointment.fullName }}</div>
      <div class="detail-row"><span class="label">Phone:</span> {{ appointment.phone }}</div>
      <div class="detail-row"><span class="label">Email:</span> {{ appointment.email }}</div>
      <div class="detail-row"><span class="label">Date of Birth:</span> {{ formatDate(appointment.dateOfBirth) }}</div>
      <div class="detail-row"><span class="label">Gender:</span> {{ appointment.gender }}</div>
      <div class="detail-row"><span class="label">Address:</span> {{ appointment.address }}</div>
      <div class="detail-row"><span class="label">Reason:</span> {{ appointment.reason }}</div>
      <div class="detail-row">
        <span class="label">Status:</span>
        <span :class="'status ' + appointment.status.toLowerCase()">{{ appointment.status }}</span>
      </div>
      <div v-if="appointment.statusDetail.doctorName" class="detail-row">
        <span class="label">Doctor:</span> {{ appointment.statusDetail.doctorName }} ({{ appointment.statusDetail.doctorCode }})
      </div>
    </div>
    <p v-else-if="error" class="error">{{ error }}</p>
    <button class="back-btn" @click="$router.push('/doctorappointment')">← Back to list</button>
  </div>
</template>

<script setup lang="ts">
import axios from 'axios'
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const route = useRoute()
const auth = useAuthStore()

const api = axios.create({
  baseURL: 'https://localhost:7235/api',
  headers: { Authorization: `Bearer ${auth.token}` }
})

const appointment = ref<any>(null)
const error = ref<string | null>(null)

const loadDetail = async () => {
  try {
    const res = await api.get(`/doctor/DoctorAppointments/${route.params.id}`)
    appointment.value = res.data
  } catch (err) {
    error.value = "You are not authorized to view this appointment."
  }
}

const formatDate = (dateStr: string) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  return date.toLocaleDateString()
}

onMounted(() => {
  loadDetail()
})
</script>

<style src="@/styles/layouts/appointment-detail.css"></style>
