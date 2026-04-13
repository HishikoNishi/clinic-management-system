<template>
  <div class="staff-page-container">
    <div class="container" v-if="staff">
      <div class="page-header-box d-flex justify-content-between align-items-center mb-4">
        <div>
          <h1 class="page-title mb-1">Hồ sơ nhân viên</h1>
          <p class="text-muted mb-0">Quản lý thông tin tài khoản và trạng thái công tác</p>
        </div>
        <div class="header-actions">
          <button v-if="!showEdit" class="btn-primary-modern" @click="showEdit = true">
            Chỉnh sửa hồ sơ
          </button>
          <div v-else class="d-flex gap-2">
            <button class="btn-update-modern" :disabled="saving" @click="saveProfile">
              <span v-if="saving" class="spinner-border spinner-border-sm me-1"></span>
              Lưu thay đổi
            </button>
            <button class="btn-cancel-modern" @click="showEdit = false">Hủy bỏ</button>
          </div>
        </div>
      </div>

      <div class="staff-info-card" :class="{ 'is-editing': showEdit }">
        
        <div class="staff-profile-section">
          <div class="avatar-wrapper">
            <img :src="staff.avatarUrl || '/default-avatar.png'" class="main-avatar" />
          </div>
          
          <div v-if="showEdit" class="mt-2 text-center">
            <button class="btn-upload-avatar" type="button" :disabled="uploadBusy" @click="pickFile">
              <i v-if="!uploadBusy" class="bi bi-camera"></i>
              <span v-else class="spinner-border spinner-border-sm"></span>
              <span>Đổi ảnh</span>
            </button>
            <input ref="fileInput" type="file" accept="image/*" class="d-none" @change="handleFileChange" />
          </div>

          <div v-if="!showEdit" class="profile-summary mt-3 text-center">
            <h2 class="staff-name-display">{{ staff.fullName }}</h2>
            <span :class="['status-badge-lg', staff.isActive ? 'bg-success-soft' : 'bg-secondary-soft']">
              {{ staff.isActive ? 'Hoạt động' : 'Ngưng hoạt động' }}
            </span>
          </div>
        </div>

        <div class="staff-details-container">
          <div v-if="toast" class="alert alert-success py-2 mb-3 border-0 small">{{ toast }}</div>
          
          <div class="staff-details-grid">
            <div class="form-group">
              <label>Họ và tên</label>
              <input v-model="editForm.fullName" :disabled="!showEdit" :class="{ 'disabled-view': !showEdit }" />
            </div>

            <div class="form-group">
              <label>Mã nhân viên</label>
              <input v-model="editForm.code" :disabled="!showEdit" :class="{ 'disabled-view': !showEdit }" />
            </div>

            <div class="form-group">
              <label class="label-disabled">Tài khoản (Username)</label>
              <input :value="staff.username" disabled class="input-readonly-static" />
            </div>

            <div class="form-group">
              <label class="label-disabled">Ngày tham gia</label>
              <input :value="new Date(staff.createdAt).toLocaleDateString()" disabled class="input-readonly-static" />
            </div>

            <div class="form-group full-row">
              <label>Trạng thái tài khoản</label>
              <select v-model="editForm.isActive" :disabled="!showEdit" :class="{ 'disabled-view': !showEdit }">
                <option :value="true">Hoạt động (Đang công tác)</option>
                <option :value="false">Ngưng (Đã nghỉ việc)</option>
              </select>
            </div>
          </div>
          
          <div class="profile-footer-note mt-4">
            <i class="bi bi-info-circle me-2"></i>Mọi thay đổi về Tài khoản và Ngày tạo vui lòng liên hệ bộ phận Kỹ thuật.
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

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

const loadProfile = async () => {
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
  } catch (err: any) {
    console.error("Lỗi load profile:", err)
  } finally {
    loading.value = false
  }
}

const saveProfile = async () => {
  try {
    saving.value = true
    await api.put('/Staffs/profile', editForm.value)
    showEdit.value = false
    await loadProfile()
    toast.value = 'Cập nhật thành công ✨'
    setTimeout(() => (toast.value = null), 2500)
  } catch (err: any) {
    alert("Có lỗi xảy ra khi lưu thông tin")
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
      staff.value.avatarUrl = url
      editForm.value.avatarUrl = url
      toast.value = 'Cập nhật ảnh thành công'
      setTimeout(() => (toast.value = null), 2000)
    }
  } catch (err: any) {
    alert("Tải ảnh thất bại")
  } finally {
    uploadBusy.value = false
    target.value = ''
  }
}

onMounted(loadProfile)
</script>
 
<style src="@/styles/layouts/staff-profile.css"></style>
