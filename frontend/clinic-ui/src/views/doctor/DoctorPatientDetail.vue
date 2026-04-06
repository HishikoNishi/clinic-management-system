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
              <div class="mb-3">
                <div class="text-muted small">Xét nghiệm</div>
                <div v-if="testsLoading" class="text-muted small">
                  <span class="spinner-border spinner-border-sm me-1"></span>Đang tải xét nghiệm...
                </div>
                <div v-else-if="testsError" class="alert alert-warning py-2">{{ testsError }}</div>
                <ul v-else class="list-group list-group-flush">
                  <li v-for="t in tests" :key="t.id" class="list-group-item px-0">
                    <div class="d-flex justify-content-between">
                      <div>
                        <div class="fw-semibold">{{ t.testName }}</div>
                        <div class="text-muted small">{{ t.result || 'Chưa có kết quả' }}</div>
                      </div>
                      <div class="text-muted small text-end">
                        <div>{{ t.technicianName || '—' }}</div>
                        <div>{{ formatDate(t.createdAt) }}</div>
                      </div>
                    </div>
                  </li>
                  <li v-if="tests.length === 0" class="list-group-item px-0 text-muted small">
                    Chưa có xét nghiệm nào.
                  </li>
                </ul>
              </div>
              <div class="text-muted small">Các lần khám trước</div>
              <ul class="list-group list-group-flush history-list">
                <li v-for="r in olderRecords" :key="r.id" class="list-group-item px-0">
                  <div class="d-flex justify-content-between">
                    <div>
                      <div class="fw-semibold">{{ r.diagnosis }}</div>
                      <div class="text-muted small">{{ r.note }}</div>
                    </div>
                    <div class="text-end">
                      <span class="text-muted small d-block">{{ formatDate(r.createdAt) }}</span>
                      <button class="btn btn-link p-0 small" @click="loadRecordDetail(r.id)">Xem chi tiết</button>
                    </div>
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

    <div v-if="selectedRecord" class="overlay" @click="selectedRecord = null">
      <div class="overlay-card" @click.stop>
        <div class="d-flex justify-content-between align-items-start mb-2">
          <div>
            <h6 class="mb-1">Hồ sơ chi tiết</h6>
            <div class="text-muted small">Ngày: {{ formatDate(selectedRecord.createdAt) || '—' }}</div>
          </div>
          <button class="btn btn-sm btn-outline-secondary" @click="selectedRecord = null">Đóng</button>
        </div>
        <div class="small mb-1">Chẩn đoán: <strong>{{ selectedRecord.diagnosis || '—' }}</strong></div>
        <div class="small mb-1">Ghi chú: {{ selectedRecord.note || '—' }}</div>
        <div class="small mb-1">BHYT: {{ Math.round((selectedRecord.insuranceCoverPercent || 0) * 100) }}%</div>
        <div class="small mb-1">Phụ thu/Giảm trừ: {{ selectedRecord.surcharge }} / {{ selectedRecord.discount }}</div>
        <div class="mt-2">
          <div class="fw-semibold">Đơn thuốc</div>
          <ul class="small mb-2">
            <li v-for="(p, idx) in selectedRecord.prescriptionItems || []" :key="idx">
              {{ p.medicineName }} ({{ p.dosage }}), SL {{ p.quantity }}
            </li>
            <li v-if="!selectedRecord.prescriptionItems?.length" class="text-muted">Không có</li>
          </ul>
        </div>
        <div class="mt-2">
          <div class="fw-semibold">Xét nghiệm</div>
          <ul class="small mb-0">
            <li v-for="t in selectedTests" :key="t.id">
              {{ t.testName }} — {{ t.result || 'Chưa có kết quả' }} <span class="text-muted">({{ t.technicianName || '—' }})</span>
            </li>
            <li v-if="selectedTests.length === 0" class="text-muted">Không có</li>
          </ul>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.overlay {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.6);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1050;
  padding: 16px;
}
.overlay-card {
  background: #fff;
  border-radius: 10px;
  box-shadow: 0 12px 30px rgba(0,0,0,0.25);
  max-width: 640px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
  padding: 16px;
}
</style>
<script setup lang="ts">
import { computed, onMounted, reactive, ref } from "vue"
import { useRoute, useRouter } from "vue-router"
import api from "@/services/api"

const route = useRoute()
const router = useRouter()
const patientId = route.params.id as string

const patient = reactive<any>({})
const records = ref<any[]>([])
const selectedRecord = ref<any | null>(null)
const selectedTests = ref<any[]>([])
const recordsLoading = ref(false)
const recordsError = ref<string | null>(null)
const error = ref<string | null>(null)
const tests = ref<any[]>([])
const testsLoading = ref(false)
const testsError = ref<string | null>(null)

const age = computed(() => {
  if (!patient.dateOfBirth) return ""
  const d = new Date(patient.dateOfBirth)
  const diff = Date.now() - d.getTime()
  return Math.max(0, Math.floor(diff / (365.25 * 24 * 60 * 60 * 1000)))
})

const latestRecord = computed(() => records.value[0] || null)
const olderRecords = computed(() => records.value.slice(1))

const formatDate = (d?: string) => {
  if (!d) return ""
  const date = new Date(d)
  const dd = String(date.getDate()).padStart(2, "0")
  const mm = String(date.getMonth() + 1).padStart(2, "0")
  const yyyy = date.getFullYear()
  const hh = String(date.getHours()).padStart(2, "0")
  const mi = String(date.getMinutes()).padStart(2, "0")
  return `${dd}/${mm}/${yyyy} ${hh}:${mi}`
}

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
    if (records.value.length) {
      await loadTests(records.value[0].id)
    }
  } catch (err: any) {
    console.error(err)
    recordsError.value = err?.response?.data?.message || "Không tải được hồ sơ"
  } finally {
    recordsLoading.value = false
  }
}

const loadTests = async (recordId: string) => {
  testsLoading.value = true
  testsError.value = null
  try {
    const res = await api.get(`/ClinicalTests/medical-record/${recordId}`)
    tests.value = res.data ?? []
  } catch (err: any) {
    console.error(err)
    testsError.value = err?.response?.data?.message || "Không tải được xét nghiệm"
  } finally {
    testsLoading.value = false
  }
}

const loadRecordDetail = async (recordId: string) => {
  try {
    const res = await api.get(`/medical-record/${recordId}`)
    selectedRecord.value = res.data
    const testRes = await api.get(`/ClinicalTests/medical-record/${recordId}`)
    selectedTests.value = testRes.data ?? []
  } catch (err: any) {
    alert(err?.response?.data?.message || "Không tải được hồ sơ")
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
