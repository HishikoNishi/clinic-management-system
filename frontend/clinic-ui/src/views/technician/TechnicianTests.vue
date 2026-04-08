<template>
  <div class="tech-tests">
    <div class="card tech-card">
      <div class="card-header tech-header">
        <div class="tech-title">
          <div class="tech-eyebrow">Technician</div>
          <h5 class="mb-1">Xét nghiệm</h5>
          <small class="text-muted">Chỉ hiển thị các yêu cầu đã thanh toán</small>
        </div>

        <div class="tech-controls">
          <div class="btn-group btn-group-sm" role="group" aria-label="Chế độ xem">
            <button
              type="button"
              class="btn"
              :class="viewMode === 'pending' ? 'btn-primary' : 'btn-outline-primary'"
              @click="switchMode('pending')"
            >
              <i class="bi bi-list-check me-1"></i>
              Đang chờ
            </button>
            <button
              type="button"
              class="btn"
              :class="viewMode === 'history' ? 'btn-primary' : 'btn-outline-primary'"
              @click="switchMode('history')"
            >
              <i class="bi bi-clock-history me-1"></i>
              Lịch sử
            </button>
          </div>

          <div class="d-flex align-items-center gap-2 flex-wrap">
          <div class="tech-select">
            <i class="bi bi-building"></i>
            <select
              class="form-select form-select-sm"
              v-model="selectedDepartmentId"
                @change="handleDepartmentChange"
                aria-label="Chọn khoa"
              >
                <option value="">Tất cả khoa</option>
                <option v-for="d in departments" :key="d.id" :value="d.id">
                  {{ d.name }}
                </option>
              </select>
            </div>

            <div class="tech-search">
              <i class="bi bi-search"></i>
              <input
                v-model="patientQuery"
                type="text"
                class="form-control form-control-sm"
                placeholder="Tìm bệnh nhân (tên / SĐT)..."
              />
            </div>

            <div v-if="viewMode === 'history'" class="tech-search">
              <i class="bi bi-calendar"></i>
              <input
                v-model="historyDateFilter"
                type="date"
                class="form-control form-control-sm"
                aria-label="Lọc theo ngày hoàn thành"
              />
            </div>

            <button class="btn btn-outline-secondary btn-sm" :disabled="loading" @click="loadTests">
              <span v-if="loading" class="spinner-border spinner-border-sm me-1" aria-hidden="true"></span>
              <i v-else class="bi bi-arrow-clockwise me-1" aria-hidden="true"></i>
              Làm mới
            </button>
          </div>
        </div>
      </div>

      <div class="card-body p-0">
        <div v-if="uiError" class="alert alert-danger m-3 mb-0">
          <i class="bi bi-exclamation-triangle me-2"></i>
          {{ uiError }}
        </div>

        <div class="row g-0 tech-shell">
          <div class="col-12 col-lg-4 tech-pane border-end">
            <div class="tech-pane-header">
              <div class="tech-pane-title">
                <span class="fw-semibold">Bệnh nhân</span>
                <span class="badge text-bg-light border ms-2">{{ filteredPatients.length }}</span>
              </div>
              <div class="text-muted small">
                {{ viewMode === 'pending' ? 'Danh sách chờ xét nghiệm' : 'Danh sách đã hoàn tất' }}
              </div>
            </div>

            <ul class="list-group list-group-flush tech-list">
              <li v-if="loadingPatients" class="list-group-item text-muted small">
                <span class="spinner-border spinner-border-sm me-2" aria-hidden="true"></span>
                Đang tải danh sách bệnh nhân...
              </li>

              <li
                v-for="p in filteredPatients"
                :key="`${p.patientId}-${p.medicalRecordId}`"
                :class="[
                  'list-group-item list-group-item-action tech-patient',
                  selectedPatientId === p.patientId?.toString() ? 'active' : ''
                ]"
                role="button"
                @click="selectPatient(p)"
              >
                <div class="d-flex align-items-start justify-content-between gap-2">
                  <div class="min-w-0">
                    <div class="fw-semibold text-truncate">{{ p.fullName || '—' }}</div>
                    <div class="text-muted small text-truncate">
                      <i class="bi bi-telephone me-1" aria-hidden="true"></i>{{ p.phone || '—' }}
                    </div>
                    <div class="text-muted small text-truncate">
                      <i class="bi bi-file-earmark-medical me-1" aria-hidden="true"></i>{{ p.medicalRecordId || '—' }}
                    </div>
                    <div v-if="viewMode === 'history'" class="text-muted small">
                      <i class="bi bi-calendar3 me-1"></i>{{ p.recordDate || '—' }}
                    </div>
                  </div>
                  <span v-if="viewMode === 'pending'" class="badge rounded-pill text-bg-primary">
                    {{ p.pendingCount ?? 0 }}
                  </span>
                </div>
              </li>

              <li
                v-if="!loadingPatients && filteredPatients.length === 0"
                class="list-group-item text-muted small"
              >
                {{ viewMode === 'pending' ? 'Không có bệnh nhân chờ xét nghiệm.' : 'Không có lịch sử phù hợp.' }}
              </li>
            </ul>
          </div>

          <div class="col-12 col-lg-8 tech-content">
            <div class="tech-content-header">
              <div class="d-flex align-items-center justify-content-between gap-2 flex-wrap">
                <div class="min-w-0">
                  <div class="fw-semibold text-truncate">
                    {{ selectedPatient?.fullName ? `Bệnh nhân: ${selectedPatient.fullName}` : 'Chưa chọn bệnh nhân' }}
                  </div>
                  <div class="text-muted small text-truncate">
                    <span v-if="selectedPatient?.phone"><i class="bi bi-telephone me-1"></i>{{ selectedPatient.phone }}</span>
                    <span v-if="selectedPatient?.phone && selectedPatient?.medicalRecordId" class="mx-2">·</span>
                    <span v-if="selectedPatient?.medicalRecordId"><i class="bi bi-file-earmark-medical me-1"></i>{{ selectedPatient.medicalRecordId }}</span>
                  </div>
                </div>

                <div class="tech-search tech-search--tests">
                  <i class="bi bi-funnel"></i>
                  <input
                    v-model="testQuery"
                    type="text"
                    class="form-control form-control-sm"
                    placeholder="Lọc xét nghiệm (tên / record)..."
                  />
                </div>
              </div>
            </div>

            <div v-if="loadingTests" class="tech-empty">
              <div class="spinner-border text-primary" role="status" aria-label="Đang tải"></div>
              <div class="text-muted mt-2">Đang tải danh sách xét nghiệm...</div>
            </div>

            <div v-else-if="!selectedPatientId" class="tech-empty">
              <i class="bi bi-person-lines-fill tech-empty-icon" aria-hidden="true"></i>
              <div class="fw-semibold">Chọn bệnh nhân</div>
              <div class="text-muted">Chọn bệnh nhân ở danh sách bên trái để xem yêu cầu xét nghiệm.</div>
            </div>

            <div v-else-if="filteredTests.length === 0" class="tech-empty">
              <i class="bi bi-clipboard2-check tech-empty-icon" aria-hidden="true"></i>
              <div class="fw-semibold">Không có yêu cầu phù hợp</div>
              <div class="text-muted">
                {{ viewMode === 'pending' ? 'Bệnh nhân này chưa có xét nghiệm chờ / đang làm.' : 'Không có kết quả trong lịch sử.' }}
              </div>
            </div>

            <div v-else class="table-responsive tech-table">
              <table class="table align-middle mb-0">
                <thead class="table-light">
                  <tr>
                    <th>Tên xét nghiệm</th>
                    <th>Medical Record</th>
                    <th>Trạng thái</th>
                    <th style="width: 26%;">Kết quả</th>
                    <th style="width: 18%;">Kỹ thuật viên</th>
                    <th style="width: 90px;"></th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="t in filteredTests" :key="t.id" :class="{ 'tech-row-completed': t.status === 'Completed' }">
                    <td class="fw-semibold">{{ t.testName }}</td>
                    <td class="text-monospace small">{{ t.medicalRecordId }}</td>
                    <td>
                      <span :class="['badge', statusClass(t.status)]">
                        {{ statusLabel(t.status) }}
                      </span>
                    </td>
                    <td>
                      <input
                        v-model="t.result"
                        class="form-control form-control-sm"
                        :disabled="t.status === 'Completed'"
                        placeholder="Nhập kết quả..."
                      />
                    </td>
                    <td>
                      <input
                        v-model="t.technicianName"
                        class="form-control form-control-sm"
                        :disabled="t.status === 'Completed'"
                        placeholder="Tên KTV..."
                      />
                    </td>
                    <td class="text-end">
                      <div class="d-flex gap-1 justify-content-end">
                        <button
                          v-if="t.status === 'Pending'"
                          class="btn btn-outline-secondary btn-sm"
                          @click.stop="startTest(t)"
                          :disabled="savingId === t.id"
                        >
                          Bắt đầu
                        </button>
                        <button
                          v-if="t.status !== 'Completed'"
                          class="btn btn-primary btn-sm"
                          @click.stop="updateResult(t)"
                          :disabled="savingId === t.id"
                        >
                          <span v-if="savingId === t.id" class="spinner-border spinner-border-sm me-1" aria-hidden="true"></span>
                          <i v-else class="bi bi-save me-1" aria-hidden="true"></i>Lưu
                        </button>
                        <span v-else class="text-muted small">Đã lưu</span>
                      </div>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, onMounted, watch } from "vue"
