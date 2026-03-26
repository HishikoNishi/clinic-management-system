<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
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
const avatarFile = ref<File|null>(null)

onMounted(async () => {
  await loadDoctor()
  await loadAppointments()
})

async function loadDoctor() {
  try {
    const resDoctor = await api.get(`/Doctor/${doctorId}`)
    doctor.value = resDoctor.data
    
    editForm.value = { ...doctor.value }
  } catch (err:any) {
    alert("Không thể tải thông tin bác sĩ")
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

function onFileChange(e: Event) {
  const target = e.target as HTMLInputElement
  if (target.files && target.files.length > 0) {
    avatarFile.value = target.files[0]
  }
}

async function saveDoctor() {
  try {
    let avatarUrl = doctor.value.avatarUrl;
if (avatarFile.value) {
  const formData = new FormData();
  formData.append("file", avatarFile.value);
  const resUpload = await api.post("/upload/avatar", formData);
  // Kiểm tra field trả về
  avatarUrl = resUpload.data.url || resUpload.data.path;
}

await api.put(`/Doctor/${doctorId}`, {
  fullName: editForm.value.fullName,
  code: editForm.value.code,
  specialty: editForm.value.specialty,
  licenseNumber: editForm.value.licenseNumber,
  departmentId: editForm.value.departmentId, 
  avatarUrl: avatarUrl
});



    showEdit.value = false
    await loadDoctor()
  } catch (err:any) {
    alert("Không thể cập nhật bác sĩ: " + err.message)
  }
}

const filteredAppointments = computed(() => {
  const term = searchTerm.value.trim().toLowerCase()
  return appointments.value
    .filter(a => !term || a.fullName.toLowerCase().includes(term))
    .sort((a, b) => {
      const activeStatuses = ['Pending', 'Confirmed']
      if (activeStatuses.includes(a.status) && !activeStatuses.includes(b.status)) return -1
      if (!activeStatuses.includes(a.status) && activeStatuses.includes(b.status)) return 1
      return 0
    })
})
</script>

<template>
  <div class="doctor-detail-page container py-4">
    <h2 class="page-title">Chi tiết bác sĩ</h2>

    <!-- Thông tin bác sĩ -->
    <div class="doctor-info card mb-4">
      <div class="row g-4 align-items-center">
        <div class="col-md-3 text-center">
          <img :src="doctor?.avatarUrl || '/default-avatar.png'" class="avatar" />
        </div>
        <div class="col-md-9">
<h3 class="doctor-name">Tên: {{ doctor?.fullName || doctor?.name }}</h3>
          <p><strong>Mã:</strong> {{ doctor?.code }}</p>
          <p><strong>Chuyên khoa:</strong> {{ doctor?.specialty }}</p>
          <p><strong>Khoa:</strong> {{ doctor?.departmentName }}</p>
          <p><strong>Số giấy phép:</strong> {{ doctor?.licenseNumber }}</p>
          <span class="badge status-badge">Hoạt động</span>
          <div class="mt-3">
            <button class="btn btn-primary" @click="showEdit = true">Sửa thông tin</button>
          </div>
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
                  {{ a.status }}
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
        <input v-model="editForm.specialty" class="form-control mb-2" placeholder="Chuyên khoa" />
        <input v-model="editForm.licenseNumber" class="form-control mb-2" placeholder="Số giấy phép" />
        <input type="file" class="form-control mb-2" @change="onFileChange" />
        <img v-if="editForm.avatarUrl" :src="editForm.avatarUrl" class="avatar-preview mt-2" />
        <div class="text-end mt-3">
          <button class="btn btn-secondary me-2" @click="showEdit=false">Hủy</button>
          <button class="btn btn-success" @click="saveDoctor">Lưu</button>
        </div>
      </div>
    </div>
  </div>
</template>

