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
          <p class="mb-1"><span class="text-muted">Số BHYT:</span> <strong>{{ patient.insuranceCardNumber || patient.InsuranceCardNumber || '—' }}</strong></p>          <p class="mb-1"><span class="text-muted">Tuổi:</span> <strong>{{ age }}</strong></p>
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
            <div class="small mt-1">Lý do vào khám: {{ historyDetail.chiefComplaint || '—' }}</div>
            <div class="small mt-1">Triệu chứng chi tiết: {{ historyDetail.detailedSymptoms || '—' }}</div>
            <div class="small mt-1">Tiền sử bệnh: {{ historyDetail.pastMedicalHistory || '—' }}</div>
            <div class="small mt-1">Dị ứng: {{ historyDetail.allergies || '—' }}</div>
            <div class="small mt-1">Nghề nghiệp: {{ historyDetail.occupation || '—' }}</div>
            <div class="small mt-1">Thói quen: {{ historyDetail.habits || '—' }}</div>
            <div class="small mt-1">Sinh hiệu: HR {{ historyDetail.heartRate || '—' }}, HA {{ historyDetail.bloodPressure || '—' }}, Nhiệt {{ historyDetail.temperature || '—' }}°C, SpO₂ {{ historyDetail.spo2 || '—' }}%</div>
            <div class="small mt-1">Nhân trắc: Cao {{ historyDetail.heightCm || '—' }} cm, Nặng {{ historyDetail.weightKg || '—' }} kg, BMI {{ historyDetail.bmi || '—' }}</div>
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
          <div class="row g-3 mb-3">
            <div class="col-12 col-lg-6">
              <label class="form-label">Lý do vào khám</label>
              <textarea v-model="form.chiefComplaint" class="form-control" rows="2" placeholder="Lý do vào khám chính"></textarea>
            </div>
            <div class="col-12 col-lg-6">
              <label class="form-label">Triệu chứng chi tiết</label>
              <textarea v-model="form.detailedSymptoms" class="form-control" rows="2" placeholder="Mô tả chi tiết triệu chứng"></textarea>
            </div>
            <div class="col-12 col-lg-6">
              <label class="form-label">Bệnh nền / tiền sử bệnh</label>
              <textarea v-model="form.pastMedicalHistory" class="form-control" rows="2" placeholder="Ví dụ: tăng huyết áp, tiểu đường..."></textarea>
            </div>
            <div class="col-12 col-lg-6">
              <label class="form-label">Dị ứng</label>
              <textarea v-model="form.allergies" class="form-control" rows="2" placeholder="Thuốc, thức ăn, thời tiết..."></textarea>
            </div>
            <div class="col-12 col-lg-6">
              <label class="form-label">Nghề nghiệp</label>
              <input v-model="form.occupation" type="text" class="form-control" placeholder="Nhân viên văn phòng, học sinh..." />
            </div>
            <div class="col-12 col-lg-6">
              <label class="form-label">Thói quen</label>
              <input v-model="form.habits" type="text" class="form-control" placeholder="Hút thuốc, uống rượu, ít vận động..." />
            </div>
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
          <h6 class="mb-0">Chỉ số sinh hiệu & nhân trắc</h6>
        </div>
        <div class="card-body">
          <section class="mb-4">
            <div class="d-flex justify-content-between align-items-center mb-3">
              <h6 class="mb-0 small text-uppercase text-secondary">Chỉ số sinh hiệu & nhân trắc</h6>
            </div>
            <div class="row g-3">
              <div class="col-6 col-md-3">
                <label class="form-label">Chiều cao (cm)</label>
                <input v-model="form.vitals.heightCm" type="number" min="30" max="250" step="0.1" class="form-control" placeholder="170" />
              </div>
              <div class="col-6 col-md-3">
                <label class="form-label">Cân nặng (kg)</label>
                <input v-model="form.vitals.weightKg" type="number" min="1" max="400" step="0.1" class="form-control" placeholder="65" />
              </div>
              <div class="col-6 col-md-3">
                <label class="form-label">BMI</label>
                <input :value="computedBmi" type="text" class="form-control" placeholder="Tự tính" readonly />
              </div>
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
                <input v-model="form.vitals.temperature" type="number" step="0.1" min="34" max="42" class="form-control" placeholder="37.0" />
              </div>
              <div class="col-6 col-md-3">
                <label class="form-label">SpO₂ (%)</label>
                <input v-model="form.vitals.spo2" type="number" min="70" max="100" class="form-control" placeholder="98" />
              </div>
            </div>
            <small class="text-muted d-block mt-2">Các chỉ số này sẽ được lưu riêng trong hồ sơ khám để xem lại và thống kê.</small>
          </section>

          <hr class="my-4" />

          <section>
            <div class="d-flex justify-content-between align-items-center mb-3">
              <h6 class="mb-0 small text-uppercase text-secondary">Đơn thuốc</h6>
              <button class="btn btn-outline-primary btn-sm" type="button" @click="openMedicineModal">
                <i class="bi bi-search"></i> Tìm & Chọn thuốc
              </button>
            </div>
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
          </section>
        </div>
      </div>
    </div>
  </div>
    <div v-if="error" class="alert alert-danger mt-2 py-2">{{ error }}</div>



    <div class="card shadow-sm mt-3" v-if="clinicalTestsDetail.length">
      <div class="card-header d-flex justify-content-between align-items-center">
        <h6 class="mb-0">📋 Xét nghiệm đã yêu cầu</h6>
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

    <!-- Chuyển khoa -->
    <div class="card shadow-sm mt-3">
      <div class="card-header d-flex justify-content-between align-items-center">
        <h6 class="mb-0">Chuyển khoa</h6>
        <button
          v-if="canTransferDepartment"
          class="btn btn-sm btn-outline-primary"
          @click="showTransferForm = !showTransferForm"
        >
          {{ showTransferForm ? "Ẩn form" : "Mở form chuyển khoa" }}
        </button>
        <span v-else class="text-muted small">Lưu khám nền trước để mở chuyển khoa</span>
      </div>
      <div v-if="canTransferDepartment && showTransferForm" class="card-body">
        <div class="row g-3">
          <div class="col-md-4">
            <label class="form-label">Khoa đích</label>
            <select v-model="transferForm.targetDepartmentId" class="form-select">
              <option value="">Chọn khoa</option>
              <option v-for="d in departments" :key="d.id" :value="d.id">{{ toVietnameseDepartmentName(d.name) }}</option>
            </select>
          </div>
          <div class="col-md-4">
            <label class="form-label">Bác sĩ đích</label>
            <select v-model="transferForm.targetDoctorId" class="form-select" :disabled="!transferForm.targetDepartmentId || transferLoadingDoctors">
              <option value="">{{ transferLoadingDoctors ? "Đang tải bác sĩ..." : "Chọn bác sĩ" }}</option>
              <option v-for="d in transferDoctors" :key="d.id" :value="d.id">
                {{ d.fullName }}{{ d.specialtyName ? ` - ${d.specialtyName}` : "" }}
              </option>
            </select>
          </div>
          <div class="col-md-2">
            <label class="form-label">Ngày</label>
            <input v-model="transferForm.appointmentDate" type="date" class="form-control" :min="todayStr" />
          </div>
          <div class="col-md-2">
            <label class="form-label">Khung giờ</label>
            <select v-model="transferForm.appointmentTime" class="form-select" :disabled="!transferForm.targetDoctorId || transferLoadingSlots">
              <option value="">{{ transferLoadingSlots ? "Đang tải..." : "Chọn giờ" }}</option>
              <option v-for="slot in transferSlots" :key="slot.id || slot.startTime" :value="toApiTime(slot.startTime)">
                {{ slot.slotLabel || `${slot.startTime} - ${slot.endTime}` }}
              </option>
            </select>
          </div>
        </div>

        <div class="mt-3">
          <label class="form-label">Lý do chuyển khoa</label>
          <textarea v-model="transferForm.reason" class="form-control" rows="2" placeholder="Ví dụ: nghi ngờ cần khám chuyên khoa khác"></textarea>
        </div>

        <div class="form-check mt-2">
          <input v-model="transferForm.enqueueNow" class="form-check-input" type="checkbox" id="enqueueNow" />
          <label class="form-check-label" for="enqueueNow">Cho vào hàng chờ ngay (nếu là lịch trong hôm nay)</label>
        </div>

        <div v-if="transferResult" class="alert alert-success mt-3 py-2 mb-0">
          Lịch mới: <strong>{{ transferResult.targetAppointmentCode }}</strong>
          <span v-if="transferResult.queueNumber"> · STT: <strong>#{{ transferResult.queueNumber }}</strong></span>
        </div>

        <div class="d-flex justify-content-end mt-3">
          <button class="btn btn-outline-primary" @click="submitTransfer" :disabled="transferring">
            <span v-if="transferring" class="spinner-border spinner-border-sm me-1"></span>
            Xác nhận chuyển khoa
          </button>
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
              <th>Tên thuốc & Hàm lượng</th>
              <th>ĐVT</th>
              <th>Đơn giá</th>
              <th style="width: 90px;"></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="m in filteredMedicines" :key="m.id" @click="addMedicineFromModal(m)" style="cursor: pointer;">
              <td>
                <div class="fw-bold text-primary">{{ m.name }}</div>
                <div class="small text-secondary">
                  <i class="bi bi-capsule"></i> {{ m.defaultDosage || "Không có liều mặc định" }}
                </div>
              </td>
              <td><span class="badge bg-light text-dark border">{{ m.unit }}</span></td>
              <td><strong class="text-danger">{{ m.price?.toLocaleString() }}đ</strong></td>
              <td class="text-end">
                <button class="btn btn-sm btn-outline-primary">Chọn</button>
              </td>
            </tr>
            <tr v-if="filteredMedicines.length === 0">
              <td colspan="4" class="text-center py-4 text-muted">Không tìm thấy thuốc phù hợp</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  
  </div>
