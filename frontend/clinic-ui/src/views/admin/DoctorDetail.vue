<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute } from 'vue-router'
import axios from 'axios'
import { useAuthStore } from '@/stores/auth'

const route = useRoute()
const doctorId = route.params.id

// Lấy token từ store
const auth = useAuthStore()

const api = axios.create({
  baseURL: 'https://localhost:7235/api',
  headers: { Authorization: `Bearer ${auth.token}` }
})

const doctor = ref<any>(null)
const appointments = ref<any[]>([])
const searchTerm = ref('')

onMounted(async () => {
  try {
    // lấy thông tin bác sĩ
    const resDoctor = await api.get(`/Doctor/${doctorId}`)
    doctor.value = resDoctor.data

    // lấy danh sách lịch khám của bác sĩ
    const resAppointments = await api.get(`/Doctor/${doctorId}/appointments`)
    appointments.value = resAppointments.data
  } catch (err: any) {
    console.error("Lỗi khi load dữ liệu bác sĩ:", err)
    alert("Không thể tải thông tin bác sĩ. Vui lòng kiểm tra đăng nhập hoặc quyền truy cập.")
  }
})

const filteredAppointments = computed(() => {
  const term = searchTerm.value.trim().toLowerCase()
  return appointments.value
    .filter(a => !term || a.fullName.toLowerCase().includes(term))
    .sort((a, b) => {
      const activeStatuses = ['Pending', 'Confirmed']
      if (activeStatuses.includes(a.status) && !activeStatuses.includes(b.status)) return -1
      if (!activeStatuses.includes(a.status) && activeStatuses.includes(b.status)) return 1
      return 0
    })
})
</script>

<template>
  <div class="doctor-detail-page container py-4">
    <h2 class="mb-4">Chi tiết bác sĩ</h2>

    <div class="row">
      <!-- Bên trái: avatar + thông tin cơ bản -->
      <div class="col-md-4 text-center">
        <img :src="doctor?.avatarUrl || '/default-avatar.png'" class="avatar mb-3" />
        <h4>{{ doctor?.fullName }}</h4>
        <p>Số giấy phép: {{ doctor?.licenseNumber }}</p>
      </div>

      <!-- Bên phải: card thông tin -->
      <div class="col-md-8">
        <div class="card p-3">
          <p><strong>Mã:</strong> {{ doctor?.code }}</p>
          <p><strong>Chuyên khoa:</strong> {{ doctor?.specialty }}</p>
          <p><strong>Khoa:</strong> {{ doctor?.departmentName }}</p>
          <p><strong>Trạng thái:</strong> Hoạt động</p>
        </div>
      </div>
    </div>

    <!-- Danh sách lịch khám -->
    <div class="mt-4">
      <h4>Lịch khám của bác sĩ</h4>
      <input v-model="searchTerm" class="form-control mb-3" placeholder="Tìm bệnh nhân..." />

      <table class="table table-striped">
        <thead>
          <tr>
            <th>Mã lịch</th>
            <th>Bệnh nhân</th>
            <th>Điện thoại</th>
            <th>Ngày</th>
            <th>Giờ</th>
            <th>Lý do</th>
            <th>Trạng thái</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="a in filteredAppointments" :key="a.id">
            <td>{{ a.appointmentCode }}</td>
            <td>{{ a.fullName }}</td>
            <td>{{ a.phone }}</td>
            <td>{{ a.appointmentDate }}</td>
            <td>{{ a.appointmentTime }}</td>
            <td>{{ a.reason }}</td>
            <td>
              <span :class="a.status === 'Completed' ? 'text-muted' : 'text-success'">
                {{ a.status }}
              </span>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<style scoped>
.avatar {
  width: 120px;
  height: 120px;
  border-radius: 50%;
  object-fit: cover;
}
</style>
