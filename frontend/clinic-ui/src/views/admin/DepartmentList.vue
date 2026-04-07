<template>
  <div class="doctor-page-container">
    <div class="container">
      <div class="page-header-box d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2 class="page-title mb-1">Quản lý khoa</h2>
          <p class="text-muted mb-0">Danh sách các khoa chuyên môn trong hệ thống</p>
        </div>
        <button class="btn-primary shadow-sm" @click="openCreate">
          <i class="bi bi-plus-lg me-1"></i> Thêm khoa mới
        </button>
      </div>

      <div class="card border-0 shadow-sm overflow-hidden">
        <div class="table-responsive">
          <table class="table table-custom align-middle mb-0">
            <thead>
              <tr>
                <th width="250">Tên khoa</th>
                <th>Mô tả chuyên môn</th>
                <th width="180" class="text-end">Thao tác</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="loading">
                <td colspan="3" class="text-center py-5">
                  <div class="spinner-border spinner-border-sm text-primary"></div>
                  <span class="ms-2 text-muted">Đang tải dữ liệu...</span>
                </td>
              </tr>
              
              <tr v-else-if="departments.length === 0">
                <td colspan="3" class="text-center py-5 text-muted">
                  <i class="bi bi-folder-x fs-2 d-block mb-2"></i>
                  Chưa có dữ liệu khoa nào.
                </td>
              </tr>

              <tr v-else v-for="d in departments" :key="d.id">
                <td>
                  <div class="d-flex align-items-center">
                    <span class="fw-bold text-dark">{{ d.name }}</span>
                  </div>
                </td>
                <td>
                  <span class="text-muted small">{{ d.description || 'Chưa có mô tả' }}</span>
                </td>
                <td class="text-end">
                  <div class="btn-group-action">
                    <button class="btn-icon edit" title="Chỉnh sửa" @click="openEdit(d)">
                      <i class="bi bi-pencil-square"></i>
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

      <div v-if="showModal" class="modal-backdrop-custom">
        <div class="modal-modern shadow-lg">
          <div class="d-flex justify-content-between align-items-center mb-4">
            <h4 class="fw-bold mb-0 text-primary">
              {{ editingId ? 'Cập nhật khoa' : 'Thêm khoa mới' }}
            </h4>
            <button class="btn-close" @click="closeModal"></button>
          </div>

          <div class="vstack gap-3">
            <div class="form-group">
              <label class="small fw-bold text-muted mb-1">Tên khoa <span class="text-danger">*</span></label>
              <input 
                class="form-control" 
                v-model="form.name" 
                placeholder="Ví dụ: Khoa Nội, Khoa Ngoại..." 
              />
            </div>
            
            <div class="form-group">
              <label class="small fw-bold text-muted mb-1">Mô tả chi tiết</label>
              <textarea 
                class="form-control" 
                v-model="form.description" 
                placeholder="Nhập mô tả ngắn về chức năng của khoa..." 
                rows="4"
              ></textarea>
            </div>

            <div class="d-flex justify-content-end gap-2 mt-4">
              <button class="btn-cancel px-4" @click="closeModal">Hủy bỏ</button>
              <button class="btn-primary px-4" @click="save">
                {{ editingId ? 'Lưu thay đổi' : 'Xác nhận thêm' }}
              </button>
            </div>
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
  } catch (err) {
    console.error("Lỗi load khoa:", err)
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
    alert('Vui lòng nhập tên khoa')
    return
  }
  try {
    if (editingId.value) {
      // Đã sửa thành dấu huyền (backticks) để nhận biến ID
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
    alert(err?.response?.data?.message || 'Không thể lưu thông tin khoa')
  }
}

const remove = async (id: string) => {
  if (!confirm('Bạn có chắc chắn muốn xóa khoa này không?')) return
  try {
    // Đã sửa thành dấu huyền (backticks)
    await api.delete(`/Departments/${id}`)
    await loadDepartments()
  } catch (err: any) {
    alert(err?.response?.data?.message || 'Không thể xóa (Khoa có thể đang chứa bác sĩ)')
  }
}

onMounted(loadDepartments)
</script>

<style scoped>
/* Thừa hưởng các biến và style Modern từ root */
.doctor-page-container {
  padding: 30px 50px;
  background-color: #f8fafc;
  min-height: 100vh;
  font-family: 'Plus Jakarta Sans', sans-serif;
}

.page-title {
  font-size: 26px;
  font-weight: 800;
  color: #1e293b;
}

.table-custom thead th {
  background: #f1f5f9;
  font-size: 11px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  font-weight: 700;
  color: #64748b;
  padding: 15px;
  border: none;
}

.avatar-mini {
  width: 32px;
  height: 32px;
  background: #3678a7;
  color: white;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
}

/* Nút Icon Thao Tác */
.btn-group-action {
  display: flex;
  gap: 8px;
  justify-content: flex-end;
}

.btn-icon {
  width: 34px;
  height: 34px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 10px;
  border: none;
  background: #f1f5f9;
  color: #64748b;
  transition: all 0.2s;
}

.btn-icon.edit:hover { background: #e0f2fe; color: #0369a1; }
.btn-icon.delete:hover { background: #fee2e2; color: #b91c1c; }

/* Modal Modern */
.modal-backdrop-custom {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.5);
  backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 2000;
}

.modal-modern {
  background: white;
  width: 100%;
  max-width: 480px;
  padding: 35px;
  border-radius: 28px;
  animation: modalIn 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
}

@keyframes modalIn {
  from { opacity: 0; transform: scale(0.95) translateY(10px); }
  to { opacity: 1; transform: scale(1) translateY(0); }
}

.form-control {
  border: 2px solid #f1f5f9;
  background: #f8fafc;
  border-radius: 12px;
  padding: 12px 15px;
  font-weight: 600;
}

.form-control:focus {
  border-color: #3678a7;
  background: white;
  box-shadow: none;
}

.btn-primary {
  background: #3678a7;
  color: white;
  border: none;
  border-radius: 12px;
  font-weight: 700;
  padding: 10px 25px;
  transition: all 0.25s;
}

.btn-primary:hover {
  background: #2b6188;
  transform: translateY(-2px);
  box-shadow: 0 8px 15px rgba(54, 120, 167, 0.2);
}

.btn-cancel {
  background: #f1f5f9;
  border: none;
  border-radius: 12px;
  font-weight: 700;
  color: #64748b;
}
</style>