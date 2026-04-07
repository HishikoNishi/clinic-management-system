<script setup lang="ts">
import { ref, computed, onMounted } from "vue"
import { useRouter } from "vue-router"
import {
  getStaffs,
  deleteStaff,
  updateStaff
} from "@/services/staffService"
import "@/styles/layouts/staff-list.css"

const staffs = ref<any[]>([])
const loading = ref(false)
const searchTerm = ref("")
const router = useRouter()

const filteredStaffs = computed(() => {
  const term = searchTerm.value.trim().toLowerCase()
  if (!term) return staffs.value
  return staffs.value.filter((s: any) => {
    const text = `${s.code} ${s.fullName} ${s.username} ${s.role}`.toLowerCase()
    return text.includes(term)
  })
})

const loadStaffs = async () => {
  loading.value = true
  const res = await getStaffs()
  staffs.value = res.data
  loading.value = false
}

const goEdit = (id: string) => {
  router.push(`/staff/edit/${id}`)
}

const toggleStatus = async (staff: any) => {
  await updateStaff(staff.id, {
    userId: staff.userId,
    code: staff.code,
    fullName: staff.fullName,
    role: staff.role,
    isActive: !staff.isActive,
    departmentId: staff.departmentId
  })

  loadStaffs()
}

const handleDelete = async (id: string) => {
  if (!confirm("Xóa nhân viên này?")) return
  await deleteStaff(id)
  loadStaffs()
}

onMounted(loadStaffs)
</script>

<template>
  <div class="staff-container">
    <div class="toolbar">
      <div>
        <h1>Quản lý nhân viên</h1>
        <p class="subtitle">Tìm kiếm, chỉnh sửa hoặc xóa tài khoản nhân viên.</p>
      </div>
      <div class="toolbar-actions">
        <div class="search-box">
          <i class="bi bi-search"></i>
          <input
            v-model="searchTerm"
            type="search"
            placeholder="Tìm theo mã, tên, username"
          />
        </div>
        <router-link to="/staff/create" class="btn-create">
          + Tạo nhân viên
        </router-link>
      </div>
    </div>

    <div class="table-card">
      <table>
        <thead>
          <tr>
            <th>Mã</th>
            <th>Họ và tên</th>
            <th>Vị trí</th>
            <th>Tên đăng nhập</th>
            <th>Trạng thái</th>
            <th class="center">Hành động</th>
          </tr>
        </thead>

        <tbody>
          <tr v-if="loading">
            <td colspan="6" class="empty">Đang tải...</td>
          </tr>
          <tr v-else-if="filteredStaffs.length === 0">
            <td colspan="6" class="empty">
              Không tìm thấy nhân viên
            </td>
          </tr>
          <tr v-else v-for="s in filteredStaffs" :key="s.id">
            <td>{{ s.code }}</td>
            <td>{{ s.fullName }}</td>
            <td>{{ s.role }}</td>
            <td>{{ s.username }}</td>

            <td>
              <span
                class="status-badge"
                :class="s.isActive ? 'active' : 'inactive'"
              >
                {{ s.isActive ? "Hoạt động" : "Không hoạt động" }}
              </span>
            </td>

            <td class="center actions">
              <button class="btn-edit" @click="goEdit(s.id)">
                Chỉnh sửa
              </button>

              <button class="btn-delete" @click="handleDelete(s.id)">
                Xóa
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<style src="@/styles/layouts/staff-list.css" scoped></style>
