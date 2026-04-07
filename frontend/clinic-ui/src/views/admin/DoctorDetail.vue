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
  <div class="doctor-page">
    <div class="container">
      <div class="d-flex flex-wrap justify-content-between align-items-center gap-3 mb-3">
        <div>
          <h2 class="mb-1">Chi tiết bác sĩ</h2>
          <p class="text-muted mb-0">Xem và chỉnh sửa thông tin bác sĩ.</p>
        </div>
        <div class="d-flex flex-wrap gap-2 align-items-center">
          <button class="btn btn-primary" @click="openEdit">Sửa thông tin</button>
        </div>
      </div>

      <!-- Thông tin bác sĩ -->
      <div class="doctor-info card mb-4">
      <div class="row g-4 align-items-center">
        <div class="col-md-3 text-center">
          <img :src="doctor?.avatarUrl || '/default-avatar.png'" class="avatar" />
        </div>
        <div class="col-md-9">
          <h3 class="doctor-name">Tên: {{ doctor?.fullName }}</h3>
          <p><strong>Mã:</strong> {{ doctor?.code }}</p>
          <p><strong>Chuyên khoa:</strong> {{ doctor?.specialtyName }}</p>
          <p><strong>Khoa:</strong> {{ doctor?.departmentName }}</p>
          <p><strong>Số giấy phép:</strong> {{ doctor?.licenseNumber }}</p>
          <span :class="['badge', doctorBadgeClass(doctor?.status)]">
            {{ doctorStatusLabel(doctor?.status) }}
          </span>
        </div>
      </div>
    </div>

    <!-- Lịch khám -->
    <div class="appointments card">
      <div class="card-header d-flex justify-content-between align-items-center">
        <h4 class="mb-0">Lịch khám của bác sĩ</h4>
        <input v-model="searchTerm" class="form-control search-box" placeholder="Tìm bệnh nhân..." />
      </div>
      <div class="table-responsive">
        <table class="table table-hover align-middle mb-0">
          <thead class="table-light">
            <tr>
              <th>Mã lịch</th>
              <th>Bệnh nhân</th>
              <th>Điện thoại</th>
              <th>Ngày</th>
              <th>Giờ</th>
              <th>Lý do</th>
              <th>Trạng thái</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="a in filteredAppointments" :key="a.id">
              <td class="fw-semibold">{{ a.appointmentCode }}</td>
              <td>{{ a.fullName }}</td>
              <td>{{ a.phone }}</td>
              <td>{{ a.appointmentDate }}</td>
              <td>{{ a.appointmentTime }}</td>
              <td>{{ a.reason }}</td>
              <td>
                <span :class="['badge', a.status === 'Completed' ? 'bg-secondary' : 'bg-success']">
                  {{ appointmentStatusLabel(a.status) }}
                </span>
              </td>
            </tr>
            <tr v-if="filteredAppointments.length === 0">
              <td colspan="7" class="text-center text-muted py-3">Không có lịch khám nào</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Modal chỉnh sửa -->
    <div v-if="showEdit" class="modal-backdrop">
      <div class="modal-content p-4">
        <h4>Cập nhật thông tin bác sĩ</h4>
        <input v-model="editForm.fullName" class="form-control mb-2" placeholder="Tên bác sĩ" />
        <input v-model="editForm.code" class="form-control mb-2" placeholder="Mã bác sĩ" />

        <select v-model="editForm.departmentId" class="form-select mb-2">
          <option value="">Chọn khoa</option>
         <option v-for="dep in departments" :key="dep.id" :value="dep.id.toString()">
  {{ dep.name }}
</option>
        </select>

        <select v-model="editForm.specialtyId" class="form-select mb-2" :disabled="!editForm.departmentId">
          <option value="">Chọn chuyên khoa</option>
         <option v-for="s in specialties" :key="s.id" :value="s.id.toString()">
  {{ s.name }}
</option>
        </select>

        <input v-model="editForm.licenseNumber" class="form-control mb-2" placeholder="Số giấy phép" />
        <label class="form-label mt-2">Ảnh bác sĩ</label>
        <input v-model="editForm.avatarUrl" class="form-control mb-2" placeholder="Link ảnh (tuỳ chọn nếu không upload)" />
        <div class="d-flex align-items-center gap-2 mb-2">
          <input type="file" accept="image/*" @change="onAvatarSelected" :disabled="uploadLoading" />
          <span v-if="uploadLoading" class="small text-muted">Đang tải...</span>
        </div>
        <div v-if="uploadError" class="text-danger small mb-2">{{ uploadError }}</div>
        <img v-if="editForm.avatarUrl" :src="editForm.avatarUrl" class="avatar-preview mt-2" />

        <div class="text-end mt-3">
          <button class="btn btn-secondary me-2" @click="showEdit=false">Hủy</button>
          <button class="btn btn-success" @click="saveDoctor">Lưu</button>
        </div>
      </div>
    </div>
    </div>
  </div>
</template>
