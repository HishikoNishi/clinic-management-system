<template>
  <div class="staff-form-container">
    <div class="form-card">
      <h1 class="form-title">
        {{ isEdit ? "Chỉnh sửa nhân viên" : "Tạo nhân viên mới" }}
      </h1>

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
          <label>Mã</label>
          <input v-model="form.code" required />
        </div>

        <div class="form-group">
          <label>Họ và tên</label>
          <input v-model="form.fullName" required />
        </div>

        <div class="form-group">
          <label>Vị trí</label>
          <select v-model="form.role" required>
            <option value="Staff">Lễ Tân</option>
          </select>
        </div>

        <div v-if="isEdit" class="form-group">
          <label>Trạng thái</label>
          <select v-model="form.isActive">
            <option :value="true">Hoạt động</option>
            <option :value="false">Không hoạt động</option>
          </select>
        </div>

        <div class="form-actions">
          <button type="submit" class="btn-primary">
            {{ isEdit ? "Cập nhật" : "Tạo" }}
          </button>

          <button type="button" class="btn-secondary" @click="router.push('/staff')">
            Hủy
          </button>
        </div>

      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref, onMounted, computed } from "vue"
import { useRoute, useRouter } from "vue-router"
import {
  createStaff,
  getStaffById,
  updateStaff
} from "@/services/staffService"
import { getUsers } from "@/services/userService"

const route = useRoute()
const router = useRouter()
const id = route.params.id as string
const isEdit = computed(() => !!id)

const users = ref<any[]>([])

const form = reactive({
  userId: "",
  code: "",
  fullName: "",
  role: "Staff",
  isActive: true
})

onMounted(async () => {
  const resUsers = await getUsers()

  users.value = resUsers.data.filter(
    (u: any) => u.role === "Staff"
  )

  if (isEdit.value) {
    const res = await getStaffById(id)
    Object.assign(form, res.data)
  }
})

const handleSubmit = async () => {
  if (isEdit.value) {
    await updateStaff(id, form)
  } else {
    await createStaff(form)
  }

  router.push("/staff")
}
</script>

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
  color: black; /* theo yêu cầu của bạn */
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
  transition: 0.2s;
}

.btn-primary:hover {
  background: #5a3de0;
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