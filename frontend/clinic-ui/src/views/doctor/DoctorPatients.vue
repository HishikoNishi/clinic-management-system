<template>
  <div class="doctor-container">
    <div class="header">
      <div>
        <h2>Bệnh nhân của tôi</h2>
        <p class="text-muted mb-0">Tìm kiếm nhanh theo tên / số điện thoại, xem lại hồ sơ đã khám.</p>
      </div>
      <div class="filter">
        <input
          v-model="keyword"
          type="search"
          class="form-control form-control-sm"
          placeholder="Tìm tên hoặc SĐT..."
          @input="applyFilter"
        />
      </div>
    </div>

    <div v-if="error" class="alert alert-danger py-2">{{ error }}</div>

    <div class="card">
      <div class="card-body p-0">
        <table class="table mb-0 align-middle">
          <thead>
            <tr>
               <th>Mã bệnh nhân</th>
              <th>Họ tên</th>
              <th>SĐT</th>

              <th>Tuổi</th>
             
    <th>CCCD</th>
    <th>BHYT</th>
              <th>Ghi chú</th>
              <th class="text-end">Hành động</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loadingPatients">
              <td colspan="5" class="text-center py-3 text-muted">
                <span class="spinner-border spinner-border-sm me-2" />Đang tải danh sách...
              </td>
            </tr>
            <tr v-else-if="filteredPatients.length === 0">
              <td colspan="5" class="text-center py-3 text-muted">Không có bệnh nhân.</td>
            </tr>
            <tr v-else v-for="p in filteredPatients" :key="p.id">
                <td>{{ p.patientCode || '—' }}</td>
              <td class="fw-semibold">{{ p.fullName }}</td>
              <td>{{ p.phone }}</td>
              <td>{{ calcAge(p.dateOfBirth) }}</td>
            
              <td>{{ p.citizenId || '—' }}</td>
              <td>{{ p.insuranceCardNumber || '—' }}</td>
              <td>{{ p.note }}</td>
              <td class="text-end">
                <button class="btn btn-outline-primary btn-sm" @click="goDetail(p.id)">
                  <i class="bi bi-folder2-open me-1" /> Hồ sơ
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue"
import { useRouter } from "vue-router"
import api from "@/services/api"

const patients = ref<any[]>([])
const loadingPatients = ref(false)
const error = ref<string | null>(null)
const keyword = ref("")
const router = useRouter()

const filteredPatients = computed(() => {
  if (!keyword.value.trim()) return patients.value
  const term = keyword.value.toLowerCase()
  return patients.value.filter(
    (p) =>
      p.fullName?.toLowerCase().includes(term) ||
      p.phone?.toLowerCase().includes(term)
  )
})

const loadPatients = async () => {
  loadingPatients.value = true
  error.value = null
  try {
    const res = await api.get("/patients/my")
    patients.value = res.data ?? []
  } catch (err: any) {
    console.error(err)
    error.value = err?.response?.data?.message || "Không tải được danh sách bệnh nhân"
  } finally {
    loadingPatients.value = false
  }
}

const calcAge = (dob?: string) => {
  if (!dob) return ""
  const d = new Date(dob)
  const diff = Date.now() - d.getTime()
  return Math.max(0, Math.floor(diff / (365.25 * 24 * 60 * 60 * 1000)))
}

const applyFilter = () => {
  // computed handles filtering; keep method for future debounce if needed
}

const goDetail = (id: string) => {
  router.push(`/doctor/patients/${id}`)
}

onMounted(() => {
  loadPatients()
})
</script>

<style src="@/styles/layouts/doctor-patients.css"></style>
