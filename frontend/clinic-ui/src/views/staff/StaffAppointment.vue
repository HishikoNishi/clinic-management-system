<template>
  <div class="staff-container">
    <div class="staff-page-header">
      <div>
        <div class="staff-eyebrow">Staff</div>
        <h2 class="staff-title">Quản lý lịch khám</h2>
        <p class="staff-subtitle">Tìm kiếm, lọc trạng thái và điều phối bác sĩ cho từng lịch khám.</p>
      </div>
      <button class="btn btn-primary" @click="$router.push('/staff/create-appointment')">
        <i class="bi bi-calendar-plus me-2"></i>Đặt lịch tại quầy
      </button>
    </div>

    <div class="staff-filters card shadow-sm">
      <div class="card-body">
        <div class="search-bar">
          <div class="field">
            <i class="bi bi-upc-scan" aria-hidden="true"></i>
            <input v-model="searchCode" placeholder="Tìm theo mã..." />
          </div>
          <div class="field">
            <i class="bi bi-person" aria-hidden="true"></i>
            <input v-model="searchName" placeholder="Tìm theo tên bệnh nhân..." />
          </div>
          <div class="field">
            <i class="bi bi-telephone" aria-hidden="true"></i>
            <input v-model="searchPhone" placeholder="Tìm theo số điện thoại..." />
          </div>
          <div class="field">
            <i class="bi bi-calendar3" aria-hidden="true"></i>
            <input type="date" v-model="searchDate" />
          </div>

          <div class="field">
            <i class="bi bi-building" aria-hidden="true"></i>
            <select v-model="selectedDepartment" @change="handleDepartmentFilter">
              <option value="">Tất cả khoa</option>
              <option v-for="dep in departments" :key="dep.id" :value="dep.id.toString()">
                {{ dep.name }}
              </option>
            </select>
          </div>

          <div class="field">
            <i class="bi bi-person-workspace" aria-hidden="true"></i>
            <select v-model="selectedDoctor" :disabled="!selectedDepartment">
              <option value="">Tất cả bác sĩ</option>
              <option v-for="d in doctorOptions" :key="d.id" :value="d.id.toString()">
                {{ d.fullName }}
              </option>
            </select>
          </div>
        </div>
      </div>
    </div>

    <div class="filter">
      <button
        v-for="s in statuses"
        :key="s"
        :class="{ active: currentStatus === s }"
        @click="changeStatus(s)"
      >
        {{ statusLabel(s) }}
      </button>
    </div>

    <div class="staff-table card shadow-sm">
      <div class="table-responsive">
        <table>
          <thead>
            <tr>
              <th>Mã</th>
              <th>Bệnh nhân</th>
              <th>Điện thoại</th>
              <th>Ngày sinh</th>
              <th>Ngày khám</th>
              <th>Trạng thái</th>
              <th>Triệu chứng</th>
              <th>Bác sĩ</th>
              <th>Gán/Điều phối bác sĩ</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="a in filteredAppointments" :key="a.id">
              <td>{{ a.appointmentCode }}</td>
              <td class="fw-semibold">{{ a.fullName }}</td>
              <td>{{ a.phone }}</td>
              <td>{{ formatDate(a.dateOfBirth) }}</td>
              <td>{{ formatDateTime(a.appointmentDate, a.appointmentTime) }}</td>
              <td>
                <span :class="'status ' + a.statusDetail.value.toLowerCase()">
                  {{ statusLabel(a.statusDetail.value) }}
                </span>
              </td>
              <td class="staff-reason">{{ a.reason }}</td>
              <td>{{ a.statusDetail.doctorName || 'Chưa gán' }}</td>
              <td @click.stop class="staff-actions">
                <template v-if="canAssignDoctor(a)">
                  <button class="btn btn-sm btn-primary me-2" @click="openAssignModal(a)">
                    {{ a.statusDetail.doctorId ? "Đổi bác sĩ" : "Gán bác sĩ" }}
                  </button>
                  <button v-if="a.statusDetail.doctorId" class="btn btn-sm btn-success me-2" @click="checkInAppointment(a)">
                    Check-in + Tạm ứng
                  </button>
                  <span v-else class="text-muted small">(gán bác sĩ trước)</span>
                </template>
                <button class="btn btn-sm btn-outline-primary" @click="$router.push(`/staff/appointments/${a.id}`)">
                  Chi tiết
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <p v-if="appointments.length === 0">Không có lịch khám</p>

    <div v-if="showAssignModal" class="modal-backdrop" @click="showAssignModal = false">
      <div class="modal-content assign-modal" @click.stop>
        <div class="modal-header">
          <h4 class="modal-title">
            <span class="icon-doctor">👨‍⚕️</span> Gán/Đổi bác sĩ cho lịch khám
          </h4>
          <button class="modal-close" @click="showAssignModal = false">&times;</button>
        </div>

        <div class="modal-body assign-grid">
          <div class="assign-form">
            <div class="form-group">
              <label for="departmentSelect">Chọn khoa *</label>
              <select 
                id="departmentSelect"
                v-model="assignForm.departmentId" 
                class="form-select form-control"
              >
                <option value="">-- Chọn khoa --</option>
                <option v-for="dep in departments" :key="dep.id" :value="dep.id.toString()">
                  {{ dep.name }}
                </option>
              </select>
            </div>

            <div class="form-group">
              <label for="specialtySelect">Chọn chuyên khoa *</label>
              <select 
                id="specialtySelect"
                v-model="assignForm.specialtyId" 
                class="form-select form-control"
                :disabled="!assignForm.departmentId"
              >
                <option value="">-- Chọn chuyên khoa --</option>
                <option v-for="s in assignSpecialties" :key="s.id" :value="s.id.toString()">
                  {{ s.name }}
                </option>
              </select>
            </div>

            <div class="form-group">
              <label for="doctorSelect">Chọn bác sĩ *</label>
              <select 
                id="doctorSelect"
                v-model="assignForm.doctorId" 
                class="form-select form-control"
                :disabled="!assignForm.specialtyId"
              >
                <option value="">-- Chọn bác sĩ --</option>
                <option
                  v-for="d in assignDoctorsList"
                  :key="d.id"
                  :value="d.id.toString()"
                  :disabled="isDoctorBusy(d)"
                >
                  {{ d.fullName }} <span v-if="isDoctorBusy(d)">- (Đang khám)</span>
                </option>
              </select>
            </div>
          </div>

          <div class="assign-list card shadow-sm">
            <div class="card-body p-2">
              <div class="d-flex justify-content-between align-items-center mb-2">
                <h6 class="mb-0">Bác sĩ khả dụng</h6>
                <small class="text-muted">{{ assignDoctorsList.length }} bác sĩ</small>
              </div>
              <div class="doctor-card-list">
                <div v-if="!assignForm.specialtyId" class="text-muted small">Chọn khoa & chuyên khoa để xem.</div>
                <div v-else-if="assignDoctorsList.length === 0" class="text-muted small">Không có bác sĩ phù hợp.</div>
                <div
                  v-else
                  v-for="d in assignDoctorsList"
                  :key="d.id"
                  :class="[
                    'doctor-chip',
                    assignForm.doctorId === d.id.toString() ? 'selected' : '',
                    isDoctorBusy(d) ? 'disabled' : ''
                  ]"
                  @click="!isDoctorBusy(d) && (assignForm.doctorId = d.id.toString())"
                >
                  <div class="d-flex justify-content-between align-items-start">
                    <div>
                      <div class="fw-semibold">{{ d.fullName }}</div>
                      <div class="text-muted small">{{ d.code }} · {{ d.specialtyName }}</div>
                      <div class="d-flex align-items-center gap-1 text-muted small">
                        <span class="status-dot" :class="isDoctorBusy(d) ? 'busy' : 'idle'"></span>
                        <span>{{ doctorStatusLabel(d) }}</span>
                      </div>
                    </div>
                    <!-- <span v-if="isDoctorBusy(d)" class="badge bg-danger-subtle text-danger ms-2">Đang khám</span> -->
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="insurance-box card shadow-sm">
          <div class="card-body">
            <div class="d-flex justify-content-between align-items-center mb-2">
              <div>
                <h6 class="mb-0">Xác thực bảo hiểm trước khi thu tạm ứng</h6>
                <small class="text-muted">Mã lịch: {{ assignForm.appointmentId || 'Chưa chọn' }}</small>
              </div>
              <span
                v-if="insuranceVerified && insuranceForAppointmentId === assignForm.appointmentId"
                class="badge bg-success"
              >
                Đã xác thực
              </span>
            </div>
            <div class="d-flex gap-2 align-items-center">
              <input
                v-model="insuranceCode"
                type="text"
                class="form-control"
                placeholder="Nhập mã bảo hiểm (ví dụ: BHYT-A)"
              />
              <button class="btn btn-outline-primary" :disabled="verifyingInsurance" @click="verifyInsurance()">
                <span v-if="verifyingInsurance" class="spinner-border spinner-border-sm me-1"></span>
                Kiểm tra
              </button>
            </div>
            <div v-if="insuranceError" class="text-danger small mt-1">{{ insuranceError }}</div>
            <div
              v-if="insuranceVerified && insuranceInfo"
              class="alert alert-success mt-2 py-2 mb-0"
            >
              Kế hoạch: {{ insuranceInfo.name || insuranceInfo.code }} ·
              Chi trả: {{ Math.round((insuranceInfo.coveragePercent ?? insuranceInfo.coverage ?? 0) * 100) }}%
            </div>
          </div>
        </div>

        <div class="modal-footer">
          <button class="btn btn-secondary" @click="showAssignModal = false">
            ✕ Hủy
          </button>
          <button class="btn btn-success" @click="confirmAssignDoctor">
            ✓ Xác nhận gán bác sĩ
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import api from '@/services/api'
import { useAuthStore } from '@/stores/auth'

