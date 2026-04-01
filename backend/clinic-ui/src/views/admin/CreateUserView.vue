<script setup lang="ts">
import { computed, reactive, ref, onMounted } from 'vue'
import {
  getUsers,
  createUser,
  updateUser,
  deleteUser,
  getRoles,
  type UserDto
} from '@/services/userService'

const users = ref<UserDto[]>([])
const roles = ref<string[]>([])

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
  role: ''
})

// ================= LOAD DATA =================
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

const loadRoles = async () => {
  try {
    const res = await getRoles()
    roles.value = res.data

    // set default role
    if (roles.value.length > 0 && !form.role) {
      form.role = roles.value[0] || ''
    }
  } catch (error) {
    console.error('Không load được roles', error)
  }
}

// ================= COMPUTED =================
const filteredUsers = computed(() => {
  const term = searchText.value.trim().toLowerCase()
  return users.value.filter((u) => {
    const text = `${u.username ?? ''} ${u.role ?? ''}`.toLowerCase()
    const matchesText = !term || text.includes(term)
    const matchesRole = filterRole.value === 'all' || u.role === filterRole.value
    return matchesText && matchesRole
  })
})

// ================= FORM =================
const resetForm = () => {
  mode.value = 'create'
  editingId.value = null
  form.username = ''
  form.password = ''
  form.role = roles.value[0] || ''
}

const startEdit = (user: UserDto) => {
  mode.value = 'edit'
  editingId.value = user.id
  form.username = user.username
  form.role = user.role || roles.value[0] || ''
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
    if (mode.value === 'create') resetForm()
  } catch (error: any) {
    errorMessage.value =
      error?.response?.data?.message ||
      error?.response?.data ||
      error?.message ||
      'Không thể xử lý yêu cầu.'
  } finally {
    loading.value = false
  }
}

// ================= DELETE =================
const removeUser = async (user: UserDto) => {
  if (!confirm(`Xóa tài khoản "${user.username}"?`)) return
  await deleteUser(user.id)
  if (editingId.value === user.id) resetForm()
  await loadUsers()
}

// ================= INIT =================
onMounted(() => {
  loadUsers()
  loadRoles()
})
</script>

<template>
  <div class="container-fluid py-3">
    <div class="d-flex flex-wrap gap-3 justify-content-between align-items-start mb-3">
      <div>
        <h2 class="mb-1">Quản lý tài khoản</h2>
        <p class="text-muted mb-0">Tạo, chỉnh sửa hoặc xoá tài khoản.</p>
      </div>

      <!-- SEARCH + FILTER -->
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
          <option v-for="r in roles" :key="r" :value="r">
            {{ r }}
          </option>
        </select>
      </div>
    </div>

    <div class="row g-3">
      <!-- FORM -->
      <div class="col-lg-4">
        <div class="card shadow-sm h-100">
          <div class="card-header d-flex justify-content-between">
            <h5 class="mb-0">
              {{ mode === 'edit' ? 'Cập nhật' : 'Tạo tài khoản' }}
            </h5>
          </div>

          <div class="card-body">
            <div v-if="successMessage" class="alert alert-success py-2">
              {{ successMessage }}
            </div>

            <div v-if="errorMessage" class="alert alert-danger py-2">
              {{ errorMessage }}
            </div>

            <form class="vstack gap-3" @submit.prevent="submit">
              <input
                class="form-control"
                v-model="form.username"
                placeholder="Tên đăng nhập"
                required
              />

              <input
                class="form-control"
                v-model="form.password"
                type="password"
                :required="mode === 'create'"
                placeholder="Mật khẩu"
              />

              <!-- ROLE -->
              <select v-model="form.role" class="form-select">
                <option v-for="r in roles" :key="r" :value="r">
                  {{ r }}
                </option>
              </select>

              <div class="d-flex justify-content-end gap-2">
                <button v-if="mode === 'edit'" type="button" class="btn btn-secondary" @click="resetForm">
                  Hủy
                </button>

                <button class="btn btn-primary" :disabled="loading">
                  {{ loading ? 'Đang xử lý...' : 'Lưu' }}
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>

      <!-- TABLE -->
      <div class="col-lg-8">
        <div class="card shadow-sm h-100">
          <div class="card-body p-0">
            <table class="table mb-0">
              <thead>
                <tr>
                  <th>Username</th>
                  <th>Role</th>
                  <th></th>
                </tr>
              </thead>

              <tbody>
                <tr v-if="listLoading">
                  <td colspan="3" class="text-center">Loading...</td>
                </tr>

                <tr v-else v-for="user in filteredUsers" :key="user.id">
                  <td>{{ user.username }}</td>
                  <td>{{ user.role }}</td>
                  <td class="text-end">
                    <button class="btn btn-sm btn-primary me-2" @click="startEdit(user)">
                      Sửa
                    </button>
                    <button class="btn btn-sm btn-danger" @click="removeUser(user)">
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
</template>