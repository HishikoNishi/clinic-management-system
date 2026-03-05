<template>
  <div class="staff-container">
    <div class="header">
      <h1>Staff Management</h1>
      <router-link to="/staff/create" class="btn-create">
        + Create Staff
      </router-link>
    </div>

    <div class="table-wrapper">
      <table>
        <thead>
          <tr>
            <th>Code</th>
            <th>Full Name</th>
            <th>Email</th>
            <th>Phone</th>
            <th>Status</th>
            <th>Actions</th>
          </tr>
        </thead>

        <tbody>
          <tr v-for="staff in staffs" :key="staff.id">
            <td>{{ staff.code }}</td>
            <td>{{ staff.fullName }}</td>
            <td>{{ staff.email }}</td>
            <td>{{ staff.phone }}</td>
            <td>
              <span
                :class="staff.isActive ? 'status-active' : 'status-inactive'"
              >
                {{ staff.isActive ? "Active" : "Inactive" }}
              </span>
            </td>
            <td class="actions">
              <button @click="goEdit(staff.id)" class="btn-edit">
                Edit
              </button>
              <button @click="handleDelete(staff.id)" class="btn-delete">
                Delete
              </button>
            </td>
          </tr>

          <tr v-if="staffs.length === 0">
            <td colspan="6" class="empty">
              No staff found
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue"
import { useRouter } from "vue-router"
import { getStaffs, deleteStaff } from "@/services/staffService"

const staffs = ref([])
const router = useRouter()

const loadStaffs = async () => {
  try {
    const res = await getStaffs()
    staffs.value = res.data
  } catch (err) {
    console.error(err)
  }
}

const goEdit = (id) => {
  router.push(`/staff/edit/${id}`)
}

const handleDelete = async (id) => {
  if (!confirm("Are you sure to delete this staff?")) return
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
  font-size: 28px;
  font-weight: 600;
}

.btn-create {
  background: #6c4cf1;
  color: white;
  padding: 10px 16px;
  border-radius: 8px;
  text-decoration: none;
}

.table-wrapper {
  background: white;
  border-radius: 10px;
  overflow: hidden;
  box-shadow: 0 5px 20px rgba(0, 0, 0, 0.05);
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
  padding: 14px;
  text-align: left;
}

tbody tr {
  border-top: 1px solid #eee;
}

.actions button {
  margin-right: 8px;
  padding: 6px 12px;
  border-radius: 6px;
  border: none;
  cursor: pointer;
}

.btn-edit {
  background: #3498db;
  color: white;
}

.btn-delete {
  background: #e74c3c;
  color: white;
}

.status-active {
  color: #2ecc71;
  font-weight: 600;
}

.status-inactive {
  color: #e74c3c;
  font-weight: 600;
}

.empty {
  text-align: center;
  padding: 20px;
}
</style>