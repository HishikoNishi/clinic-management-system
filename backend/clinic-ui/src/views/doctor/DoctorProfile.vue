<script setup lang="ts">
import { ref, onMounted } from "vue"
import api from "@/services/api"

const doctor = ref<any>(null)
const loading = ref(false)
const error = ref<string | null>(null)
const showModal = ref(false)
const form = ref<any>({})

const loadProfile = async () => {
  loading.value = true
  error.value = null
  try {
    const res = await api.get("/Doctor/profile")
    doctor.value = res.data
    form.value = { ...doctor.value }
  } catch (err:any) {
    error.value = err.response?.data?.message || "Không tải được profile"
  } finally {
    loading.value = false
  }
}

const saveProfile = async () => {
  try {
    await api.put("/Doctor/profile", form.value)
    showModal.value = false
    await loadProfile()
    alert("Cập nhật thành công ✅")
  } catch (err:any) {
    alert(err.response?.data?.message || "Không thể cập nhật profile")
  }
}

onMounted(loadProfile)
</script>

<template>
  <div class="profile-card">
    <h3>Thông tin bác sĩ</h3>
    <div v-if="loading">Đang tải...</div>
    <div v-else-if="error" class="alert alert-danger">{{ error }}</div>
    <div v-else-if="doctor">
      <img :src="doctor.avatarUrl || '/default-avatar.png'" class="avatar" />
      <div class="profile-info">
        <p><strong>Tên:</strong> {{ doctor.fullName }}</p>
        <p><strong>Mã:</strong> {{ doctor.code }}</p>
        <p><strong>Khoa:</strong> {{ doctor.departmentName }}</p>
        <p><strong>Chuyên khoa:</strong> {{ doctor.specialtyName }}</p>
        <p><strong>Số giấy phép:</strong> {{ doctor.licenseNumber }}</p>
      </div>
      <div class="profile-actions">
        <button class="btn btn-primary" @click="showModal = true">Chỉnh sửa</button>
      </div>
    </div>

    <!-- Modal -->
    <div v-if="showModal" class="modal-backdrop">
      <div class="modal-box">
        <h4>Chỉnh sửa hồ sơ</h4>
        <input v-model="form.fullName" class="form-control mb-2" placeholder="Tên bác sĩ" />
        <input v-model="form.code" class="form-control mb-2" placeholder="Mã bác sĩ" />
        <input v-model="form.licenseNumber" class="form-control mb-2" placeholder="Số giấy phép" />
        <input v-model="form.avatarUrl" class="form-control mb-2" placeholder="Link ảnh avatar" />
        <div class="modal-actions">
          <button class="btn btn-success" @click="saveProfile">Lưu</button>
          <button class="btn btn-secondary" @click="showModal = false">Hủy</button>
        </div>
      </div>
    </div>
  </div>
</template>
<style src="@/styles/layouts/doctor-profile.css"></style>