import { useRoute, useRouter } from "vue-router"
import api from "@/services/api"

const tests = ref<any[]>([])
const patients = ref<any[]>([])
const historyRecords = ref<any[]>([])
const departments = ref<any[]>([])
const selectedDepartmentId = ref<string>("")
const selectedPatientId = ref<string | null>(null)
const selectedRecordId = ref<string | null>(null)
const historyDateFilter = ref<string>("")
const route = useRoute()
const router = useRouter()
const viewMode = ref<"pending" | "history">(route.path.includes("/history") ? "history" : "pending")
const uiError = ref<string>("")
const loadingPatients = ref(false)
const loadingTests = ref(false)
const savingId = ref<string | number | null>(null)
const patientQuery = ref("")
const testQuery = ref("")

const loading = computed(() => loadingPatients.value || loadingTests.value)

const currentEntries = computed(() =>
  viewMode.value === "pending" ? patients.value : historyRecords.value
)

const selectedPatient = computed(() => {
  if (!selectedPatientId.value) return null
  return currentEntries.value.find((p: any) => p.patientId?.toString() === selectedPatientId.value) || null
})

const filteredPatients = computed(() => {
  const q = patientQuery.value.trim().toLowerCase()
  return currentEntries.value.filter((p: any) => {
    if (viewMode.value === "history" && historyDateFilter.value) {
      if (p.recordDate !== historyDateFilter.value) return false
    }
    const name = (p.fullName || "").toString().toLowerCase()
    const phone = (p.phone || "").toString().toLowerCase()
    const record = (p.medicalRecordId || "").toString().toLowerCase()
    return !q || name.includes(q) || phone.includes(q) || record.includes(q)
  })
})