useAuthStore()

interface Appointment {
  id: string
  appointmentCode: string
  fullName: string
  phone: string
  dateOfBirth: string
  appointmentDate: string
  appointmentTime: string
  reason: string
  statusDetail: {
    value: string
    doctorName: string
    doctorId?: string
    doctorCode?: string
    doctorDepartmentName?: string
  }
}

const searchCode = ref('')
const searchName = ref('')
const searchPhone = ref('')
const searchDate = ref('')
const selectedDoctor = ref('')
const selectedDepartment = ref('')

const appointments = ref<Appointment[]>([])
const doctors = ref<any[]>([])
const departments = ref<any[]>([])

const statuses = ['All', 'Pending', 'Confirmed', 'CheckedIn', 'Completed', 'Cancelled']
const currentStatus = ref('All')

const showAssignModal = ref(false)
const assignForm = ref({
  appointmentId: '',
  departmentId: '',
  specialtyId: '',
  doctorId: ''
})
const assignSpecialties = ref<any[]>([])
const assignDoctorsList = ref<any[]>([])
const insuranceCode = ref('')
const insuranceInfo = ref<any>(null)
const insuranceVerified = ref(false)
const insuranceError = ref('')
const verifyingInsurance = ref(false)
const insuranceForAppointmentId = ref<string>('')

