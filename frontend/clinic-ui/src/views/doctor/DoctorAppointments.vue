<template>
  <div class="doctor-container">
    <h2>Lịch khám của tôi</h2>

    <!-- SEARCH BAR -->
    <div class="search-bar">
      <input v-model="searchCode" placeholder="Tìm theo mã..." />
      <input v-model="searchName" placeholder="Tìm theo tên bệnh nhân..." />
      <input v-model="searchPhone" placeholder="Tìm theo số điện thoại..." />
      <input type="date" v-model="searchDate" />

      <button class="search-btn" @click="loadAppointments">🔍</button>
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

    <!-- LOADING -->
    <p v-if="loading">Đang tải dữ liệu...</p>

    <!-- TABLE -->
    <table v-if="!loading">
      <thead>
        <tr>
          <th>Mã</th>
          <th>Bệnh nhân</th>
          <th>Điện thoại</th>
          <th>Ngày khám</th>
          <th>Trạng thái</th>
          <th>Hành động</th>
        </tr>
      </thead>

      <tbody>
        <tr
          v-for="a in filteredAppointments"
          :key="a.id"
          @click="goDetail(a.id)"
        >
          <td>{{ a.appointmentCode }}</td>

          <td>{{ a.fullName }}</td>

          <td>{{ a.phone }}</td>

          <td>
            {{ formatDateTime(a.appointmentDate, a.appointmentTime) }}
          </td>

          <td>
            <span :class="'status ' + a.status.toLowerCase()">
              {{ statusLabel(a.status) }}
            </span>
          </td>

          <td class="actions">

            <!-- APPROVE -->
            <button
              v-if="a.status === 'Pending'"
              @click.stop="approveAppointment(a.id)"
            >
              Xác nhận
            </button>

            <!-- COMPLETE -->
            <button
              v-if="a.status === 'Confirmed'"
              @click.stop="completeAppointment(a.id)"
            >
              Hoàn thành
            </button>

            <!-- CANCEL -->
            <button
              v-if="a.status === 'Pending' || a.status === 'Confirmed'"
              @click.stop="cancelAppointment(a.id)"
            >
              Hủy
            </button>

          </td>
        </tr>
      </tbody>
    </table>

    <p v-if="!loading && filteredAppointments.length === 0">
      Không có lịch khám
    </p>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from "vue"
import { useRouter } from "vue-router"
import api from "@/services/api"

/* ROUTER */
const router = useRouter()

/* DATA */
const appointments = ref<any[]>([])
const loading = ref(false)

/* SEARCH */
const searchCode = ref("")
const searchName = ref("")
const searchPhone = ref("")
const searchDate = ref("")

/* STATUS FILTER */
const statuses = [
  "All",
  "Pending",
  "Confirmed",
  "Completed",
  "Cancelled"
]

const currentStatus = ref("All")

/* LOAD DATA */
const loadAppointments = async () => {

  loading.value = true

  try {

    const res = await api.get("/doctor/DoctorAppointments")

    appointments.value = res.data

  } catch (err) {

    console.error("Load appointments error:", err)

  } finally {

    loading.value = false

  }

}

/* FILTER LOGIC */

const filteredAppointments = computed(() => {

  return appointments.value.filter(a => {

    const matchStatus =
      currentStatus.value === "All" ||
      a.status === currentStatus.value

    const matchCode =
      !searchCode.value ||
      a.appointmentCode
        ?.toLowerCase()
        .includes(searchCode.value.toLowerCase())

    const matchName =
      !searchName.value ||
      a.fullName
        ?.toLowerCase()
        .includes(searchName.value.toLowerCase())

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

/* ROUTE DETAIL */

const goDetail = (id: string) => {

  router.push(`/doctor/appointments/${id}`)

}

/* APPROVE */

const approveAppointment = async (appointmentId: string) => {

  const ok = confirm("Xác nhận lịch khám này?")

  if (!ok) return

  try {

    await api.patch(`/doctor/DoctorAppointments/${appointmentId}/approve`)

    alert("Đã xác nhận lịch khám")

    loadAppointments()

  } catch (err) {

    console.error(err)

  }

}

/* COMPLETE */

const completeAppointment = async (appointmentId: string) => {

  const ok = confirm("Đánh dấu lịch khám hoàn thành?")

  if (!ok) return

  try {

    await api.patch(`/doctor/DoctorAppointments/${appointmentId}/complete`)

    alert("Đã hoàn thành lịch khám")

    loadAppointments()

  } catch (err) {

    console.error(err)

  }

}

/* CANCEL */

const cancelAppointment = async (appointmentId: string) => {

  const ok = confirm("Bạn chắc chắn muốn hủy lịch khám?")

  if (!ok) return

  try {

    await api.patch(`/doctor/DoctorAppointments/${appointmentId}/cancel`)

    alert("Đã hủy lịch khám")

    loadAppointments()

  } catch (err) {

    console.error(err)

  }

}

/* STATUS LABEL */

const statusLabel = (status: string) => {

  const labels: { [key: string]: string } = {

    All: "Tất cả",

    Pending: "Chờ xử lý",

    Confirmed: "Đã xác nhận",

    Completed: "Hoàn thành",

    Cancelled: "Đã hủy"

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

const formatDateTime = (
  dateStr: string,
  timeStr: string
) => {

  if (!dateStr || !timeStr) return ""

  const date = new Date(dateStr)

  const [hours, minutes] = timeStr.split(":")

  const day = String(date.getDate()).padStart(2, "0")
  const month = String(date.getMonth() + 1).padStart(2, "0")
  const year = date.getFullYear()

  return `${day}/${month}/${year} ${hours}:${minutes}`

}

/* INIT */
  
onMounted(() => {

  loadAppointments()

})
</script>

<style src="@/styles/layouts/doctor-appointment.css"></style>