<template>
  <div class="container py-3 page">
    <div class="page-header">
      <div>
        <div class="page-eyebrow">Admin</div>
        <h2 class="page-title mb-0">Quản lý thuốc</h2>
        <p class="page-subtitle">Danh mục thuốc để bác sĩ kê đơn và gợi ý nhanh khi nhập.</p>
      </div>
      <button class="btn btn-primary" @click="openCreate">
        <i class="bi bi-plus-lg me-1"></i>
        Thêm thuốc
      </button>
    </div>

    <div class="card page-card mb-3">
      <div class="card-body">
        <div class="row g-2">
          <div class="col-md-6">
            <input
              v-model="searchText"
              class="form-control"
              placeholder="Tìm theo tên thuốc, hàm lượng, đơn vị..."
            />
          </div>
          <div class="col-md-3">
            <select v-model="statusFilter" class="form-select">
              <option value="all">Tất cả trạng thái</option>
              <option value="active">Đang hoạt động</option>
              <option value="inactive">Đã ẩn</option>
            </select>
          </div>
          <div class="col-md-3 text-md-end">
            <button class="btn btn-outline-secondary w-100 w-md-auto" @click="loadData" :disabled="loading">
              <span v-if="loading" class="spinner-border spinner-border-sm me-1"></span>
              Làm mới
            </button>
          </div>
        </div>
      </div>
    </div>

    <div class="card page-card">
      <div class="table-responsive">
        <table class="table align-middle mb-0">
          <thead class="table-light">
            <tr>
              <th>Tên thuốc</th>
              <th>Hàm lượng mặc định</th>
              <th>Đơn vị</th>
              <th>Giá</th>
              <th>Trạng thái</th>
              <th class="text-end">Hành động</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
              <td colspan="6" class="text-center py-3 text-muted">Đang tải...</td>
            </tr>
            <tr v-else-if="filteredRows.length === 0">
              <td colspan="6" class="text-center py-3 text-muted">Không có dữ liệu.</td>
            </tr>
            <tr v-else v-for="item in filteredRows" :key="item.id">
              <td class="fw-semibold">{{ item.name }}</td>
              <td>{{ item.defaultDosage || "-" }}</td>
              <td>{{ item.unit }}</td>
              <td>{{ toCurrency(item.price) }}</td>
              <td>
                <span :class="['badge', item.isActive ? 'bg-success-subtle text-success' : 'bg-secondary-subtle text-secondary']">
                  {{ item.isActive ? "Hoạt động" : "Đã ẩn" }}
                </span>
              </td>
              <td class="text-end">
                <button class="btn btn-sm btn-outline-primary me-2" @click="openEdit(item)">Sửa</button>
                <button class="btn btn-sm btn-outline-warning" @click="toggleStatus(item)" :disabled="savingId === item.id">
                  {{ item.isActive ? "Ẩn" : "Kích hoạt" }}
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <div v-if="showModal" class="modal-backdrop-custom">
      <div class="modal-modern small">
        <h4 class="mb-3">{{ editingId ? "Sửa thuốc" : "Thêm thuốc" }}</h4>
        <div class="vstack gap-2">
          <input class="form-control" v-model="form.name" placeholder="Tên thuốc" />
          <input class="form-control" v-model="form.defaultDosage" placeholder="Hàm lượng mặc định (vd: 500mg)" />
          <input class="form-control" v-model="form.unit" placeholder="Đơn vị (Viên, Chai...)" />
          <input class="form-control" v-model.number="form.price" type="number" min="0" step="500" placeholder="Giá" />
          <label class="form-check">
            <input class="form-check-input" type="checkbox" v-model="form.isActive" />
            <span class="form-check-label">Đang hoạt động</span>
          </label>
          <div class="text-end mt-2">
            <button class="btn btn-secondary me-2" @click="closeModal">Hủy</button>
            <button class="btn btn-primary" @click="save" :disabled="saving">
              <span v-if="saving" class="spinner-border spinner-border-sm me-1"></span>
              Lưu
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from "vue"
import { medicineApi, type MedicineItem } from "@/services/medicineApi"

const rows = ref<MedicineItem[]>([])
const loading = ref(false)
const saving = ref(false)
const savingId = ref<string | null>(null)
const showModal = ref(false)
const editingId = ref<string | null>(null)
const searchText = ref("")
const statusFilter = ref<"all" | "active" | "inactive">("all")

const form = reactive({
  name: "",
  defaultDosage: "",
  unit: "Viên",
  price: 0,
  isActive: true
})

const filteredRows = computed(() => {
  const query = searchText.value.trim().toLowerCase()
  return rows.value.filter((item) => {
    if (statusFilter.value === "active" && !item.isActive) return false
    if (statusFilter.value === "inactive" && item.isActive) return false
    if (!query) return true
    const mix = `${item.name} ${item.defaultDosage || ""} ${item.unit}`.toLowerCase()
    return mix.includes(query)
  })
})

const toCurrency = (val: number) => `${(val || 0).toLocaleString("vi-VN")} VND`

const resetForm = () => {
  form.name = ""
  form.defaultDosage = ""
  form.unit = "Viên"
  form.price = 0
  form.isActive = true
}

const openCreate = () => {
  editingId.value = null
  resetForm()
  showModal.value = true
}

const openEdit = (item: MedicineItem) => {
  editingId.value = item.id
  form.name = item.name
  form.defaultDosage = item.defaultDosage || ""
  form.unit = item.unit
  form.price = item.price
  form.isActive = item.isActive
  showModal.value = true
}

const closeModal = () => {
  showModal.value = false
}

const loadData = async () => {
  try {
    loading.value = true
    rows.value = await medicineApi.list(true)
  } catch (err: any) {
    alert(err?.response?.data?.message || "Không tải được danh mục thuốc")
  } finally {
    loading.value = false
  }
}

const save = async () => {
  if (!form.name.trim()) {
    alert("Tên thuốc là bắt buộc")
    return
  }
  if (!form.unit.trim()) {
    alert("Đơn vị là bắt buộc")
    return
  }
  if (form.price < 0) {
    alert("Giá phải lớn hơn hoặc bằng 0")
    return
  }

  try {
    saving.value = true
    const payload = {
      name: form.name.trim(),
      defaultDosage: form.defaultDosage.trim() || null,
      unit: form.unit.trim(),
      price: Number(form.price) || 0,
      isActive: form.isActive
    }

    if (editingId.value) {
      await medicineApi.update(editingId.value, payload)
    } else {
      await medicineApi.create(payload)
    }

    showModal.value = false
    await loadData()
  } catch (err: any) {
    alert(err?.response?.data?.message || "Không lưu được thuốc")
  } finally {
    saving.value = false
  }
}

const toggleStatus = async (item: MedicineItem) => {
  try {
    savingId.value = item.id
    await medicineApi.toggle(item.id)
    await loadData()
  } catch (err: any) {
    alert(err?.response?.data?.message || "Không đổi được trạng thái")
  } finally {
    savingId.value = null
  }
}

onMounted(loadData)
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
  min-width: 420px;
}
</style>