const doctorOptions = computed(() => {
  if (!selectedDepartment.value) return doctors.value
  return doctors.value.filter(d => d.departmentId?.toString() === selectedDepartment.value)
})

const filteredAppointments = computed(() => {
  return appointments.value.filter(a => {
    const matchCode = !searchCode.value || a.appointmentCode?.toLowerCase().includes(searchCode.value.toLowerCase())
    const matchName = !searchName.value || a.fullName?.toLowerCase().includes(searchName.value.toLowerCase())
    const matchPhone = !searchPhone.value || a.phone?.includes(searchPhone.value)
    const matchDate = !searchDate.value || a.appointmentDate?.startsWith(searchDate.value)
    const matchDept = !selectedDepartment.value || a.statusDetail.doctorDepartmentName === getDepartmentName(selectedDepartment.value)
    const matchDoctor = !selectedDoctor.value || a.statusDetail.doctorId === selectedDoctor.value
    return matchCode && matchName && matchPhone && matchDate && matchDept && matchDoctor
  })
})

const getDepartmentName = (depId: string) => departments.value.find(d => d.id === depId)?.name || ''

const loadDepartments = async () => {
  const res = await api.get('/Departments')
  departments.value = res.data ?? []
}

const loadDoctors = async () => {
  const res = await api.get('/Doctor')
  doctors.value = res.data ?? []
}

