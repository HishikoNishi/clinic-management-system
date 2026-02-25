<template>
  <div class="staff-container">
    <h2>Quản lý Bệnh nhân</h2>

    <!-- SEARCH -->
    <input
      v-model="searchKeyword"
      class="staff-search"
      placeholder="Tìm bệnh nhân..."
    />

    <table>
      <thead>
        <tr>
          <th>Mã bệnh nhân</th>
          <th>Tên</th>
          <th>Ngày sinh</th>
          <th>Email</th>
          <th>Số điện thoại</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="p in filteredPatients" :key="p.id">
          <td>{{ p.code }}</td>
          <td>{{ p.name }}</td>
          <td>{{ formatDate(p.birthDate) }}</td>
          <td>{{ p.email }}</td>
          <td>{{ p.phone }}</td>
        </tr>
      </tbody>
    </table>

    <p v-if="filteredPatients.length === 0">
      Không có bệnh nhân nào
    </p>
  </div>
</template>

<script setup lang="ts">
import axios from 'axios'
import { ref, onMounted, computed } from 'vue'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const patients = ref<any[]>([])
const searchKeyword = ref('')

const api = axios.create({
  baseURL: 'https://localhost:7235/api',
  headers: { Authorization: `Bearer ${auth.token}` }
})

const loadPatients = async () => {
  const res = await api.get('/Patients')
  patients.value = res.data
}

const filteredPatients = computed(() => {
  if (!searchKeyword.value) return patients.value
  return patients.value.filter(p =>
    p.name.toLowerCase().includes(searchKeyword.value.toLowerCase())
  )
})

const formatDate = (d: string) =>
  new Date(d).toLocaleDateString()

onMounted(loadPatients)
</script>
