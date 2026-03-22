<template>
  <div class="appointment-detail">
    <h2>Chi tiết lịch khám</h2>
<<<<<<< Updated upstream
    <div v-if="appointment" class="detail-card">
      <div class="detail-row"><span class="label">Mã:</span> {{ appointment.appointmentCode }}</div>
      <div class="detail-row"><span class="label">Bệnh nhân:</span> {{ appointment.fullName }}</div>
      <div class="detail-row"><span class="label">Điện thoại:</span> {{ appointment.phone }}</div>
      <div class="detail-row"><span class="label">Email:</span> {{ appointment.email }}</div>
      <div class="detail-row"><span class="label">Ngày sinh:</span> {{ formatDate(appointment.dateOfBirth) }}</div>
      <div class="detail-row"><span class="label">Giới tính:</span> {{ appointment.gender }}</div>
      <div class="detail-row"><span class="label">Địa chỉ:</span> {{ appointment.address }}</div>
      <div class="detail-row"><span class="label">Lý do:</span> {{ appointment.reason }}</div>
      <div class="detail-row">
        <span class="label">Trạng thái:</span>
        <span :class="'status ' + appointment.status.toLowerCase()">{{ appointment.status }}</span>
      </div>
      <div v-if="appointment.statusDetail.doctorName" class="detail-row">
        <span class="label">Bác sĩ:</span> {{ appointment.statusDetail.doctorName }} ({{ appointment.statusDetail.doctorCode }})
      </div>
    </div>
    <p v-else-if="error" class="error">{{ error }}</p>
    <button class="back-btn" @click="$router.push('/doctorappointment')">← Quay lại danh sách</button>
=======

    <!-- APPOINTMENT INFO -->
    <div v-if="appointment" class="detail-card">

      <div class="detail-row">
        <span class="label">Mã:</span>
        {{ appointment.appointmentCode }}
      </div>

      <div class="detail-row">
        <span class="label">Bệnh nhân:</span>
        {{ appointment.fullName }}
      </div>

      <div class="detail-row">
        <span class="label">Điện thoại:</span>
        {{ appointment.phone }}
      </div>

      <div class="detail-row">
        <span class="label">Email:</span>
        {{ appointment.email }}
      </div>

      <div class="detail-row">
        <span class="label">Ngày sinh:</span>
        {{ formatDate(appointment.dateOfBirth) }}
      </div>

      <div class="detail-row">
        <span class="label">Giới tính:</span>
        {{ appointment.gender }}
      </div>

      <div class="detail-row">
        <span class="label">Địa chỉ:</span>
        {{ appointment.address }}
      </div>

      <div class="detail-row">
        <span class="label">Lý do:</span>
        {{ appointment.reason }}
      </div>

      <div class="detail-row">
        <span class="label">Trạng thái:</span>
        <span :class="'status ' + appointment.status.toLowerCase()">
          {{ appointment.status }}
        </span>
      </div>

      <div v-if="appointment.statusDetail?.doctorName" class="detail-row">
        <span class="label">Bác sĩ:</span>
        {{ appointment.statusDetail.doctorName }}
        ({{ appointment.statusDetail.doctorCode }})
      </div>

      <!-- ACTION BUTTONS -->
      <div class="actions">

        <button
          v-if="appointment.status === 'Pending'"
          @click="approveAppointment"
        >
          Xác nhận lịch
        </button>

        <button
          v-if="appointment.status === 'Pending' || appointment.status === 'Confirmed'"
          @click="cancelAppointment"
        >
          Hủy lịch
        </button>

        <button
          v-if="appointment.status === 'Confirmed'"
          @click="completeAppointment"
        >
          Hoàn thành
        </button>

      </div>

    </div>

    <!-- MEDICAL RECORD -->
    <div v-if="appointment && appointment.status === 'Confirmed'" class="medical-form">

      <h3>Medical Record</h3>

      <div class="form-group">
        <label>Triệu chứng</label>
        <textarea v-model="symptoms"></textarea>
      </div>

      <div class="form-group">
        <label>Chẩn đoán</label>
        <textarea v-model="diagnosis"></textarea>
      </div>

      <div class="form-group">
        <label>Ghi chú</label>
        <textarea v-model="notes"></textarea>
      </div>

      <!-- PRESCRIPTION -->
      <h3>Prescription</h3>

      <table class="medicine-table">
        <thead>
          <tr>
            <th>Medicine</th>
            <th>Dose</th>
            <th>Quantity</th>
            <th>Instruction</th>
            <th></th>
          </tr>
        </thead>

        <tbody>
          <tr v-for="(m, index) in medicines" :key="index">

            <td>
              <input v-model="m.medicineName" />
            </td>

            <td>
              <input v-model="m.dose" />
            </td>

            <td>
              <input type="number" v-model="m.quantity" />
            </td>

            <td>
              <input v-model="m.instruction" />
            </td>

            <td>
              <button @click="removeMedicine(index)">X</button>
            </td>

          </tr>
        </tbody>
      </table>

      <button class="add-btn" @click="addMedicine">
        + Thêm thuốc
      </button>

      <button class="save-btn" @click="saveMedicalRecord">
        Lưu Medical Record
      </button>

      <!-- ====== CLINICAL TEST (THÊM MỚI) ====== -->

      <h3>Clinical Tests</h3>

      <div class="form-group">
        <label>Test Name</label>
        <input v-model="testName" placeholder="VD: X-quang, Xét nghiệm máu..." />
      </div>

      <button class="add-btn" @click="createClinicalTest">
        + Tạo yêu cầu xét nghiệm
      </button>

      <table class="medicine-table" v-if="clinicalTests.length">
        <thead>
          <tr>
            <th>Test</th>
            <th>Result</th>
            <th>Technician</th>
            <th></th>
          </tr>
        </thead>

        <tbody>
          <tr v-for="t in clinicalTests" :key="t.id">

            <td>{{ t.testName }}</td>

            <td>
              <input v-model="t.result" placeholder="Nhập kết quả..." />
            </td>

            <td>
              <input v-model="t.technicianName" placeholder="KTV..." />
            </td>

            <td>
              <button @click="updateResult(t)">Lưu</button>
            </td>

          </tr>
        </tbody>
      </table>

    </div>

    <p v-else-if="error" class="error">
      {{ error }}
    </p>

    <button
      class="back-btn"
      @click="$router.push('/doctor/appointments')"
    >
      ← Quay lại danh sách
    </button>

