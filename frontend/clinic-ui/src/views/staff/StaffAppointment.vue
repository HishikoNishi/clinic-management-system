<template>
  <div class="staff-container">
    <h2>Appointment Management</h2>

    <!-- SEARCH BAR -->
    <div class="search-bar">
      <input v-model="searchCode" placeholder="Search by code..." />
      <input v-model="searchName" placeholder="Search by patient name..." />
      <input v-model="searchPhone" placeholder="Search by phone..." />
      <input type="date" v-model="searchDate" />

      <!-- Chọn bác sĩ -->
      <select v-model="selectedDoctor" @change="loadAppointments">
        <option value="">All doctors</option>
        <option v-for="d in doctors" :key="d.id" :value="d.id">
   {{ d.name }}

        </option>
      </select>

      <!-- Nút kính lúp -->
      <button class="search-btn" @click="loadAppointments">🔍</button>
      <button class="clear-btn" @click="clearFilters">Clear</button>
    </div>

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
      <th>Code</th>
      <th>Patient</th>
      <th>Phone</th> <!-- thêm cột -->
      <th>Date</th>
      <th>Status</th>
      <th>Doctor</th>
      <th v-if="currentStatus === 'Pending'">Assign</th>
    </tr>
  </thead>
  <tbody>
    <tr
      v-for="a in filteredAppointments"
      :key="a.id"
      @click="$router.push(`/staff/appointments/${a.id}`)"
    >
      <td>{{ a.appointmentCode }}</td>
      <td>{{ a.fullName }}</td>
      <td>{{ a.phone }}</td> <!-- thêm dữ liệu -->
      <td>{{ formatDateTime(a.appointmentDate, a.appointmentTime) }}</td>
      <td>
        <span :class="'status ' + a.statusDetail.value.toLowerCase()">
          {{ a.statusDetail.value }}
        </span>
      </td>
      <td>{{ a.statusDetail.doctorName || 'Not assigned' }}</td>
      <td v-if="a.statusDetail.value === 'Pending'">
        <select @change="assignDoctor(a.id, $event)" @click.stop>
          <option value="">Select doctor</option>
          <option v-for="d in doctors" :key="d.id" :value="d.id">
            {{ d.username }}
          </option>
        </select>
      </td>
    </tr>
  </tbody>
</table>


    <p v-if="appointments.length === 0">No appointments</p>
  </div>
</template>

<script setup lang="ts">
import axios from 'axios'
import { ref, computed, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()

interface Appointment {
  id: string
  appointmentCode: string
  fullName: string
  phone: string
  appointmentDate: string
  appointmentTime: string
  statusDetail: {
    value: string
    doctorName: string
  }
}

const searchCode = ref('')
const searchName = ref('')
const searchPhone = ref('')
const searchDate = ref('')
const selectedDoctor = ref('')

const appointments = ref<Appointment[]>([])
const doctors = ref<any[]>([])

const statuses = ['All', 'Pending', 'Confirmed', 'Completed', 'Cancelled']
const currentStatus = ref('All')

const filteredAppointments = computed(() => {
  return appointments.value.filter(a => {
    const matchCode = !searchCode.value || a.appointmentCode?.toLowerCase().includes(searchCode.value.toLowerCase())
    const matchName = !searchName.value || a.fullName?.toLowerCase().includes(searchName.value.toLowerCase())
    const matchPhone = !searchPhone.value || a.phone?.includes(searchPhone.value)
    const matchDate = !searchDate.value || a.appointmentDate?.startsWith(searchDate.value)
    return matchCode && matchName && matchPhone && matchDate
  })
})

const api = axios.create({
  baseURL: 'https://localhost:7235/api',
  headers: { Authorization: `Bearer ${auth.token}` }
})

const loadAppointments = async () => {
  let res

  if (selectedDoctor.value) {
    res = await api.get<Appointment[]>(
      `/staff/StaffAppointments/by-doctor?doctorId=${selectedDoctor.value}`
    )

    let data: Appointment[] = res.data

    if (currentStatus.value !== 'All') {
      data = data.filter(a => a.statusDetail.value === currentStatus.value)
    }

    appointments.value = data
  } else {
    if (currentStatus.value === 'All') {
      res = await api.get<Appointment[]>(`/staff/StaffAppointments`)
    } else {
      res = await api.get<Appointment[]>(
        `/staff/StaffAppointments/filter?status=${currentStatus.value}`
      )
    }

    appointments.value = res.data
  }
}

const loadDoctors = async () => {
  const res = await api.get('/Doctor')
  doctors.value = res.data
}

const changeStatus = (s: string) => {
  currentStatus.value = s
  loadAppointments()
}

const assignDoctor = async (appointmentId: string, e: Event) => {
  const doctorId = (e.target as HTMLSelectElement).value
  if (!doctorId) return
  await api.post('/staff/StaffAppointments/assign-doctor', { appointmentId, doctorId })
  alert('Doctor assigned ✅')
  loadAppointments()
}

const clearFilters = () => {
  searchCode.value = ''
  searchName.value = ''
  searchPhone.value = ''
  searchDate.value = ''
  selectedDoctor.value = ''
  loadAppointments()
}

const formatDateTime = (dateStr: string, timeStr: string) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  const [hours, minutes] = timeStr.split(':')
  return `${date.getDate().toString().padStart(2,'0')}/${(date.getMonth()+1).toString().padStart(2,'0')}/${date.getFullYear()} ${hours}:${minutes}`
}

onMounted(() => {
  loadAppointments()
  loadDoctors()
})
</script>

<style src="@/styles/layouts/staff-appointment.css"></style>
