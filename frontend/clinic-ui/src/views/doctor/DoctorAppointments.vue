<<<<<<< Updated upstream
<template>
  <div class="doctor-container">
    <h2>Lịch khám của tôi</h2>

    <!-- SEARCH BAR -->
    <div class="search-bar">
      <input v-model="searchCode" placeholder="Tìm kiếm theo mã..." />
      <input v-model="searchName" placeholder="Tìm kiếm theo tên bệnh nhân..." />
      <input v-model="searchPhone" placeholder="Tìm kiếm theo số điện thoại..." />
      <input type="date" v-model="searchDate" />

      <button class="search-btn">🔍</button>
      <button class="clear-btn" @click="clearFilters">Xóa</button>
    </div>

    <!-- FILTER STATUS -->
    <div class="filter">
      <button
        v-for="s in statuses"
        :key="s"
        :class="{ active: currentStatus === s }"
        @click="changeStatus(s)"
      >
        {{ statusLabel(s) }}
      </button>
    </div>

    <!-- TABLE -->
   <table>
  <thead>
    <tr>
      <th>Mã</th>
      <th>Bệnh nhân</th>
      <th>Điện thoại</th> <!-- thêm cột -->
      <th>Ngày</th>
      <th>Trạng thái</th>
      <th>Hành động</th>
    </tr>
  </thead>
  <tbody>
    <tr
      v-for="a in filteredAppointments"
      :key="a.id"
      @click="$router.push(`/doctor/appointments/${a.id}`)"
    >
      <td>{{ a.appointmentCode }}</td>
      <td>{{ a.fullName }}</td>
      <td>{{ a.phone }}</td> <!-- thêm dữ liệu -->
      <td>{{ formatDateTime(a.appointmentDate, a.appointmentTime) }}</td>
      <td>
        <span :class="'status ' + a.status.toLowerCase()">
          {{ statusLabel(a.status) }}
        </span>
      </td>
      <td>
        <button v-if="a.status === 'Confirmed'" @click.stop="completeAppointment(a.id)">
          Hoàn thành lịch khám
        </button>
      </td>
    </tr>
  </tbody>
</table>


    <p v-if="filteredAppointments.length === 0">Không có lịch khám</p>
=======
﻿<template>
  <div class="doctor-container">
    <div class="header">
      <div>
        <h2>Lịch khám của tôi</h2>
        <p class="text-muted mb-0">Hiển thị lịch đã được phân cho bác sĩ (Đã check-in / Chờ khám).</p>
      </div>
      <div class="filter d-flex gap-2">
        <select v-model="currentStatus" class="form-select form-select-sm" @change="loadAppointments">
          <option value="CheckedIn,Confirmed">Đã check-in + Chờ khám</option>
          <option value="CheckedIn">Đã check-in</option>
          <option value="Confirmed">Chờ khám</option>
          <option value="Completed">Đã khám xong</option>
          <option value="All">Tất cả</option>
        </select>
        <button class="btn btn-outline-secondary btn-sm" @click="loadAppointments">
          <i class="bi bi-arrow-clockwise"></i> Làm mới
        </button>
      </div>
    </div>

    <div v-if="error" class="alert alert-danger py-2">{{ error }}</div>

    <div class="card">
      <div class="card-body p-0">
        <table class="table mb-0">
          <thead>
            <tr>
              <th>Bệnh nhân</th>
              <th>Điện thoại</th>
              <th>Ngày khám</th>
              <th>Trạng thái</th>
              <th class="text-end">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
              <td colspan="5" class="text-center py-4 text-muted">
                <span class="spinner-border spinner-border-sm me-2"></span>Đang tải...
              </td>
            </tr>
            <tr v-else-if="appointments.length === 0">
              <td colspan="5" class="text-center py-4 text-muted">Không có lịch khám</td>
            </tr>
            <tr v-else v-for="a in appointments" :key="a.id">
              <td class="fw-semibold">{{ a.fullName }}</td>
              <td>{{ a.phone }}</td>
              <td>{{ formatDateTime(a.appointmentDate, a.appointmentTime) }}</td>
              <td>
                <span :class="['badge', badgeClass(a.status)]">{{ statusLabel(a.status) }}</span>
              </td>
              <td class="text-end">
                <button class="btn btn-primary btn-sm" @click="goExamine(a.id)">
                  <i class="bi bi-stethoscope me-1"></i> Khám
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
>>>>>>> Stashed changes
  </div>
</template>

<script setup lang="ts">
<<<<<<< Updated upstream
import axios from "axios"
import { ref, computed, onMounted } from "vue"
import { useAuthStore } from "@/stores/auth"

const auth = useAuthStore()

/* SEARCH */
const searchCode = ref("")
const searchName = ref("")
const searchPhone = ref("")
const searchDate = ref("")

