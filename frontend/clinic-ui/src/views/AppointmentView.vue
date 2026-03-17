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

<style src="@/styles/layouts/appointment.css"></style>
