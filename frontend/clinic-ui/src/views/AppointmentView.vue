<<<<<<< Updated upstream
<template>
  <div class="appointment-container">
    <h2>Tạo lịch khám</h2>

    <form class="appointment-form" @submit.prevent="submit">
      <label>Họ và tên</label>
      <input v-model="form.fullName" required />

      <label>Ngày sinh</label>
      <input type="date" v-model="form.dateOfBirth" required />

      <label>Giới tính</label>
      <select v-model="form.gender" required>
        <option value="">Chọn giới tính</option>
        <option value="Male">Nam</option>
        <option value="Female">Nữ</option>
      </select>

      <label>Số điện thoại</label>
      <input type="text" v-model="form.phone" @input="handlePhoneInput" @blur="validatePhone" maxlength="10" required />
      <span v-if="errors.phone" class="error">{{ errors.phone }}</span>

      <label>Email</label>
      <input type="email" v-model="form.email" @input="validateEmail" required />
      <span v-if="errors.email" class="error">{{ errors.email }}</span>

      <label>Địa chỉ</label>
      <input v-model="form.address" required />

      <label>Ngày khám</label>
      <input type="date" v-model="form.date" required />

      <label>Thời gian khám</label>
      <input type="time" v-model="form.time" required />

      <label>Lý do / Ghi chú</label>
      <textarea v-model="form.note"></textarea>

      <div class="actions">
        <button type="submit">Tạo lịch khám</button>
        <button type="button" @click="goToMyAppointment">Lịch khám của tôi</button>
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import { reactive } from "vue"
import { useRouter } from "vue-router"
import axios from "axios"

const router = useRouter()

const form = reactive({
  fullName: "",
  dateOfBirth: "",
  gender: "",
  phone: "",
  email: "",
  address: "",
  date: "",
  time: "",
  note: ""
})

const errors = reactive({ phone: "", email: "" })

const handlePhoneInput = () => {
  form.phone = form.phone.replace(/\D/g, '')
  if (form.phone.length > 10) form.phone = form.phone.slice(0, 10)
  errors.phone = ""
}

const validatePhone = () => {
  errors.phone = form.phone.length !== 10 ? "Vui lòng nhập số điện thoại hợp lệ" : ""
}

const validateEmail = () => {
  const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  errors.email = !regex.test(form.email) ? "Vui lòng nhập địa chỉ email hợp lệ" : ""
}

const submit = async () => {
  validatePhone()
  validateEmail()
  if (!errors.phone && !errors.email) {
    const payload = {
      fullName: form.fullName,
      dateOfBirth: form.dateOfBirth,
      gender: form.gender,
      phone: form.phone,
      email: form.email,
      address: form.address,
      appointmentDate: form.date,
      appointmentTime: form.time + ":00",
      reason: form.note
    }

    try {
      const res = await axios.post("https://localhost:7235/api/Appointments", payload)
      const appointmentData = res.data
      localStorage.setItem("appointmentCode", appointmentData.appointmentCode)

      if (window.confirm("Lịch khám đã được tạo thành công! Đi đến lịch khám của bạn?")) {
        router.push("/appointmentdetail")
      }
    } catch (err: any) {
      alert("Lỗi: " + (err.response?.data || err.message))
    }
  }
}

const goToMyAppointment = () => {
  router.push("/appointmentdetail")
}
</script>

=======
<script setup lang="ts">
import { ref, computed, onMounted } from "vue"
import api from "@/services/api"
import { useAuthStore } from "@/stores/auth"

const auth = useAuthStore()

/* SEARCH */

const searchCode = ref("")
const searchName = ref("")
const searchPhone = ref("")
const searchDate = ref("")

/* DATA */

const appointments = ref<any[]>([])

const statuses = [
  "All",
  "Pending",
  "Confirmed",
  "Completed",
  "Cancelled"
]

const currentStatus = ref("All")

/* LOAD APPOINTMENTS */

const loadAppointments = async () => {

  try {

    const res = await api.get("/doctor/DoctorAppointments")

    appointments.value = res.data || []

  } catch (err) {

    console.error("Load appointments error:", err)

  }

}

/* FILTER */

const filteredAppointments = computed(() => {

  return appointments.value.filter((a: any) => {

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

  const labels: Record<string, string> = {

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

/* FORMAT DATE */

const formatDateTime = (dateStr: string, timeStr: string) => {

  if (!dateStr) return ""

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
>>>>>>> Stashed changes
<style src="@/styles/layouts/appointment.css"></style>