/* DATA */
const appointments = ref<any[]>([])
const statuses = ["All", "Pending", "Confirmed", "Completed", "Cancelled"]
const currentStatus = ref("All")

/* API */
const api = axios.create({
  baseURL: "https://localhost:7235/api",
  headers: {
    Authorization: `Bearer ${auth.token}`
  }
})

/* LOAD APPOINTMENTS */
const loadAppointments = async () => {
  const res = await api.get("/doctor/DoctorAppointments")
  appointments.value = res.data
}

/* FILTER LOGIC */
const filteredAppointments = computed(() => {
  return appointments.value.filter(a => {

    const matchStatus =
      currentStatus.value === "All" ||
      a.status === currentStatus.value

    const matchCode =
      !searchCode.value ||
      a.appointmentCode?.toLowerCase().includes(searchCode.value.toLowerCase())

    const matchName =
      !searchName.value ||
      a.fullName?.toLowerCase().includes(searchName.value.toLowerCase())

    const matchPhone =
      !searchPhone.value ||
      a.phone?.includes(searchPhone.value)

    const matchDate =
      !searchDate.value ||
      a.appointmentDate?.startsWith(searchDate.value)

    return (
      matchStatus &&
      matchCode &&
      matchName &&
      matchPhone &&
      matchDate
    )
  })
})

/* CHANGE STATUS */
const changeStatus = (s: string) => {
  currentStatus.value = s
}

/* COMPLETE APPOINTMENT */
const completeAppointment = async (appointmentId: string) => {
  const ok = confirm("Bạn có chắc chắn muốn đánh dấu lịch khám này hoàn thành?")
  if (!ok) return

  await api.patch(`/doctor/DoctorAppointments/${appointmentId}/complete`)
  alert("Cập nhật trạng thái ✅")

  loadAppointments()
}

/* STATUS LABEL */
const statusLabel = (status: string) => {
  const labels: { [key: string]: string } = {
    'All': 'Tất cả',
    'Pending': 'Chờ xử lý',
    'Confirmed': 'Đã xác nhận',
    'Completed': 'Hoàn thành',
    'Cancelled': 'Đã hủy'
=======
import { ref, onMounted } from "vue"
import { useRouter } from "vue-router"
import api from "@/services/api"

const router = useRouter()
const appointments = ref<any[]>([])
const loading = ref(false)
const error = ref<string | null>(null)
const currentStatus = ref("CheckedIn,Confirmed")

const loadAppointments = async () => {
  loading.value = true
  error.value = null
  appointments.value = []
  try {
    const params = currentStatus.value === "All"
      ? {}
      : { status: currentStatus.value }

    const res = await api.get("/doctor/DoctorAppointments", { params })
    appointments.value = res.data ?? []
  } catch (err) {
    console.error(err)
    error.value = "Không tải được dữ liệu. Vui lòng thử lại."
  } finally {
    loading.value = false
  }
}

const goExamine = (id: string) => {
  router.push(`/doctor/examination/${id}`)
}

const statusLabel = (status: string) => {
  const labels: Record<string, string> = {
    CheckedIn: "Đã check-in",
    Confirmed: "Chờ khám",
    Completed: "Đã khám xong"
>>>>>>> Stashed changes
  }
  return labels[status] || status
}

<<<<<<< Updated upstream
/* CLEAR FILTER */
const clearFilters = () => {
  searchCode.value = ""
  searchName.value = ""
  searchPhone.value = ""
  searchDate.value = ""
}

/* DATE FORMAT */
const formatDateTime = (dateStr: string, timeStr: string) => {
  if (!dateStr) return ""

  const date = new Date(dateStr)
  const [hours, minutes] = timeStr.split(":")

  const day = String(date.getDate()).padStart(2, "0")
  const month = String(date.getMonth() + 1).padStart(2, "0")
  const year = date.getFullYear()

=======
const badgeClass = (status: string) => {
  if (status === "CheckedIn") return "bg-info-subtle text-info"
  if (status === "Confirmed") return "bg-warning-subtle text-warning"
  if (status === "Completed") return "bg-success-subtle text-success"
  return "bg-secondary-subtle text-secondary"
}

const formatDateTime = (dateStr: string, timeStr: string) => {
  if (!dateStr || !timeStr) return ""
  const date = new Date(dateStr)
  const [hours, minutes] = timeStr.split(":")
  const day = String(date.getDate()).padStart(2, "0")
  const month = String(date.getMonth() + 1).padStart(2, "0")
  const year = date.getFullYear()
>>>>>>> Stashed changes
  return `${day}/${month}/${year} ${hours}:${minutes}`
}

onMounted(() => {
  loadAppointments()
})
</script>

<<<<<<< Updated upstream
<style src="@/styles/layouts/doctor-appointment.css"></style>
=======
<style src="@/styles/layouts/doctor-appointments.css"></style>
>>>>>>> Stashed changes
