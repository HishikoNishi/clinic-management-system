<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue'
import { useRoute } from 'vue-router'
import axios from 'axios'
import { useAuthStore } from '@/stores/auth'
import '@/styles/layouts/doctor-detail.css'

const route = useRoute()
const doctorId = route.params.id
const auth = useAuthStore()

const api = axios.create({
  baseURL: 'https://localhost:7235/api',
  headers: { Authorization: `Bearer ${auth.token}` }
})

const doctor = ref<any>(null)
const appointments = ref<any[]>([])
const searchTerm = ref('')
const showEdit = ref(false)
const editForm = ref<any>({})
const departments = ref<any[]>([])
const specialties = ref<any[]>([])

onMounted(async () => {
  await loadDoctor()
  await loadAppointments()
})

async function loadDoctor() {
  const resDoctor = await api.get(`/Doctor/${doctorId}`)
  doctor.value = resDoctor.data
  editForm.value = {
    ...doctor.value,
    departmentId: doctor.value.departmentId?.toString() || '',
    specialtyId: doctor.value.specialtyId?.toString() || ''   // ✅ thêm dòng này
  }
}


async function loadAppointments() {
  try {
    const resAppointments = await api.get(`/Doctor/${doctorId}/appointments`)
    appointments.value = resAppointments.data
  } catch (err:any) {
    console.error("Lỗi khi load lịch khám:", err)
  }
}

async function loadDepartments() {
  const res = await api.get('/Departments')
  departments.value = res.data
}

async function loadSpecialties(departmentId: string) {
  if (!departmentId) {
    specialties.value = []
    return
  }
  const res = await api.get(`/Doctor/departments/${departmentId}/specialties`)
  specialties.value = res.data
}

async function saveDoctor() {
  try {
    await api.put(`/Doctor/${doctorId}`, {
      fullName: editForm.value.fullName,
      code: editForm.value.code,
      specialtyId: editForm.value.specialtyId,   
      licenseNumber: editForm.value.licenseNumber,
      departmentId: editForm.value.departmentId,
      avatarUrl: editForm.value.avatarUrl
    })

    showEdit.value = false
    await loadDoctor()
  } catch (err:any) {
    alert("Không thể cập nhật bác sĩ: " + err.message)
  }
}

async function openEdit() {
  // Reset editForm từ doctor mới nhất
  editForm.value = {
    ...doctor.value,
    departmentId: doctor.value.departmentId?.toString() || '',
    specialtyId: doctor.value.specialtyId?.toString() || ''
  }
  await loadDepartments()
  if (editForm.value.departmentId) {
    await loadSpecialties(editForm.value.departmentId)
  }
  showEdit.value = true
}
watch(() => editForm.value.departmentId, async (newVal, oldVal) => {
  if (newVal) {
    await loadSpecialties(newVal)
    // Chỉ reset nếu department thực sự thay đổi khác với cũ
    if (newVal !== oldVal) {
      editForm.value.specialtyId = ''
    }
  } else {
    specialties.value = []
    editForm.value.specialtyId = ''
  }
})



const filteredAppointments = computed(() => {
  const term = searchTerm.value.trim().toLowerCase()
  return appointments.value
    .filter(a => !term || (a.fullName ?? '').toLowerCase().includes(term))
    .sort((a, b) => {
      const activeStatuses = ['Pending', 'Confirmed']
      if (activeStatuses.includes(a.status) && !activeStatuses.includes(b.status)) return -1
      if (!activeStatuses.includes(a.status) && activeStatuses.includes(b.status)) return 1
      return 0
    })
})

const doctorBadgeClass = (status: string) => {
  if (status === "Active") return "bg-success"
  if (status === "Busy") return "bg-warning"
  if (status === "Inactive") return "bg-secondary"
  return "bg-light"
}

const doctorStatusLabel = (status: string) => {
  const labels: Record<string, string> = {
    Active: "Hoạt động",
    Busy: "Đang khám",
    Inactive: "Không hoạt động"
  }
  return labels[status] || status
}

