<template>
  <div class="staff-container">
    <h2>Quáº£n lÃ½ lá»‹ch khÃ¡m</h2>

    <div class="search-bar">
      <input v-model="searchCode" placeholder="TÃ¬m kiáº¿m theo mÃ£..." />
      <input v-model="searchName" placeholder="TÃ¬m kiáº¿m theo tÃªn bá»‡nh nhÃ¢n..." />
      <input v-model="searchPhone" placeholder="TÃ¬m kiáº¿m theo sá»‘ Ä‘iá»‡n thoáº¡i..." />
      <input type="date" v-model="searchDate" />

      <select v-model="selectedDepartment" @change="handleDepartmentFilter">
        <option value="">Chá»n khoa</option>
        <option v-for="dep in departments" :key="dep.id" :value="dep.id.toString()">
          {{ dep.name }}
        </option>
      </select>

      <select v-model="selectedDoctor" :disabled="!selectedDepartment">
        <option value="">Chá»n bÃ¡c sÄ©</option>
        <option v-for="d in doctorOptions" :key="d.id" :value="d.id.toString()">
          {{ d.fullName }}
        </option>
      </select>
    </div>

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

    <table>
      <thead>
        <tr>
          <th>MÃ£</th>
          <th>Bá»‡nh nhÃ¢n</th>
          <th>Äiá»‡n thoáº¡i</th>
          <th>NgÃ y sinh</th>
          <th>NgÃ y</th>
          <th>Tráº¡ng thÃ¡i</th>
          <th>Triá»‡u chá»©ng</th>
          <th>BÃ¡c sÄ©</th>
          <th>GÃ¡n/Äá»•i bÃ¡c sÄ©</th>
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
          <td>{{ a.statusDetail.doctorName || 'ChÆ°a gÃ¡n' }}</td>
          <td @click.stop>
            <template v-if="canAssignDoctor(a)">
              <button class="btn btn-sm btn-primary me-2" @click="openAssignModal(a)">
                {{ a.statusDetail.doctorId ? "Doi bac si" : "Gan bac si" }}
              </button>
              <button v-if="a.statusDetail.doctorId" class="btn btn-sm btn-success me-2" @click="checkInAppointment(a)">
                Check-in + tam ung
              </button>
              <span v-else class="text-muted small">(Gan bac si truoc)</span>
            </template>
            <button class="btn btn-sm btn-info" @click="$router.push(`/staff/appointments/${a.id}`)">
              Chi tiet
            </button>
          </td>
        </tr>
      </tbody>
    </table>

    <p v-if="appointments.length === 0">KhÃ´ng cÃ³ lá»‹ch khÃ¡m</p>

    <div v-if="showAssignModal" class="modal-backdrop" @click="showAssignModal = false">
      <div class="modal-content assign-modal" @click.stop>
        <div class="modal-header">
          <h4 class="modal-title">
            <span class="icon-doctor">ðŸ‘¨â€âš•ï¸</span> GÃ¡n/Äá»•i bÃ¡c sÄ© cho lá»‹ch khÃ¡m
          </h4>
          <button class="modal-close" @click="showAssignModal = false">&times;</button>
        </div>

        <div class="modal-body">
          <div class="form-group">
            <label for="departmentSelect">Chá»n khoa *</label>
            <select 
              id="departmentSelect"
              v-model="assignForm.departmentId" 
              class="form-select form-control"
            >
              <option value="">-- Chá»n khoa --</option>
              <option v-for="dep in departments" :key="dep.id" :value="dep.id.toString()">
                {{ dep.name }}
              </option>
            </select>
          </div>

          <div class="form-group">
            <label for="specialtySelect">Chá»n chuyÃªn khoa *</label>
            <select 
              id="specialtySelect"
              v-model="assignForm.specialtyId" 
              class="form-select form-control"
              :disabled="!assignForm.departmentId"
            >
              <option value="">-- Chá»n chuyÃªn khoa --</option>
              <option v-for="s in assignSpecialties" :key="s.id" :value="s.id.toString()">
                {{ s.name }}
              </option>
            </select>
          </div>

          <div class="form-group">
            <label for="doctorSelect">Chá»n bÃ¡c sÄ© *</label>
            <select 
              id="doctorSelect"
              v-model="assignForm.doctorId" 
              class="form-select form-control"
              :disabled="!assignForm.specialtyId"
            >
              <option value="">-- Chá»n bÃ¡c sÄ© --</option>
              <option v-for="d in assignDoctorsList" :key="d.id" :value="d.id.toString()">
                {{ d.fullName }}
              </option>
            </select>
          </div>
        </div>

        <div class="modal-footer">
          <button class="btn btn-secondary" @click="showAssignModal = false">
            âœ• Há»§y
          </button>
          <button class="btn btn-success" @click="confirmAssignDoctor">
            âœ“ XÃ¡c nháº­n gÃ¡n bÃ¡c sÄ©
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import api from '@/services/api'
import { useAuthStore } from '@/stores/auth'

