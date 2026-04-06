<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import axios from 'axios'
import { useAuthStore } from '@/stores/auth'
import '@/styles/layouts/doctor-detail.css'

const route = useRoute()
const router = useRouter()
const doctorId = route.params.id
const auth = useAuthStore()

const api = axios.create({
  baseURL: 'https://localhost:7235/api',
  headers: { Authorization: `Bearer ${auth.token}` }
})

function goBackToStaffList() {
  router.push('/doctors')
}

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
  
  // Quan trọng: Cập nhật editForm ngay sau khi load doctor
  editForm.value = {
    ...doctor.value,
    status: doctor.value.status, // Đảm bảo status được gán ở đây
    departmentId: doctor.value.departmentId?.toString() || '',
    specialtyId: doctor.value.specialtyId?.toString() || ''
  }
}

function syncEditForm() {
  editForm.value = {
    ...doctor.value,
    status: doctor.value.status,
    departmentId: doctor.value.departmentId?.toString() || '',
    specialtyId: doctor.value.specialtyId?.toString() || ''
  }
}

async function loadAppointments() {
  try {
    const resAppointments = await api.get(`/Doctor/${doctorId}/appointments`)
    appointments.value = resAppointments.data
  } catch (err: any) {
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

// Thêm hàm này vào phần script của bạn
async function updateStatus(id: string, status: string) {
  try {
    // Gọi đúng endpoint như code tham khảo của bạn
    await api.put(`/Doctor/${id}/status`, { status })
    console.log("Đã cập nhật trạng thái riêng biệt thành công")
  } catch (err: any) {
    console.error("Lỗi cập nhật trạng thái:", err)
    throw err // Ném lỗi để hàm saveDoctor phía dưới xử lý
  }
}

async function saveDoctor() {
  try {
    // 1. Cập nhật thông tin chung (Họ tên, khoa, chuyên khoa...)
    await api.put(`/Doctor/${doctorId}`, {
      fullName: editForm.value.fullName,
      code: editForm.value.code,
      specialtyId: editForm.value.specialtyId,
      licenseNumber: editForm.value.licenseNumber,
      departmentId: editForm.value.departmentId,
      avatarUrl: editForm.value.avatarUrl
      // Không gửi status ở đây nếu API cũ không nhận
    })

    // 2. Cập nhật TRẠNG THÁI (Gọi endpoint riêng giống code tham khảo)
await updateStatus(doctorId as string, editForm.value.status)
    showEdit.value = false
    await loadDoctor() // Load lại để UI cập nhật badge mới
    alert("Cập nhật thông tin và trạng thái thành công!")
  } catch (err: any) {
    alert("Không thể cập nhật: " + (err.response?.data?.message || err.message))
  }
}

async function openEdit() {
  // Đồng bộ từ doctor hiện tại sang form sửa
  editForm.value = {
    ...doctor.value,
    status: doctor.value.status, // Lấy status hiện tại của doctor
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
        <div class="header-actions d-flex gap-2">
          <template v-if="!showEdit">
            <button class="btn btn-outline-secondary px-3" @click="goBackToStaffList">
              <i class="bi bi-arrow-left me-1"></i> Danh sách
            </button>
            <button class="btn-primary px-3" @click="openEdit">Sửa thông tin</button>
          </template>
          <template v-else>
            <button class="btn-update px-3" @click="saveDoctor">Lưu thay đổi</button>
            <button class="btn-cancel px-3" @click="showEdit = false">Hủy</button>
          </template>
        </div>
      </div>

      <div class="doctor-info-card" :class="{ 'is-editing': showEdit }">
        <div class="doctor-profile-section">
          <div class="avatar-wrapper">
            <img :src="(showEdit ? editForm.avatarUrl : doctor?.avatarUrl) || '/default-avatar.png'" class="main-avatar" />
          </div>

          <div v-if="!showEdit" class="profile-text text-center">
            <h2 class="doctor-name-display">{{ doctor?.fullName }}</h2>
            <div :class="['status-badge-modern', doctorBadgeClass(doctor?.status)]">
              {{ doctorStatusLabel(doctor?.status) }}
            </div>
          </div>

          <div v-else class="edit-profile-box mt-3 w-100 px-4">
           <div class="form-group mb-3">
  <label class="small fw-bold text-muted mb-1">Trạng thái bác sĩ</label>
  <select 
    v-model="editForm.status" 
    class=" status-badge-morden select form-select-sm status-select-edit"
  >
    <option value="Active">Hoạt động</option>
    <option value="Busy">Đang khám</option>
    <option value="Inactive">Không hoạt động</option>
  </select>
</div>
            <div class="form-group">
              <label class="small fw-bold text-muted mb-1">Link ảnh đại diện</label>
              <input v-model="editForm.avatarUrl" class="form-control form-control-sm" />
            </div>
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
              <input v-model="editForm.fullName" class="form-control" />
            </div>
            <div class="form-group">
              <label>Mã nhân viên</label>
              <input v-model="editForm.code" class="form-control" />
            </div>
            <div class="form-group">
              <label>Khoa</label>
              <select v-model="editForm.departmentId" class="form-select">
                <option v-for="dep in departments" :key="dep.id" :value="dep.id.toString()">{{ dep.name }}</option>
              </select>
            </div>
            <div class="form-group">
              <label>Chuyên khoa</label>
              <select v-model="editForm.specialtyId" :disabled="!editForm.departmentId" class="form-select">
                <option v-for="s in specialties" :key="s.id" :value="s.id.toString()">{{ s.name }}</option>
              </select>
            </div>
            <div class="form-group full-width">
              <label>Số giấy phép hành nghề</label>
              <input v-model="editForm.licenseNumber" class="form-control" />
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