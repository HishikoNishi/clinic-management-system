<template>
  <div class="staff-container">
    <h2>Quản lý lịch khám</h2>

    <!-- SEARCH BAR -->
    <div class="search-bar">
      <input v-model="searchCode" placeholder="Tìm kiếm theo mã..." />
      <input v-model="searchName" placeholder="Tìm kiếm theo tên bệnh nhân..." />
      <input v-model="searchPhone" placeholder="Tìm kiếm theo số điện thoại..." />
      <input type="date" v-model="searchDate" />

      <select v-model="selectedDepartment" @change="loadDoctorsByDepartment(null)">
        <option value="">Chọn khoa</option>
        <option v-for="dep in departments" :key="dep.id" :value="dep.id.toString()">
          {{ dep.name }}
        </option>
      </select>

      <select v-model="selectedDoctor" :disabled="!selectedDepartment">
        <option value="">Chọn bác sĩ</option>
        <option v-for="d in doctors" :key="d.id" :value="d.id.toString()">
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
          <th>Gán/Đổi bác sĩ</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="a in filteredAppointments" :key="a.id">
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
          <td @click.stop>
  <template v-if="a.statusDetail.value === 'Pending' || a.statusDetail.value === 'Confirmed'">
    <button class="btn btn-sm btn-primary me-2" @click="openAssignModal(a)">
      {{ a.statusDetail.value === 'Pending' ? 'Gán bác sĩ' : 'Đổi bác sĩ' }}
    </button>
  </template>
  <button class="btn btn-sm btn-info" @click="$router.push(`/staff/appointments/${a.id}`)">
    Chi tiết
  </button>
</td>

        </tr>
      </tbody>
    </table>

    <p v-if="appointments.length === 0">Không có lịch khám</p>

    <!-- Modal gán bác sĩ -->
    <div v-if="showAssignModal" class="modal-backdrop">
      <div class="modal-content p-4">
        <h4>Gán/Đổi bác sĩ cho lịch khám</h4>
        <select v-model="assignForm.departmentId" class="form-select mb-2">
          <option value="">Chọn khoa</option>
        <option v-for="dep in departments" :key="dep.id" :value="dep.id.toString()">
  {{ dep.name }}
</option>
        </select>

        <select v-model="assignForm.specialtyId" class="form-select mb-2" :disabled="!assignForm.departmentId">
          <option value="">Chọn chuyên khoa</option>
        <option v-for="s in assignSpecialties" :key="s.id" :value="s.id.toString()">
  {{ s.name }}
</option>
        </select>

        <select v-model="assignForm.doctorId" class="form-select mb-2" :disabled="!assignForm.specialtyId">
          <option value="">Chọn bác sĩ</option>
          <option v-for="d in assignDoctorsList" :key="d.id" :value="d.id.toString()">
  {{ d.fullName }}
</option>
        </select>

        <div class="text-end mt-3">
          <button class="btn btn-secondary me-2" @click="showAssignModal=false">Hủy</button>
          <button class="btn btn-success" @click="confirmAssignDoctor">Xác nhận</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import axios from 'axios'
import { ref, computed, onMounted, watch } from 'vue'
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
    doctorId?: string
    doctorCode?: string
    doctorDepartmentName?: string
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
const departments = ref<any[]>([])

const statuses = ['All', 'Pending', 'Confirmed', 'Completed', 'Cancelled']
const currentStatus = ref('All')

const api = axios.create({
  baseURL: 'https://localhost:7235/api',
  headers: { Authorization: `Bearer ${auth.token}` }
})

const showAssignModal = ref(false)
const assignForm = ref({
  appointmentId: '',
  departmentId: '',
  specialtyId: '',
  doctorId: ''
})
const assignSpecialties = ref<any[]>([])
const assignDoctorsList = ref<any[]>([])
function openAssignModal(appointment: Appointment) {
  assignForm.value = {
    appointmentId: appointment.id,
    departmentId: '',
    specialtyId: '',
    doctorId: ''
  }
  showAssignModal.value = true
  // gọi luôn cả 3 API để có dữ liệu sẵn
  loadDepartments()
  loadAllSpecialties()
  loadAllDoctors()
}

const loadAllSpecialties = async () => {
  const res = await api.get('/Doctor/specialties')
  assignSpecialties.value = res.data
}

const loadAllDoctors = async () => {
  const res = await api.get('/Doctor/all')
  assignDoctorsList.value = res.data
}


watch(() => assignForm.value.departmentId, async (newVal) => {
  if (newVal) {
    const res = await api.get(`/Doctor/departments/${newVal}/specialties`)
    assignSpecialties.value = res.data
  }
})


watch(() => assignForm.value.specialtyId, async (newVal) => {
  if (newVal) {
    const res = await api.get(`/Doctor/by-specialty/${newVal}`)
    assignDoctorsList.value = res.data
  }
})


async function confirmAssignDoctor() {
  if (!assignForm.value.doctorId) {
    alert("Vui lòng chọn bác sĩ")
    return
  }
  await api.post('/staff/StaffAppointments/assign-doctor', {
    appointmentId: assignForm.value.appointmentId,
    doctorId: assignForm.value.doctorId
  })
  alert('Bác sĩ đã được gán/đổi ✅')
  showAssignModal.value = false
  loadAppointments()
}

const filteredAppointments = computed(() => {
  return appointments.value.filter(a => {
    const matchCode = !searchCode.value || a.appointmentCode?.toLowerCase().includes(searchCode.value.toLowerCase())
    const matchName = !searchName.value || a.fullName?.toLowerCase().includes(searchName.value.toLowerCase())
    const matchPhone = !searchPhone.value || a.phone?.includes(searchPhone.value)
    const matchDate = !searchDate.value || a.appointmentDate?.startsWith(searchDate.value)

    let matchDept = true
    if (selectedDepartment.value) {
      matchDept = a.statusDetail.doctorDepartmentName === getDepartmentName(selectedDepartment.value)
    }

    let matchDoctor = true
    if (selectedDoctor.value) {
      matchDoctor = a.statusDetail.doctorId === selectedDoctor.value
    }

    return matchCode && matchName && matchPhone && matchDate && matchDept && matchDoctor
  })
})

const getDepartmentName = (depId: string) => {
  const dep = departments.value.find(d => d.id === depId)
  return dep ? dep.name : ''
}

const loadDepartments = async () => {
  try {
    const res = await api.get('/Doctor/departments')
    departments.value = res.data
  } catch (err) {
    console.error("Không tải được danh sách khoa", err)
  }
}



const loadAppointments = async () => {
  let res
  if (currentStatus.value === 'All') {
    res = await api.get<Appointment[]>(`/staff/StaffAppointments`)
  } else {
    res = await api.get<Appointment[]>(`/staff/StaffAppointments/filter?status=${currentStatus.value}`)
  }
  appointments.value = res.data
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

const formatDateTime = (dateStr: string, timeStr: string) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  const [hours, minutes] = timeStr.split(':')
  return `${date.getDate().toString().padStart(2,'0')}/${(date.getMonth()+1).toString().padStart(2,'0')}/${date.getFullYear()} ${hours}:${minutes}`
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

onMounted(() => {
  loadDepartments()
  loadAppointments()
})
</script>

<style src="@/styles/layouts/staff-appointment.css"></style>