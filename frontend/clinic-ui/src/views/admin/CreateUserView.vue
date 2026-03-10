<template>
  <div class="create-user-page">
    <div class="create-user-card card">
      <div class="card-header">
        <h2>Tạo người dùng mới</h2>
      </div>
      <div class="card-body">
        <div v-if="successMessage" class="create-user-alert create-user-alert--success">
          <span class="create-user-alert__icon">✓</span>
          <span class="create-user-alert__text">{{ successMessage }}</span>
        </div>

        <div v-if="errorMessage" class="create-user-alert create-user-alert--error">
          <span class="create-user-alert__icon">⚠️</span>
          <span class="create-user-alert__text">{{ errorMessage }}</span>
        </div>

        <form class="form" @submit.prevent="createUser">
          <div class="form-group">
            <label class="form-label" for="username">Tên đăng nhập</label>
            <input
              id="username"
              class="form-input"
              v-model="form.username"
              type="text"
              autocomplete="username"
              required
            />
          </div>

          <div class="form-group">
            <label class="form-label" for="password">Mật khẩu</label>
            <input
              id="password"
              class="form-input"
              v-model="form.password"
              type="password"
              autocomplete="new-password"
              required
            />
          </div>

          <div class="form-group">
            <label class="form-label" for="role">Vị trí</label>
            <select
              id="role"
              class="form-select"
              v-model="form.role"
              required
            >
              <option value="Doctor">Bác sĩ</option>
              <option value="Staff">Nhân viên</option>
            </select>
          </div>

          <div class="form-actions">
            <button
              type="submit"
              class="btn btn-primary"
              :disabled="loading"
            >
              <span v-if="!loading">Tạo tài khoản</span>
              <span v-else>Đang tạo…</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue'
import api from '@/services/api'

const loading = ref(false)
const successMessage = ref('')
const errorMessage = ref('')

const form = reactive({
  username: '',
  password: '',
  role: 'Admin'
})

const createUser = async () => {
  successMessage.value = ''
  errorMessage.value = ''
  loading.value = true

  try {
    await api.post('/Admin', {
      username: form.username,
      password: form.password,
      role: form.role
    })

    successMessage.value = 'Người dùng đã được tạo thành công.'
    form.username = ''
    form.password = ''
    form.role = 'Admin'
  } catch (error: any) {
    // Support common error shapes
    const serverMessage =
      error?.response?.data?.message ||
      error?.response?.data ||
      error?.message ||
      'Không thể tạo người dùng. Vui lòng thử lại.'

    errorMessage.value = serverMessage
  } finally {
    loading.value = false
  }
}
</script>
