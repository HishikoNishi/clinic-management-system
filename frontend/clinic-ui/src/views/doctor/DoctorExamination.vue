<template>
  <div class="exam-page">
    <div class="exam-grid">
      <div class="card shadow-sm h-100">
        <div class="card-header">
          <h6 class="mb-0">Thông tin bệnh nhân</h6>
        </div>
      <div class="card-body">
        <p class="mb-1"><span class="text-muted">Mã bệnh nhân:</span> <strong>{{ patient.patientCode || '—' }}</strong></p>
          <p class="mb-1"><span class="text-muted">Họ tên:</span> <strong>{{ patient.fullName }}</strong></p>
          <p class="mb-1"><span class="text-muted">Điện thoại:</span> <strong>{{ patient.phone }}</strong></p>
          <p class="mb-1"><span class="text-muted">CCCD:</span> <strong>{{ patient.citizenId || '—' }}</strong></p>
<p class="mb-1"><span class="text-muted">Số BHYT:</span> <strong>{{ patient.InsuranceCardNumber || '—' }}</strong></p>          <p class="mb-1"><span class="text-muted">Tuổi:</span> <strong>{{ age }}</strong></p>
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
              <button class="btn btn-link p-0 small" @click="viewHistoryDetail(h.id)">Xem chi tiết</button>
            </li>
          </ul>

          <div v-if="historyDetail" class="mt-3 border rounded p-2 bg-light">
            <div class="d-flex justify-content-between align-items-start">
              <div>
                <div class="fw-semibold">Hồ sơ ngày {{ formatDate(historyDetail.createdAt) }}</div>
                <div class="small text-muted">Chẩn đoán: {{ historyDetail.diagnosis || '—' }}</div>
              </div>
              <button class="btn btn-sm btn-outline-secondary" @click="historyDetail = null">Đóng</button>
            </div>
            <div class="small mt-1">Ghi chú: {{ historyDetail.note || '—' }}</div>
            <div class="small mt-1">BHYT: {{ Math.round((historyDetail.insuranceCoverPercent || 0) * 100) }}%</div>
            <div class="small mt-1">Phụ thu/Giảm trừ: {{ historyDetail.surcharge }} / {{ historyDetail.discount }}</div>
            <div class="mt-2">
              <div class="fw-semibold">Đơn thuốc</div>
              <ul class="small mb-1">
                <li v-for="(p, i) in historyDetail.prescriptionItems || []" :key="i">
                  {{ p.medicineName }} ({{ p.dosage }}), SL {{ p.quantity }}
                </li>
                <li v-if="!historyDetail.prescriptionItems?.length" class="text-muted">Không có</li>
              </ul>
            </div>
            <div class="mt-1">
              <div class="fw-semibold">Xét nghiệm</div>
              <ul class="small">
                <li v-for="(t, i) in historyDetail.clinicalTests || []" :key="i">{{ t }}</li>
                <li v-if="!historyDetail.clinicalTests?.length" class="text-muted">Không có</li>
              </ul>
            </div>
          </div>
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
            <div class="mb-2">
              <label class="form-label small mb-1">Chọn nhanh loại xét nghiệm</label>
              <div class="d-flex gap-2 flex-wrap">
                <button type="button" class="btn btn-outline-secondary btn-sm" @click="addClinicalTest(testTypeLabel('Blood'))">Xét nghiệm máu</button>
                <button type="button" class="btn btn-outline-secondary btn-sm" @click="addClinicalTest(testTypeLabel('XRay'))">X-quang</button>
                <button type="button" class="btn btn-outline-secondary btn-sm" @click="addClinicalTest(testTypeLabel('Ultrasound'))">Siêu âm</button>
              </div>
            </div>
            <div class="list-group">
              <div class="list-group-item d-flex align-items-center gap-2" v-for="(t, idx) in form.clinicalTests" :key="idx">
                <input v-model="form.clinicalTests[idx]" type="text" class="form-control" placeholder="Nhập tên xét nghiệm" />
                <button class="btn btn-outline-danger btn-sm" type="button" @click="removeClinicalTest(idx)" :disabled="form.clinicalTests.length === 1">
                  <i class="bi bi-x"></i>
                </button>
              </div>
            </div>
            <button class="btn btn-link p-0 mt-2" type="button" @click="addClinicalTest('')">
              + Thêm xét nghiệm
            </button>
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
              <input v-model="form.vitals.heartRate" type="number" min="30" max="220" class="form-control" placeholder="80" />
            </div>
            <div class="col-6 col-md-3">
              <label class="form-label">HA (mmHg)</label>
              <input v-model="form.vitals.bloodPressure" type="text" class="form-control" placeholder="120/80" />
            </div>
            <div class="col-6 col-md-3">
              <label class="form-label">Nhiệt độ (°C)</label>
              <input v-model="form.vitals.temperature" type="number" step="0.1" min="34" max="41.5" class="form-control" placeholder="37.0" />
            </div>
            <div class="col-6 col-md-3">
              <label class="form-label">SpO₂ (%)</label>
              <input v-model="form.vitals.spo2" type="number" min="70" max="100" class="form-control" placeholder="98" />
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
        <div class="d-flex gap-2">
  <button class="btn btn-outline-primary btn-sm" type="button" @click="openMedicineModal">
    <i class="bi bi-search"></i> Tìm & Chọn thuốc
  </button>
  
