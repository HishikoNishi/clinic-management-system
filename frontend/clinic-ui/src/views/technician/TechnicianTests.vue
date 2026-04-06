<template>
  <div class="tech-tests">
    <div class="card shadow-sm">
      <div class="card-header d-flex flex-wrap justify-content-between align-items-center gap-2">
        <div>
          <h5 class="mb-0">Danh sách xét nghiệm</h5>
          <small class="text-muted">Chỉ hiển thị các yêu cầu đã thanh toán</small>
        </div>
        <div class="btn-group btn-group-sm" role="group">
          <button
            type="button"
            class="btn"
            :class="viewMode === 'pending' ? 'btn-primary' : 'btn-outline-primary'"
            @click="switchMode('pending')"
          >
            Đang chờ
          </button>
          <button
            type="button"
            class="btn"
            :class="viewMode === 'history' ? 'btn-primary' : 'btn-outline-primary'"
            @click="switchMode('history')"
          >
            Lịch sử đã xong
          </button>
        </div>
        <div class="d-flex align-items-center gap-2">
          <select
            class="form-select form-select-sm"
            style="min-width: 200px"
            v-model="selectedDepartmentId"
            @change="handleDepartmentChange"
          >
            <option value="">Tất cả khoa</option>
            <option v-for="d in departments" :key="d.id" :value="d.id">
              {{ d.name }}
            </option>
          </select>
          <button class="btn btn-outline-secondary btn-sm" @click="loadTests">
            <i class="bi bi-arrow-clockwise me-1"></i> Làm mới
          </button>
        </div>
      </div>

      <div class="card-body p-0">
        <div class="row g-0">
          <div class="col-12 col-lg-4 border-end" style="max-height:60vh;overflow:auto">
            <ul class="list-group list-group-flush">
              <li
                v-for="p in patients"
                :key="`${p.patientId}-${p.medicalRecordId}`"
                :class="['list-group-item list-group-item-action', selectedPatientId === p.patientId?.toString() ? 'active' : '']"
                role="button"
                style="cursor:pointer"
                @click="selectPatient(p)"
              >
                <div class="fw-semibold">{{ p.fullName }}</div>
                <div class="text-muted small">{{ p.phone || '—' }}</div>
                <div class="small">Số yêu cầu: {{ p.pendingCount }}</div>
              </li>
              <li v-if="patients.length === 0" class="list-group-item text-muted small">Không có bệnh nhân chờ xét nghiệm.</li>
            </ul>
          </div>
          <div class="col-12 col-lg-8">
            <div v-if="tests.length === 0" class="p-3 text-center text-muted">
              Chọn bệnh nhân để xem yêu cầu xét nghiệm.
            </div>

            <div v-else class="table-responsive">
              <table class="table table-striped align-middle mb-0">
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
                  <tr v-for="t in tests" :key="t.id">
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
                        >
                          Bắt đầu
                        </button>
                        <button
                          v-if="t.status !== 'Completed'"
                          class="btn btn-primary btn-sm"
                          @click.stop="updateResult(t)"
                        >
                          <i class="bi bi-save me-1"></i>Lưu
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
import { ref, onMounted, watch } from "vue"
import { useRoute, useRouter } from "vue-router"
import api from "@/services/api"

const tests = ref<any[]>([])
const patients = ref<any[]>([])
const historyPatients = ref<any[]>([])
const departments = ref<any[]>([])
const selectedDepartmentId = ref<string>("")
const selectedPatientId = ref<string | null>(null)
const selectedRecordId = ref<string | null>(null)
const route = useRoute()
const router = useRouter()
const viewMode = ref<"pending" | "history">(route.path.includes("/history") ? "history" : "pending")

const loadDepartments = async () => {
  const res = await api.get("/Departments")
  departments.value = res.data || []
  if (!selectedDepartmentId.value && departments.value.length > 0) {
    selectedDepartmentId.value = departments.value[0].id
  }
}

const loadTests = async () => {
  try {
    await Promise.all([loadPendingPatients(), loadHistoryPatients()])
    if (viewMode.value === "pending") {
      await loadTestsForSelected("Pending,InProgress", patients)
    } else {
      await loadTestsForSelected("Completed", historyPatients)
    }
  } catch (err) {
    console.log(err)
  }
}

const loadPendingPatients = async () => {
  const pendingUrl = selectedDepartmentId.value
    ? "/ClinicalTests/pending-patients/by-department"
    : "/ClinicalTests/pending-patients"

  const resPatient = await api.get(pendingUrl, {
    params: selectedDepartmentId.value
      ? { departmentId: selectedDepartmentId.value, paidOnly: true }
      : {}
  })
  patients.value = resPatient.data || []
}

const loadHistoryPatients = async () => {
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
    const pid = t.patientId || "unknown"
    if (!grouped.has(pid)) {
      grouped.set(pid, {
        patientId: t.patientId,
        fullName: t.patientName || "",
        phone: t.patientPhone || "",
        medicalRecordId: t.medicalRecordId,
        pendingCount: 0
      })
    }
  })
  historyPatients.value = Array.from(grouped.values())
}

const loadTestsForSelected = async (status: string, sourcePatients: any) => {
  if (sourcePatients.value.length) {
    const found = sourcePatients.value.find((p: any) => p.patientId?.toString() === selectedPatientId.value)
    if (!found) {
      selectedPatientId.value = sourcePatients.value[0].patientId?.toString() || null
      selectedRecordId.value = sourcePatients.value[0].medicalRecordId
    }
  } else {
    selectedPatientId.value = null
    selectedRecordId.value = null
    tests.value = []
    return
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
    const normalized = res.data.map((t: any) => ({
      ...t,
      status: t.status || (t.result ? "Completed" : "Pending")
    }))
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
}

const handleDepartmentChange = async () => {
  selectedPatientId.value = null
  selectedRecordId.value = null
  await loadTests()
}

const selectPatient = async (p: any) => {
  selectedPatientId.value = p.patientId?.toString() || null
  selectedRecordId.value = p.medicalRecordId
  if (viewMode.value === "pending") {
    await loadTestsForSelected("Pending,InProgress", patients)
  } else {
    await loadTestsForSelected("Completed", historyPatients)
  }
}

const updateResult = async (test: any) => {
  if (!test.result || !test.technicianName) {
    alert("Nhập đầy đủ thông tin")
    return
  }

  await api.patch(`/ClinicalTests/${test.id}/result`, {
    result: test.result,
    technicianName: test.technicianName
  })

  alert("Đã lưu kết quả")
  loadTests()
}

const startTest = async (test: any) => {
  await api.patch(`/ClinicalTests/${test.id}/start`, {
    technicianName: test.technicianName
  })
  loadTests()
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
