<template>
  <div class="doctor-page-container">
    <div class="container">
      <div class="page-header-box d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2 class="page-title mb-1">Quản lý chuyên khoa</h2>
        <p class="text-muted mb-0">Theo khoa, thêm/sửa/xóa chuyên khoa.</p>
        </div>
        <button class="btn-primary shadow-sm" @click="openCreate">
          <i class="bi bi-plus-lg me-1"></i> Thêm chuyên khoa
        </button>
      </div>

      <div class="card border-0 shadow-sm p-4 mb-4" style="border-radius: 24px;">
        <div class="row g-3 align-items-end">
          <div class="col-md-4">
            <label class="small fw-bold text-muted mb-2 ms-1">Lọc theo khoa</label>
            <select class="form-select status-select-edit" v-model="filterDepartment">
              <option value="">Tất cả khoa</option>
              <option v-for="d in departments" :key="d.id" :value="d.id">{{ d.name }}</option>
            </select>
          </div>
          <div class="col-md-6">
            <label class="small fw-bold text-muted mb-2 ms-1">Tìm kiếm</label>
            <div class="search-wrapper">
              <i class="bi bi-search"></i>
              <input class="form-control search-input" v-model="searchTerm" placeholder="Nhập tên chuyên khoa cần tìm..." />
            </div>
          </div>
        </div>
      </div>

      <div class="card border-0 shadow-sm overflow-hidden" style="border-radius: 20px;">
        <div class="table-responsive">
          <table class="table table-custom align-middle mb-0">
            <thead>
              <tr>
                <th class="ps-4">Tên chuyên khoa</th>
                <th>Thuộc khoa</th>
                <th width="180" class="text-end pe-4">Hành động</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="loading"><td colspan="3" class="text-center py-5 text-muted">Đang tải dữ liệu...</td></tr>
              <tr v-else-if="filteredSpecialties.length === 0">
                <td colspan="3" class="text-center py-5 text-muted">Không tìm thấy chuyên khoa nào.</td>
              </tr>
              <tr v-else v-for="s in filteredSpecialties" :key="s.id" class="hover-row">
                <td class="ps-4"><span class="fw-bold text-dark">{{ s.name }}</span></td>
                <td>
                  <span class="badge-dept">{{ s.departmentName || 'Chưa phân khoa' }}</span>
                </td>
                <td class="text-end pe-4">
                  <div class="btn-group-action">
                    <button class="btn-icon info" @click="viewDetail(s)" title="Xem bác sĩ">
                      <i class="bi bi-eye"></i>
                    </button>
                    <button class="btn-icon edit" @click="openEdit(s)" title="Sửa"><i class="bi bi-pencil-square"></i></button>
                    <button class="btn-icon delete" @click="remove(s.id)" title="Xóa"><i class="bi bi-trash"></i></button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <div v-if="showDetailModal" class="modal-backdrop-custom" @click.self="showDetailModal = false">
        <div class="modal-modern detail-modal">
          <div class="d-flex justify-content-between align-items-start mb-4">
            <div>
              <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-1">
                  <li class="breadcrumb-item text-primary fw-bold">{{ selectedSpecialty?.departmentName }}</li>
                  <li class="breadcrumb-item active text-dark">{{ selectedSpecialty?.name }}</li>
                </ol>
              </nav>
              <h3 class="fw-800 text-dark mb-0">Nhân sự trực thuộc</h3>
            </div>
            <button class="btn-close" @click="showDetailModal = false"></button>
          </div>

          <div class="doctor-scroll-area">
            <div v-if="doctorsInSpec.length > 0" class="list-group list-group-flush">
              <div v-for="doc in doctorsInSpec" :key="doc.id" class="list-group-item d-flex align-items-center py-3 border-0 px-0">
                <div class="doc-avatar">{{ doc.fullName ? doc.fullName.charAt(0) : 'D' }}</div>
                <div class="ms-3">
                  <div class="fw-bold text-dark">{{ doc.fullName }}</div>
                  <div class="small text-muted">{{ doc.userName || doc.code || 'BS' }} • {{ doc.degree || 'Chuyên gia' }}</div>
                </div>
                <span class="ms-auto badge-status active">Hoạt động</span>
              </div>
            </div>
            <div v-else class="text-center py-5">
              <i class="bi bi-person-dash fs-1 text-light mb-2 d-block"></i>
              <p class="text-muted small">Hiện chưa có bác sĩ nào được phân vào chuyên khoa này.</p>
            </div>
          </div>
          <button class="btn-cancel w-100 mt-4 shadow-sm" @click="showDetailModal = false">Đóng cửa sổ</button>
        </div>
      </div>

      <div v-if="showModal" class="modal-backdrop-custom" @click.self="closeModal">
        <div class="modal-modern">
          <h4 class="fw-800 text-primary mb-4">{{ editingId ? 'Cập nhật chuyên khoa' : 'Thêm chuyên khoa mới' }}</h4>
          <div class="vstack gap-3">
            <div class="form-group">
              <label class="small fw-bold text-muted mb-1">Khoa chủ quản</label>
              <select class="form-select status-select-edit w-100" v-model="form.departmentId">
                <option value="">-- Chọn khoa --</option>
                <option v-for="d in departments" :key="d.id" :value="d.id">{{ d.name }}</option>
              </select>
            </div>
            <div class="form-group">
              <label class="small fw-bold text-muted mb-1">Tên chuyên khoa</label>
              <input class="form-control" v-model="form.name" placeholder="Ví dụ: Chẩn đoán hình ảnh" />
            </div>
            <div class="d-flex justify-content-end gap-2 mt-3">
              <button class="btn-cancel px-4" @click="closeModal">Hủy</button>
              <button class="btn-primary px-4" @click="save">Lưu dữ liệu</button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import api from '@/services/api'