</div>
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

   <div class="card shadow-sm mt-3" v-if="clinicalTestsDetail.length">
  <div class="card-header d-flex justify-content-between align-items-center">
    <h6 class="mb-0">📋 Xét nghiệm đã yêu cầu</h6>
    <button class="btn btn-outline-secondary btn-sm" type="button" @click="currentMedicalRecordId && loadClinicalTestsDetail(currentMedicalRecordId)">
      Làm mới
    </button>
  </div>
  
  <div class="card shadow-sm mt-3" v-if="clinicalTestsDetail.length">
      <div class="card-header d-flex justify-content-between align-items-center">
        <h6 class="mb-0">Xét nghiệm đã yêu cầu</h6>
        <button class="btn btn-outline-secondary btn-sm" type="button" @click="currentMedicalRecordId && loadClinicalTestsDetail(currentMedicalRecordId)">
          Làm mới
        </button>
      </div>
      <div class="card-body p-0">
        <table class="table mb-0">
          <thead class="table-light">
            <tr>
              <th>Tên</th>
              <th>Trạng thái</th>
              <th>Kết quả</th>
              <th>KTV</th>
              <th>Thời gian</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="t in clinicalTestsDetail" :key="t.id">
              <td>{{ t.testName }}</td>
              <td><span :class="['badge', labStatusClass(t.status)]">{{ labStatusLabel(t.status) }}</span></td>
              <td>{{ t.result || '—' }}</td>
              <td>{{ t.technicianName || '—' }}</td>
              <td class="text-muted small">{{ formatDate(t.resultAt || t.createdAt) }}</td>
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
  <div v-if="showMedicineModal" class="modal-backdrop" @click="showMedicineModal = false">
  <div class="modal-content medicine-modal" @click.stop>
    <div class="modal-header">
      <h5 class="modal-title">📦 Danh mục thuốc & Vật tư</h5>
      <button class="btn-close" @click="showMedicineModal = false"></button>
    </div>
    <div class="modal-body">
      <div class="input-group mb-3">
        <span class="input-group-text"><i class="bi bi-search"></i></span>
<input 
  v-model="medicineSearch" 
  type="text" 
  class="form-control" 
  placeholder="Gõ tên thuốc hoặc hàm lượng để lọc nhanh..." 
  autocomplete="off"
/>      </div>
      
      <div class="table-responsive" style="max-height: 450px;">
        <table class="table table-hover align-middle">
          <thead class="sticky-top bg-light">
            <tr>
              <th>Mã thuốc</th>
              <th>Tên thuốc & Hàm lượng</th>
              <th>ĐVT</th>
              <th>Đơn giá</th>
              <th style="width: 100px;"></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="m in filteredMedicines" :key="m.id" @click="addMedicineFromModal(m)" style="cursor: pointer;">
              <td><small class="text-muted">{{ m.medicineCode }}</small></td>
              <td>
                <div class="fw-bold text-primary">{{ m.name }}</div>
                <div class="small text-secondary">
                  <i class="bi bi-capsule"></i> {{ m.dosageForm }} {{ m.strength ? ' - ' + m.strength : '' }}
                </div>
              </td>
              <td><span class="badge bg-light text-dark border">{{ m.unit }}</span></td>
              <td><strong class="text-danger">{{ m.price?.toLocaleString() }}đ</strong></td>
              <td class="text-end">
                <button class="btn btn-sm btn-outline-primary">Chọn</button>
              </td>
            </tr>
            <tr v-if="filteredMedicines.length === 0">
              <td colspan="5" class="text-center py-4 text-muted">Không tìm thấy thuốc phù hợp</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
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
const historyDetail = ref<any | null>(null)
const clinicalTestsDetail = ref<any[]>([])
const currentMedicalRecordId = ref<string | null>(null)
const saving = ref(false)
const error = ref<string | null>(null)
const dataFromRecord = ref<any | null>(null)

