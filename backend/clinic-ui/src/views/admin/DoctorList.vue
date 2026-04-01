<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { doctorService, type Doctor } from '@/services/doctorService'
import { getUsers } from '@/services/userService'
import api from '@/services/api'
import '@/styles/layouts/doctor.css'

const users = ref<any[]>([])
const doctors = ref<Doctor[]>([])
const departments = ref<any[]>([])
const specialties = ref<any[]>([])
const loading = ref(false)
const showModal = ref(false)
const editingId = ref<string | null>(null)
const searchTerm = ref('')

const form = reactive({
  userId: '',
  fullName: '',
  code: '',
  departmentId: '',
  specialtyId: '',
  licenseNumber: '',
  avatarUrl: ''
})

const filteredDoctors = computed(() => {
  const term = searchTerm.value.trim().toLowerCase()
  if (!term) return doctors.value
  return doctors.value.filter((d) => {
    const text = `${d.code} ${d.fullName ?? ''} ${d.specialtyName ?? ''} ${d.departmentName ?? ''} ${d.licenseNumber ?? ''}`.toLowerCase()
    return text.includes(term)
  })
})

async function loadDoctors() {
  loading.value = true
  doctors.value = await doctorService.getAll()
  loading.value = false
}

async function loadDepartments() {
  const res = await api.get('/Departments')
  departments.value = res.data ?? []
}

async function loadSpecialties(departmentId: string) {
  specialties.value = []
  form.specialtyId = ''
  if (!departmentId) return
  try {
    const res = await api.get(`/Doctor/departments/${departmentId}/specialties`)
    specialties.value = res.data ?? []
  } catch {
    specialties.value = []
  }
}

function openCreate() {
  editingId.value = null
  form.userId = ''
  form.fullName = ''
  form.code = ''
  form.departmentId = ''
  form.specialtyId = ''
  form.licenseNumber = ''
  form.avatarUrl = ''
  specialties.value = []
  showModal.value = true
}

function openEdit(doctor: Doctor) {
  editingId.value = doctor.id
  form.userId = doctor.userId
  form.fullName = doctor.fullName || ''
  form.code = doctor.code
  form.departmentId = doctor.departmentId
  form.specialtyId = doctor.specialtyId
  form.licenseNumber = doctor.licenseNumber || ''
  form.avatarUrl = doctor.avatarUrl || ''
  showModal.value = true
  if (form.departmentId) {
    loadSpecialties(form.departmentId)
  }
}

async function save() {
  try {
    if (editingId.value) {
      await doctorService.update(editingId.value, {
        code: form.code,
        fullName: form.fullName,
        specialtyId: form.specialtyId,
        licenseNumber: form.licenseNumber,
        departmentId: form.departmentId,
        status: 'Active',
        avatarUrl: form.avatarUrl
      })
    } else {
      await doctorService.create({
        userId: form.userId,
        fullName: form.fullName,
        code: form.code,
        specialtyId: form.specialtyId,
        departmentId: form.departmentId,
        licenseNumber: form.licenseNumber,
        avatarUrl: form.avatarUrl
      })
    }

    showModal.value = false
    await loadDoctors()
  } catch (error: any) {
    if (error.response?.data?.errors) {
      const errors = Object.values(error.response.data.errors)
        .flat()
        .join("\n")
      alert(errors)
      return
    }

    if (error.response?.data?.message) {
      alert(error.response.data.message)
      return
    }

    if (typeof error.response?.data === 'string') {
      alert(error.response.data)
      return
    }

    alert('Something went wrong')
  }
}

async function remove(id: string) {
  if (confirm('Bạn có chắc chắn muốn xóa bác sĩ này?')) {
    await doctorService.delete(id)
    await loadDoctors()
  }
}

onMounted(async () => {
  const res = await getUsers()
  users.value = res.data.filter((u: any) => u.role === 'Doctor')
  await loadDepartments()
  await loadDoctors()
})
</script>