const appointmentStatusLabel = (status: string) => {
  const labels: Record<string, string> = {
    Pending: "Chờ xác nhận",
    Confirmed: "Đã xác nhận",
    Completed: "Đã khám xong"
  }
  return labels[status] || status
}
</script>
<template>
  <div class="doctor-page-container">
    <div class="container">
      <div class="page-header-box d-flex justify-content-between align-items-center mb-4">
        <div>
          <h1 class="page-title mb-1">Chi tiết bác sĩ</h1>
          <p class="text-muted mb-0">Quản lý hồ sơ và lịch khám chuyên khoa</p>
        </div>
        <div class="header-actions" v-if="!showEdit">
          <button class="btn-primary" @click="openEdit">Sửa thông tin</button>
        </div>
        <div class="header-actions d-flex gap-2" v-else>
          <button class="btn-update" @click="saveDoctor">Lưu thay đổi</button>
          <button class="btn-cancel" @click="showEdit = false">Quay lại</button>
        </div>
      </div>

      <div class="doctor-info-card" :class="{ 'is-editing': showEdit }">
        <div class="doctor-profile-section">
          <img :src="(showEdit ? editForm.avatarUrl : doctor?.avatarUrl) || '/default-avatar.png'" class="main-avatar" />
          
          <div v-if="!showEdit" class="profile-text text-center">
            <h2 class="doctor-name-display">{{ doctor?.fullName }}</h2>
            <span :class="['status-badge-lg', doctorBadgeClass(doctor?.status)]">
              {{ doctorStatusLabel(doctor?.status) }}
            </span>
          </div>
          <div v-else class="form-group full-width mt-3">
            <label>Link ảnh đại diện</label>
            <input v-model="editForm.avatarUrl" placeholder="Dán link ảnh vào đây..." />
          </div>
        </div>

        <div class="doctor-details-grid">
          <template v-if="!showEdit">
            <div class="detail-box">
              <label>Mã bác sĩ</label>
              <p>{{ doctor?.code }}</p>
            </div>
            <div class="detail-box">
              <label>Số giấy phép</label>
              <p>{{ doctor?.licenseNumber }}</p>
            </div>
            <div class="detail-box">
              <label>Khoa</label>
              <p>{{ doctor?.departmentName }}</p>
            </div>
            <div class="detail-box">
              <label>Chuyên khoa</label>
              <p>{{ doctor?.specialtyName }}</p>
            </div>
          </template>

          <template v-else>
            <div class="form-group">
              <label>Họ và tên</label>
              <input v-model="editForm.fullName" />
            </div>
            <div class="form-group">
              <label>Mã nhân viên</label>
              <input v-model="editForm.code" />
            </div>
            <div class="form-group">
              <label>Khoa</label>
              <select v-model="editForm.departmentId">
                <option v-for="dep in departments" :key="dep.id" :value="dep.id.toString()">{{ dep.name }}</option>
              </select>
            </div>
            <div class="form-group">
              <label>Chuyên khoa</label>
              <select v-model="editForm.specialtyId" :disabled="!editForm.departmentId">
                <option v-for="s in specialties" :key="s.id" :value="s.id.toString()">{{ s.name }}</option>
              </select>
            </div>
            <div class="form-group full-width">
              <label>Số giấy phép hành nghề</label>
              <input v-model="editForm.licenseNumber" />
            </div>
          </template>
        </div>
      </div>

      <div class="appointments-section" v-if="!showEdit">
        <div class="section-header">
          <h4 class="fw-bold m-0">Lịch khám của bác sĩ</h4>
          <div class="search-wrapper">
            <input v-model="searchTerm" class="search-box" placeholder="Tìm bệnh nhân..." />
          </div>
        </div>
        
        <div class="table-responsive mt-3">
          <table class="table table-custom">
            <thead>
              <tr>
                <th>Mã lịch</th>
                <th>Bệnh nhân</th>
                <th>Điện thoại</th>
                <th>Ngày khám</th>
                <th>Giờ</th>
                <th class="text-center">Trạng thái</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="a in filteredAppointments" :key="a.id">
                <td class="code-text">{{ a.appointmentCode }}</td>
                <td class="fw-bold">{{ a.fullName }}</td>
                <td>{{ a.phone }}</td>
                <td>{{ a.appointmentDate }}</td>
                <td>{{ a.appointmentTime }}</td>
                <td class="text-center">
                  <span :class="['status-pill', a.status === 'Completed' ? 'pill-gray' : 'pill-green']">
                    {{ appointmentStatusLabel(a.status) }}
                  </span>
                </td>
              </tr>
              <tr v-if="filteredAppointments.length === 0">
                <td colspan="6" class="text-center text-muted py-4">Không tìm thấy lịch khám nào.</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

    </div>
  </div>
</template>