>>>>>>> Stashed changes
  </div>
</template>

<script setup lang="ts">
<<<<<<< Updated upstream
import axios from 'axios'
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const route = useRoute()
const auth = useAuthStore()

const api = axios.create({
  baseURL: 'https://localhost:7235/api',
  headers: { Authorization: `Bearer ${auth.token}` }
})
=======
import { ref, onMounted } from "vue"
import { useRoute } from "vue-router"
import api from "@/services/api"

const route = useRoute()
>>>>>>> Stashed changes

const appointment = ref<any>(null)
const error = ref<string | null>(null)

<<<<<<< Updated upstream
const loadDetail = async () => {
  try {
    const res = await api.get(`/doctor/DoctorAppointments/${route.params.id}`)
    appointment.value = res.data
  } catch (err) {
=======
/* MEDICAL RECORD */

const symptoms = ref("")
const diagnosis = ref("")
const notes = ref("")

/* PRESCRIPTION */

const medicines = ref<any[]>([
  {
    medicineName: "",
    dose: "",
    quantity: 1,
    instruction: ""
  }
])

/* ===== CLINICAL TEST (THÊM) ===== */

const testName = ref("")
const clinicalTests = ref<any[]>([])

const loadClinicalTests = async () => {
  try {
    const res = await api.get(`/ClinicalTests/${appointment.value.id}`)
    clinicalTests.value = res.data
  } catch (err) {
    console.log(err)
  }
}

const createClinicalTest = async () => {
  if (!testName.value) return

  await api.post("/ClinicalTests", {
    medicalRecordId: appointment.value.id,
    testName: testName.value
  })

  alert("Đã tạo yêu cầu xét nghiệm")

  testName.value = ""
  loadClinicalTests()
}

const updateResult = async (test: any) => {
  await api.patch(`/ClinicalTests/${test.id}/result`, {
    result: test.result,
    technicianName: test.technicianName
  })

  alert("Đã cập nhật kết quả")
}

/* LOAD DETAIL */

const loadDetail = async () => {
  try {
    const res = await api.get(
      `/doctor/DoctorAppointments/${route.params.id}`
    )
    appointment.value = res.data
  } catch {
>>>>>>> Stashed changes
    error.value = "Bạn không được phép xem lịch khám này."
  }
}

<<<<<<< Updated upstream
const formatDate = (dateStr: string) => {
  if (!dateStr) return ''
=======
/* APPROVE */

const approveAppointment = async () => {
  await api.patch(
    `/doctor/DoctorAppointments/${appointment.value.id}/approve`
  )
  alert("Đã xác nhận lịch khám")
  loadDetail()
}

/* COMPLETE */

const completeAppointment = async () => {
  await api.patch(
    `/doctor/DoctorAppointments/${appointment.value.id}/complete`
  )
  alert("Đã hoàn thành lịch khám")
  loadDetail()
}

/* CANCEL */

const cancelAppointment = async () => {
  await api.patch(
    `/doctor/DoctorAppointments/${appointment.value.id}/cancel`
  )
  alert("Đã hủy lịch khám")
  loadDetail()
}

/* PRESCRIPTION FUNCTIONS */

const addMedicine = () => {
  medicines.value.push({
    medicineName: "",
    dose: "",
    quantity: 1,
    instruction: ""
  })
}

const removeMedicine = (index: number) => {
  medicines.value.splice(index, 1)
}

/* SAVE MEDICAL RECORD */

const saveMedicalRecord = async () => {

  if (!confirm("Lưu Medical Record?")) return

  const payload = {
    appointmentId: appointment.value.id,
    symptoms: symptoms.value,
    diagnosis: diagnosis.value,
    notes: notes.value,
    medicines: medicines.value.filter(m => m.medicineName)
  }

  await api.post("/doctor/MedicalRecords", payload)

  alert("Đã lưu Medical Record")

  symptoms.value = ""
  diagnosis.value = ""
  notes.value = ""

  medicines.value = [
    {
      medicineName: "",
      dose: "",
      quantity: 1,
      instruction: ""
    }
  ]
}

/* FORMAT DATE */

const formatDate = (dateStr: string) => {
  if (!dateStr) return ""
>>>>>>> Stashed changes
  const date = new Date(dateStr)
  return date.toLocaleDateString()
}

onMounted(() => {
  loadDetail()
<<<<<<< Updated upstream
=======
  loadClinicalTests()
>>>>>>> Stashed changes
})
</script>

<style src="@/styles/layouts/appointment-detail.css"></style>