const loadAppointments = async () => {
  const url = currentStatus.value === 'All'
    ? '/staff/StaffAppointments'
    : `/staff/StaffAppointments/filter?status=${currentStatus.value}`
  const res = await api.get<Appointment[]>(url)
  appointments.value = res.data
}

const loadAssignSpecialties = async (departmentId: string) => {
  if (!departmentId) { assignSpecialties.value = []; return }
  const res = await api.get(`/Doctor/departments/${departmentId}/specialties`)
  assignSpecialties.value = res.data ?? []
}

const loadAssignDoctorsBySpecialty = async (specialtyId: string) => {
  if (!specialtyId) { assignDoctorsList.value = []; return }
  const res = await api.get(`/Doctor/by-specialty/${specialtyId}`)
  const list = res.data ?? []
  const doctorMap = new Map((doctors.value || []).map((d: any) => [d.id, d]))
  assignDoctorsList.value = list.map((d: any) => {
    const merged = { ...d }
    const match = doctorMap.get(d.id)
    if (match) {
      merged.status = merged.status ?? match.status
      merged.currentStatus = merged.currentStatus ?? match.status
    }
    return merged
  })
}

function openAssignModal(appointment: Appointment) {
  assignForm.value = {
    appointmentId: appointment.id,
    departmentId: '',
    specialtyId: '',
    doctorId: ''
  }
  // Refresh latest doctor statuses before selecting
  loadDoctors()
  insuranceCode.value = ''
  insuranceInfo.value = null
  insuranceVerified.value = false
  insuranceError.value = ''
  insuranceForAppointmentId.value = ''
  showAssignModal.value = true
  loadDepartments()
}

watch(() => assignForm.value.departmentId, async (newVal) => {
  await loadAssignSpecialties(newVal)
})

watch(() => assignForm.value.specialtyId, async (newVal) => {
  await loadAssignDoctorsBySpecialty(newVal)
})

async function confirmAssignDoctor() {
  if (!assignForm.value.doctorId) {
    alert('Vui lòng chọn bác sĩ')
    return
  }
  try {
    await api.post('/staff/StaffAppointments/assign-doctor', {
      appointmentId: assignForm.value.appointmentId,
      doctorId: assignForm.value.doctorId
    })
    alert('Bác sĩ đã được gán/đổi ✅')
    showAssignModal.value = false
    loadAppointments()
  } catch (err: any) {
    const msg = err?.response?.data?.message || 'Không gán được bác sĩ (có thể đang bận)'
    alert(msg)
  }
}

const verifyInsurance = async (codeOverride?: string, appointmentIdForCode?: string) => {
  insuranceError.value = ''
  insuranceInfo.value = null
  insuranceVerified.value = false
  const code = (codeOverride ?? insuranceCode.value).trim()
  if (!code) {
    insuranceError.value = 'Nhập mã bảo hiểm trước khi xác thực'
    return false
  }
  try {
    verifyingInsurance.value = true
    const { data } = await api.get('/insurance/validate', { params: { code } })
    insuranceInfo.value = data
    insuranceVerified.value = true
    insuranceForAppointmentId.value = appointmentIdForCode ?? assignForm.value.appointmentId
    insuranceCode.value = code
    return true
  } catch (err: any) {
    insuranceError.value = err?.response?.data?.message || 'Mã bảo hiểm không hợp lệ'
    insuranceVerified.value = false
    return false
  } finally {
    verifyingInsurance.value = false
  }
}

const ensureInsuranceBeforeDeposit = async (appointment: Appointment) => {
  const alreadyVerifiedForThisAppointment =
    insuranceVerified.value && insuranceForAppointmentId.value === appointment.id

  if (alreadyVerifiedForThisAppointment) return true

  const code = prompt('Nhập mã bảo hiểm trước khi thu tạm ứng')
  if (!code) {
    alert('Cần nhập và xác thực mã bảo hiểm')
    return false
  }
  const ok = await verifyInsurance(code, appointment.id)
  if (!ok) {
    alert(insuranceError.value || 'Mã bảo hiểm không hợp lệ')
  }
  return ok
}