// --- State ---
const departments = ref<any[]>([])
const specialties = ref<any[]>([])
const doctorsInSpec = ref<any[]>([])
const loading = ref(false)
const showModal = ref(false)
const showDetailModal = ref(false)
const editingId = ref<string | null>(null)
const selectedSpecialty = ref<any>(null)
const searchTerm = ref('')
const filterDepartment = ref('')

const form = reactive({ name: '', departmentId: '' })

// --- Computed: Filter dữ liệu ---
const filteredSpecialties = computed(() => {
  return specialties.value.filter(s => {
    const matchDep = !filterDepartment.value || s.departmentId === filterDepartment.value
    const matchTerm = !searchTerm.value || s.name.toLowerCase().includes(searchTerm.value.toLowerCase())
    return matchDep && matchTerm
  })
})

// --- Actions ---
const loadDepartments = async () => {
  try {
    const res = await api.get('/Departments')
    departments.value = res.data ?? []
  } catch (err) { console.error("Lỗi load khoa:", err) }
}

const loadData = async () => {
  loading.value = true
  try {
    const res = await api.get('/Specialties')
    specialties.value = res.data ?? []
  } finally {
    loading.value = false
  }
}

const viewDetail = async (spec: any) => {
  selectedSpecialty.value = spec
  showDetailModal.value = true
  doctorsInSpec.value = [] // Reset danh sách cũ
  
  try {
    // Giải pháp an toàn: Lấy tất cả bác sĩ rồi lọc ở Frontend theo specialtyId
    const res = await api.get('/Doctors')
    const allDoctors = res.data ?? []
    // Lọc những bác sĩ có specialtyId trùng với id của chuyên khoa đang xem
    doctorsInSpec.value = allDoctors.filter((d: any) => d.specialtyId === spec.id)
  } catch (err) {
    console.error("Lỗi lấy danh sách bác sĩ:", err)
    doctorsInSpec.value = []
  }
}

const openCreate = () => {
  editingId.value = null; form.name = ''; form.departmentId = ''; showModal.value = true
}

const openEdit = (s: any) => {
  editingId.value = s.id; form.name = s.name; form.departmentId = s.departmentId; showModal.value = true
}

const closeModal = () => { showModal.value = false }

const save = async () => {
  if (!form.name || !form.departmentId) return alert('Vui lòng điền đủ thông tin')
  try {
    if (editingId.value) {
      await api.put(`/Specialties/${editingId.value}`, form)
    } else {
      await api.post('/Specialties', form)
    }
    showModal.value = false
    await loadData()
  } catch (err: any) {
    alert(err?.response?.data?.message || 'Lỗi lưu dữ liệu')
  }
}