const filteredTests = computed(() => {
  const q = testQuery.value.trim().toLowerCase()
  if (!q) return tests.value
  return tests.value.filter((t: any) => {
    const name = (t.testName || "").toString().toLowerCase()
    const record = (t.medicalRecordId || "").toString().toLowerCase()
    const status = (t.status || "").toString().toLowerCase()
    return name.includes(q) || record.includes(q) || status.includes(q)
  })
})

const loadDepartments = async () => {
  try {
    const res = await api.get("/Departments")
    departments.value = res.data || []
  } catch (err: any) {
    console.log(err)
    uiError.value = err?.response?.data?.message || "Không tải được danh sách khoa."
  }
}

const loadTests = async () => {
  try {
    uiError.value = ""
    await Promise.all([loadPendingPatients(), loadHistoryPatients()])
    if (viewMode.value === "pending") {
      await loadTestsForSelected("Pending,InProgress", patients)
    } else {
      await loadTestsForSelected("Completed", historyRecords)
    }
  } catch (err: any) {
    console.log(err)
    uiError.value = err?.response?.data?.message || "Không tải được danh sách xét nghiệm."
  }
}

const loadPendingPatients = async () => {
  try {
    loadingPatients.value = true
    const pendingUrl = selectedDepartmentId.value
      ? "/ClinicalTests/pending-patients/by-department"
      : "/ClinicalTests/pending-patients"

    const resPatient = await api.get(pendingUrl, {
      params: selectedDepartmentId.value
        ? { departmentId: selectedDepartmentId.value, paidOnly: true }
        : {}
    })
    patients.value = resPatient.data || []
  } finally {
    loadingPatients.value = false
  }
}

const loadHistoryPatients = async () => {
  try {
    loadingPatients.value = true
    const res = await api.get("/ClinicalTests", {
      params: {
        status: "Completed",
        paidOnly: true,
        departmentId: selectedDepartmentId.value || undefined
      }
    })
    const list = Array.isArray(res.data) ? res.data : []
    const grouped = new Map<string, any>()
    list.forEach((t: any) => {
      const mr = t.medicalRecordId || "unknown"
      if (!grouped.has(mr)) {
        const date = (t.resultAt || t.createdAt || "").toString().slice(0, 10)
        grouped.set(mr, {
          medicalRecordId: t.medicalRecordId,
          appointmentId: t.appointmentId,
          patientId: t.patientId,
          fullName: t.patientName || "",
          phone: t.patientPhone || "",
          recordDate: date
        })
      }
    })
    historyRecords.value = Array.from(grouped.values())
  } finally {
    loadingPatients.value = false
  }
}

