<template>
  <div class="staff-container">
    <h2>Quản lý Bác sĩ</h2>

    <!-- SEARCH -->
    <input
      v-model="searchKeyword"
      class="staff-search"
      placeholder="Tìm bác sĩ..."
    />

    <table>
      <thead>
        <tr>
          <th>Tên</th>
          <th>Chuyên khoa</th>
          <th>Email</th>
          <th>Số điện thoại</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="d in filteredDoctors" :key="d.id">
          <td>{{ d.name }}</td>
          <td>{{ d.specialty }}</td>
          <td>{{ d.email }}</td>
          <td>{{ d.phone }}</td>
        </tr>
      </tbody>
    </table>

    <p v-if="filteredDoctors.length === 0">
      Không có bác sĩ nào
    </p>
  </div>
</template>

<script setup lang="ts">
import axios from 'axios'
import { ref, onMounted, computed } from 'vue'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const doctors = ref<any[]>([])
const searchKeyword = ref('')

const api = axios.create({
  baseURL: 'https://localhost:7235/api',
  headers: { Authorization: `Bearer ${auth.token}` }
})

const loadDoctors = async () => {
  const res = await api.get('/Doctors')
  doctors.value = res.data
}

const filteredDoctors = computed(() => {
  if (!searchKeyword.value) return doctors.value
  return doctors.value.filter(d =>
    d.name.toLowerCase().includes(searchKeyword.value.toLowerCase())
  )
})

onMounted(loadDoctors)
</script>
