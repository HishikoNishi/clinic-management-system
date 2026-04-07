<script setup lang="ts">
import { ref, onMounted } from 'vue'
import api from '@/services/api'

const staff = ref<any>(null)
const loading = ref(false)
const error = ref<string | null>(null)
const showEdit = ref(false)
const toast = ref<string | null>(null)
const saving = ref(false)
const uploadBusy = ref(false)
const fileInput = ref<HTMLInputElement | null>(null)

const editForm = ref<any>({
  code: '',
  fullName: '',
  isActive: true,
  avatarUrl: ''
})

onMounted(async () => {
  await loadProfile()
})

async function loadProfile() {
  loading.value = true
  try {
    const res = await api.get('/Staffs/profile')
    staff.value = res.data
    editForm.value = {
      code: staff.value.code,
      fullName: staff.value.fullName,
      isActive: staff.value.isActive,
      avatarUrl: staff.value.avatarUrl || ''
    }
  } catch (err:any) {
    error.value = err.response?.data?.message || 'Không thể tải hồ sơ'
  } finally {
    loading.value = false
  }
}

async function saveProfile() {
  try {
    saving.value = true
    await api.put('/Staffs/profile', editForm.value, {
      headers: { 'Content-Type': 'application/json' }
    })
    showEdit.value = false
    await loadProfile()
    toast.value = 'Cập nhật thành công'
    window.setTimeout(() => (toast.value = null), 2500)
  } catch (err:any) {
    error.value = err.response?.data?.message || 'Không thể cập nhật hồ sơ'
    window.setTimeout(() => (error.value = null), 3500)
  } finally {
    saving.value = false
  }
}

const pickFile = () => fileInput.value?.click()

const handleFileChange = async (e: Event) => {
  const target = e.target as HTMLInputElement
  if (!target.files || !target.files.length || !staff.value) return
  const file = target.files[0]
  if (!file) return
  const formData = new FormData()
  formData.append('file', file)
  try {
    uploadBusy.value = true
    const res = await api.post(`/Staffs/${staff.value.id}/avatar`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    const url = res.data?.url
    if (url) {
      editForm.value.avatarUrl = url
      staff.value.avatarUrl = url
      toast.value = 'Cập nhật ảnh thành công'
      setTimeout(() => (toast.value = null), 2000)
    }
  } catch (err:any) {
    error.value = err.response?.data?.message || 'Tải ảnh thất bại'
    setTimeout(() => (error.value = null), 2500)
  } finally {
    uploadBusy.value = false
    target.value = ''
  }
}
</script>

<template>
  <div class="profile-card page">
    <div class="page-header">
      <div>
        <div class="page-eyebrow">Staff</div>
        <h3 class="page-title">Thông tin nhân viên</h3>
        <p class="page-subtitle">Xem và cập nhật hồ sơ cá nhân.</p>
      </div>
      <div>
        <button v-if="staff && !loading" class="btn btn-primary" @click="showEdit = true">
          <i class="bi bi-pencil-square me-1"></i>
          Chỉnh sửa
        </button>
      </div>
    </div>

    <div v-if="loading">Đang tải...</div>
    <div v-else-if="error" class="alert alert-danger">{{ error }}</div>
    <div v-else-if="staff">
      <div v-if="toast" class="alert alert-success py-2 mb-3">{{ toast }}</div>

      <div class="d-flex align-items-start gap-3">
        <div class="text-center">
          <img :src="staff.avatarUrl || '/default-avatar.png'" class="avatar" alt="Avatar" />
          <div class="mt-2">
            <button class="btn btn-outline-secondary btn-sm" type="button" :disabled="uploadBusy" @click="pickFile">
              <span v-if="uploadBusy" class="spinner-border spinner-border-sm me-1"></span>
              Đổi ảnh
            </button>
            <input ref="fileInput" type="file" accept="image/*" class="d-none" @change="handleFileChange" />
          </div>
        </div>
        <div class="profile-info">
          <p><strong>Mã:</strong> {{ staff.code }}</p>
          <p><strong>Họ tên:</strong> {{ staff.fullName }}</p>
          <p><strong>Tài khoản:</strong> {{ staff.username }}</p>
          <p><strong>Trạng thái:</strong> {{ staff.isActive ? 'Hoạt động' : 'Ngưng' }}</p>
          <p><strong>Ngày tạo:</strong> {{ new Date(staff.createdAt).toLocaleDateString() }}</p>
        </div>
      </div>
    </div>

    <!-- Modal chỉnh sửa -->
    <div v-if="showEdit" class="modal-backdrop">
      <div class="modal-box">
        <h4>Cập nhật hồ sơ nhân viên</h4>
        <input v-model="editForm.fullName" class="form-control mb-2" placeholder="Họ tên" />
        <input v-model="editForm.code" class="form-control mb-2" placeholder="Mã nhân viên" />
        <select v-model="editForm.isActive" class="form-select mb-2">
          <option :value="true">Hoạt động</option>
          <option :value="false">Ngưng</option>
        </select>
        <div class="modal-actions">
          <button class="btn btn-primary" :disabled="saving" @click="saveProfile">
            <span v-if="saving" class="spinner-border spinner-border-sm me-1" aria-hidden="true"></span>
            Lưu
          </button>
          <button class="btn btn-secondary" @click="showEdit = false">Hủy</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style src="@/styles/layouts/staff-profile.css"></style>