const loadTestsForSelected = async (status: string, sourceEntries: any) => {
  loadingTests.value = true
  const entries = sourceEntries.value || []
  if (!entries.length) {
    selectedPatientId.value = null
    selectedRecordId.value = null
    tests.value = []
    loadingTests.value = false
    return
  }

  const found = entries.find((p: any) => p.patientId?.toString() === selectedPatientId.value && (!selectedRecordId.value || p.medicalRecordId === selectedRecordId.value))
  if (!found) {
    selectedPatientId.value = entries[0].patientId?.toString() || null
    selectedRecordId.value = entries[0].medicalRecordId || null
  }

  if (selectedPatientId.value) {
    const res = await api.get("/ClinicalTests", {
      params: {
        status,
        patientId: selectedPatientId.value,
        paidOnly: true,
        departmentId: selectedDepartmentId.value || undefined
      }
    })
    let normalized = res.data.map((t: any) => ({
      ...t,
      status: t.status || (t.result ? "Completed" : "Pending")
    }))

    if (viewMode.value === "history" && selectedRecordId.value) {
      normalized = normalized.filter((t: any) => t.medicalRecordId === selectedRecordId.value)
    }

    tests.value = normalized.sort((a: any, b: any) => {
      if (status === "Completed") {
        return (new Date(b.resultAt || b.createdAt).getTime() - new Date(a.resultAt || a.createdAt).getTime())
      }
      if (a.status === b.status) return 0
      return a.status === "Pending" ? -1 : 1
    })
  } else {
    tests.value = []
  }
  loadingTests.value = false
}

const handleDepartmentChange = async () => {
  selectedPatientId.value = null
  selectedRecordId.value = null
  historyDateFilter.value = ""
  await loadTests()
}

const selectPatient = async (p: any) => {
  selectedPatientId.value = p.patientId?.toString() || null
  selectedRecordId.value = p.medicalRecordId
  if (viewMode.value === "pending") {
    await loadTestsForSelected("Pending,InProgress", patients)
  } else {
    await loadTestsForSelected("Completed", historyRecords)
  }
}

const updateResult = async (test: any) => {
  if (!test.result || !test.technicianName) {
    uiError.value = "Vui lòng nhập đầy đủ kết quả và tên kỹ thuật viên."
    return
  }

  try {
    uiError.value = ""
    savingId.value = test.id
    await api.patch(`/ClinicalTests/${test.id}/result`, {
      result: test.result,
      technicianName: test.technicianName
    })
    await loadTests()
  } catch (err: any) {
    console.log(err)
    uiError.value = err?.response?.data?.message || "Không thể lưu kết quả. Vui lòng thử lại."
  } finally {
    savingId.value = null
  }
}

const startTest = async (test: any) => {
  try {
    uiError.value = ""
    savingId.value = test.id
    await api.patch(`/ClinicalTests/${test.id}/start`, {
      technicianName: test.technicianName
    })
    await loadTests()
  } catch (err: any) {
    console.log(err)
    uiError.value = err?.response?.data?.message || "Không thể bắt đầu xét nghiệm. Vui lòng thử lại."
  } finally {
    savingId.value = null
  }
}

const switchMode = async (mode: "pending" | "history") => {
  if (viewMode.value === mode) return
  viewMode.value = mode
  selectedPatientId.value = null
  selectedRecordId.value = null
  await loadTests()
  const targetPath = mode === "history" ? "/technician/tests/history" : "/technician/tests"
  if (route.path !== targetPath) {
    router.push(targetPath)
  }
}

const statusLabel = (s: string) => {
  const map: Record<string, string> = {
    Pending: "Chờ thực hiện",
    InProgress: "Đang làm",
    Completed: "Đã có kết quả"
  }
  return map[s] || s
}

const statusClass = (s: string) => {
  if (s === "Completed") return "bg-success-subtle text-success"
  if (s === "InProgress") return "bg-info-subtle text-info"
  return "bg-warning-subtle text-warning"
}

onMounted(async () => {
  await loadDepartments()
  await loadTests()
})

watch(
  () => route.path,
  async (newPath) => {
    const mode = newPath.includes("/history") ? "history" : "pending"
    if (viewMode.value !== mode) {
      viewMode.value = mode
      selectedPatientId.value = null
      selectedRecordId.value = null
      await loadTests()
    }
  }
)
</script>

<style src="@/styles/layouts/technician-tests.css"></style>
