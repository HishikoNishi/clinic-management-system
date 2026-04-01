<template>
  <div class="container py-3">
    <div class="d-flex justify-content-between align-items-center mb-3">
      <div>
        <h2 class="mb-1">Quản lý chuyên khoa</h2>
        <p class="text-muted mb-0">Theo khoa, thêm/sửa/xóa chuyên khoa.</p>
      </div>
      <button class="btn btn-primary" @click="openCreate">+ Thêm chuyên khoa</button>
    </div>

    <div class="card p-3 mb-3">
      <div class="row g-2 align-items-end">
        <div class="col-md-4">
          <label class="form-label">Lọc theo khoa</label>
          <select class="form-select" v-model="filterDepartment" @change="loadData">
            <option value="">Tất cả khoa</option>
            <option v-for="d in departments" :key="d.id" :value="d.id">{{ d.name }}</option>
          </select>
        </div>
        <div class="col-md-4">
          <label class="form-label">Tìm kiếm</label>
          <input class="form-control" v-model="searchTerm" placeholder="Tên chuyên khoa..." />
        </div>
      </div>
    </div>

    <div class="card">
      <div class="table-responsive">
        <table class="table align-middle mb-0">
          <thead class="table-light">
            <tr>
              <th>Tên chuyên khoa</th>
              <th>Khoa</th>
              <th class="text-end">Hành động</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
              <td colspan="4" class="text-center py-3 text-muted">Đang tải...</td>
            </tr>
            <tr v-else-if="filteredSpecialties.length === 0">
              <td colspan="4" class="text-center py-3 text-muted">Chưa có chuyên khoa.</td>
            </tr>
            <tr v-else v-for="s in filteredSpecialties" :key="s.id">
              <td class="fw-semibold">{{ s.name }}</td>
              <td>{{ s.departmentName || '-' }}</td>
              <td class="text-end">
                <button class="btn btn-sm btn-outline-primary me-2" @click="openEdit(s)">Sửa</button>
                <button class="btn btn-sm btn-outline-danger" @click="remove(s.id)">Xóa</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <div v-if="showModal" class="modal-backdrop-custom">
      <div class="modal-modern small">
        <h4 class="mb-3">{{ editingId ? 'Sửa chuyên khoa' : 'Thêm chuyên khoa' }}</h4>
        <div class="vstack gap-2">
          <select class="form-select" v-model="form.departmentId">
            <option value="">Chọn khoa</option>
            <option v-for="d in departments" :key="d.id" :value="d.id">{{ d.name }}</option>
          </select>
          <input class="form-control" v-model="form.name" placeholder="Tên chuyên khoa" />
          <div class="text-end mt-2">
            <button class="btn btn-secondary me-2" @click="closeModal">Hủy</button>
            <button class="btn btn-primary" @click="save">Lưu</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import api from '@/services/api'

interface Department {
  id: string
  name: string
}
interface Specialty {
  id: string
  name: string
  departmentId: string
  departmentName?: string
}

const departments = ref<Department[]>([])
const specialties = ref<Specialty[]>([])
const loading = ref(false)
const searchTerm = ref('')
const filterDepartment = ref<string>('')
const showModal = ref(false)
const editingId = ref<string | null>(null)
const form = reactive({
  name: '',
  departmentId: ''
})

const filteredSpecialties = computed(() => {
  const term = searchTerm.value.trim().toLowerCase()
  return specialties.value.filter(s => {
    const matchDep = !filterDepartment.value || s.departmentId === filterDepartment.value
    const matchTerm = !term || s.name.toLowerCase().includes(term)
    return matchDep && matchTerm
  })
})

const loadDepartments = async () => {
  const res = await api.get('/Departments')
  departments.value = res.data ?? []
}

const loadData = async () => {
  loading.value = true
  try {
    const url = filterDepartment.value ? `/Specialties?departmentId=${filterDepartment.value}` : '/Specialties'
    const res = await api.get(url)
    specialties.value = res.data ?? []
  } finally {
    loading.value = false
  }
}

const openCreate = () => {
  editingId.value = null
  form.name = ''
  form.departmentId = ''
  showModal.value = true
}

const openEdit = (s: Specialty) => {
  editingId.value = s.id
  form.name = s.name
  form.departmentId = s.departmentId
  showModal.value = true
}

const closeModal = () => {
  showModal.value = false
}

const save = async () => {
  if (!form.name.trim() || !form.departmentId) {
    alert('Chọn khoa và nhập tên chuyên khoa')
    return
  }
  try {
    if (editingId.value) {
      await api.put(`/Specialties/${editingId.value}`, {
        name: form.name,
        departmentId: form.departmentId
      })
    } else {
      await api.post('/Specialties', {
        name: form.name,
        departmentId: form.departmentId
      })
    }
    showModal.value = false
    await loadData()
  } catch (err: any) {
    alert(err?.response?.data?.message || 'Không thể lưu chuyên khoa')
  }
}

const remove = async (id: string) => {
  if (!confirm('Xóa chuyên khoa này?')) return
  try {
    await api.delete(`/Specialties/${id}`)
    await loadData()
  } catch (err: any) {
    alert(err?.response?.data?.message || 'Không thể xóa (có bác sĩ đang thuộc chuyên khoa này?).')
  }
}

onMounted(async () => {
  await loadDepartments()
  await loadData()
})
</script>

<style scoped>
.modal-backdrop-custom {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1050;
}
.modal-modern {
  background: #fff;
  border-radius: 10px;
  padding: 20px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.15);
  min-width: 380px;
}
</style>