</div>
</template>
<script setup lang="ts">
import { onMounted, reactive, ref, computed, watch } from "vue"
import { useRoute, useRouter } from "vue-router"
import api from "@/services/api"
import { useAuthStore } from "@/stores/auth"
import type { AxiosError } from "axios"
import { useToast } from "@/composables/useToast"
import { toVietnameseDepartmentName } from "@/utils/departmentName"
const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const toast = useToast()
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
const isPaid = ref(false)
const showMedicineModal = ref(false)
const medicineSearch = ref("")
const medicines = ref<any[]>([])
const departments = ref<any[]>([])
const transferDoctors = ref<any[]>([])
const transferSlots = ref<any[]>([])
const transferLoadingDoctors = ref(false)
const transferLoadingSlots = ref(false)
const transferring = ref(false)
const transferResult = ref<any | null>(null)
const showTransferForm = ref(false)
const localDateInput = (d: Date = new Date()) => {
  const year = d.getFullYear()
  const month = String(d.getMonth() + 1).padStart(2, "0")
  const day = String(d.getDate()).padStart(2, "0")
  return `${year}-${month}-${day}`
}
const todayStr = localDateInput()
const transferForm = reactive({
  targetDepartmentId: "",
  targetDoctorId: "",
  appointmentDate: todayStr,
  appointmentTime: "",
  reason: "",
  enqueueNow: true
})

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

  // Lọc theo tên và liều mặc định.
  return medicines.value.filter(m => {
    const name = (m.name || "").toLowerCase();
    const dosage = (m.defaultDosage || "").toLowerCase();
    
    return name.includes(s) || dosage.includes(s);
  });
});
const openMedicineModal = () => {
  medicineSearch.value = ""
  showMedicineModal.value = true
}

