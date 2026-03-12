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
  </div>
</template>

<script setup lang="ts">
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
  }
  return labels[status] || status
}

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

  return `${day}/${month}/${year} ${hours}:${minutes}`
}

onMounted(() => {
  loadAppointments()
})
</script>

<style src="@/styles/layouts/doctor-appointment.css"></style>