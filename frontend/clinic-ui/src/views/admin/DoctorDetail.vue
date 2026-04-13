<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue'
import { useRoute } from 'vue-router'
import api from '@/services/api'
import '@/styles/layouts/doctor-detail.css'
const route = useRoute()
const doctorId = route.params.id

const doctor = ref<any>(null)
const appointments = ref<any[]>([])
const searchTerm = ref('')
const showEdit = ref(false)
const editForm = ref<any>({})
const departments = ref<any[]>([])
const specialties = ref<any[]>([])
const uploadLoading = ref(false)
const uploadError = ref('')
const fileInputRef = ref<HTMLInputElement | null>(null)

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

const onAvatarSelected = async (e: Event) => {
  const input = e.target as HTMLInputElement
  const file = input.files?.[0]
  if (!file) return
  uploadError.value = ''
  const form = new FormData()
  form.append('file', file)
  try {
    uploadLoading.value = true
    const res = await api.post(`/Doctor/${doctorId}/avatar`, form, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    editForm.value.avatarUrl = res.data?.url
    doctor.value.avatarUrl = res.data?.url
  } catch (err: any) {
    uploadError.value = err?.response?.data?.message || 'Tải ảnh thất bại'
  } finally {
    uploadLoading.value = false
    input.value = ''
  }
}
</script>
<template>
  <div class="doctor-page-container">
    <div class="container" v-if="doctor">
      
      <div class="page-header-box d-flex justify-content-between align-items-center">
        <div>
          <h2 class="page-title mb-1">Chi tiết bác sĩ</h2>
          <p class="text-muted mb-0">Quản lý thông tin hồ sơ bác sĩ trực tiếp</p>
        </div>
        <div class="header-actions">
          <button v-if="!showEdit" class="btn btn-outline-primary me-2" @click="$router.push(`/doctors/${doctorId}/schedules`)">
            <i class="bi bi-calendar3 me-2"></i>Lịch làm việc
          </button>
          <button v-if="!showEdit" class="btn-primary" @click="openEdit">
            <i class="bi bi-pencil-square me-2"></i>Sửa thông tin
          </button>
          <div v-else class="d-flex gap-2">
            <button class="btn-update" @click="saveDoctor" :disabled="uploadLoading">Lưu thay đổi</button>
            <button class="btn-cancel" @click="showEdit = false">Hủy</button>
          </div>
        </div>
      </div>

      <div class="doctor-info-card" :class="{ 'is-editing': showEdit }">
        
        <div class="doctor-profile-section">
          <div class="avatar-container-inline">
            <img :src="editForm.avatarUrl || doctor?.avatarUrl || '/default-avatar.png'" class="main-avatar" />
            <div v-if="showEdit" class="avatar-upload-overlay" @click="fileInputRef?.click()">
              <i v-if="!uploadLoading" class="bi bi-camera-fill"></i>
              <div v-else class="spinner-border spinner-border-sm text-light"></div>
            </div>
          </div>
          <input ref="fileInputRef" type="file" accept="image/*" class="d-none" @change="onAvatarSelected" />
          
          <div v-if="!showEdit" class="mt-2">
            <span :class="['status-badge-modern', doctorBadgeClass(doctor?.status)]">
              {{ doctorStatusLabel(doctor?.status) }}
            </span>
          </div>
          <p v-else class="text-primary small fw-bold mt-2">Click ảnh để đổi</p>
        </div>

        <div class="doctor-details-grid">
          
          <div class="detail-box">
            <label>Họ và tên</label>
            <p v-if="!showEdit">{{ doctor?.fullName }}</p>
            <input v-else v-model="editForm.fullName" class="form-control-modern" />
          </div>

          <div class="detail-box">
            <label>Mã bác sĩ</label>
            <p v-if="!showEdit">{{ doctor?.code }}</p>
            <input v-else v-model="editForm.code" class="form-control-modern" />
          </div>

          <div class="detail-box">
            <label>Khoa</label>
            <p v-if="!showEdit">{{ doctor?.departmentName }}</p>
            <select v-else v-model="editForm.departmentId" class="form-select-modern">
              <option v-for="dep in departments" :key="dep.id" :value="dep.id.toString()">{{ dep.name }}</option>
            </select>
          </div>

          <div class="detail-box">
            <label>Chuyên khoa</label>
            <p v-if="!showEdit">{{ doctor?.specialtyName }}</p>
            <select v-else v-model="editForm.specialtyId" class="form-select-modern" :disabled="!editForm.departmentId">
              <option v-for="s in specialties" :key="s.id" :value="s.id.toString()">{{ s.name }}</option>
            </select>
          </div>

          <div class="detail-box full-row">
            <label>Số giấy phép hành nghề</label>
            <p v-if="!showEdit">{{ doctor?.licenseNumber }}</p>
            <input v-else v-model="editForm.licenseNumber" class="form-control-modern" />
          </div>

        </div>
      </div>

      <div class="appointments-section">
        <div class="section-header">
          <h4 class="mb-0 fw-bold">Lịch khám của {{ doctor?.fullName }}</h4>
          <div class="d-flex gap-2">
            <input v-model="searchTerm" class="search-box" placeholder="Tìm bệnh nhân..." />
          </div>
        </div>
        <div class="table-responsive">
          <table class="table table-custom align-middle">
            <thead>
              <tr>
                <th>Mã</th>
                <th>Bệnh nhân</th>
                <th>Điện thoại</th>
                <th>Ngày khám</th>
                <th>Giờ</th>
                <th>Trạng thái</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="a in filteredAppointments" :key="a.id">
                <td class="fw-bold">{{ a.appointmentCode }}</td>
                <td>{{ a.fullName }}</td>
                <td>{{ a.phone }}</td>
                <td>{{ a.appointmentDate }}</td>
                <td>{{ a.appointmentTime }}</td>
                <td>
                  <span :class="['status-pill', a.status === 'Completed' ? 'pill-gray' : 'pill-green']">
                    {{ appointmentStatusLabel(a.status) }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

    </div>
  </div>
</template>
