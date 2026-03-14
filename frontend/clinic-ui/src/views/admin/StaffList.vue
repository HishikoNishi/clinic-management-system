<template>
  <div class="staff-container">

    <div class="header">
      <h1>Quản lý nhân viên</h1>
      <router-link to="/staff/create" class="btn-create">
        + Tạo nhân viên
      </router-link>
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
          <tr v-for="s in staffs" :key="s.id">
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

          <tr v-if="staffs.length === 0">
            <td colspan="6" class="empty">
              Không tìm thấy nhân viên
            </td>
          </tr>
        </tbody>
      </table>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue"
import { useRouter } from "vue-router"
import {
  getStaffs,
  deleteStaff,
  updateStaff
} from "@/services/staffService"

const staffs = ref<any[]>([])
const router = useRouter()

const loadStaffs = async () => {
  const res = await getStaffs()
  staffs.value = res.data
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
    isActive: !staff.isActive
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

<style scoped>
.staff-container {
  padding: 40px;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 25px;
}

.header h1 {
  font-size: 26px;
  font-weight: 600;
}

.btn-create {
  background: var(--color-primary);
  color: var(--color-text-inverse);
  padding: 10px 18px;
  border-radius: 8px;
  text-decoration: none;
  font-weight: 500;
  transition: 0.2s;
}

.btn-create:hover {
  background: var(--color-primary-dark);
}

.table-card {
  background: white;
  border-radius: 14px;
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.06);
  overflow: hidden;
}

table {
  width: 100%;
  border-collapse: collapse;
}

thead {
  background: #f5f6fa;
}

th,
td {
  padding: 14px 16px;
  text-align: left;
  font-size: 14px;
}

tbody tr {
  border-top: 1px solid #eee;
  transition: 0.2s;
}

tbody tr:hover {
  background: #faf9ff;
}

.center {
  text-align: center;
}

.status-badge {
  padding: 5px 12px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
}

.active {
  background: var(--color-primary-light);
  color: var(--color-primary-dark);
}

.inactive {
  background: #fdecea;
  color: #e74c3c;
}

.actions button {
  margin: 0 4px;
  padding: 6px 10px;
  border-radius: 6px;
  border: none;
  cursor: pointer;
  font-size: 12px;
  transition: 0.2s;
}

.btn-edit {
  background: #3498db;
  color: white;
}

.btn-edit:hover {
  background: #2980b9;
}

.btn-toggle {
  background: #f39c12;
  color: white;
}

.btn-toggle:hover {
  background: #d68910;
}

.btn-delete {
  background: #e74c3c;
  color: white;
}

.btn-delete:hover {
  background: #c0392b;
}

.empty {
  text-align: center;
  padding: 25px;
  color: #888;
}
</style>
