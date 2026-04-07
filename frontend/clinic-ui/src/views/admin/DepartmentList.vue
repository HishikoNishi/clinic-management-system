<template>
  <div class="container py-3 page">
    <div class="page-header">
      <div>
        <div class="page-eyebrow">Admin</div>
        <h2 class="page-title mb-0">Quản lý khoa</h2>
        <p class="page-subtitle">Danh sách khoa, thêm/sửa/xóa.</p>
      </div>
      <button class="btn btn-primary" @click="openCreate">
        <i class="bi bi-plus-lg me-1"></i>
        Thêm khoa
      </button>
    </div>

    <div class="card page-card">
      <div class="table-responsive">
        <table class="table align-middle mb-0">
          <thead class="table-light">
            <tr>
              <th>Tên khoa</th>
              <th>Mô tả</th>
              <th class="text-end">Hành động</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
              <td colspan="3" class="text-center py-3 text-muted">Đang tải...</td>
            </tr>
            <tr v-else-if="departments.length === 0">
              <td colspan="3" class="text-center py-3 text-muted">Chưa có khoa.</td>
            </tr>
            <tr v-else v-for="d in departments" :key="d.id">
              <td class="fw-semibold">{{ d.name }}</td>
              <td>{{ d.description || '-' }}</td>
              <td class="text-end">
                <button class="btn btn-sm btn-outline-primary me-2" @click="openEdit(d)">Sửa</button>
                <button class="btn btn-sm btn-outline-danger" @click="remove(d.id)">Xóa</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <div v-if="showModal" class="modal-backdrop-custom">
      <div class="modal-modern small">
        <h4 class="mb-3">{{ editingId ? 'Sửa khoa' : 'Thêm khoa' }}</h4>
        <div class="vstack gap-2">
          <input class="form-control" v-model="form.name" placeholder="Tên khoa" />
          <textarea class="form-control" v-model="form.description" placeholder="Mô tả" rows="3"></textarea>
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
import { ref, reactive, onMounted } from 'vue'
import api from '@/services/api'

interface Department {
  id: string
  name: string
  description?: string
}

const departments = ref<Department[]>([])
const loading = ref(false)
const showModal = ref(false)
const editingId = ref<string | null>(null)
const form = reactive({
  name: '',
  description: ''
})

const loadDepartments = async () => {
  loading.value = true
  try {
    const res = await api.get('/Departments')
    departments.value = res.data ?? []
  } finally {
    loading.value = false
  }
}

const openCreate = () => {
  editingId.value = null
  form.name = ''
  form.description = ''
  showModal.value = true
}

const openEdit = (dep: Department) => {
  editingId.value = dep.id
  form.name = dep.name
  form.description = dep.description || ''
  showModal.value = true
}

const closeModal = () => {
  showModal.value = false
}

const save = async () => {
  if (!form.name.trim()) {
    alert('Tên khoa là bắt buộc')
    return
  }
  try {
    if (editingId.value) {
      await api.put(`/Departments/${editingId.value}`, {
        name: form.name,
        description: form.description
      })
    } else {
      await api.post('/Departments', {
        name: form.name,
        description: form.description
      })
    }
    showModal.value = false
    await loadDepartments()
  } catch (err: any) {
    alert(err?.response?.data?.message || 'Không thể lưu khoa')
  }
}

const remove = async (id: string) => {
  if (!confirm('Xóa khoa này?')) return
  try {
    await api.delete(`/Departments/${id}`)
    await loadDepartments()
  } catch (err: any) {
    alert(err?.response?.data?.message || 'Không thể xóa khoa (có bác sĩ đang thuộc khoa này?).')
  }
}

onMounted(loadDepartments)
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
