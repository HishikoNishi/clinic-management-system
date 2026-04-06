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
async function updateStatus(id: string, status: string) {
  try {
    await api.put(`/doctor/${id}/status`, { status })
    await loadDoctors()
  } catch (err:any) {
    alert("Không thể cập nhật trạng thái bác sĩ")
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
    <div class="container-fluid px-4">
      <div class="d-flex justify-content-between align-items-center mb-3">
        <div>
          <h4 class="fw-bold mb-0">Quản lý bác sĩ</h4>
          <small class="text-muted">Tổng cộng: {{ filteredDoctors.length }} bác sĩ</small>
        </div>
        <div class="d-flex gap-2">
          <div class="search-wrapper">
            <i class="bi bi-search"></i>
            <input
              v-model="searchTerm"
              type="text"
              class="form-control form-control-sm search-input"
              placeholder="Tìm kiếm nhanh..."
            />
          </div>
          <button class="btn btn-sm btn-primary px-3 shadow-sm" @click="openCreate">
            <i class="bi bi-plus-lg me-1"></i> Thêm bác sĩ
          </button>
        </div>
      </div>

      <div class="card border-0 shadow-sm overflow-hidden">
        <div class="table-responsive">
          <table class="table table-hover align-middle mb-0 custom-table">
            <thead>
              <tr>
                <th width="80">Mã</th>
                <th>Họ và tên</th>
                <th>Khoa / Chuyên khoa</th>
                <th>Giấy phép</th>
                <th width="160">Trạng thái</th>
                <th width="180" class="text-end">Hành động</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="loading">
                <td colspan="6" class="text-center py-5">
                  <div class="spinner-border spinner-border-sm text-primary"></div>
                </td>
              </tr>
              <tr v-for="d in filteredDoctors" :key="d.id">
                <td><span class="code-badge">{{ d.code }}</span></td>
                <td>
                  <div class="d-flex align-items-center">
                    <div class="avatar-mini me-2">{{ d.fullName?.charAt(0) }}</div>
                    <span class="fw-bold text-dark">{{ d.fullName }}</span>
                  </div>
                </td>
                <td>
                  <div class="dept-text">{{ d.departmentName || '-' }}</div>
                  <div class="spec-text">{{ d.specialtyName || '-' }}</div>
                </td>
                <td class="text-muted small">{{ d.licenseNumber || '-' }}</td>
                <td>
                  <select
                    v-model="d.status"
                    @change="updateStatus(d.id, d.status)"
                    :class="['status-select-sm', d.status?.toLowerCase()]"
                  >
                    <option value="Active">Hoạt động</option>
                    <option value="Busy">Đang khám</option>
                    <option value="Inactive">Không hoạt động</option>
                  </select>
                </td>
                <td class="text-end">
                  <div class="btn-group-action">
                    <button class="btn-icon info" title="Chi tiết" @click="$router.push(`/doctors/${d.id}`)">
                      <i class="bi bi-eye"></i>
                    </button>
                 
                    <button class="btn-icon delete" title="Xóa" @click="remove(d.id)">
                      <i class="bi bi-trash"></i>
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <div v-if="showModal" class="modal-backdrop-custom">
      <div class="modal-modern shadow-lg">
        <div class="d-flex justify-content-between align-items-center mb-3">
          <h5 class="fw-bold mb-0">{{ editingId ? 'Cập nhật bác sĩ' : 'Thêm bác sĩ mới' }}</h5>
          <button class="btn-close" @click="showModal = false"></button>
        </div>

        <div class="vstack gap-2">
          <div v-if="!editingId" class="form-group">
            <label class="small fw-bold text-muted">Người dùng (Role: Doctor)</label>
            <select class="form-select form-select-sm" v-model="form.userId">
              <option value="">-- Chọn tài khoản --</option>
              <option v-for="u in users" :key="u.id" :value="u.id">{{ u.username }}</option>
            </select>
          </div>

          <div class="form-group">
            <label class="small fw-bold text-muted">Họ và tên</label>
            <input class="form-control form-control-sm" v-model="form.fullName" placeholder="VD: Nguyễn Văn A" />
          </div>

          <div class="form-group">
            <label class="small fw-bold text-muted">Mã bác sĩ</label>
            <input class="form-control form-control-sm" v-model="form.code" placeholder="VD: DOC001" />
          </div>

          <div class="row g-2">
            <div class="col-6">
              <label class="small fw-bold text-muted">Khoa</label>
              <select class="form-select form-select-sm" v-model="form.departmentId" @change="loadSpecialties(form.departmentId)">
                <option value="">Chọn khoa</option>
                <option v-for="d in departments" :key="d.id" :value="d.id">{{ d.name }}</option>
              </select>
            </div>
            <div class="col-6">
              <label class="small fw-bold text-muted">Chuyên khoa</label>
              <select class="form-select form-select-sm" v-model="form.specialtyId">
                <option value="">Chọn chuyên khoa</option>
                <option v-for="s in specialties" :key="s.id" :value="s.id">{{ s.name }}</option>
              </select>
            </div>
          </div>

          <div class="form-group">
            <label class="small fw-bold text-muted">Số giấy phép</label>
            <input class="form-control form-control-sm" v-model="form.licenseNumber" />
          </div>

          <div class="form-group">
            <label class="small fw-bold text-muted">Đường dẫn ảnh (URL)</label>
            <input class="form-control form-control-sm" v-model="form.avatarUrl" />
          </div>

          <div class="d-flex justify-content-end gap-2 mt-3">
            <button class="btn btn-sm btn-light px-3" @click="showModal = false">Hủy</button>
            <button class="btn btn-sm btn-primary px-4" @click="save">Lưu lại</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