useAuthStore()

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

const statuses = ['All', 'Pending', 'Confirmed', 'CheckedIn', 'Completed', 'Cancelled']
const currentStatus = ref('All')

const showAssignModal = ref(false)
const assignForm = ref({
  appointmentId: '',
  departmentId: '',
  specialtyId: '',
  doctorId: ''
})
const assignSpecialties = ref<any[]>([])
const assignDoctorsList = ref<any[]>([])

const doctorOptions = computed(() => {
  if (!selectedDepartment.value) return doctors.value
  return doctors.value.filter(d => d.departmentId?.toString() === selectedDepartment.value)
})

const filteredAppointments = computed(() => {
  return appointments.value.filter(a => {
    const matchCode = !searchCode.value || a.appointmentCode?.toLowerCase().includes(searchCode.value.toLowerCase())
    const matchName = !searchName.value || a.fullName?.toLowerCase().includes(searchName.value.toLowerCase())
    const matchPhone = !searchPhone.value || a.phone?.includes(searchPhone.value)
    const matchDate = !searchDate.value || a.appointmentDate?.startsWith(searchDate.value)
    const matchDept = !selectedDepartment.value || a.statusDetail.doctorDepartmentName === getDepartmentName(selectedDepartment.value)
    const matchDoctor = !selectedDoctor.value || a.statusDetail.doctorId === selectedDoctor.value
    return matchCode && matchName && matchPhone && matchDate && matchDept && matchDoctor
  })
})

const getDepartmentName = (depId: string) => departments.value.find(d => d.id === depId)?.name || ''

const loadDepartments = async () => {
  const res = await api.get('/Departments')
  departments.value = res.data ?? []
}

const loadDoctors = async () => {
  const res = await api.get('/Doctor')
  doctors.value = res.data ?? []
}

const loadAppointments = async () => {
  const url = currentStatus.value === 'All'
    ? '/staff/StaffAppointments'
    : `/staff/StaffAppointments/filter?status=${currentStatus.value}`
  const res = await api.get<Appointment[]>(url)
  appointments.value = res.data
}

const loadAssignSpecialties = async (departmentId: string) => {
  if (!departmentId) { assignSpecialties.value = []; return }
  const res = await api.get(`/Doctor/departments/${departmentId}/specialties`)
  assignSpecialties.value = res.data ?? []
}

const loadAssignDoctorsBySpecialty = async (specialtyId: string) => {
  if (!specialtyId) { assignDoctorsList.value = []; return }
  const res = await api.get(`/Doctor/by-specialty/${specialtyId}`)
  assignDoctorsList.value = res.data ?? []
}

function openAssignModal(appointment: Appointment) {
  assignForm.value = {
    appointmentId: appointment.id,
    departmentId: '',
    specialtyId: '',
    doctorId: ''
  }
  showAssignModal.value = true
  loadDepartments()
}

watch(() => assignForm.value.departmentId, async (newVal) => {
  await loadAssignSpecialties(newVal)
})

watch(() => assignForm.value.specialtyId, async (newVal) => {
  await loadAssignDoctorsBySpecialty(newVal)
})

async function confirmAssignDoctor() {
  if (!assignForm.value.doctorId) {
    alert('Vui lÃ²ng chá»n bÃ¡c sÄ©')
    return
  }
  await api.post('/staff/StaffAppointments/assign-doctor', {
    appointmentId: assignForm.value.appointmentId,
    doctorId: assignForm.value.doctorId
  })
  alert('BÃ¡c sÄ© Ä‘Ã£ Ä‘Æ°á»£c gÃ¡n/Ä‘á»•i âœ…')
  showAssignModal.value = false
  loadAppointments()
}

async function checkInAppointment(a: Appointment) {
  const input = prompt("Nh?p s? ti?n t?m ?ng (VND)", "300000")
  const amount = Number(input ?? "0")
  if (!amount || amount <= 0) {
    alert("S? ti?n t?m ?ng không h?p l?")
    return
  }
  await api.post("/staff/StaffAppointments/checkin", {
    appointmentId: a.id,
    depositAmount: amount,
    method: "cash"
  })
  alert("Check-in & thu t?m ?ng thành công")
  loadAppointments()
}
const canAssignDoctor = (a: Appointment) => {
  if (!a) return false
  const status = a.statusDetail?.value
  return status === 'Pending' || status === 'Confirmed' || (status === 'CheckedIn' && !a.statusDetail?.doctorId)
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
    All: 'T?t c?',
    Pending: 'Ch? x? lý',
    Confirmed: 'Ðã xác nh?n',
    CheckedIn: 'Ðã check-in',
    Completed: 'Hoàn thành',
    Cancelled: 'Ðã h?y',
  }
  return labels[status] || status
}
const handleDepartmentFilter = () => {
  selectedDoctor.value = ""
}

onMounted(() => {
  loadDepartments()
  loadDoctors()
  loadAppointments()
})
</script>

<style src="@/styles/layouts/staff-appointment.css"></style>



