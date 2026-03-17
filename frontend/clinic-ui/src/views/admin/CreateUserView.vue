<script setup lang="ts">
import { computed, reactive, ref, onMounted } from 'vue'
import {
  getUsers,
  createUser,
  updateUser,
  deleteUser,
  type UserDto
} from '@/services/userService'

const users = ref<UserDto[]>([])
const listLoading = ref(false)
const loading = ref(false)
const successMessage = ref('')
const errorMessage = ref('')
const mode = ref<'create' | 'edit'>('create')
const editingId = ref<string | null>(null)
const searchText = ref('')
const filterRole = ref<'all' | string>('all')

const form = reactive({
  username: '',
  password: '',
  role: 'Staff'
})

const loadUsers = async () => {
  listLoading.value = true
  try {
    const res = await getUsers()
    users.value = res.data
  } catch (error: any) {
    errorMessage.value =
      error?.response?.data?.message ||
      error?.message ||
      'Không thể tải danh sách tài khoản.'
  } finally {
    listLoading.value = false
  }
}

const filteredUsers = computed(() => {
  const term = searchText.value.trim().toLowerCase()
  return users.value.filter((u) => {
    const text = `${u.username ?? ''} ${u.role ?? ''}`.toLowerCase()
    const matchesText = !term || text.includes(term)
    const matchesRole = filterRole.value === 'all' || u.role === filterRole.value
    return matchesText && matchesRole
  })
})

const resetForm = () => {
  mode.value = 'create'
  editingId.value = null
  form.username = ''
  form.password = ''
  form.role = 'Staff'
}

const startEdit = (user: UserDto) => {
  mode.value = 'edit'
  editingId.value = user.id
  form.username = user.username
  form.role = user.role || 'Staff'
  form.password = ''
}

const submit = async () => {
  successMessage.value = ''
  errorMessage.value = ''
  loading.value = true

  try {
    if (mode.value === 'edit' && editingId.value) {
      const payload: Record<string, string> = {
        username: form.username,
        role: form.role
      }

      if (form.password) {
        payload.password = form.password
      }

      await updateUser(editingId.value, payload)
      successMessage.value = 'Đã cập nhật tài khoản.'
    } else {
      await createUser({
        username: form.username,
        password: form.password,
        role: form.role
      })
      successMessage.value = 'Đã tạo tài khoản mới.'
    }

    await loadUsers()
    if (mode.value === 'create') {
      resetForm()
    }
  } catch (error: any) {
    const serverMessage =
      error?.response?.data?.message ||
      error?.response?.data ||
      error?.message ||
      'Không thể xử lý yêu cầu. Vui lòng thử lại.'

    errorMessage.value = serverMessage
  } finally {
    loading.value = false
  }
}

const removeUser = async (user: UserDto) => {
  if (!confirm(`Xóa tài khoản "${user.username}"?`)) return
  await deleteUser(user.id)
  if (editingId.value === user.id) resetForm()
  await loadUsers()
}

onMounted(loadUsers)
</script>

<template>
  <div class="container-fluid py-3">
    <div class="d-flex flex-wrap gap-3 justify-content-between align-items-start mb-3">
      <div>
        <h2 class="mb-1">Quản lý tài khoản</h2>
        <p class="text-muted mb-0">Tạo, chỉnh sửa hoặc xoá tài khoản Admin/Staff/Doctor.</p>
      </div>
      <div class="d-flex flex-wrap gap-2">
        <input
          v-model="searchText"
          type="search"
          class="form-control"
          style="min-width: 220px"
          placeholder="Tìm theo tên đăng nhập/role"
        />
        <select v-model="filterRole" class="form-select">
          <option value="all">Tất cả vai trò</option>
          <option value="Admin">Admin</option>
          <option value="Doctor">Bác sĩ</option>
          <option value="Staff">Nhân viên</option>
        </select>
      </div>
    </div>

    <div class="row g-3">
      <div class="col-lg-4">
        <div class="card shadow-sm h-100">
          <div class="card-header d-flex align-items-center justify-content-between">
            <h5 class="mb-0">
              {{ mode === 'edit' ? 'Cập nhật tài khoản' : 'Tạo tài khoản mới' }}
            </h5>
            <span class="badge bg-primary-subtle text-primary">Form</span>
          </div>
          <div class="card-body">
            <div v-if="successMessage" class="alert alert-success py-2">
              {{ successMessage }}
            </div>
            <div v-if="errorMessage" class="alert alert-danger py-2">
              {{ errorMessage }}
            </div>

            <form class="vstack gap-3" @submit.prevent="submit">
              <div>
                <label class="form-label" for="username">Tên đăng nhập</label>
                <input
                  id="username"
                  class="form-control"
                  v-model="form.username"
                  type="text"
                  autocomplete="username"
                  required
                />
              </div>

              <div>
                <label class="form-label" for="password">
                  Mật khẩu
                  <span class="text-muted small" v-if="mode === 'edit'">(để trống nếu giữ nguyên)</span>
                </label>
                <input
                  id="password"
                  class="form-control"
                  v-model="form.password"
                  type="password"
                  :required="mode === 'create'"
                  :placeholder="mode === 'edit' ? 'Bỏ trống nếu không đổi' : ''"
                  autocomplete="new-password"
                />
              </div>

              <div>
                <label class="form-label" for="role">Vai trò</label>
                <select id="role" class="form-select" v-model="form.role" required>
                  <option value="Doctor">Bác sĩ</option>
                  <option value="Staff">Nhân viên</option>
                </select>
              </div>

              <div class="d-flex justify-content-end gap-2">
                <button
                  v-if="mode === 'edit'"
                  type="button"
                  class="btn btn-outline-secondary"
                  @click="resetForm"
                >
                  Hủy
                </button>
                <button type="submit" class="btn btn-primary" :disabled="loading">
                  <span v-if="!loading">{{ mode === 'edit' ? 'Lưu thay đổi' : 'Tạo tài khoản' }}</span>
                  <span v-else>Đang xử lý...</span>
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>

      <div class="col-lg-8">
        <div class="card shadow-sm h-100">
          <div class="card-header d-flex justify-content-between align-items-center">
            <h5 class="mb-0">Danh sách tài khoản</h5>
            <span class="text-muted small">{{ filteredUsers.length }} tài khoản</span>
          </div>
          <div class="card-body p-0">
            <div class="table-responsive">
              <table class="table align-middle mb-0">
                <thead class="table-light">
                  <tr>
                    <th class="ps-4">Tên đăng nhập</th>
                    <th>Vai trò</th>
                    <th class="text-end pe-4">Hành động</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-if="listLoading">
                    <td colspan="3" class="text-center py-4">Đang tải...</td>
                  </tr>
                  <tr v-else-if="filteredUsers.length === 0">
                    <td colspan="3" class="text-center py-4 text-muted">
                      Không có tài khoản phù hợp
                    </td>
                  </tr>
                  <tr v-else v-for="user in filteredUsers" :key="user.id">
                    <td class="ps-4 fw-semibold">{{ user.username }}</td>
                    <td>
                      <span class="badge bg-primary-subtle text-primary px-3 py-2">
                        {{ user.role || 'Chưa xác định' }}
                      </span>
                    </td>
                    <td class="text-end pe-4">
                      <button class="btn btn-sm btn-outline-primary me-2" @click="startEdit(user)">
                        Sửa
                      </button>
                      <button class="btn btn-sm btn-outline-danger" @click="removeUser(user)">
                        Xóa
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
