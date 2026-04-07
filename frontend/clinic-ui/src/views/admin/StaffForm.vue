<script setup lang="ts">
import { reactive, ref, onMounted, computed } from "vue"
import { useRoute, useRouter } from "vue-router"
import {
  createStaff,
  getStaffById,
  updateStaff
} from "@/services/staffService"
import { getUsers } from "@/services/userService"
import api from "@/services/api"

// ================= ROUTER =================
const route = useRoute()
const router = useRouter()
const id = route.params.id as string
const isEdit = computed(() => !!id)

// ================= ROLE =================
const allowedRoles = ["Staff", "Cashier", "Technician"] as const
type Role = typeof allowedRoles[number]

// ================= TYPES =================
type StaffForm = {
  userId: string
  code: string
  fullName: string
  role: Role
  isActive: boolean
  departmentId?: string | null
}

// ================= DATA =================
const users = ref<any[]>([])
const departments = ref<any[]>([])

const form = reactive<StaffForm>({
  userId: "",
  code: "",
  fullName: "",
  role: allowedRoles[0],
  isActive: true,
  departmentId: ""
})

// ================= LABEL =================
const roleLabel = (role: Role) => {
  switch (role) {
    case "Staff":
      return "Lễ tân"
    case "Cashier":
      return "Thu ngân"
    case "Technician":
      return "Kỹ thuật viên"
    default:
      return role
  }
}

// ================= LOAD DATA =================
onMounted(async () => {
  try {
    const resUsers = await getUsers()

    // lọc user đúng role
    users.value = resUsers.data.filter((u: any) =>
      allowedRoles.includes(u.role)
    )

    const resDeps = await api.get("/Departments")
    departments.value = resDeps.data || []

    if (isEdit.value) {
      const res = await getStaffById(id)
      Object.assign(form, res.data, {
        departmentId: res.data.departmentId || ""
      })
    }
  } catch (error) {
    console.error("Lỗi load dữ liệu", error)
  }
})

// ================= VALIDATE =================
const isValid = computed(() => {
  return (
    form.userId &&
    form.code.trim() &&
    form.fullName.trim() &&
    form.role &&
    (form.role !== "Technician" || !!form.departmentId)
  )
})

// ================= SUBMIT =================
const handleSubmit = async () => {
  if (!isValid.value) {
    alert("Vui lòng nhập đầy đủ thông tin")
    return
  }

  try {
    if (isEdit.value) {
      await updateStaff(id, form)
    } else {
      await createStaff(form)
    }

    alert(isEdit.value ? "Cập nhật thành công" : "Tạo thành công")
    router.push("/staff")
  } catch (error) {
    console.error("Lỗi submit", error)
    alert("Có lỗi xảy ra")
  }
}
</script>

<template>
  <div class="staff-form-container">
    <div class="form-card">
      <h1 class="form-title">
        {{ isEdit ? "Chỉnh sửa nhân viên" : "Tạo nhân viên mới" }}
      </h1>

      <form @submit.prevent="handleSubmit">

        <!-- USER -->
        <div class="form-group">
          <label>Người dùng</label>
          <select v-model="form.userId" required>
            <option disabled value="">Chọn người dùng</option>
            <option v-for="u in users" :key="u.id" :value="u.id">
              {{ u.username }}
            </option>
          </select>
        </div>

        <!-- CODE -->
        <div class="form-group">
          <label>Mã</label>
          <input v-model="form.code" required />
        </div>

        <!-- NAME -->
        <div class="form-group">
          <label>Họ và tên</label>
          <input v-model="form.fullName" required />
        </div>

        <!-- ROLE -->
        <div class="form-group">
          <label>Vị trí</label>
          <select v-model="form.role" required>
            <option v-for="r in allowedRoles" :key="r" :value="r">
              {{ roleLabel(r) }}
            </option>
          </select>
        </div>

        <!-- DEPARTMENT FOR TECHNICIAN -->
        <div
          v-if="form.role === 'Technician'"
          class="form-group"
        >
          <label>Khoa phụ trách</label>
          <select v-model="form.departmentId" required>
            <option value="">Chọn khoa</option>
            <option v-for="d in departments" :key="d.id" :value="d.id">
              {{ d.name }}
            </option>
          </select>
        </div>

        <!-- STATUS -->
        <div v-if="isEdit" class="form-group">
          <label>Trạng thái</label>
          <select v-model="form.isActive">
            <option :value="true">Hoạt động</option>
            <option :value="false">Không hoạt động</option>
          </select>
        </div>

        <!-- ACTION -->
        <div class="form-actions">
          <button type="submit" class="btn-primary" :disabled="!isValid">
            {{ isEdit ? "Cập nhật" : "Tạo" }}
          </button>

          <button
            type="button"
            class="btn-secondary"
            @click="router.push('/staff')"
          >
            Hủy
          </button>
        </div>

      </form>
    </div>
  </div>
</template>

<style scoped>
.staff-form-container {
  display: flex;
  justify-content: center;
  padding: 40px;
}

.form-card {
  width: 450px;
  background: white;
  padding: 30px;
  border-radius: 14px;
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.08);
}

.form-title {
  margin-bottom: 25px;
  font-size: 24px;
  font-weight: 600;
}

.form-group {
  margin-bottom: 18px;
}

label {
  display: block;
  margin-bottom: 6px;
  font-weight: 600;
  color: black;
}

input,
select {
  width: 100%;
  padding: 10px;
  border-radius: 8px;
  border: 1px solid #ddd;
  font-size: 14px;
  transition: 0.2s;
}

input:focus,
select:focus {
  border-color: #6c4cf1;
  outline: none;
}

.form-actions {
  display: flex;
  gap: 12px;
  margin-top: 20px;
}

.btn-primary {
  flex: 1;
  background: #6c4cf1;
  color: white;
  padding: 10px;
  border-radius: 8px;
  border: none;
  cursor: pointer;
}

.btn-primary:hover {
  background: #5a3de0;
}

.btn-primary:disabled {
  background: #aaa;
  cursor: not-allowed;
}

.btn-secondary {
  flex: 1;
  background: #e0e0e0;
  padding: 10px;
  border-radius: 8px;
  border: none;
  cursor: pointer;
}

.btn-secondary:hover {
  background: #d5d5d5;
}
</style>
