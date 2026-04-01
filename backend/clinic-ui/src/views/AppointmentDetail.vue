<template>
  <div class="appointment-container">
    <h2>Lịch khám của tôi</h2>

    <!-- Link mở form tìm kiếm -->
    <p class="find-link" @click="showSearchForm = true">
      🔎 Tìm lịch khám của bạn
    </p>

    <!-- Appointment Detail -->
    <div v-if="appointment && !showSearchForm" class="appointment-detail">
      <p><strong>Mã:</strong> {{ appointment.appointmentCode }}</p>
      <p><strong>Tên:</strong> {{ appointment.fullName }}</p>
      <p><strong>Điện thoại:</strong> {{ appointment.phone }}</p>
      <p><strong>Email:</strong> {{ appointment.email }}</p>

      <p>
        <strong>Ngày sinh:</strong>
        {{ formatDate(appointment.dateOfBirth) }}
      </p>

      <p><strong>Giới tính:</strong> {{ appointment.gender }}</p>
      <p><strong>Địa chỉ:</strong> {{ appointment.address }}</p>
      <p><strong>Trạng thái:</strong> {{ appointment.status }}</p>

      <p>
        <strong>Ngày khám:</strong>
        {{ formatDate(appointment.appointmentDate) }}
      </p>

      <p>
        <strong>Thời gian khám:</strong>
        {{ formatTime(appointment.appointmentTime) }}
      </p>

      <p><strong>Lý do:</strong> {{ appointment.reason }}</p>

      <p>
        <strong>Tạo lúc:</strong>
        {{ formatDateTime(appointment.createdAt) }}
      </p>

      <div class="actions">
        <button @click="goBack">Quay lại</button>
        <button
          class="cancel"
          v-if="appointment.status !== 'Cancelled'"
          @click="showCancelForm = true"
        >
          Hủy
        </button>
      </div>
    </div>

    <!-- Không có appointment -->
    <div v-else-if="!appointment && !showSearchForm">
      <p>
        Vui lòng tạo lịch khám để xem thông tin hoặc nếu bạn đã có lịch khám,
        hãy tìm kiếm lịch khám của bạn!
      </p>
    </div>

    <!-- Form tìm kiếm -->
    <div v-if="showSearchForm" class="search-form">
      <h3>Tìm lịch khám của bạn</h3>

      <label>Mã lịch khám</label>
      <input v-model="searchCode" placeholder="Nhập mã" />

      <label>Số điện thoại</label>
      <input v-model="searchPhone" placeholder="Nhập số điện thoại" />

      <div class="actions">
        <button @click="searchAppointment">Tìm kiếm</button>
        <button type="button" @click="closeSearch">Đóng</button>
      </div>

      <span v-if="searchError" class="error">
        {{ searchError }}
      </span>
    </div>

    <!-- Form hủy -->
    <div v-if="showCancelForm && appointment" class="cancel-form">
      <h3>Hủy lịch khám</h3>

      <p>Nhập mã lịch khám để xác nhận hủy:</p>

      <input v-model="enteredCode" placeholder="Mã lịch khám" />

      <div class="actions">
        <button @click="confirmCancel">Xác nhận hủy</button>
        <button type="button" @click="showCancelForm = false">Đóng</button>
      </div>

      <span v-if="cancelError" class="error">
        {{ cancelError }}
      </span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue"
import { useRouter } from "vue-router"
import axios from "axios"

const router = useRouter()

const appointment = ref<any>(null)

const showCancelForm = ref(false)
const enteredCode = ref("")
const cancelError = ref("")

const appointmentCode = localStorage.getItem("appointmentCode")

onMounted(async () => {
  if (!appointmentCode) return

  try {
    const res = await axios.get(
      `https://localhost:7235/api/Appointments/${appointmentCode}`
    )

    appointment.value = res.data
  } catch (err) {
    console.error("Load appointment failed", err)
  }
})

const goBack = () => router.push("/appointment")

/* ================= SEARCH ================= */

const showSearchForm = ref(false)
const searchCode = ref("")
const searchPhone = ref("")
const searchError = ref("")

const searchAppointment = async () => {
  searchError.value = ""

  if (!searchCode.value || !searchPhone.value) {
    searchError.value = "Vui lòng nhập đầy đủ thông tin."
    return
  }

  try {
    const res = await axios.get(
      `https://localhost:7235/api/Appointments/${searchCode.value}`
    )

    if (res.data.phone === searchPhone.value) {
      appointment.value = res.data
      showSearchForm.value = false
    } else {
      searchError.value = "Số điện thoại không khớp."
    }
  } catch (err) {
    searchError.value = "Lịch khám không tìm thấy."
  }
}

const closeSearch = () => {
  showSearchForm.value = false
  searchCode.value = ""
  searchPhone.value = ""
  searchError.value = ""
}

/* ================= CANCEL ================= */

const confirmCancel = async () => {
  if (!appointment.value) return

  if (enteredCode.value !== appointment.value.appointmentCode) {
    cancelError.value = "Mã không đúng. Vui lòng thử lại."
    return
  }

  if (!confirm("Bạn có chắc chắn muốn hủy lịch khám?")) return

  try {
    const payload = {
      appointmentCode: appointment.value.appointmentCode,
      fullName: appointment.value.fullName,
      phone: appointment.value.phone
    }

    await axios.post(
      "https://localhost:7235/api/Appointments/cancel",
      payload
    )

    appointment.value.status = "Cancelled"
    showCancelForm.value = false
    cancelError.value = ""

    alert("Lịch khám của bạn đã bị hủy.")
  } catch (err) {
    cancelError.value = "Không thể hủy lịch khám. Vui lòng thử lại."
  }
}

/* ================= FORMAT ================= */

const formatDate = (dateStr: string) => {
  if (!dateStr) return ""
  return new Date(dateStr).toLocaleDateString("vi-VN")
}

const formatTime = (timeStr: string) => {
  if (!timeStr) return ""
  return timeStr.substring(0, 5)
}

const formatDateTime = (dateStr: string) => {
  if (!dateStr) return ""
  return new Date(dateStr).toLocaleString("vi-VN")
}
</script>

<style src="@/styles/layouts/appointment.css"></style>