<template>
  <div class="doctor-container">
    <h2>My Appointments</h2>

    <table>
      <thead>
        <tr>
          <th>Code</th>
          <th>Patient</th>
          <th>Date</th>
          <th>Status</th>
          <th>Action</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="a in appointments" :key="a.id" @click="$router.push(`/doctor/appointments/${a.id}`)">
  <td>{{ a.appointmentCode }}</td>
  <td>{{ a.fullName }}</td>
  <td>{{ formatDateTime(a.appointmentDate, a.appointmentTime) }}</td>
  <td>{{ a.status }}</td>
  <td>
    <button v-if="a.status === 'Confirmed'" @click.stop="completeAppointment(a.id)">
      Completed
    </button>
  </td>
</tr>

      </tbody>
    </table>

    <p v-if="appointments.length === 0">No appointments</p>
  </div>
</template>

<script setup lang="ts">
import axios from 'axios'
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()

const api = axios.create({
  baseURL: 'https://localhost:7235/api',
  headers: {
    Authorization: `Bearer ${auth.token}`
  }
})

const appointments = ref<any[]>([])

const loadAppointments = async () => {
  const res = await api.get('/doctor/DoctorAppointments')
  appointments.value = res.data
}

const completeAppointment = async (appointmentId: string) => {
  const ok = confirm("Are you sure you want to mark this appointment as completed?")
  if (!ok) return

  await api.patch(`/doctor/DoctorAppointments/${appointmentId}/complete`)
  alert('Status updated ✅')
  loadAppointments()
}

const formatDateTime = (dateStr: string, timeStr: string) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  const [hours, minutes] = timeStr.split(':')
  const day = String(date.getDate()).padStart(2, '0')
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const year = date.getFullYear()
  return `${day}/${month}/${year} ${hours}:${minutes}`
}

onMounted(() => {
  loadAppointments()
})
</script>

<style src="@/styles/layouts/doctor-appointment.css"></style>