<template>
  <div class="doctor-page">
    <div class="container">
      <div class="d-flex flex-wrap justify-content-between align-items-center gap-3 mb-3">
        <div>
          <h2 class="mb-1">Quản lý bác sĩ</h2>
          <p class="text-muted mb-0">Danh sách, tìm kiếm và chỉnh sửa bác sĩ.</p>
        </div>
        <div class="d-flex flex-wrap gap-2 align-items-center">
          <input
            v-model="searchTerm"
            type="search"
            class="form-control"
            style="min-width: 260px"
            placeholder="Tìm theo mã, chuyên khoa, khoa, giấy phép"
          />
          <button class="btn btn-primary" @click="openCreate">+ Thêm bác sĩ</button>
        </div>
      </div>

      <div class="card">
        <div class="table-responsive">
          <table class="doctor-table table align-middle mb-0">
            <thead>
              <tr>
                <th>Mã</th>
                <th>Họ tên</th>
                <th>Khoa</th>
                <th>Chuyên khoa</th>
                <th>Giấy phép</th>
                <th style="text-align:right">Hành động</th>
              </tr>
            </thead>

            <tbody>
              <tr v-if="loading">
                <td colspan="6" class="text-center py-4">Đang tải...</td>
              </tr>
              <tr v-else-if="filteredDoctors.length === 0">
                <td colspan="6" class="text-center py-4 text-muted">
                  Không có bác sĩ nào phù hợp
                </td>
              </tr>
              <tr v-else v-for="d in filteredDoctors" :key="d.id">
                <td class="fw-semibold">{{ d.code }}</td>
                <td>{{ d.fullName }}</td>
                <td>{{ d.departmentName || '-' }}</td>
                <td>{{ d.specialtyName || '-' }}</td>
                <td>{{ d.licenseNumber || '-' }}</td>
                <td class="text-end">
                  <button class="btn btn-sm btn-info me-2" @click="$router.push(`/doctors/${d.id}`)">
    Chi tiết
  </button>
                  <button class="btn btn-sm btn-outline-primary me-2" @click="openEdit(d)">
                    Chỉnh sửa
                  </button>
                  <button class="btn btn-sm btn-outline-danger" @click="remove(d.id)">
                    Xóa
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <div v-if="showModal" class="modal-backdrop-custom">
      <div class="modal-modern">
        <h3>{{ editingId ? 'Chỉnh sửa bác sĩ' : 'Tạo bác sĩ mới' }}</h3>

        <div class="vstack gap-3">
          <div v-if="!editingId">
            <label class="form-label">Người dùng</label>
            <select class="form-select" v-model="form.userId">
              <option disabled value="">Chọn người dùng</option>
              <option v-for="u in users" :key="u.id" :value="u.id">
                {{ u.username }}
              </option>
            </select>
          </div>

          <input class="form-control" v-model="form.fullName" placeholder="Họ và tên" />
          <input class="form-control" v-model="form.code" placeholder="Mã" />

          <div class="row g-2">
            <div class="col-6">
              <label class="form-label">Khoa</label>
              <select class="form-select" v-model="form.departmentId" @change="loadSpecialties(form.departmentId)">
                <option value="">Chọn khoa</option>
                <option v-for="d in departments" :key="d.id" :value="d.id">{{ d.name }}</option>
              </select>
            </div>
            <div class="col-6">
              <label class="form-label">Chuyên khoa</label>
              <select class="form-select" v-model="form.specialtyId">
                <option value="">Chọn chuyên khoa</option>
                <option v-for="s in specialties" :key="s.id" :value="s.id">{{ s.name }}</option>
              </select>
            </div>
          </div>

          <input class="form-control" v-model="form.licenseNumber" placeholder="Số giấy phép" />
          <input class="form-control" v-model="form.avatarUrl" placeholder="Avatar URL (tùy chọn)" />

          <div class="d-flex justify-content-end gap-2">
            <button class="btn btn-secondary" @click="showModal = false">Hủy</button>
            <button class="btn btn-primary" @click="save">Lưu</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
