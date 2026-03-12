<template>
  <div class="appointment-container">
    <h2>Lịch khám của tôi</h2>

    <!-- Link để mở form tìm kiếm -->
    <p class="find-link" @click="showSearchForm = true">
      🔎 Tìm lịch khám của bạn
    </p>

   <div v-if="appointment && !showSearchForm" class="appointment-detail">
  <p><strong>Mã:</strong> {{ appointment.appointmentCode }}</p>
  <p><strong>Tên:</strong> {{ appointment.fullName }}</p>
  <p><strong>Điện thoại:</strong> {{ appointment.phone }}</p>
  <p><strong>Email:</strong> {{ appointment.email }}</p>
  <p><strong>Ngày sinh:</strong> {{ new Date(appointment.dateOfBirth).toLocaleDateString('vi-VN') }}</p>
  <p><strong>Giới tính:</strong> {{ appointment.gender }}</p>
  <p><strong>Địa chỉ:</strong> {{ appointment.address }}</p>
  <p><strong>Trạng thái:</strong> {{ appointment.status }}</p>
  <p><strong>Ngày khám:</strong> {{ new Date(appointment.appointmentDate).toLocaleDateString('vi-VN') }}</p>
  <p><strong>Thời gian khám:</strong> {{ appointment.appointmentTime.substring(0,5) }}</p>
  <p><strong>Lý do:</strong> {{ appointment.reason }}</p>
  <p><strong>Tạo lúc:</strong> {{ new Date(appointment.createdAt).toLocaleString('vi-VN') }}</p>

  <div class="actions">
    <button @click="goBack">Quay lại</button>
    <button class="cancel" @click="showCancelForm = true">Hủy</button>
  </div>
</div>


    <!-- Nếu chưa có appointment -->
    <div v-else-if="!appointment && !showSearchForm">
      <p>
        Vui lòng tạo lịch khám để xem thông tin hoặc nếu bạn đã có lịch khám, hãy tìm kiếm lịch khám của bạn!
     
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
      <span v-if="searchError" class="error">{{ searchError }}</span>
    </div>

    <!-- Form cancel -->
    <div v-if="showCancelForm" class="cancel-form">
      <h3>Hủy lịch khám</h3>
      <p>Nhập mã lịch khám để xác nhận hủy:</p>
      <input v-model="enteredCode" placeholder="Mã lịch khám" />
      <div class="actions">
        <button @click="confirmCancel">Xác nhận hủy</button>
        <button type="button" @click="showCancelForm = false">Đóng</button>
      </div>
      <span v-if="cancelError" class="error">{{ cancelError }}</span>
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
  if (appointmentCode) {
    const res = await axios.get(`https://localhost:7235/api/Appointments/${appointmentCode}`)
    appointment.value = res.data
  }
})

const goBack = () => router.push("/appointment")

// Search form
const showSearchForm = ref(false)
const searchCode = ref("")
const searchPhone = ref("")
const searchError = ref("")

const searchAppointment = async () => {
  try {
    const res = await axios.get(`https://localhost:7235/api/Appointments/${searchCode.value}`)
    if (res.data.phone === searchPhone.value) {
      appointment.value = res.data
      showSearchForm.value = false
      searchError.value = ""
    } else {
      searchError.value = "Số điện thoại không khớp."
    }
  } catch (err: any) {
    searchError.value = "Lịch khám không tìm thấy."
  }
}

const closeSearch = () => {
  showSearchForm.value = false
  searchCode.value = ""
  searchPhone.value = ""
  searchError.value = ""
}

// Cancel form
const confirmCancel = async () => {
  if (enteredCode.value === appointment.value.appointmentCode) {
    if (window.confirm("Bạn có chắc chắn muốn hủy lịch khám?")) {
      try {
        const payload = {
          appointmentCode: appointment.value.appointmentCode,
          fullName: appointment.value.fullName,
          phone: appointment.value.phone
        }
        await axios.post("https://localhost:7235/api/Appointments/cancel", payload)
        appointment.value.status = "Cancelled"
        showCancelForm.value = false
        cancelError.value = ""
        alert("Lịch khám của bạn đã bị hủy.")
      } catch (err: any) {
        cancelError.value = "Không thể hủy lịch khám. Vui lòng thử lại."
      }
    }
  } else {
    cancelError.value = "Mã không đúng. Vui lòng thử lại."
  }
}
</script>
<style src="@/styles/layouts/appointment.css"></style>