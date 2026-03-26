<template>
  <div class="exam-page">
    <div class="exam-grid">
      <div class="card shadow-sm h-100">
        <div class="card-header">
          <h6 class="mb-0">Thông tin bệnh nhân</h6>
        </div>
        <div class="card-body">
          <p class="mb-1"><span class="text-muted">Họ tên:</span> <strong>{{ patient.fullName }}</strong></p>
          <p class="mb-1"><span class="text-muted">Điện thoại:</span> <strong>{{ patient.phone }}</strong></p>
          <p class="mb-1"><span class="text-muted">Tuổi:</span> <strong>{{ age }}</strong></p>
          <p class="mb-1"><span class="text-muted">Mã hẹn:</span> <strong>{{ appointment.appointmentCode }}</strong></p>
          <p class="mb-1"><span class="text-muted">Ngày giờ:</span> <strong>{{ formatDateTime(appointment.appointmentDate, appointment.appointmentTime) }}</strong></p>
          <hr />
          <div class="d-flex align-items-center mb-2">
            <i class="bi bi-clock-history me-2 text-primary"></i>
            <span class="fw-semibold">Lịch sử khám gần đây</span>
          </div>
          <ul class="list-group history-list">
            <li v-if="history.length === 0" class="list-group-item text-muted">Chưa có hồ sơ</li>
            <li v-else v-for="h in history" :key="h.id" class="list-group-item">
              <div class="small text-muted">{{ formatDate(h.createdAt) }}</div>
              <div class="fw-semibold">{{ h.diagnosis }}</div>
              <div class="text-muted">{{ h.note }}</div>
            </li>
          </ul>
        </div>
      </div>

      <div class="card shadow-sm">
        <div class="card-header">
          <h6 class="mb-0">Khám bệnh</h6>
        </div>
        <div class="card-body">
          <div class="mb-3">
            <label class="form-label">Chẩn đoán <span class="text-danger">*</span></label>
            <input v-model="form.diagnosis" type="text" class="form-control" required />
          </div>
          <div class="mb-3">
            <label class="form-label">Ghi chú</label>
            <textarea v-model="form.notes" class="form-control" rows="3"></textarea>
          </div>
          <div class="form-check">
            <input v-model="form.requestClinicalTest" class="form-check-input" type="checkbox" id="testReq" />
            <label class="form-check-label" for="testReq">Yêu cầu xét nghiệm</label>
          </div>
          <div v-if="form.requestClinicalTest" class="mt-2">
            <div class="row g-2">
              <div class="col-12 col-md-5">
                <select v-model="form.clinicalTestType" class="form-select">
                  <option value="Blood">Xét nghiệm máu</option>
                  <option value="XRay">X-quang</option>
                  <option value="Ultrasound">Siêu âm</option>
                  <option value="Other">Khác</option>
                </select>
              </div>
              <div class="col-12 col-md-7">
                <input
                  v-model="form.clinicalTestName"
                  type="text"
                  class="form-control"
                  :placeholder="form.clinicalTestType === 'Other' ? 'Mô tả xét nghiệm' : 'Ghi chú (ví dụ: công thức máu, tim phổi...)'"
                />
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="card shadow-sm">
        <div class="card-header">
          <h6 class="mb-0">Chỉ số sinh hiệu</h6>
        </div>
        <div class="card-body">
          <div class="row g-3">
            <div class="col-6 col-md-3">
              <label class="form-label">Nhịp tim (bpm)</label>
              <input v-model="form.vitals.heartRate" type="number" min="0" class="form-control" placeholder="80" />
            </div>
            <div class="col-6 col-md-3">
              <label class="form-label">HA (mmHg)</label>
              <input v-model="form.vitals.bloodPressure" type="text" class="form-control" placeholder="120/80" />
            </div>
            <div class="col-6 col-md-3">
              <label class="form-label">Nhiệt độ (°C)</label>
              <input v-model="form.vitals.temperature" type="number" step="0.1" class="form-control" placeholder="37.0" />
            </div>
            <div class="col-6 col-md-3">
              <label class="form-label">SpO₂ (%)</label>
              <input v-model="form.vitals.spo2" type="number" min="0" max="100" class="form-control" placeholder="98" />
            </div>
          </div>
          <small class="text-muted d-block mt-2">Các chỉ số này sẽ được lưu kèm trong ghi chú hồ sơ.</small>
        </div>
      </div>
    </div>

    <div v-if="error" class="alert alert-danger mt-2 py-2">{{ error }}</div>

    <div class="card shadow-sm mt-3">
      <div class="card-header d-flex justify-content-between align-items-center">
        <h6 class="mb-0">Đơn thuốc</h6>
        <button class="btn btn-outline-primary btn-sm" type="button" @click="addRow">
          <i class="bi bi-plus"></i> Thêm thuốc
        </button>
      </div>
      <div class="card-body p-0">
        <div class="table-responsive prescription-table">
          <table class="table mb-0 align-middle">
            <thead class="table-light">
              <tr>
                <th>Tên thuốc</th>
                <th>Liều dùng</th>
                <th style="width: 120px;">Số lượng</th>
                <th style="width: 70px;"></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(item, idx) in form.prescriptionItems" :key="idx">
                <td><input v-model="item.medicineName" class="form-control" placeholder="Paracetamol" /></td>
                <td><input v-model="item.dosage" class="form-control" placeholder="500mg x 3 lần/ngày" /></td>
                <td><input v-model.number="item.quantity" type="number" min="1" class="form-control" /></td>
                <td class="text-end">
                  <button class="btn btn-outline-danger btn-sm" type="button" @click="removeRow(idx)" :disabled="form.prescriptionItems.length === 1">
                    <i class="bi bi-trash"></i>
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <div class="d-flex justify-content-end gap-2 mt-3">
      <button class="btn btn-outline-secondary" @click="goBack">Quay lại</button>
      <button class="btn btn-primary" @click="submit" :disabled="saving">
        <span v-if="saving" class="spinner-border spinner-border-sm me-1"></span>
        Lưu khám
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref, computed } from "vue"
import { useRoute, useRouter } from "vue-router"
import api from "@/services/api"
import { useAuthStore } from "@/stores/auth"
import type { AxiosError } from "axios"

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const appointmentId = route.params.id as string

