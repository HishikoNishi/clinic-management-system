<template>
  <div class="staff-container">
    <h2>Staff Dashboard</h2>

    <div class="layout">

      <!-- LEFT -->
      <div class="doctor-list">
        <h3>Doctors</h3>

        <!-- SEARCH -->
        <input
          v-model="searchKeyword"
          class="doctor-search"
          placeholder="Search doctor name..."
        />

        <div
          v-for="doctor in filteredDoctors"
          :key="doctor.id"
          class="doctor-item"
          @click="selectDoctor(doctor)"
        >
          <b>{{ doctor.name }}</b>
          <p>{{ doctor.specialty }}</p>
        </div>
      </div>

      <!-- RIGHT -->
      <div class="patient-list" v-if="selectedDoctor">
        <h3>Patients of {{ selectedDoctor.name }}</h3>

        <table>
          <thead>
            <tr>
              <th>Code</th>
              <th>Patient</th>
              <th>Reason</th>
              <th>Date</th>
              <th>Status</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="p in patients" :key="p.appointmentId">
              <td>{{ p.appointmentCode }}</td>
              <td>{{ p.patientName }}</td>
              <td>{{ p.reason }}</td>
              <td>{{ formatDate(p.date) }}</td>
              <td>{{ p.status }}</td>
            </tr>
          </tbody>
        </table>

        <p v-if="patients.length === 0">
          No patients assigned
        </p>
      </div>

    </div>
  </div>
</template>
<script setup lang="ts">
import axios from 'axios'
import { ref, onMounted, computed } from 'vue'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()

const doctors = ref<any[]>([])
const patients = ref<any[]>([])
const selectedDoctor = ref<any>(null)
const searchKeyword = ref('') // ✅ thiếu cái này nãy giờ

const api = axios.create({
  baseURL: 'https://localhost:7235/api',
  headers: {
    Authorization: `Bearer ${auth.token}`
  }
})

// filter doctor realtime
const filteredDoctors = computed(() => {
  if (!searchKeyword.value) return doctors.value

  return doctors.value.filter(d =>
    d.name.toLowerCase().includes(
      searchKeyword.value.toLowerCase()
    )
  )
})

// load doctors
const loadDoctors = async () => {
  const res = await api.get('/Doctors')
  doctors.value = res.data
}

// click doctor
const selectDoctor = async (doctor: any) => {
  selectedDoctor.value = doctor

  const res = await api.get(
    `/Doctors/${doctor.id}/patients`
  )

  patients.value = res.data.patients
}

const formatDate = (d: string) =>
  new Date(d).toLocaleDateString()

onMounted(loadDoctors)
</script>