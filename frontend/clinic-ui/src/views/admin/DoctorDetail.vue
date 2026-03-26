<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute } from 'vue-router'
import axios from 'axios'
import { useAuthStore } from '@/stores/auth'
import '@/styles/layouts/doctor-detail.css'
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
    <h2 class="page-title">Chi tiết bác sĩ</h2>

    <!-- Thông tin bác sĩ -->
    <div class="doctor-info card mb-4">
      <div class="row g-4 align-items-center">
        <!-- Avatar -->
        <div class="col-md-3 text-center">
          <img :src="doctor?.avatarUrl || '/default-avatar.png'" class="avatar" />
        </div>
        <!-- Thông tin -->
        <div class="col-md-9">
          <h3 class="doctor-name">{{ doctor?.fullName }}</h3>
          <p><strong>Mã:</strong> {{ doctor?.code }}</p>
          <p><strong>Chuyên khoa:</strong> {{ doctor?.specialty }}</p>
          <p><strong>Khoa:</strong> {{ doctor?.departmentName }}</p>
          <p><strong>Số giấy phép:</strong> {{ doctor?.licenseNumber }}</p>
          <span class="badge status-badge">Hoạt động</span>
        </div>
      </div>
    </div>

    <!-- Lịch khám -->
    <div class="appointments card">
      <div class="card-header d-flex justify-content-between align-items-center">
        <h4 class="mb-0">Lịch khám của bác sĩ</h4>
        <input v-model="searchTerm" class="form-control search-box" placeholder="Tìm bệnh nhân..." />
      </div>

      <div class="table-responsive">
        <table class="table table-hover align-middle mb-0">
          <thead class="table-light">
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
              <td class="fw-semibold">{{ a.appointmentCode }}</td>
              <td>{{ a.fullName }}</td>
              <td>{{ a.phone }}</td>
              <td>{{ a.appointmentDate }}</td>
              <td>{{ a.appointmentTime }}</td>
              <td>{{ a.reason }}</td>
              <td>
                <span :class="['badge', a.status === 'Completed' ? 'bg-secondary' : 'bg-success']">
                  {{ a.status }}
                </span>
              </td>
            </tr>
            <tr v-if="filteredAppointments.length === 0">
              <td colspan="7" class="text-center text-muted py-3">Không có lịch khám nào</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>
