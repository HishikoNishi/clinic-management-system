<template>
  <div class="doctor-page-container">
    <div class="container" v-if="doctor">
      <div class="page-header-box d-flex justify-content-between align-items-center mb-4">
        <div>
          <h1 class="page-title mb-1">Hồ sơ của tôi</h1>
          <p class="text-muted mb-0">Quản lý thông tin cá nhân và trạng thái làm việc</p>
        </div>
        <div class="header-actions">
          <button v-if="!isEditing" class="btn-primary-modern" @click="openEdit">
            Chỉnh sửa hồ sơ
          </button>
          <div v-else class="d-flex gap-2">
            <button class="btn-update-modern" @click="saveProfile">Lưu thay đổi</button>
            <button class="btn-cancel-modern" @click="isEditing = false">Hủy bỏ</button>
          </div>
        </div>
      </div>

      <div class="doctor-info-card" :class="{ 'is-editing': isEditing }">
        
        <div class="doctor-profile-section">
          <div class="avatar-wrapper">
            <img :src="(isEditing ? form.avatarUrl : doctor.avatarUrl) || '/default-avatar.png'" class="main-avatar" />
          </div>
          
          <div v-if="!isEditing" class="profile-summary mt-3">
            <h2 class="doctor-name-display">{{ doctor.fullName }}</h2>
            <span :class="['status-badge-lg', getStatusClass(doctor.status)]">
              {{ getStatusLabel(doctor.status) }}
            </span>
          </div>
          <div v-else class="form-group mt-4 w-100 px-3">
            <label>Link ảnh đại diện (URL)</label>
            <input v-model="form.avatarUrl" placeholder="https://..." />
          </div>
        </div>

        <div class="doctor-details-container">
 <div class="doctor-details-grid">
  <div class="form-group">
    <label>Họ và tên</label>
    <input v-model="form.fullName" :disabled="!isEditing" :class="{ 'disabled-view': !isEditing }" />
  </div>

  <div class="form-group">
    <label>Mã bác sĩ</label>
    <input v-model="form.code" :disabled="!isEditing" :class="{ 'disabled-view': !isEditing }" />
  </div>

  <div class="form-group">
    <label>Khoa</label>
    <input :value="doctor.departmentName" disabled class="disabled-view" />
  </div>

  <div class="form-group">
    <label>Số giấy phép hành nghề</label>
    <input v-model="form.licenseNumber" :disabled="!isEditing" :class="{ 'disabled-view': !isEditing }" />
  </div>

  <div class="form-group full-row">
    <label>Chuyên khoa</label>
    <input :value="doctor.specialtyName" disabled class="disabled-view" />
  </div>

  <div class="form-group full-row">
    <label>Trạng thái hoạt động</label>
    <select v-model="form.status" :disabled="!isEditing" :class="{ 'disabled-view': !isEditing }">
      <option value="Active">Hoạt động (Sẵn sàng)</option>
      <option value="Busy">Đang bận (Trong ca khám)</option>
      <option value="Inactive">Nghỉ (Không làm việc)</option>
    </select>
  </div>
</div>
          
          <div class="profile-footer-note">
            <i class="bi bi-shield-check me-2"></i>Thông tin khoa và chuyên khoa chỉ được quản lý bởi quản trị viên(admin) 
          </div>
        </div>
      </div>
    </div>

  
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue"
import api from "@/services/api"

const doctor = ref<any>(null)
const isEditing = ref(false)
const form = ref<any>({})

// 1. Tải thông tin Profile
const loadProfile = async () => {
  try {
    const res = await api.get("/Doctor/profile")
    doctor.value = res.data
    form.value = { ...res.data }
  } catch (err) {
    console.error("Lỗi load profile:", err)
  }
}

const openEdit = () => {
  form.value = { ...doctor.value }
  isEditing.value = true
}

// 2. Lưu Profile
const saveProfile = async () => {
  try {
    await api.put("/Doctor/profile", form.value)

   
    const doctorId = doctor.value.id || localStorage.getItem("doctorId")
    if (doctorId) {
      await api.put(`/doctor/${doctorId}/status`, {
        Status: form.value.status 
      })
    }
    
    isEditing.value = false
    await loadProfile() 
    alert("Cập nhật hồ sơ và trạng thái thành công ✅")
  } catch (err: any) {
    console.error("Lỗi lưu:", err)
    alert("Có lỗi xảy ra khi lưu thông tin")
  }
}


const getStatusLabel = (s: string) => {
  const map: any = { Active: 'Hoạt động', Busy: 'Đang bận', Inactive: 'Không hoạt động' }
  return map[s] || s
}

const getStatusClass = (s: string) => {
  if (s === 'Active') return 'bg-success-soft'
  if (s === 'Busy') return 'bg-warning-soft'
  return 'bg-secondary-soft'
}

onMounted(loadProfile)
</script>

<style src="@/styles/layouts/doctor-profile.css"></style>