<script setup lang="ts">
import { ref, reactive, onMounted, computed, watch } from 'vue'
import { doctorService, type Doctor } from '@/services/doctorService'
import '@/styles/layouts/doctor.css'
import { getUsers } from '@/services/userService'
import api from "@/services/api"

const departments = ref<any[]>([])
const specialties = ref<any[]>([])
const users = ref<any[]>([])
const doctors = ref<Doctor[]>([])
const loading = ref(false)
const showModal = ref(false)
const editingId = ref<string | null>(null)
const searchTerm = ref('')
const filterDepartmentId = ref<string>('')

const form = reactive({
  userId: '',
  fullName: '',
  code: '',
  specialtyId: '' as string | null,
  licenseNumber: '',
  departmentId: '' as string | null,
  status: 'Active'
})

const filteredDoctors = computed(() => {
  const term = searchTerm.value.trim().toLowerCase()
  return doctors.value.filter((d) => {
    const text = `${d.code} ${d.specialtyName} ${d.licenseNumber || ''}`.toLowerCase()
    const matchText = !term || text.includes(term)
    const matchDept = !filterDepartmentId.value || d.departmentId?.toString() === filterDepartmentId.value
    return matchText && matchDept
  })
})

async function loadDepartments() {
  const res = await api.get('/departments')
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

async function loadDoctors() {
  loading.value = true
  doctors.value = await doctorService.getAll()
  loading.value = false
}

function openCreate() {
  editingId.value = null
  form.userId = ''
  form.fullName = ''
  form.code = ''
  form.specialtyId = ''
  form.licenseNumber = ''
  form.departmentId = ''
  form.status = 'Active'
  specialties.value = []
  showModal.value = true
}

function openEdit(doctor: Doctor) {
  editingId.value = doctor.id
  form.userId = doctor.userId
  form.fullName = doctor.fullName || ''
  form.code = doctor.code
  form.specialtyId = doctor.specialtyId
  form.licenseNumber = doctor.licenseNumber || ''
  form.departmentId = doctor.departmentId?.toString() || ''
  form.status = doctor.status || 'Active'
  showModal.value = true
  loadSpecialties(form.departmentId || '')
}

async function save() {
  try {
    if (!form.departmentId || !form.specialtyId) {
      alert("Vui lòng chọn khoa và chuyên khoa")
      return
    }

    if (editingId.value) {
      await doctorService.update(editingId.value, {
        code: form.code,
        fullName: form.fullName,
        specialtyId: form.specialtyId,
        licenseNumber: form.licenseNumber,
        departmentId: form.departmentId,
        status: form.status
      })
    } else {
      await doctorService.create({
  userId: form.userId,
  fullName: form.fullName,
  code: form.code,
  specialtyId: form.specialtyId ? form.specialtyId.toString() : null,
  licenseNumber: form.licenseNumber,
  departmentId: form.departmentId ? form.departmentId.toString() : null
})


    }

    showModal.value = false
    await loadDoctors()
  } catch (error: any) {
    alert(error.response?.data?.message || "Có lỗi xảy ra")
  }
}

async function remove(id: string) {
  if (confirm("Bạn có chắc muốn xóa bác sĩ này?")) {
    await doctorService.delete(id)
    await loadDoctors()
  }
}

async function updateStatus(id: string, status: string) {
  try {
    await api.put(`/Doctor/${id}/status`, { status })
    await loadDoctors()
  } catch (err:any) {
    alert("Không thể cập nhật trạng thái bác sĩ")
  }
}

watch(() => form.departmentId, async (newVal) => {
  await loadSpecialties(newVal || '')
  form.specialtyId = ''
})

onMounted(async () => {
  const res = await getUsers()
  users.value = res.data.filter((u: any) => u.role === "Doctor")
  await loadDepartments()
  await loadDoctors()
})
</script>

<template>
  <div class="doctor-page">
    <div class="container">
      <!-- Header -->
      <div class="d-flex flex-wrap justify-content-between align-items-center gap-3 mb-3">
        <div>
          <h2 class="mb-1">Quản lý bác sĩ</h2>
          <p class="text-muted mb-0">Danh sách, tìm kiếm và chỉnh sửa bác sĩ.</p>
        </div>
        <div class="d-flex flex-wrap gap-2 align-items-center">
          <input v-model="searchTerm" type="search" class="form-control" style="min-width: 260px"
                 placeholder="Tìm theo mã, chuyên khoa, giấy phép" />
          <select class="form-select" v-model="filterDepartmentId" style="min-width:200px">
            <option value="">Chọn khoa</option>
            <option v-for="dep in departments" :key="dep.id" :value="dep.id.toString()">
              {{ dep.name }}
            </option>
          </select>
          <button class="btn btn-primary" @click="openCreate">+ Thêm bác sĩ</button>
        </div>
      </div>

      <!-- Table -->
      <div class="card">
        <div class="table-responsive">
          <table class="doctor-table table align-middle mb-0">
            <thead>
              <tr>
                <th>Mã</th>
                <th>Tên bác sĩ</th>
                <th>Chuyên khoa</th>
                <th>Khoa</th>
                <th>Giấy phép</th>
                <th>Trạng thái</th>
                <th style="text-align:right">Hành động</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="loading">
                <td colspan="7" class="text-center py-4">Đang tải...</td>
              </tr>
              <tr v-else-if="filteredDoctors.length === 0">
                <td colspan="7" class="text-center py-4 text-muted">Không có bác sĩ nào phù hợp</td>
              </tr>
              <tr v-else v-for="d in filteredDoctors" :key="d.id">
                <td class="fw-semibold">{{ d.code }}</td>
                <td class="fw-semibold">{{ d.fullName }}</td>
                <td>{{ d.specialtyName }}</td>
                <td>{{ d.departmentName || '-' }}</td>
                <td>{{ d.licenseNumber || '-' }}</td>
                <td>
                  <select v-model="d.status" @change="updateStatus(d.id, d.status)" class="form-select form-select-sm">
                    <option value="Active">Hoạt động</option>
                    <option value="Busy">Đang khám</option>
                    <option value="Inactive">Không hoạt động</option>
                  </select>
                </td>
                <td class="text-end">
                  <button class="btn btn-sm btn-outline-info me-2" @click="$router.push(`/doctors/${d.id}`)">Chi tiết</button>
                  <button class="btn btn-sm btn-outline-primary me-2" @click="openEdit(d)">Chỉnh sửa</button>
                  <button class="btn btn-sm btn-outline-danger" @click="remove(d.id)">Xóa</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Modal -->
    <div v-if="showModal" class="modal-backdrop-custom">
      <div class="modal-modern">
        <h3>{{ editingId ? "Chỉnh sửa bác sĩ" : "Tạo bác sĩ mới" }}</h3>
        <div class="vstack gap-3">
          <div v-if="!editingId">
            <label class="form-label">Người dùng</label>
            <select class="form-select" v-model="form.userId">
              <option value="">Chọn người dùng</option>
              <option v-for="u in users" :key="u.id" :value="u.id">{{ u.username }}</option>
            </select>
          </div>

          <input class="form-control" v-model="form.fullName" placeholder="Họ và tên" />
          <input class="form-control" v-model="form.code" placeholder="Mã" />
          <input class="form-control" v-model="form.licenseNumber" placeholder="Số giấy phép" />

               <!-- chọn khoa -->
          <select class="form-select" v-model="form.departmentId">
            <option value="">Chọn khoa</option>
            <option v-for="dep in departments" :key="dep.id" :value="dep.id.toString()">
              {{ dep.name }}
            </option>
          </select>

          <!-- chọn chuyên khoa theo khoa -->
          <select class="form-select" v-model="form.specialtyId" :disabled="!form.departmentId">
            <option value="">Chọn chuyên khoa</option>
            <option v-for="s in specialties" :key="s.id" :value="s.id.toString()">
              {{ s.name }}
            </option>
          </select>

          <!-- chọn trạng thái -->
          <select class="form-select" v-model="form.status">
            <option value="Active">Hoạt động</option>
            <option value="Busy">Đang khám</option>
            <option value="Inactive">Không hoạt động</option>
          </select>

          <div class="modal-actions d-flex justify-content-end gap-2">
            <button class="btn-primary" @click="save">Lưu</button>
            <button class="btn-cancel" @click="showModal = false">Hủy</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
