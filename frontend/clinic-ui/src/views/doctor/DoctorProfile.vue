<script setup lang="ts">
import { ref, onMounted } from "vue"
import api from "@/services/api"

const doctor = ref<any>(null)
const loading = ref(false)
const error = ref<string | null>(null)
const showModal = ref(false)
const form = ref<any>({})
const toast = ref<string | null>(null)
const saving = ref(false)
const uploadBusy = ref(false)
const fileInput = ref<HTMLInputElement | null>(null)

const loadProfile = async () => {
  loading.value = true
  error.value = null
  try {
    const res = await api.get("/Doctor/profile")
    doctor.value = res.data
    form.value = { ...doctor.value }
  } catch (err: any) {
    error.value = err.response?.data?.message || "Không tải được profile"
  } finally {
    loading.value = false
  }
}

const saveProfile = async () => {
  try {
    saving.value = true
    await api.put("/Doctor/profile", form.value)
    showModal.value = false
    await loadProfile()
    toast.value = "Cập nhật thành công"
    window.setTimeout(() => (toast.value = null), 2500)
  } catch (err: any) {
    error.value = err.response?.data?.message || "Không thể cập nhật profile"
    window.setTimeout(() => (error.value = null), 3500)
  } finally {
    saving.value = false
  }
}

const pickFile = () => fileInput.value?.click()

const handleFileChange = async (e: Event) => {
  const target = e.target as HTMLInputElement
  if (!target.files || !target.files.length || !doctor.value) return
  const file = target.files[0]
  const formData = new FormData()
  formData.append("file", file)
  try {
    uploadBusy.value = true
    const res = await api.post(`/Doctor/${doctor.value.id}/avatar`, formData, {
      headers: { "Content-Type": "multipart/form-data" }
    })
    const url = res.data?.url
    if (url) {
      form.value.avatarUrl = url
      doctor.value.avatarUrl = url
      toast.value = "Cập nhật ảnh thành công"
      setTimeout(() => (toast.value = null), 2000)
    }
  } catch (err: any) {
    error.value = err.response?.data?.message || "Tải ảnh thất bại"
    setTimeout(() => (error.value = null), 2500)
  } finally {
    uploadBusy.value = false
    target.value = ""
  }
}

onMounted(loadProfile)
</script>

<template>
  <div class="profile-card page">
    <div class="page-header">
      <div>
        <div class="page-eyebrow">Doctor</div>
        <h3 class="page-title">Thông tin bác sĩ</h3>
        <p class="page-subtitle">Xem và cập nhật hồ sơ cá nhân.</p>
      </div>
      <div>
        <button v-if="doctor && !loading" class="btn btn-primary" @click="showModal = true">
          <i class="bi bi-pencil-square me-1"></i>
          Chỉnh sửa
        </button>
      </div>
    </div>
    <div v-if="loading">Đang tải...</div>
    <div v-else-if="error" class="alert alert-danger">{{ error }}</div>
    <div v-else-if="doctor">
      <div v-if="toast" class="alert alert-success py-2 mb-3">{{ toast }}</div>
      <div class="d-flex align-items-start gap-3">
        <div class="text-center">
          <img :src="doctor.avatarUrl || '/default-avatar.png'" class="avatar" alt="Avatar" />
          <div class="mt-2">
            <button class="btn btn-outline-secondary btn-sm" type="button" :disabled="uploadBusy" @click="pickFile">
              <span v-if="uploadBusy" class="spinner-border spinner-border-sm me-1"></span>
              Đổi ảnh
            </button>
            <input ref="fileInput" type="file" accept="image/*" class="d-none" @change="handleFileChange" />
          </div>
        </div>
        <div class="profile-info">
          <p><strong>Tên:</strong> {{ doctor.fullName }}</p>
          <p><strong>Mã:</strong> {{ doctor.code }}</p>
          <p><strong>Khoa:</strong> {{ doctor.departmentName }}</p>
          <p><strong>Chuyên khoa:</strong> {{ doctor.specialtyName }}</p>
          <p><strong>Số giấy phép:</strong> {{ doctor.licenseNumber }}</p>
        </div>
      </div>
    </div>

    <!-- Modal -->
    <div v-if="showModal" class="modal-backdrop">
      <div class="modal-box">
        <h4>Chỉnh sửa hồ sơ</h4>
        <input v-model="form.fullName" class="form-control mb-2" placeholder="Tên bác sĩ" />
        <input v-model="form.code" class="form-control mb-2" placeholder="Mã bác sĩ" />
        <input v-model="form.licenseNumber" class="form-control mb-2" placeholder="Số giấy phép" />
        <div class="modal-actions">
          <button class="btn btn-primary" :disabled="saving" @click="saveProfile">
            <span v-if="saving" class="spinner-border spinner-border-sm me-1" aria-hidden="true"></span>
            Lưu
          </button>
          <button class="btn btn-secondary" @click="showModal = false">Hủy</button>
        </div>
      </div>
    </div>
  </div>
</template>
<style src="@/styles/layouts/doctor-profile.css"></style>
