<template>
  <div class="appointment-detail">
    <h2>Chi tiết lịch khám</h2>
    <div v-if="appointment" class="detail-card">
      <div class="detail-row"><span class="label">Mã:</span> {{ appointment.appointmentCode }}</div>
      <div class="detail-row"><span class="label">Bệnh nhân:</span> {{ appointment.fullName }}</div>
      <div class="detail-row"><span class="label">Điện thoại:</span> {{ appointment.phone }}</div>
      <div class="detail-row"><span class="label">Email:</span> {{ appointment.email }}</div>
      <div class="detail-row"><span class="label">Ngày sinh:</span> {{ formatDate(appointment.dateOfBirth) }}</div>
      <div class="detail-row"><span class="label">Giới tính:</span> {{ appointment.gender }}</div>
      <div class="detail-row"><span class="label">Địa chỉ:</span> {{ appointment.address }}</div>
      <div class="detail-row"><span class="label">Lý do:</span> {{ appointment.reason }}</div>
     <div class="detail-row">
  <span class="label">Trạng thái:</span>
  <span :class="'status ' + appointment.status.toLowerCase()">
    {{ statusLabel(appointment.status) }}
  </span>
</div>

    <div v-if="appointment.statusDetail.doctorName" class="detail-row">
  <span class="label">Bác sĩ:</span>
  {{ appointment.statusDetail.doctorName }} ({{ appointment.statusDetail.doctorCode }})
  - Khoa: {{ appointment.statusDetail.doctorDepartmentName }}
</div>


      <!-- Nếu trạng thái là Pending thì cho phép gán bác sĩ -->
      <div v-if="appointment.status === 'Pending'" class="assign-doctor">
  <h3>Gán bác sĩ</h3>
  <div class="assign-row">
    <select v-model="selectedDepartment" @change="loadDoctorsByDepartment">
      <option value="">Chọn khoa</option>
      <option v-for="dep in departments" :key="dep.id" :value="dep.id">
        {{ dep.name }}
      </option>
    </select>

    <select v-model="selectedDoctor" :disabled="!selectedDepartment">
      <option value="">Chọn bác sĩ</option>
      <option v-for="d in doctors" :key="d.id" :value="d.id">
        {{ d.fullName }}
      </option>
    </select>

    <button @click="assignDoctor">Gán</button>
  </div>
</div>

    </div>
    <p v-else-if="error" class="error">{{ error }}</p>
    <button class="back-btn" @click="$router.push('/staff/appointments')">← Quay lại danh sách</button>
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

const departments = ref<any[]>([])
const doctors = ref<any[]>([])
const selectedDepartment = ref('')
const selectedDoctor = ref('')

const loadDetail = async () => {
  try {
    const res = await api.get(`/staff/StaffAppointments/${route.params.id}`)
    appointment.value = res.data
  } catch (err) {
    error.value = "Không thể tải chi tiết lịch khám."
  }
}

const loadDepartments = async () => {
  const res = await api.get('/Departments')
  departments.value = res.data
}

const loadDoctorsByDepartment = async () => {
  if (!selectedDepartment.value) {
    doctors.value = []
    return
  }
  const res = await api.get(`/Doctor/by-department/${selectedDepartment.value}`)
  doctors.value = res.data
}

const assignDoctor = async () => {
  if (!selectedDoctor.value) return
  const doctor = doctors.value.find(d => d.id === selectedDoctor.value)
  const message = `Bạn có chắc chắn muốn gán bác sĩ ${doctor?.fullName} khoa ${doctor?.departmentName} cho bệnh nhân ${appointment.value.fullName} với triệu chứng: ${appointment.value.reason}?`
  
  if (!confirm(message)) {
    selectedDoctor.value = ''
    return
  }

  await api.post('/staff/StaffAppointments/assign-doctor', {
    appointmentId: appointment.value.id,
    doctorId: selectedDoctor.value
  })
  alert('Bác sĩ đã được gán ✅')
  loadDetail()
}


const formatDate = (dateStr: string) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  return date.toLocaleDateString()
}
const statusLabel = (status: string) => {
  const labels: { [key: string]: string } = {
    'Pending': 'Chờ xử lý',
    'Confirmed': 'Đã xác nhận',
    'Completed': 'Hoàn thành',
    'Cancelled': 'Đã hủy'
  }
  return labels[status] || status
}

onMounted(() => {
  loadDetail()
  loadDepartments()
})
</script>

<style src="@/styles/layouts/appointment-detail.css"></style>
