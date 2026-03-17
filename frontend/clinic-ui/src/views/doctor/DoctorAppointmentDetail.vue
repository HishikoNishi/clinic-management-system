<template>
  <div class="appointment-detail">
    <h2>Chi tiết lịch khám</h2>
    <div v-if="appointment" class="detail-card">
      <div class="detail-row"><span class="label">Mã:</span> {{ appointment.appointmentCode }}</div>
      <div class="detail-row"><span class="label">Bệnh nhân:</span> {{ appointment.fullName }}</div>
      <div class="detail-row"><span class="label">Điện thoại:</span> {{ appointment.phone }}</div>
      <div class="detail-row"><span class="label">Email:</span> {{ appointment.email }}</div>
      <div class="detail-row"><span class="label">Ngày sinh:</span> {{ formatDate(appointment.dateOfBirth) }}</div>
      <div class="detail-row"><span class="label">Giới tính:</span> {{ appointment.gender }}</div>
      <div class="detail-row"><span class="label">Địa chỉ:</span> {{ appointment.address }}</div>
      <div class="detail-row"><span class="label">Lý do:</span> {{ appointment.reason }}</div>
      <div class="detail-row">
        <span class="label">Trạng thái:</span>
        <span :class="'status ' + appointment.status.toLowerCase()">{{ appointment.status }}</span>
      </div>
      <div v-if="appointment.statusDetail.doctorName" class="detail-row">
        <span class="label">Bác sĩ:</span> {{ appointment.statusDetail.doctorName }} ({{ appointment.statusDetail.doctorCode }})
      </div>
    </div>
    <p v-else-if="error" class="error">{{ error }}</p>
    <button class="back-btn" @click="$router.push('/doctorappointment')">← Quay lại danh sách</button>
  </div>
</template>

<script setup lang="ts">
import axios from 'axios'
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const route = useRoute()
const auth = useAuthStore()

const api = axios.create({
  baseURL: 'https://localhost:7235/api',
  headers: { Authorization: `Bearer ${auth.token}` }
})

const appointment = ref<any>(null)
const error = ref<string | null>(null)

const loadDetail = async () => {
  try {
    const res = await api.get(`/doctor/DoctorAppointments/${route.params.id}`)
    appointment.value = res.data
  } catch (err) {
    error.value = "Bạn không được phép xem lịch khám này."
  }
}

const formatDate = (dateStr: string) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  return date.toLocaleDateString()
}

onMounted(() => {
  loadDetail()
})
</script>

<style src="@/styles/layouts/appointment-detail.css"></style>
