<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue'
import { doctorService, type Doctor } from '@/services/doctorService'
import '@/styles/layouts/doctor.css'
import { getUsers } from '@/services/userService'
import axios from 'axios'

const api = axios.create({ baseURL: 'https://localhost:7235/api' })

// danh sách khoa
const departments = ref<any[]>([])
const loadDepartments = async () => {
  const res = await api.get('/departments')
  departments.value = res.data
}

// danh sách người dùng, bác sĩ
const users = ref<any[]>([])
const doctors = ref<Doctor[]>([])
const loading = ref(false)
const showModal = ref(false)
const editingId = ref<string | null>(null)
const searchTerm = ref('')
const filterDepartmentId = ref<string>('')   // filter riêng

// form tạo/chỉnh sửa bác sĩ
const form = reactive({
  userId: '',
  fullName: '',
  code: '',
  specialty: '',
  licenseNumber: '',
  departmentId: '' as string | null
})

const filteredDoctors = computed(() => {
  const term = searchTerm.value.trim().toLowerCase()
  return doctors.value.filter((d) => {
    const text = `${d.code} ${d.specialty} ${d.licenseNumber || ''}`.toLowerCase()
    const matchText = !term || text.includes(term)
    const matchDept = !filterDepartmentId.value || d.departmentId?.toString() === filterDepartmentId.value
    return matchText && matchDept
  })
})

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
  form.specialty = ''
  form.licenseNumber = ''
  form.departmentId = ''
  showModal.value = true
}

function openEdit(doctor: Doctor) {
  editingId.value = doctor.id
  form.userId = doctor.userId
  form.fullName = doctor.fullName || ''
  form.code = doctor.code
  form.specialty = doctor.specialty
  form.licenseNumber = doctor.licenseNumber || ''
  form.departmentId = doctor.departmentId?.toString() || ''
  showModal.value = true
}

async function save() {
  try {
    if (!form.departmentId) {
      alert("Vui lòng chọn khoa")
      return
    }

    if (editingId.value) {
      await doctorService.update(editingId.value, {
        code: form.code,
        fullName: form.fullName,
        specialty: form.specialty,
        licenseNumber: form.licenseNumber,
        departmentId: form.departmentId,
        status: 1
      })
    } else {
      await doctorService.create({
        userId: form.userId,
        fullName: form.fullName,
        code: form.code,
        specialty: form.specialty,
        licenseNumber: form.licenseNumber,
        departmentId: form.departmentId
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
            placeholder="Tìm theo mã, chuyên khoa, giấy phép"
          />
          <!-- filter theo khoa -->
          <select class="form-select" v-model="filterDepartmentId" style="min-width:200px">
            <option value="">Chọn khoa</option>
            <option v-for="dep in departments" :key="dep.id" :value="dep.id.toString()">
              {{ dep.name }}
            </option>
          </select>

          <button class="btn btn-primary" @click="openCreate">+ Thêm bác sĩ</button>
        </div>
      </div>

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
                <td colspan="6" class="text-center py-4">Đang tải...</td>
              </tr>
              <tr v-else-if="filteredDoctors.length === 0">
                <td colspan="6" class="text-center py-4 text-muted">
                  Không có bác sĩ nào phù hợp
                </td>
              </tr>
              <tr v-else v-for="d in filteredDoctors" :key="d.id">
                <td class="fw-semibold">{{ d.code }}</td>
                 <td class="fw-semibold">{{ d.fullName }}</td>
                <td>{{ d.specialty }}</td>
                <td>{{ d.departmentName || '-' }}</td>
                <td>{{ d.licenseNumber || '-' }}</td>
                <td>
                  <span class="badge bg-success-subtle text-success px-3 py-2">
                    Hoạt động
                  </span>
                </td>
                <td class="text-end">
                    <button class="btn btn-sm btn-outline-info me-2" @click="$router.push(`/doctors/${d.id}`)">
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

    <!-- Modal tạo/chỉnh sửa -->
    <div v-if="showModal" class="modal-backdrop-custom">
      <div class="modal-modern">
        <h3>{{ editingId ? "Chỉnh sửa bác sĩ" : "Tạo bác sĩ mới" }}</h3>

        <div class="vstack gap-3">
          <div v-if="!editingId">
            <label class="form-label">Người dùng</label>
            <select class="form-select" v-model="form.userId">
              <option value="">Chọn người dùng</option>
              <option v-for="u in users" :key="u.id" :value="u.id">
                {{ u.username }}
              </option>
            </select>
          </div>

          <input class="form-control" v-model="form.fullName" placeholder="Họ và tên" />
          <input class="form-control" v-model="form.code" placeholder="Mã" />
          <input class="form-control" v-model="form.specialty" placeholder="Chuyên khoa" />
          <input class="form-control" v-model="form.licenseNumber" placeholder="Số giấy phép" />

          <!-- chọn khoa trong form -->
          <select class="form-select" v-model="form.departmentId">
            <option value="">Chọn khoa</option>
            <option v-for="dep in departments" :key="dep.id" :value="dep.id.toString()">
              {{ dep.name }}
            </option>
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
