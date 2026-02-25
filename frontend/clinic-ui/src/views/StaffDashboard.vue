<template>
  <div class="staff-dashboard">
    <h2>Staff Dashboard</h2>

    <div class="grid">
      <!-- Card bác sĩ -->
      <div class="card" @click="$router.push('/doctors')">
        <h3>Quản lý Bác sĩ</h3>
        <p>Xem danh sách và thông tin bác sĩ</p>
      </div>

      <!-- Card bệnh nhân -->
      <div class="card" @click="$router.push('/patients')">
        <h3>Quản lý Bệnh nhân</h3>
        <p>Xem danh sách và hồ sơ bệnh nhân</p>
      </div>

      <!-- Card lịch hẹn -->
      <div class="card" @click="$router.push('/appointments')">
        <h3>Quản lý Lịch hẹn</h3>
        <p>Theo dõi và cập nhật lịch hẹn</p>
      </div>
    </div>
  </div>
</template>

<script>
import { createRouter, createWebHistory } from 'vue-router'
import Dashboard from '@/views/Dashboard.vue'
import DoctorPage from '@/views/DoctorPage.vue'
import PatientPage from '@/views/PatientPage.vue'
import AppointmentView from '@/views/AppointmentView.vue'

const routes = [
  { path: '/', component: Dashboard },
  { path: '/doctors', component: DoctorPage },
  { path: '/patients', component: PatientPage },
  { path: '/appointments', component: AppointmentView }
]

export default createRouter({
  history: createWebHistory(),
  routes
})

</script>


