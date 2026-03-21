<template>
  <div class="patient-detail-page">
    <div class="row g-3">
      <div class="col-12 col-lg-4">
        <div class="card h-100 shadow-sm">
          <div class="card-header d-flex align-items-center justify-content-between">
            <h6 class="mb-0">Thông tin cá nhân</h6>
            <span class="badge bg-primary-subtle text-primary">Bệnh nhân</span>
          </div>
          <div class="card-body">
            <div class="d-flex align-items-center mb-3">
              <div class="avatar bg-primary-subtle text-primary rounded-circle me-3">
                <i class="bi bi-person"></i>
              </div>
              <div>
                <h5 class="mb-0">{{ patient.fullName }}</h5>
                <div class="text-muted small">{{ patient.phone }}</div>
              </div>
            </div>
            <dl class="row small mb-0">
              <dt class="col-5 text-muted">Tuổi</dt>
              <dd class="col-7 fw-semibold">{{ age }}</dd>

              <dt class="col-5 text-muted">Giới tính</dt>
              <dd class="col-7 fw-semibold">{{ patient.gender || '—' }}</dd>

              <dt class="col-5 text-muted">Email</dt>
              <dd class="col-7">{{ patient.email || '—' }}</dd>

              <dt class="col-5 text-muted">Địa chỉ</dt>
              <dd class="col-7">{{ patient.address || '—' }}</dd>

              <dt class="col-5 text-muted">Ghi chú</dt>
              <dd class="col-7">{{ patient.note || '—' }}</dd>
            </dl>
          </div>
        </div>
      </div>

      <div class="col-12 col-lg-8">
        <div class="card shadow-sm">
          <div class="card-header d-flex justify-content-between align-items-center">
            <h6 class="mb-0">Bệnh án gần nhất</h6>
            <small class="text-muted" v-if="latestRecord">Cập nhật: {{ formatDate(latestRecord.createdAt) }}</small>
          </div>
          <div class="card-body">
            <div v-if="recordsLoading" class="text-muted">
              <span class="spinner-border spinner-border-sm me-2"></span>Đang tải hồ sơ...
            </div>
            <div v-else-if="recordsError" class="alert alert-danger py-2">{{ recordsError }}</div>
            <div v-else-if="!latestRecord" class="text-muted">Chưa có hồ sơ khám nào.</div>
            <div v-else>
              <div class="mb-3">
                <div class="text-muted small">Chẩn đoán</div>
                <div class="fw-semibold">{{ latestRecord.diagnosis }}</div>
              </div>
              <div class="mb-3">
                <div class="text-muted small">Ghi chú</div>
                <div>{{ latestRecord.note || '—' }}</div>
              </div>
              <div class="text-muted small">Các lần khám trước</div>
              <ul class="list-group list-group-flush history-list">
                <li v-for="r in olderRecords" :key="r.id" class="list-group-item px-0">
                  <div class="d-flex justify-content-between">
                    <div>
                      <div class="fw-semibold">{{ r.diagnosis }}</div>
                      <div class="text-muted small">{{ r.note }}</div>
                    </div>
                    <span class="text-muted small">{{ formatDate(r.createdAt) }}</span>
                  </div>
                </li>
                <li v-if="olderRecords.length === 0" class="list-group-item px-0 text-muted small">Không có thêm hồ sơ.</li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </div>

    <button class="btn btn-outline-secondary mt-3" @click="goBack">
      <i class="bi bi-arrow-left me-1"></i> Quay lại danh sách
    </button>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from "vue"
import { useRoute, useRouter } from "vue-router"
import api from "@/services/api"

const route = useRoute()
const router = useRouter()
const patientId = route.params.id as string

const patient = reactive<any>({})
const records = ref<any[]>([])
const recordsLoading = ref(false)
const recordsError = ref<string | null>(null)
const error = ref<string | null>(null)

const age = computed(() => {
  if (!patient.dateOfBirth) return ""
  const d = new Date(patient.dateOfBirth)
  const diff = Date.now() - d.getTime()
  return Math.max(0, Math.floor(diff / (365.25 * 24 * 60 * 60 * 1000)))
})

const latestRecord = computed(() => records.value[0] || null)
const olderRecords = computed(() => records.value.slice(1))

const formatDate = (d?: string) => (d ? new Date(d).toLocaleString() : "")

const loadPatient = async () => {
  error.value = null
  try {
    const res = await api.get(`/patients/${patientId}`)
    Object.assign(patient, res.data || {})
  } catch (err: any) {
    console.error(err)
    error.value = err?.response?.data?.message || "Không tải được thông tin bệnh nhân"
  }
}

const loadRecords = async () => {
  recordsLoading.value = true
  recordsError.value = null
  try {
    const res = await api.get(`/medical-record/patient/${patientId}`)
    records.value = res.data ?? []
  } catch (err: any) {
    console.error(err)
    recordsError.value = err?.response?.data?.message || "Không tải được hồ sơ"
  } finally {
    recordsLoading.value = false
  }
}

const goBack = () => router.push("/doctor/patients")

onMounted(() => {
  loadPatient()
  loadRecords()
})
</script>

<style scoped>
.patient-detail-page {
  padding: 20px;
}
.avatar {
  width: 44px;
  height: 44px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 1.25rem;
}
.history-list {
  max-height: 220px;
  overflow-y: auto;
}
</style>