async function checkInAppointment(a: Appointment) {
  const ok = await ensureInsuranceBeforeDeposit(a)
  if (!ok) return

  // Nếu đã có BHYT hợp lệ, cho phép bỏ qua tạm ứng (mặc định 0)
  let amount = 0
const hasInsurance = insuranceVerified.value && (insuranceInfo.value?.coveragePercent ?? insuranceInfo.value?.coverage ?? 0) > 0

  const isInpatient = confirm("Bệnh nhân nhập viện? OK = Có, Cancel = Không")

  if (isInpatient) {
    const depositCap = 200000
    const input = prompt("Nhập số tiền tạm ứng (VND)", depositCap.toString())
    amount = Number(input ?? "0")
    if (!amount || amount <= 0) {
      alert("Số tiền tạm ứng không hợp lệ")
      return
    }
    if (amount > depositCap) {
      alert(`Tạm ứng tối đa ${depositCap.toLocaleString()} VND. Hệ thống sẽ thu ${depositCap.toLocaleString()}.`)
      amount = depositCap
    }
  } else {
    amount = 0 // khám ngoại trú không thu tạm ứng
  }

  const cover = insuranceInfo.value?.coveragePercent ?? insuranceInfo.value?.coverage ?? 0
  await api.post("/staff/StaffAppointments/checkin", {
    appointmentId: a.id,
    depositAmount: amount,
    method: "cash",
    isInpatient,
    insuranceCode: insuranceCode.value,
    insuranceCoverPercent: cover
  })
  alert(isInpatient ? "Check-in & tạm ứng thành công" : "Check-in ngoại trú (không thu tạm ứng)")
  loadAppointments()
}
const canAssignDoctor = (a: Appointment) => {
  if (!a) return false
  const status = a.statusDetail?.value
  return status === 'Pending' || status === 'Confirmed' || (status === 'CheckedIn' && !a.statusDetail?.doctorId)
}
const changeStatus = (s: string) => {
  currentStatus.value = s
  loadAppointments()
}

const formatDate = (dateStr: string) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  return `${date.getDate().toString().padStart(2,'0')}/${(date.getMonth()+1).toString().padStart(2,'0')}/${date.getFullYear()}`
}

const formatDateTime = (dateStr: string, timeStr: string) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  const [hours, minutes] = timeStr.split(':')
  return `${date.getDate().toString().padStart(2,'0')}/${(date.getMonth()+1).toString().padStart(2,'0')}/${date.getFullYear()} ${hours}:${minutes}`
}

const statusLabel = (status: string) => {
  const labels: { [key: string]: string } = {
    All: 'Tất cả',
    Pending: 'Chờ xử lý',
    Confirmed: 'Đã xác nhận',
    CheckedIn: 'Đã check-in',
    Completed: 'Hoàn thành',
    Cancelled: 'Đã hủy',
  }
  return labels[status] || status
}
const handleDepartmentFilter = () => {
  selectedDoctor.value = ""
}

const doctorRawStatus = (d: any) => (d?.status ?? d?.Status ?? d?.statusName ?? d?.currentStatus ?? '').toString().toLowerCase()
const doctorBusyFromAppointments = (doctorId: string) =>
  appointments.value.some(a =>
    a.statusDetail?.doctorId === doctorId &&
    ['checkedin', 'confirmed', 'pending'].includes((a.statusDetail.value || '').toLowerCase())
  )

const isDoctorBusy = (d: any) => {
  const raw = doctorRawStatus(d)
  if (doctorBusyFromAppointments(d.id?.toString?.() || d.id)) return true
  if (!raw && !Number.isNaN(Number(d?.status))) {
    return Number(d.status) !== 0 // treat 0 as Active
  }
  const busyKeywords = ['busy', 'inprogress', 'occupied', 'đang khám', 'dang kham']
  return busyKeywords.includes(raw)
}
const doctorStatusLabel = (d: any) => (isDoctorBusy(d) ? 'Đang khám' : 'Sẵn sàng')

onMounted(() => {
  loadDepartments()
  loadDoctors()
  loadAppointments()
})
</script>

<style src="@/styles/layouts/staff-appointment.css"></style>



