<script setup lang="ts">
import { ref, onMounted } from 'vue'
import api from '@/services/api'

const staff = ref<any>(null)
const loading = ref(false)
const error = ref<string | null>(null)
const showEdit = ref(false)

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
    await api.put('/Staffs/profile', editForm.value, {
      headers: { 'Content-Type': 'application/json' }
    })
    showEdit.value = false
    await loadProfile()
    alert("Cập nhật thành công ✅")
  } catch (err:any) {
    alert(err.response?.data?.message || 'Không thể cập nhật hồ sơ')
  }
}
</script>

<template>
  <div class="profile-card">
    <h3>Thông tin nhân viên</h3>

    <div v-if="loading">Đang tải...</div>
    <div v-else-if="error" class="alert alert-danger">{{ error }}</div>
    <div v-else-if="staff">
      <img :src="staff.avatarUrl || '/default-avatar.png'" class="avatar" />
      <div class="profile-info">
        <p><strong>Mã:</strong> {{ staff.code }}</p>
        <p><strong>Họ tên:</strong> {{ staff.fullName }}</p>
        <p><strong>Tài khoản:</strong> {{ staff.username }}</p>
        <p><strong>Trạng thái:</strong> {{ staff.isActive ? 'Hoạt động' : 'Ngừng' }}</p>
        <p><strong>Ngày tạo:</strong> {{ new Date(staff.createdAt).toLocaleDateString() }}</p>
      </div>
      <div class="profile-actions">
        <button class="btn btn-primary" @click="showEdit = true">Chỉnh sửa</button>
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
          <option :value="false">Ngừng</option>
        </select>
        <input v-model="editForm.avatarUrl" class="form-control mb-2" placeholder="Link avatar" />

        <div class="modal-actions">
          <button class="btn btn-success" @click="saveProfile">Lưu</button>
          <button class="btn btn-secondary" @click="showEdit = false">Hủy</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style src="@/styles/layouts/staff-profile.css"></style>