const remove = async (id: string) => {
  if (!confirm('Xóa chuyên khoa này?')) return
  try {
    await api.delete(`/Specialties/${id}`)
    await loadData()
  } catch (err: any) {
    alert(err?.response?.data?.message || 'Không thể xóa')
  }
}

onMounted(async () => {
  await loadDepartments()
  await loadData()
})
</script>

<style scoped>
/* Toàn bộ hệ thống CSS Modern */
.doctor-page-container {
  padding: 30px 20px;
  background-color: #f8fafc;
  min-height: 100vh;
  font-family: 'Plus Jakarta Sans', sans-serif;
}

.page-title { font-size: 28px; font-weight: 800; color: #1e293b; letter-spacing: -1px; }

.table-custom thead th {
  background: #f1f5f9;
  font-size: 11px;
  text-transform: uppercase;
  font-weight: 700;
  color: #64748b;
  padding: 18px 15px;
  border: none;
}

.hover-row:hover { background-color: #f1f5f9; transition: 0.2s; cursor: pointer; }

.badge-dept {
  background: #e0f2fe; color: #0369a1;
  padding: 6px 14px; border-radius: 10px;
  font-size: 12px; font-weight: 700;
}

/* Icons & Buttons */
.btn-group-action { display: flex; gap: 8px; justify-content: flex-end; }
.btn-icon {
  width: 38px; height: 38px; display: flex; align-items: center; justify-content: center;
  border-radius: 12px; border: none; background: #fff; color: #64748b;
  box-shadow: 0 2px 5px rgba(0,0,0,0.05); transition: 0.2s;
}
.btn-icon.info:hover { background: #e0f2fe; color: #0369a1; }
.btn-icon.edit:hover { background: #fef9c3; color: #a16207; }
.btn-icon.delete:hover { background: #fee2e2; color: #b91c1c; }

/* Modal & Search */
.search-wrapper { position: relative; }
.search-wrapper i { position: absolute; left: 15px; top: 50%; transform: translateY(-50%); color: #94a3b8; }
.search-input { padding-left: 45px !important; border-radius: 14px; height: 48px; border: 2px solid #f1f5f9; font-weight: 600; }

.modal-backdrop-custom {
  position: fixed; inset: 0; background: rgba(15, 23, 42, 0.6);
  backdrop-filter: blur(8px); display: flex; align-items: center; justify-content: center; z-index: 2000;
}
.modal-modern {
  background: white; width: 95%; max-width: 500px; padding: 40px; border-radius: 32px;
  box-shadow: 0 25px 50px -12px rgba(0,0,0,0.5); animation: zoomIn 0.3s ease;
}
.detail-modal { max-width: 500px; }

/* Doctor List in Modal */
.doc-avatar {
  width: 42px; height: 42px; background: #3678a7; color: white;
  border-radius: 12px; display: flex; align-items: center; justify-content: center; font-weight: 800;
}
.badge-status { padding: 4px 10px; border-radius: 8px; font-size: 11px; font-weight: 700; text-transform: uppercase; }
.badge-status.active { background: #dcfce7; color: #15803d; }

.doctor-scroll-area { max-height: 400px; overflow-y: auto; padding-right: 8px; }

/* Buttons */
.btn-primary {
  background: #3678a7; color: white; border: none; border-radius: 14px;
  font-weight: 700; padding: 12px 24px; transition: 0.3s;
}
.btn-primary:hover { background: #2b6188; transform: translateY(-2px); box-shadow: 0 10px 20px rgba(54, 120, 167, 0.3); }

.btn-cancel { background: #f1f5f9; color: #64748b; border: none; border-radius: 14px; padding: 12px; font-weight: 700; }

@keyframes zoomIn { from { opacity: 0; transform: scale(0.95); } to { opacity: 1; transform: scale(1); } }

/* Custom Scrollbar */
.doctor-scroll-area::-webkit-scrollbar { width: 5px; }
.doctor-scroll-area::-webkit-scrollbar-thumb { background: #e2e8f0; border-radius: 10px; }
</style>