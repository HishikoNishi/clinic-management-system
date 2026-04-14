<template>
  <div class="staff-container">
    <div class="staff-page-header">
        <div>
          <div class="staff-eyebrow">Staff</div>
          <h2 class="staff-title">Quản lý lịch khám</h2>
          <p class="staff-subtitle">Tìm kiếm lịch, check-in bệnh nhân và đưa vào hàng chờ theo phòng.</p>
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

    <div class="row g-3 mb-3">
      <div class="col-lg-4">
        <div class="card shadow-sm h-100">
          <div class="card-body">
            <div class="d-flex justify-content-between align-items-start mb-3">
              <div>
                <h5 class="mb-1">Phòng check-in</h5>
                <p class="text-muted small mb-0">Chọn phòng trước khi đưa bệnh nhân vào hàng chờ.</p>
              </div>
              <button class="btn btn-outline-secondary btn-sm" @click="reloadRoomQueue">
                <i class="bi bi-arrow-clockwise"></i>
              </button>
            </div>

            <label class="form-label">Phòng khám</label>
            <select v-model="selectedRoomId" class="form-select">
              <option value="">Chọn phòng</option>
              <option v-for="room in rooms" :key="room.id" :value="room.id">
                {{ room.name }} - {{ room.departmentName }}
              </option>
            </select>

            <div class="small text-muted mt-3">
              <div v-if="selectedRoomState">
                <div><strong>Mã phòng:</strong> {{ selectedRoomState.roomCode }}</div>
                <div><strong>Khoa:</strong> {{ selectedRoomState.departmentName }}</div>
                <div><strong>Đang chờ:</strong> {{ selectedRoomState.waitingCount }}</div>
                <div><strong>Đang khám:</strong> {{ selectedRoomState.inProgressCount }}</div>
              </div>
              <div v-else>
                Chưa chọn phòng hoặc phòng chưa có hàng chờ.
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="col-lg-8">
        <div class="card shadow-sm h-100">
          <div class="card-body">
            <div class="d-flex justify-content-between align-items-start mb-3">
              <div>
                <h5 class="mb-1">Hàng chờ theo phòng</h5>
                <p class="text-muted small mb-0">Ưu tiên bệnh nhân đã đặt lịch trước, sau đó theo số thứ tự.</p>
              </div>
              <span v-if="selectedRoomState" class="badge bg-primary-subtle text-primary">
                {{ selectedRoomState.roomName }}
              </span>
            </div>

            <div v-if="roomQueueLoading" class="text-muted small">
              <span class="spinner-border spinner-border-sm me-2"></span>Đang tải hàng chờ...
            </div>

            <template v-else-if="selectedRoomState">
              <div class="alert alert-info py-2" v-if="selectedRoomState.currentCalling">
                Đang gọi:
                <strong>#{{ selectedRoomState.currentCalling.queueNumber }}</strong>
                - {{ selectedRoomState.currentCalling.fullName }}
                <span class="text-muted">({{ selectedRoomState.currentCalling.appointmentCode }})</span>
              </div>
              <div v-else class="alert alert-light py-2">
                Chưa có bệnh nhân nào đang được gọi trong phòng này.
              </div>

              <div class="table-responsive">
                <table class="table table-sm align-middle mb-0">
                  <thead>
                    <tr>
                      <th>STT</th>
                      <th>Bệnh nhân</th>
                      <th>Mã lịch</th>
                      <th>Loại</th>
                      <th>Trạng thái</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="item in selectedRoomState.items" :key="item.id">
                      <td class="fw-semibold">#{{ item.queueNumber }}</td>
                      <td>
                        <div class="fw-semibold">{{ item.fullName }}</div>
                        <div class="text-muted small">{{ item.phone || '—' }}</div>
                      </td>
                      <td class="text-monospace">{{ item.appointmentCode }}</td>
                      <td>
                        <span class="badge" :class="item.isPriority ? 'bg-success-subtle text-success' : 'bg-secondary-subtle text-secondary'">
                          {{ item.isPriority ? 'Đặt trước' : 'Tại quầy' }}
                        </span>
                      </td>
                      <td>
                        <span class="badge" :class="item.status === 'InProgress' ? 'bg-warning-subtle text-warning' : 'bg-info-subtle text-info'">
                          {{ item.status === 'InProgress' ? 'Đang khám' : 'Đang chờ' }}
                        </span>
                      </td>
                    </tr>
                    <tr v-if="selectedRoomState.items.length === 0">
                      <td colspan="5" class="text-center text-muted py-3">Phòng này chưa có hàng chờ.</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </template>

            <div v-else class="text-muted small">
              Chọn một phòng để xem hàng chờ.
            </div>
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
    <th>Mã lịch</th>
    <th>Bệnh nhân</th>
    <th>Mã BN</th>
    <th>CCCD</th>
    <th>BHYT</th>
    <th>Điện thoại</th>
    <th>Ngày sinh</th>
    <th>Ngày khám</th>
    <th>Trạng thái</th>
    <th>Yêu cầu của bệnh nhân</th>
    <th>Bác sĩ</th>
    
       <th>Thao tác</th>
  </tr>
