<template>
  <div class="doctor-container">
    <h2>My Appointments</h2>

    <!-- SEARCH BAR -->
    <div class="search-bar">
      <input v-model="searchCode" placeholder="Search by code..." />
      <input v-model="searchName" placeholder="Search by patient name..." />
      <input v-model="searchPhone" placeholder="Search by phone..." />
      <input type="date" v-model="searchDate" />

      <button class="search-btn">🔍</button>
      <button class="clear-btn" @click="clearFilters">Clear</button>
    </div>

    <!-- FILTER STATUS -->
    <div class="filter">
      <button
        v-for="s in statuses"
        :key="s"
        :class="{ active: currentStatus === s }"
        @click="changeStatus(s)"
      >
        {{ s }}
      </button>
    </div>

    <!-- TABLE -->
    <table>
      <thead>
        <tr>
          <th>Code</th>
          <th>Patient</th>
          <th>Date</th>
          <th>Status</th>
          <th>Action</th>
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
          <td>{{ formatDateTime(a.appointmentDate, a.appointmentTime) }}</td>

          <td>
            <span :class="'status ' + a.status.toLowerCase()">
              {{ a.status }}
            </span>
          </td>

          <td>
            <button
              v-if="a.status === 'Confirmed'"
              @click.stop="completeAppointment(a.id)"
            >
              Completed this appointment
            </button>
          </td>
        </tr>
      </tbody>
    </table>

    <p v-if="filteredAppointments.length === 0">No appointments</p>
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
  const ok = confirm("Are you sure you want to mark this appointment as completed?")
  if (!ok) return

  await api.patch(`/doctor/DoctorAppointments/${appointmentId}/complete`)
  alert("Status updated ✅")

  loadAppointments()
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