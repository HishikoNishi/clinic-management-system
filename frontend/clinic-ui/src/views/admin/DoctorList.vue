<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { doctorService, type Doctor, type CreateDoctorDto } from '@/services/doctorService'
import '@/styles/layouts/doctor.css'
import { getUsers } from '@/services/userService'

const users = ref<any[]>([])
const doctors = ref<Doctor[]>([])
const loading = ref(false)
const showModal = ref(false)
const editingId = ref<string | null>(null)

/* ========================= */
/* ======= FORM DATA ======= */
/* ========================= */

const form = reactive({
  userId: '',
  fullName: '',
  code: '',
  specialty: '',
  licenseNumber: ''
})

/* ========================= */

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
      await doctorService.create(form)
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
  if (confirm("Bạn có chắc chắn muốn xóa không?")) {
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
      <div class="header">
        <h2>Quản lý bác sĩ</h2>
        <button class="btn-primary" @click="openCreate">
          + Thêm bác sĩ
        </button>
      </div>

      <div class="card">
        <table class="doctor-table">
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
            <tr v-for="d in doctors" :key="d.id">
              <td>{{ d.code }}</td>
              <td>{{ d.specialty }}</td>
              <td>{{ d.licenseNumber || '-' }}</td>
              <td>
                <span class="badge-active">
                  Hoạt động
                </span>
              </td>
              <td class="actions">
                <button class="btn-edit" @click="openEdit(d)">
                  Chỉnh sửa
                </button>
                <button class="btn-delete" @click="remove(d.id)">
                  Xóa
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- MODAL -->
      <div v-if="showModal" class="modal-backdrop-custom">
        <div class="modal-modern">
        <h3>{{ editingId ? "Chỉnh sửa bác sĩ" : "Tạo bác sĩ mới" }}</h3>

        <div v-if="!editingId">
          <label>Người dùng</label>
          <select v-model="form.userId">
            <option disabled value="">Chọn người dùng</option>
            <option v-for="u in users" :key="u.id" :value="u.id">
              {{ u.username }}
            </option>
          </select>
        </div>
        <input v-model="form.fullName" placeholder="Họ và tên" />
        <input v-model="form.code" placeholder="Mã" />
        <input v-model="form.specialty" placeholder="Chuyên khoa" />
        <input v-model="form.licenseNumber" placeholder="Số giấy phép" />

        <div class="modal-actions">
          <button class="btn-primary" @click="save">Lưu</button>
          <button class="btn-cancel" @click="showModal = false">Hủy</button>
        </div>
      </div>
    </div>
  </div>
</template>