</thead>
          <tbody>
            <tr v-for="a in filteredAppointments" :key="a.id">
              <td>{{ a.appointmentCode }}</td>
              <td class="fw-semibold">{{ a.fullName }}</td>
              <td class="text-monospace small">{{ a.patientCode || '—' }}</td>
              <td class="text-monospace small">{{ a.citizenId || '—' }}</td>
              <td class="text-monospace small">{{ a.insuranceCardNumber || '—' }}</td>
              <td>{{ a.phone }}</td>
              <td>{{ formatDate(a.dateOfBirth) }}</td>
              <td>{{ formatDateTime(a.appointmentDate, a.appointmentTime) }}</td>
              <td>
                <span :class="'status ' + a.statusDetail.value.toLowerCase()">
                  {{ statusLabel(a.statusDetail.value) }}
                </span>
              </td>
              <td class="staff-reason">{{ a.reason }}</td>
              <td>{{ a.statusDetail.doctorName || 'Sẽ gán theo phòng' }}</td>
              <td @click.stop class="staff-actions">
                <button
                  v-if="canCheckIn(a)"
                  class="btn btn-sm btn-success me-2"
                  @click="checkInAppointment(a)"
                >
                  Check-in + Vào queue
                </button>
                <span v-else-if="needsRoomSelection(a)" class="text-muted small">(chọn phòng trước)</span>
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
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount, watch } from 'vue'
import api from '@/services/api'
import { useAuthStore } from '@/stores/auth'
import { queueService, type RoomOption, type RoomQueueState } from '@/services/queueService'

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
  patientCode?: string       
  citizenId?: string     
  insuranceCardNumber?: string
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
const rooms = ref<RoomOption[]>([])
const selectedRoomId = ref('')
const selectedRoomState = ref<RoomQueueState | null>(null)
const roomQueueLoading = ref(false)
let roomQueueTimer: ReturnType<typeof setInterval> | null = null

const statuses = ['All', 'Pending', 'Confirmed', 'CheckedIn', 'Completed', 'Cancelled']
const currentStatus = ref('All')

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

const loadRooms = async () => {
  const list = await queueService.getRooms()
  rooms.value = list
}

const loadSelectedRoomQueue = async () => {
  if (!selectedRoomId.value) {
    selectedRoomState.value = null
    return
  }

  try {
    roomQueueLoading.value = true
    selectedRoomState.value = await queueService.getRoomQueue(selectedRoomId.value)
  } catch (err: any) {
    if (err?.response?.status === 404) {
      selectedRoomState.value = null
      return
    }
    console.error(err)
  } finally {
    roomQueueLoading.value = false
  }
}

const reloadRoomQueue = async () => {
  await loadRooms()
  await loadSelectedRoomQueue()
}

const loadAppointments = async () => {
  const url = currentStatus.value === 'All'
    ? '/staff/StaffAppointments'
    : `/staff/StaffAppointments/filter?status=${currentStatus.value}`
  const res = await api.get<Appointment[]>(url)
  appointments.value = res.data
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
    insuranceForAppointmentId.value = appointmentIdForCode ?? ''
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
  if (!a.statusDetail?.doctorId && !selectedRoomId.value) {
    alert('Vui lòng chọn phòng trước khi check-in')
    return
  }

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
  try {
    await api.post("/staff/StaffAppointments/checkin", {
      appointmentId: a.id,
      roomId: a.statusDetail?.doctorId ? null : selectedRoomId.value || null,
      depositAmount: amount,
      method: "cash",
      isInpatient,
      insuranceCode: insuranceCode.value,
      insuranceCoverPercent: cover
    })

    const queue = await queueService.checkIn(a.id, a.statusDetail?.doctorId ? null : selectedRoomId.value)
    selectedRoomId.value = queue.roomId
    alert(
      `${isInpatient ? 'Check-in & tạm ứng thành công' : 'Check-in ngoại trú thành công'}\n` +
      `Đã xếp phòng ${queue.roomName} - số thứ tự #${queue.queueNumber}.`
    )
    await loadAppointments()
    await loadSelectedRoomQueue()
  } catch (err: any) {
    const msg = err?.response?.data?.message || 'Không thể check-in vào hàng chờ'
    alert(msg)
  }
}
const canCheckIn = (a: Appointment) => {
  if (!a) return false
  const status = a.statusDetail?.value
  if (status !== 'Pending' && status !== 'Confirmed') return false
  if (a.statusDetail?.doctorId) return true
  return !!selectedRoomId.value
}
const needsRoomSelection = (a: Appointment) =>
  (a.statusDetail?.value === 'Pending' || a.statusDetail?.value === 'Confirmed') &&
  !a.statusDetail?.doctorId &&
  !selectedRoomId.value
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
    Confirmed: 'Đã đặt lịch với bác sĩ',
    CheckedIn: 'Đã check-in',
    Completed: 'Hoàn thành',
    Cancelled: 'Đã hủy',
  }
  return labels[status] || status
}
const handleDepartmentFilter = () => {
  selectedDoctor.value = ""
}

onMounted(() => {
  loadDepartments()
  loadDoctors()
  loadAppointments()
  loadRooms()
  roomQueueTimer = setInterval(() => {
    loadSelectedRoomQueue()
  }, 15000)
})

watch(selectedRoomId, () => {
  loadSelectedRoomQueue()
})

onBeforeUnmount(() => {
  if (roomQueueTimer) {
    clearInterval(roomQueueTimer)
  }
})
</script>

<style src="@/styles/layouts/staff-appointment.css"></style>



