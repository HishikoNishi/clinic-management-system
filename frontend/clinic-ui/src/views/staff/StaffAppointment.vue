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
        <tr v-for="a in appointments" :key="a.id">
          <td>{{ a.code }}</td>
          <td>{{ a.patientName }}</td>
          <td>{{ formatDate(a.appointmentDate) }}</td>
          <td>
            <span :class="'status ' + a.status.toLowerCase()">
              {{ a.status }}
            </span>
          </td>
          <td>{{ a.doctorName || 'Chưa có' }}</td>

          <!-- ASSIGN -->
          <td v-if="a.status === 'Pending'">
            <select @change="assignDoctor(a.id, $event)">
              <option value="">Chọn bác sĩ</option>
              <option
                v-for="d in doctors"
                :key="d.id"
                :value="d.id"
              >
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

const statuses = ['Pending', 'Confirmed', 'Completed']
const currentStatus = ref('Pending')

/* ================= LOAD ================= */

const loadAppointments = async () => {
  const res = await api.post('/Appointments/search', {
    status: currentStatus.value
  })
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

  await api.post('/Appointments/assign-doctor', {
    appointmentId,
    doctorId
  })

  alert('Đã phân bác sĩ ✅')
  loadAppointments()
}

/* ================= UTIL ================= */

const formatDate = (d: string) =>
  new Date(d).toLocaleString()

onMounted(() => {
  loadAppointments()
  loadDoctors()
})
</script>
<style src="@/styles/layouts/staff-appointment.css"></style>