const appointment = reactive<any>({})
const patient = reactive<any>({})
const history = ref<any[]>([])
const saving = ref(false)
const error = ref<string | null>(null)

const form = reactive({
  diagnosis: "",
  notes: "",
  requestClinicalTest: false,
  clinicalTestType: "Blood",
  clinicalTestName: "",
  vitals: {
    heartRate: "",
    bloodPressure: "",
    temperature: "",
    spo2: ""
  },
  prescriptionItems: [{ medicineName: "", dosage: "", quantity: 1 }]
})

const age = computed(() => {
  if (!patient.dateOfBirth) return ""
  const dob = new Date(patient.dateOfBirth)
  const diff = Date.now() - dob.getTime()
  const ageDt = new Date(diff)
  return Math.abs(ageDt.getUTCFullYear() - 1970)
})

const addRow = () => {
  form.prescriptionItems.push({ medicineName: "", dosage: "", quantity: 1 })
}

const removeRow = (idx: number) => {
  if (form.prescriptionItems.length === 1) return
  form.prescriptionItems.splice(idx, 1)
}

const formatDate = (d: string) => {
  return d ? new Date(d).toLocaleString() : ""
}

const formatDateTime = (dateStr: string, timeStr: string) => {
  if (!dateStr || !timeStr) return ""
  const date = new Date(dateStr)
  const [h, m] = timeStr.split(":")
  return `${date.toLocaleDateString()} ${h}:${m}`
}

const testTypeLabel = (type: string) => {
  const map: Record<string, string> = {
    Blood: "Xét nghiệm máu",
    XRay: "X-quang",
    Ultrasound: "Siêu âm",
    Other: "Xét nghiệm"
  }
  return map[type] || "Xét nghiệm"
}

const loadDetail = async () => {
  try {
    const res = await api.get(`/doctor/DoctorAppointments/${appointmentId}/examination`)
    const data = res.data
    Object.assign(appointment, data.appointment || {})
    Object.assign(patient, {
      fullName: data.appointment?.fullName,
      phone: data.appointment?.phone,
      dateOfBirth: data.appointment?.dateOfBirth
    })
    history.value = data.medicalHistory || []
  } catch (err: any) {
    console.error(err)
    error.value = err?.response?.data?.message || "Không tải được thông tin khám"
  }
}

const submit = async () => {
  if (!form.diagnosis.trim()) {
    alert("Vui lòng nhập chẩn đoán")
    return
  }

  if (form.requestClinicalTest && form.clinicalTestType === "Other" && !form.clinicalTestName.trim()) {
    alert("Vui lòng mô tả xét nghiệm cần thực hiện")
    return
  }

  const prescriptionItems = form.prescriptionItems
    .filter(item => item.medicineName && item.medicineName.trim())
    .map(item => ({
      ...item,
      dosage: item.dosage?.trim() || "",
      quantity: item.quantity || 1
    }))

  saving.value = true
  error.value = null
  try {
    const vitalsNote = [
      form.vitals.heartRate ? `Nhịp tim: ${form.vitals.heartRate} bpm` : null,
      form.vitals.bloodPressure ? `HA: ${form.vitals.bloodPressure} mmHg` : null,
      form.vitals.temperature ? `Nhiệt độ: ${form.vitals.temperature} °C` : null,
      form.vitals.spo2 ? `SpO₂: ${form.vitals.spo2}%` : null
    ].filter(Boolean).join(" | ")

    const combinedNotes = [form.notes, vitalsNote].filter(Boolean).join(" --- ")

    await api.post("/medical-record/examination", {
      appointmentId,
      diagnosis: form.diagnosis,
      notes: combinedNotes,
      requestClinicalTest: form.requestClinicalTest,
      clinicalTestName: form.requestClinicalTest
        ? `${testTypeLabel(form.clinicalTestType)}${form.clinicalTestName ? ": " + form.clinicalTestName.trim() : ""}`
        : null,
      prescriptionItems
    })
    alert("Đã lưu hồ sơ khám")
    router.push("/doctor/appointments")
  } catch (err: unknown) {
  const errorAxios = err as AxiosError<any>
  console.error(err)
  error.value =
    errorAxios?.response?.data?.message ||
    "Không tải được thông tin khám"
} finally {
    saving.value = false
  }
}

const goBack = () => router.push("/doctor/appointments")

onMounted(() => {
  if (!authStore.token) {
    router.push("/login")
    return
  }
  loadDetail()
})
</script>

<style src="@/styles/layouts/doctor-exam.css"></style>
