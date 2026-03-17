<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue'
import { doctorService, type Doctor } from '@/services/doctorService'
import '@/styles/layouts/doctor.css'
import { getUsers } from '@/services/userService'

const users = ref<any[]>([])
const doctors = ref<Doctor[]>([])
const loading = ref(false)
const showModal = ref(false)
const editingId = ref<string | null>(null)
const searchTerm = ref('')

const form = reactive({
  userId: '',
  fullName: '',
  code: '',
  specialty: '',
  licenseNumber: ''
})

const filteredDoctors = computed(() => {
  const term = searchTerm.value.trim().toLowerCase()
  if (!term) return doctors.value
  return doctors.value.filter((d) => {
    const text = `${d.code} ${d.specialty} ${d.licenseNumber || ''}`.toLowerCase()
    return text.includes(term)
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
  showModal.value = true
}

function openEdit(doctor: Doctor) {
  editingId.value = doctor.id
  form.userId = doctor.userId
  form.fullName = (doctor as any).fullName || ''
  form.code = doctor.code
  form.specialty = doctor.specialty
  form.licenseNumber = doctor.licenseNumber || ''
  showModal.value = true
}

async function save() {
  try {
    if (editingId.value) {
      await doctorService.update(editingId.value, {
        code: form.code,
        specialty: form.specialty,
        licenseNumber: form.licenseNumber,
        status: 1
      })
    } else {
      await doctorService.create({
        userId: form.userId,
        fullName: form.fullName,
        code: form.code,
        specialty: form.specialty,
        licenseNumber: form.licenseNumber
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

    if (typeof error.response?.data === "string") {
      alert(error.response.data)
      return
    }

    alert("Something went wrong")
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
          <button class="btn btn-primary" @click="openCreate">+ Thêm bác sĩ</button>
        </div>
      </div>

      <div class="card">
        <div class="table-responsive">
          <table class="doctor-table table align-middle mb-0">
            <thead>
              <tr>
                <th>Mã</th>
                <th>Chuyên khoa</th>
                <th>Giấy phép</th>
                <th>Trạng thái</th>
                <th style="text-align:right">Hành động</th>
              </tr>
            </thead>

            <tbody>
              <tr v-if="loading">
                <td colspan="5" class="text-center py-4">Đang tải...</td>
              </tr>
              <tr v-else-if="filteredDoctors.length === 0">
                <td colspan="5" class="text-center py-4 text-muted">
                  Không có bác sĩ nào phù hợp
                </td>
              </tr>
              <tr v-else v-for="d in filteredDoctors" :key="d.id">
                <td class="fw-semibold">{{ d.code }}</td>
                <td>{{ d.specialty }}</td>
                <td>{{ d.licenseNumber || '-' }}</td>
                <td>
                  <span class="badge bg-success-subtle text-success px-3 py-2">
                    Hoạt động
                  </span>
                </td>
                <td class="text-end">
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
        <h3>{{ editingId ? "Chỉnh sửa bác sĩ" : "Tạo bác sĩ mới" }}</h3>

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
          <input class="form-control" v-model="form.specialty" placeholder="Chuyên khoa" />
          <input class="form-control" v-model="form.licenseNumber" placeholder="Số giấy phép" />

          <div class="modal-actions d-flex justify-content-end gap-2">
            <button class="btn-primary" @click="save">Lưu</button>
            <button class="btn-cancel" @click="showModal = false">Hủy</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
