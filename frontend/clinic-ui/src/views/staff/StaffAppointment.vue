<template>
  <div class="staff-container">
    <h2>Quản lý lịch khám</h2>

    <!-- SEARCH BAR -->
    <div class="search-bar">
      <input v-model="searchCode" placeholder="Tìm kiếm theo mã..." />
      <input v-model="searchName" placeholder="Tìm kiếm theo tên bệnh nhân..." />
      <input v-model="searchPhone" placeholder="Tìm kiếm theo số điện thoại..." />
      <input type="date" v-model="searchDate" />

      <!-- Chọn khoa để lọc -->
      <select v-model="selectedDepartment" @change="loadDoctorsByDepartment(null)">
        <option value="">Chọn khoa</option>
        <option v-for="dep in departments" :key="dep.id" :value="dep.id">
          {{ dep.name }}
        </option>
      </select>

      <!-- Chọn bác sĩ để lọc -->
      <select v-model="selectedDoctor" @change="loadAppointments" :disabled="!selectedDepartment">
        <option value="">Chọn bác sĩ</option>
        <option v-for="d in doctors" :key="d.id" :value="d.id">
          {{ d.fullName }}
        </option>
      </select>
    </div>

    <!-- FILTER STATUS -->
    <div class="filter">
      <button
        v-for="s in statuses"
        :key="s"
        :class="{ active: currentStatus === s }"
        @click="changeStatus(s)"
      >
        {{ statusLabel(s) }}
      </button>
    </div>

    <!-- TABLE -->
    <table>
      <thead>
        <tr>
          <th>Mã</th>
          <th>Bệnh nhân</th>
          <th>Điện thoại</th>
          <th>Ngày sinh</th>
          <th>Ngày</th>
          <th>Trạng thái</th>
          <th>Triệu chứng</th>
          <th>Bác sĩ</th>
          <th>Gán bác sĩ</th>
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
          <td>{{ a.phone }}</td>
          <td>{{ formatDate(a.dateOfBirth) }}</td>
          <td>{{ formatDateTime(a.appointmentDate, a.appointmentTime) }}</td>
          <td>
            <span :class="'status ' + a.statusDetail.value.toLowerCase()">
              {{ statusLabel(a.statusDetail.value) }}
            </span>
          </td>
          <td>{{ a.reason }}</td>
          <td>{{ a.statusDetail.doctorName || 'Chưa gán' }}</td>
          <td @click.stop class="assign-cell">
            <template v-if="a.statusDetail.value === 'Pending'">
              <!-- Chọn khoa riêng cho từng appointment -->
             
      <select v-model="assignDepartments[a.id]" @change="loadDoctorsByDepartment(a.id)">
  <option value="">Chọn khoa</option>
  <option v-for="dep in departments" :key="dep.id" :value="dep.id">
    {{ dep.name }}
  </option>
</select>


              <!-- Chọn bác sĩ riêng cho từng appointment -->
              <select @change="assignDoctor(a.id, $event)" :disabled="!assignDepartments[a.id]">
                <option value="">Chọn bác sĩ</option>
                <option v-for="d in assignDoctors[a.id] || []" :key="d.id" :value="d.id">
                  {{ d.fullName }}
                </option>
              </select>
         
            </template>
            <template v-else>
              —
            </template>
          </td>
        </tr>
      </tbody>
    </table>

    <p v-if="appointments.length === 0">Không có lịch khám</p>
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
  dateOfBirth: string
  appointmentDate: string
  appointmentTime: string
  reason: string
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
const selectedDepartment = ref('')

const appointments = ref<Appointment[]>([])
const doctors = ref<any[]>([])
const departments = ref<any[]>([]) // load từ API

// riêng cho từng appointment
const assignDepartments = ref<{ [key: string]: string }>({})
const assignDoctors = ref<{ [key: string]: any[] }>({})

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

const loadDepartments = async () => {
  const res = await api.get('/departments')
  departments.value = res.data
}

const loadAppointments = async () => {
  let res
  if (selectedDoctor.value) {
    res = await api.get<Appointment[]>(`/staff/StaffAppointments/by-doctor?doctorId=${selectedDoctor.value}`)
  } else if (selectedDepartment.value) {
    res = await api.get<Appointment[]>(`/staff/StaffAppointments/by-department?departmentId=${selectedDepartment.value}`)
  } else {
    if (currentStatus.value === 'All') {
      res = await api.get<Appointment[]>(`/staff/StaffAppointments`)
    } else {
      res = await api.get<Appointment[]>(`/staff/StaffAppointments/filter?status=${currentStatus.value}`)
    }
  }

  appointments.value = res.data

  // reset dropdown cho từng appointment
  appointments.value.forEach(a => {
    assignDepartments.value[a.id] = ''   // luôn rỗng khi load lại
    assignDoctors.value[a.id] = []       // danh sách bác sĩ trống
  })

  if (currentStatus.value !== 'All') {
    appointments.value = appointments.value.filter(a => a.statusDetail.value === currentStatus.value)
  }
}


const loadDoctorsByDepartment = async (appointmentId: string | null) => {
  const depId = appointmentId ? assignDepartments.value[appointmentId] : selectedDepartment.value
  if (!depId) {
    if (appointmentId) assignDoctors.value[appointmentId] = []
    else doctors.value = []
    return
  }
  const res = await api.get(`/Doctor/by-department/${depId}`)
  if (appointmentId) assignDoctors.value[appointmentId] = res.data
  else doctors.value = res.data
}

const changeStatus = (s: string) => {
  currentStatus.value = s
  loadAppointments()
}

const formatDate = (dateStr: string) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  return `${date.getDate().toString().padStart(2,'0')}/${(date.getMonth()+1).toString().padStart(2,'0')}/${date.getFullYear()}`
}

const assignDoctor = async (appointmentId: string, e: Event) => {
  const doctorId = (e.target as HTMLSelectElement).value
  if (!doctorId) return
  const appointment = appointments.value.find(a => a.id === appointmentId)
  const doctor = (assignDoctors.value[appointmentId] || []).find(d => d.id === doctorId)
const message = `Bạn có chắc chắn muốn gán bác sĩ ${doctor?.fullName} khoa ${doctor?.departmentName} cho bệnh nhân ${appointment?.fullName} với triệu chứng: ${appointment?.reason}?`
  if (!confirm(message)) {
    (e.target as HTMLSelectElement).value = ''
    return
  }
  await api.post('/staff/StaffAppointments/assign-doctor', { appointmentId, doctorId })
  alert('Bác sĩ đã được gán ✅')
  loadAppointments()
}

const statusLabel = (status: string) => {
  const labels: { [key: string]: string } = {
    'All': 'Tất cả',
    'Pending': 'Chờ xử lý',
    'Confirmed': 'Đã xác nhận',
    'Completed': 'Hoàn thành',
    'Cancelled': 'Đã hủy'
  }
  return labels[status] || status
}

const formatDateTime = (dateStr: string, timeStr: string) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  const [hours, minutes] = timeStr.split(':')
  return `${date.getDate().toString().padStart(2,'0')}/${(date.getMonth()+1).toString().padStart(2,'0')}/${date.getFullYear()} ${hours}:${minutes}`
}

onMounted(() => {
  loadDepartments()
  loadAppointments()
})
</script>

<style src="@/styles/layouts/staff-appointment.css"></style>
