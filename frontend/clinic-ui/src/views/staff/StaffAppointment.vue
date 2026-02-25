<template>
  <div class="staff-container">
    <h2>Quản lý lịch hẹn</h2>

    <!-- FILTER STATUS -->
    <div class="filter">
      <button
        v-for="s in statuses"
        :key="s"
        :class="{ active: currentStatus === s }"
        @click="changeStatus(s)"
      >
        {{ s }}
      </button>
    </div>

    <!-- TABLE -->
    <table>
      <thead>
        <tr>
          <th>Mã</th>
          <th>Bệnh nhân</th>
          <th>Ngày khám</th>
          <th>Trạng thái</th>
          <th>Bác sĩ</th>
          <th v-if="currentStatus === 'Pending'">Assign</th>
        </tr>
      </thead>

      <tbody>
        <tr v-for="a in appointments" :key="a.appointmentCode">
          <td>{{ a.appointmentCode }}</td>
          <td>{{ a.fullName }}</td>
          <td>{{ formatDateTime(a.appointmentDate, a.appointmentTime) }}</td>
          <td>
            <span :class="'status ' + a.statusDetail.value.toLowerCase()">
              {{ a.statusDetail.value }}
            </span>
          </td>
          <td>{{ a.statusDetail.doctorName || 'Chưa có' }}</td>

          <!-- ASSIGN -->
          <td v-if="a.statusDetail.value === 'Pending'">
        <select @change="assignDoctor(a.id, $event)">
  <option value="">Chọn bác sĩ</option>
  <option v-for="d in doctors" :key="d.id" :value="d.id">
    {{ d.name }}
  </option>
</select>

          </td>
        </tr>
      </tbody>
    </table>

    <p v-if="appointments.length === 0">
      Không có lịch hẹn
    </p>
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
const doctors = ref<any[]>([])

const statuses = ['Pending', 'Confirmed', 'Completed', 'Cancelled']
const currentStatus = ref('Pending')

/* ================= LOAD ================= */

const loadAppointments = async () => {
  const res = await api.get(`/staff/StaffAppointments/filter?status=${currentStatus.value}`)
  appointments.value = res.data
}

const loadDoctors = async () => {
  const res = await api.get('/Doctors')
  doctors.value = res.data
}

/* ================= ACTION ================= */

const changeStatus = (s: string) => {
  currentStatus.value = s
  loadAppointments()
}

const assignDoctor = async (appointmentId: string, e: Event) => {
  const doctorId = (e.target as HTMLSelectElement).value
  if (!doctorId) return

  await api.post('/staff/StaffAppointments/assign-doctor', {
    appointmentId,
    doctorId
  })

  alert('Đã phân bác sĩ ✅')
  loadAppointments()
}

/* ================= UTIL ================= */

const formatDate = (d: string) => {
  const date = new Date(d)
  const day = String(date.getDate()).padStart(2, '0')
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const year = date.getFullYear()
  const hours = String(date.getHours()).padStart(2, '0')
  const minutes = String(date.getMinutes()).padStart(2, '0')
  return `${day}/${month}/${year} ${hours}:${minutes}`
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
  loadDoctors()
})
</script>

<style src="@/styles/layouts/staff-appointment.css"></style>