const addMedicineFromModal = (m: any) => {
  const medicineName = m.name;
  const dosage = m.defaultDosage || "";
  
  // 1. Tìm xem thuốc này đã có trong đơn chưa (so sánh tên thuốc)
  const existingItem = form.prescriptionItems.find(
    (item: any) => item.medicineName === medicineName
  );

  if (existingItem) {
    // 2. Nếu đã có, tăng số lượng lên 1
    existingItem.quantity = (Number(existingItem.quantity) || 0) + 1;
  } else {
    // 3. Nếu chưa có, kiểm tra dòng đầu tiên có đang trống không
    const firstItem = form.prescriptionItems[0] as any;
    
    if (form.prescriptionItems.length === 1 && (!firstItem.medicineName || firstItem.medicineName.trim() === "")) {
      // Điền vào dòng trống duy nhất
      firstItem.medicineName = medicineName;
      firstItem.dosage = dosage;
      firstItem.quantity = 1;
    } else {
      // Thêm dòng mới hoàn toàn
      form.prescriptionItems.push({ 
        medicineName: medicineName, 
        dosage: dosage, 
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
  loadTransferDepartments()
})
const form = reactive({
  chiefComplaint: "",
  detailedSymptoms: "",
  pastMedicalHistory: "",
  allergies: "",
  occupation: "",
  habits: "",
  diagnosis: "",
  notes: "",
  requestClinicalTest: false,
  clinicalTestType: "Blood",
  clinicalTestName: "",
  clinicalTests: [""],
  vitals: {
    heightCm: "",
    weightKg: "",
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

const computedBmi = computed(() => {
  const height = form.vitals.heightCm ? Number(form.vitals.heightCm) : 0
  const weight = form.vitals.weightKg ? Number(form.vitals.weightKg) : 0
  if (!height || !weight || height <= 0 || weight <= 0) return ""
  const bmi = weight / Math.pow(height / 100, 2)
  if (!Number.isFinite(bmi)) return ""
  return bmi.toFixed(1)
})

const canTransferDepartment = computed(() => Boolean(currentMedicalRecordId.value))

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

const toApiTime = (value: string | null | undefined) => {
  const raw = String(value || "").trim()
  if (!raw) return ""
  if (/^\d{2}:\d{2}$/.test(raw)) return `${raw}:00`
  if (/^\d{2}:\d{2}:\d{2}$/.test(raw)) return raw
  const matched = raw.match(/\b(\d{2}:\d{2})(?::(\d{2}))?\b/)
  if (!matched) return raw
  return matched[2] ? `${matched[1]}:${matched[2]}` : `${matched[1]}:00`
}

const loadDetail = async () => {
  try {
    const res = await api.get(`/doctor/DoctorAppointments/${appointmentId}/examination`)
    const data = res.data
    currentMedicalRecordId.value = data.currentMedicalRecordId || null
    if (!currentMedicalRecordId.value) {
      showTransferForm.value = false
    }
    dataFromRecord.value = data
    Object.assign(appointment, data.appointment || {})
   Object.assign(patient, {
  fullName: data.appointment?.fullName,
  phone: data.appointment?.phone,
  dateOfBirth: data.appointment?.dateOfBirth,
  patientCode: data.appointment?.patientCode, 
  citizenId: data.appointment?.citizenId,
  // Thử tất cả các trường hợp tên biến có thể trả về từ API
  insuranceCardNumber: data.appointment?.insuranceCardNumber,
  InsuranceCardNumber: data.appointment?.insuranceCardNumber 
  
})
    history.value = data.medicalHistory || []

    // Prefill from current medical record if exists
    if (data.chiefComplaint) form.chiefComplaint = data.chiefComplaint
    else if (data.appointment?.reason) form.chiefComplaint = data.appointment.reason
    if (data.detailedSymptoms) form.detailedSymptoms = data.detailedSymptoms
    if (data.pastMedicalHistory) form.pastMedicalHistory = data.pastMedicalHistory
    if (data.allergies) form.allergies = data.allergies
    if (data.occupation) form.occupation = data.occupation
    if (data.habits) form.habits = data.habits
    if (data.heightCm !== null && data.heightCm !== undefined) form.vitals.heightCm = String(data.heightCm)
    if (data.weightKg !== null && data.weightKg !== undefined) form.vitals.weightKg = String(data.weightKg)
    if (data.heartRate !== null && data.heartRate !== undefined) form.vitals.heartRate = String(data.heartRate)
    if (data.bloodPressure) form.vitals.bloodPressure = data.bloodPressure
    if (data.temperature !== null && data.temperature !== undefined) form.vitals.temperature = String(data.temperature)
    if (data.spo2 !== null && data.spo2 !== undefined) form.vitals.spo2 = String(data.spo2)
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
      const errorAxios = err as AxiosError<any>
      const data: any = errorAxios?.response?.data
      error.value =
        data?.message ||
        (typeof data === "string" ? data : "") ||
        "SubmitExamination failed"
    }
}
const loadTransferDepartments = async () => {
  try {
    const { data } = await api.get("/Departments")
    departments.value = Array.isArray(data) ? data : []
  } catch (err) {
    console.error("Không tải được khoa", err)
    departments.value = []
  }
}

const loadTransferDoctors = async () => {
  transferDoctors.value = []
  transferSlots.value = []
  transferForm.targetDoctorId = ""
  transferForm.appointmentTime = ""
  if (!transferForm.targetDepartmentId) return

  try {
    transferLoadingDoctors.value = true
    const { data } = await api.get(`/Doctor/by-department/${transferForm.targetDepartmentId}`)
    const list = Array.isArray(data) ? data : []
    transferDoctors.value = list.filter((d: any) => d.id !== authStore.doctorId)
  } catch (err) {
    console.error("Không tải được bác sĩ theo khoa", err)
    transferDoctors.value = []
  } finally {
    transferLoadingDoctors.value = false
  }
}

const loadTransferSlots = async () => {
  transferSlots.value = []
  transferForm.appointmentTime = ""
  if (!transferForm.targetDoctorId || !transferForm.appointmentDate) return

  try {
    transferLoadingSlots.value = true
    const { data } = await api.get(`/DoctorSchedules/doctors/${transferForm.targetDoctorId}/available-slots`, {
      params: { date: transferForm.appointmentDate }
    })
    const slots = Array.isArray(data) ? data : []
    transferSlots.value = slots.filter((s: any) => !s.isBooked)
  } catch (err) {
    console.error("Không tải được slot chuyển khoa", err)
    transferSlots.value = []
  } finally {
    transferLoadingSlots.value = false
  }
}

const submitTransfer = async () => {
  if (!transferForm.targetDepartmentId || !transferForm.targetDoctorId || !transferForm.appointmentDate || !transferForm.appointmentTime) {
    toast.warning("Vui lòng chọn đủ khoa, bác sĩ, ngày và khung giờ chuyển khoa")
    return
  }

  transferring.value = true
  transferResult.value = null
  try {
    const { data } = await api.post(`/doctor/DoctorAppointments/${appointmentId}/transfer-department`, {
      targetDepartmentId: transferForm.targetDepartmentId,
      targetDoctorId: transferForm.targetDoctorId,
      appointmentDate: transferForm.appointmentDate,
      appointmentTime: toApiTime(transferForm.appointmentTime),
      reason: transferForm.reason?.trim() || null,
      enqueueNow: transferForm.enqueueNow
    })
    transferResult.value = data
    toast.success(
      `Đã chuyển khoa thành công. Mã lịch mới: ${data?.targetAppointmentCode || "N/A"}${
        data?.queueNumber ? ` | Số thứ tự: ${data.queueNumber}` : ""
      }`,
      4500
    )
  } catch (err: any) {
    console.error(err)
    toast.error(err?.response?.data?.message || "Không chuyển khoa được")
  } finally {
    transferring.value = false
  }
}

const validateVitals = () => {
  const hr = form.vitals.heartRate ? Number(form.vitals.heartRate) : null
  const temp = form.vitals.temperature ? Number(form.vitals.temperature) : null
  const spo2 = form.vitals.spo2 ? Number(form.vitals.spo2) : null
  const height = form.vitals.heightCm ? Number(form.vitals.heightCm) : null
  const weight = form.vitals.weightKg ? Number(form.vitals.weightKg) : null

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
    (hr !== null && (hr < 30 || hr > 220)) ||
    (temp !== null && (temp < 34 || temp > 42)) ||
    (spo2 !== null && (spo2 < 70 || spo2 > 100)) ||
    (height !== null && (height < 30 || height > 250)) ||
    (weight !== null && (weight < 1 || weight > 400)) ||
    !bpOk

  if (!vitalOut) return null
  return "Chỉ số vượt giới hạn thực tế (HR 30–220, nhiệt 34–42°C, SpO₂ 70–100%, cao 30–250cm, nặng 1–400kg, HA hợp lệ theo dạng 120/80 và trong khoảng 70–250 / 40–150)."
}

const buildClinicalTestNames = () => {
  if (!form.requestClinicalTest) return [] as string[]
  return form.clinicalTests
    .filter((t) => t && t.trim())
    .map((t) => t.trim())
    .concat(
      form.clinicalTestType === "Other" && form.clinicalTestName.trim()
        ? [`${testTypeLabel(form.clinicalTestType)}: ${form.clinicalTestName.trim()}`]
        : form.clinicalTestType !== "Other"
        ? [testTypeLabel(form.clinicalTestType)]
        : []
    )
}

const buildPrescriptionItems = () =>
  form.prescriptionItems
    .filter((item) => item.medicineName && item.medicineName.trim())
    .map((item) => ({
      ...item,
      dosage: item.dosage?.trim() || "",
      quantity: item.quantity || 1
    }))

const submit = async () => {
  if (!form.diagnosis.trim()) {
    toast.warning("Vui lòng nhập chẩn đoán")
    return
  }

  const vitalError = validateVitals()
  if (vitalError) {
    toast.warning(vitalError, 5000)
    return
  }

  const clinicalTestNames = buildClinicalTestNames()
  if (form.requestClinicalTest && !clinicalTestNames.length) {
    toast.warning("Vui lòng nhập ít nhất một xét nghiệm")
    return
  }
  const prescriptionItems = buildPrescriptionItems()

  saving.value = true
  error.value = null
  try {
    const combinedNotes = form.notes.trim()

    await api.post("/medical-record/examination", {
      appointmentId,
      diagnosis: form.diagnosis,
      notes: combinedNotes,
      chiefComplaint: form.chiefComplaint,
      detailedSymptoms: form.detailedSymptoms,
      pastMedicalHistory: form.pastMedicalHistory,
      allergies: form.allergies,
      occupation: form.occupation,
      habits: form.habits,
      heightCm: form.vitals.heightCm ? Number(form.vitals.heightCm) : null,
      weightKg: form.vitals.weightKg ? Number(form.vitals.weightKg) : null,
      bloodPressure: form.vitals.bloodPressure,
      heartRate: form.vitals.heartRate ? Number(form.vitals.heartRate) : null,
      temperature: form.vitals.temperature ? Number(form.vitals.temperature) : null,
      spo2: form.vitals.spo2 ? Number(form.vitals.spo2) : null,
      requestClinicalTest: form.requestClinicalTest,
      clinicalTestNames,
      prescriptionItems,
      insuranceCoverPercent: dataFromRecord.value?.insuranceCoverPercent ?? 0,
      surcharge: dataFromRecord.value?.surcharge ?? 0,
      discount: dataFromRecord.value?.discount ?? 0
    })
    toast.success("Đã lưu hồ sơ khám")
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
const loadClinicalTestsDetail = async (recordId: string) => {
try {
const { data } = await api.get(`/ClinicalTests/medical-record/${recordId}`)
clinicalTestsDetail.value = data || []
} catch (err) { console.error(err) }
}
const viewHistoryDetail = async (recordId: string) => {
try {
const { data } = await api.get(`/medical-record/${recordId}`)
historyDetail.value = { ...data, createdAt: history.value.find(h => h.id === recordId)?.createdAt }
} catch (err: any) { toast.error("Không tải được hồ sơ chi tiết") }
}
const goBack = () => router.push("/doctor/appointments")

watch(
  () => transferForm.targetDepartmentId,
  async () => {
    await loadTransferDoctors()
  }
)

watch(
  () => [transferForm.targetDoctorId, transferForm.appointmentDate],
  async () => {
    await loadTransferSlots()
  }
)
</script>
<style src="@/styles/layouts/doctor-exam.css"></style>