const showMedicineModal = ref(false)
const medicineSearch = ref("")
const medicines = ref<any[]>([])

const loadMedicines = async () => {
  try {
    const { data } = await api.get('/Medicines')
    medicines.value = data || []
  } catch (err) { console.error("Lỗi tải thuốc", err) }
}

const filteredMedicines = computed(() => {
  const s = medicineSearch.value.toLowerCase().trim();
  
  // Nếu không gõ gì, hiện 20 thuốc đầu tiên cho nhẹ trang
  if (!s) return medicines.value.slice(0, 20);

  // Lọc thông minh: Tìm trong Tên, Mã thuốc, và cả Hàm lượng (Strength)
  return medicines.value.filter(m => {
    const name = (m.name || "").toLowerCase();
    const code = (m.medicineCode || "").toLowerCase();
    const strength = (m.strength || "").toLowerCase();
    
    return name.includes(s) || code.includes(s) || strength.includes(s);
  });
});
const openMedicineModal = () => {
  medicineSearch.value = ""
  showMedicineModal.value = true
}

const addMedicineFromModal = (m: any) => {
  const fullMedicineName = m.strength ? `${m.name} ${m.strength}` : m.name;
  
  // 1. Tìm xem thuốc này đã có trong đơn chưa (so sánh tên thuốc)
  const existingItem = form.prescriptionItems.find(
    (item: any) => item.medicineName === fullMedicineName
  );

  if (existingItem) {
    // 2. Nếu đã có, tăng số lượng lên 1
    existingItem.quantity = (Number(existingItem.quantity) || 0) + 1;
  } else {
    // 3. Nếu chưa có, kiểm tra dòng đầu tiên có đang trống không
    const firstItem = form.prescriptionItems[0] as any;
    
    if (form.prescriptionItems.length === 1 && (!firstItem.medicineName || firstItem.medicineName.trim() === "")) {
      // Điền vào dòng trống duy nhất
      firstItem.medicineName = fullMedicineName;
      firstItem.dosage = m.defaultDosage || "";
      firstItem.quantity = 1;
    } else {
      // Thêm dòng mới hoàn toàn
      form.prescriptionItems.push({ 
        medicineName: fullMedicineName, 
        dosage: m.defaultDosage || "", 
        quantity: 1 
      });
    }
  }
  
  showMedicineModal.value = false;
};
onMounted(() => {
  if (!authStore.token) {
    router.push("/login")
    return
  }
  loadDetail()
  loadMedicines() // Thêm dòng này
})
const form = reactive({
  diagnosis: "",
  notes: "",
  requestClinicalTest: false,
  clinicalTestType: "Blood",
  clinicalTestName: "",
  clinicalTests: [""],
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

const addClinicalTest = (name: string) => {
  if (!form.clinicalTests) form.clinicalTests = [""]
  form.clinicalTests.push(name)
}

const removeClinicalTest = (idx: number) => {
  if (form.clinicalTests.length === 1) {
    form.clinicalTests[0] = ""
    return
  }
  form.clinicalTests.splice(idx, 1)
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

const labStatusLabel = (s: string) => {
  const map: Record<string, string> = {
    Pending: "Chờ thực hiện",
    InProgress: "Đang làm",
    Completed: "Đã có kết quả"
  }
  return map[s] || s
}

const labStatusClass = (s: string) => {
  if (s === "Completed") return "bg-success-subtle text-success"
  if (s === "InProgress") return "bg-info-subtle text-info"
  return "bg-warning-subtle text-warning"
}

const loadDetail = async () => {
  try {
    const res = await api.get(`/doctor/DoctorAppointments/${appointmentId}/examination`)
    const data = res.data
    currentMedicalRecordId.value = data.currentMedicalRecordId || null
    dataFromRecord.value = data
    Object.assign(appointment, data.appointment || {})
   Object.assign(patient, {
  fullName: data.appointment?.fullName,
  phone: data.appointment?.phone,
  dateOfBirth: data.appointment?.dateOfBirth,
  patientCode: data.appointment?.patientCode, 
  citizenId: data.appointment?.citizenId,
  // Thử tất cả các trường hợp tên biến có thể trả về từ API
  InsuranceCardNumber: data.appointment?.insuranceCardNumber 
  
})
    history.value = data.medicalHistory || []

    // Prefill from current medical record if exists
    if (data.diagnosis) form.diagnosis = data.diagnosis
    if (data.note) form.notes = data.note
    if (Array.isArray(data.prescriptionItems) && data.prescriptionItems.length) {
      form.prescriptionItems = data.prescriptionItems.map((p: any) => ({
        medicineName: p.medicineName || "",
        dosage: p.dosage || "",
        quantity: p.quantity || 1
      }))
    }
    if (Array.isArray(data.clinicalTests) && data.clinicalTests.length) {
      form.requestClinicalTest = true
      form.clinicalTestType = "Other"
      form.clinicalTestName = ""
      form.clinicalTests = data.clinicalTests.map((t: any) => t.testName || "")
    }

    if (currentMedicalRecordId.value) {
      await loadClinicalTestsDetail(currentMedicalRecordId.value)
    }
  } catch (err: any) {
    console.error(err)
    error.value = err?.response?.data?.message || "Không tải được thông tin khám"
  }
}

const loadClinicalTestsDetail = async (recordId: string) => {
  try {
    const { data } = await api.get(`/ClinicalTests/medical-record/${recordId}`)
    clinicalTestsDetail.value = data || []
  } catch (err) {
    console.error(err)
  }
}

const viewHistoryDetail = async (recordId: string) => {
  try {
    const { data } = await api.get(`/medical-record/${recordId}`)
    historyDetail.value = { ...data, createdAt: history.value.find(h => h.id === recordId)?.createdAt }
  } catch (err: any) {
    console.error(err)
    alert(err?.response?.data?.message || "Không tải được hồ sơ chi tiết")
  }
}

const submit = async () => {
  if (!form.diagnosis.trim()) {
    alert("Vui lòng nhập chẩn đoán")
    return
  }

  // Giới hạn đơn giản cho sinh hiệu (chặn giá trị phi thực tế)
  const hr = form.vitals.heartRate ? Number(form.vitals.heartRate) : null
  const temp = form.vitals.temperature ? Number(form.vitals.temperature) : null
  const spo2 = form.vitals.spo2 ? Number(form.vitals.spo2) : null
  let bpOk = true
  if (form.vitals.bloodPressure) {
    const m = form.vitals.bloodPressure.match(/^\s*(\d{2,3})\s*\/\s*(\d{2,3})\s*$/)
    if (!m) bpOk = false
    else {
      const sys = Number(m[1]); const dia = Number(m[2])
      if (sys < 70 || sys > 250 || dia < 40 || dia > 150) bpOk = false
    }
  }
  const vitalOut =
    (hr !== null && (hr < 30 || hr > 200)) ||
    (temp !== null && (temp < 34 || temp > 41.5)) ||
    (spo2 !== null && (spo2 < 70 || spo2 > 100)) ||
    !bpOk
  if (vitalOut) {
    alert("Chỉ số sinh hiệu vượt giới hạn thực tế (HR 30–200, nhiệt 34–41.5°C, SpO₂ 70–100%, HA hợp lệ dạng 120/80 và 70–250 / 40–150).")
    return
  }

  if (form.requestClinicalTest) {
    const anyName =
      form.clinicalTests.some(t => t && t.trim()) ||
      (form.clinicalTestType !== "Other" && form.clinicalTestType) ||
      (form.clinicalTestType === "Other" && form.clinicalTestName.trim())
    if (!anyName) {
      alert("Vui lòng nhập ít nhất một xét nghiệm")
      return
    }
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

    const clinicalTestNames = form.requestClinicalTest
      ? (form.clinicalTests
          .filter(t => t && t.trim())
          .map(t => t.trim())
          .concat(
            form.clinicalTestType === "Other" && form.clinicalTestName.trim()
              ? [`${testTypeLabel(form.clinicalTestType)}: ${form.clinicalTestName.trim()}`]
              : form.clinicalTestType !== "Other"
              ? [testTypeLabel(form.clinicalTestType)]
              : []
          ))
      : []

    await api.post("/medical-record/examination", {
      appointmentId,
      diagnosis: form.diagnosis,
      notes: combinedNotes,
      requestClinicalTest: form.requestClinicalTest,
      clinicalTestNames,
      prescriptionItems,
      insuranceCoverPercent: dataFromRecord.value?.insuranceCoverPercent ?? 0,
      surcharge: dataFromRecord.value?.surcharge ?? 0,
      discount: dataFromRecord.value?.discount ?? 0
    })
    alert("Đã lưu hồ sơ khám")
    await loadDetail()
    // reset flag để tránh cảm giác lưu xong vẫn bật ghi chú xét nghiệm cũ
    if (!form.requestClinicalTest) {
      form.clinicalTestType = "Blood"
      form.clinicalTestName = ""
      form.clinicalTests = [""]
    }
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


</script>

<style src="@/styles/layouts/doctor-exam.css"></style>