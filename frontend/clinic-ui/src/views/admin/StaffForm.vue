<script setup lang="ts">
import { reactive, ref, onMounted, computed } from "vue"
import { useRoute, useRouter } from "vue-router"
import "@/styles/layouts/StaffForm.css"

import {
  createStaff,
  getStaffById,
  updateStaff
} from "@/services/staffService"
import { getUsers } from "@/services/userService"

// ================= ROUTER =================
const route = useRoute()
const router = useRouter()
const id = route.params.id as string
const isEdit = computed(() => !!id)

// ================= ROLE =================
const allowedRoles = ["Staff", "Cashier"] as const
type Role = typeof allowedRoles[number]

// ================= TYPES =================
type StaffForm = {
  userId: string
  code: string
  fullName: string
  role: Role
  isActive: boolean
   avatarUrl: string
}

// ================= DATA =================
const users = ref<any[]>([])

const form = reactive<StaffForm>({
  userId: "",
  code: "",
  fullName: "",
  role: allowedRoles[0], // luôn hợp lệ
  isActive: true,
  avatarUrl: ""  
})

// ================= LABEL =================
const roleLabel = (role: Role) => {
  switch (role) {
    case "Staff":
      return "Lễ tân"
    case "Cashier":
      return "Thu ngân"
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

    if (isEdit.value) {
      const res = await getStaffById(id)
      Object.assign(form, res.data)
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
    form.role
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
} catch (error: any) {
  console.error("Lỗi submit", error)

  // Nếu server trả về message cụ thể
  const msg = error.response?.data?.message
  if (msg) {
    alert(msg)
  } else {
    alert("Có lỗi xảy ra")
  }
}

}
</script>
<template>

   
      <div class="form-header">
        <h1 class="form-title">
          {{ isEdit ? "Chỉnh sửa nhân viên" : "Tạo nhân viên mới" }}
        </h1>
      </div>

      <div class="form-content">
        <div class="avatar-section">
          <img 
            :src="form.avatarUrl || 'https://via.placeholder.com/150'" 
            class="avatar-preview" 
            alt="Avatar"
          />
          <div class="avatar-info">
            <span class="avatar-name">{{ form.fullName || 'Chưa nhập tên' }}</span>
            <span class="avatar-label">{{ roleLabel(form.role) }}</span>
          </div>
        </div>

        <form @submit.prevent="handleSubmit">
          <div class="form-group">
            <label>Người dùng</label>
            <select v-model="form.userId" required>
              <option disabled value="">Chọn người dùng</option>
              <option v-for="u in users" :key="u.id" :value="u.id">
                {{ u.username }}
              </option>
            </select>
          </div>

          <div class="form-group">
            <label>Mã nhân viên</label>
            <input v-model="form.code" required placeholder="VD: NV001" />
          </div>

          <div class="form-group">
            <label>Họ và tên</label>
            <input v-model="form.fullName" required placeholder="Nhập họ và tên..." />
          </div>

          <div class="form-group">
            <label>Vị trí</label>
            <select v-model="form.role" required>
              <option v-for="r in allowedRoles" :key="r" :value="r">
                {{ roleLabel(r) }}
              </option>
            </select>
          </div>

          <div class="form-group full-width">
            <label>Ảnh đại diện (URL)</label>
            <input v-model="form.avatarUrl" placeholder="https://..." />
          </div>

          <div class="form-group full-width">
            <label>Trạng thái</label>
            <select v-model="form.isActive">
              <option :value="true">Hoạt động</option>
              <option :value="false">Không hoạt động</option>
            </select>
          </div>

          <div class="form-actions">
            <button type="submit" class="btn-primary" :disabled="!isValid">
              {{ isEdit ? "Cập nhật" : "Tạo mới" }}
            </button>
            <button type="button" class="btn-secondary" @click="router.push('/staff')">
              Hủy
            </button>
          </div>
        </form>
      </div>
   

</template>

