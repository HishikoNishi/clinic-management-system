<template>
  <div class="doctor-container">
    <div class="header d-flex justify-content-between align-items-start">
      <div class="status-highlight-card">
        <h2 class="mb-3">Lịch khám của tôi</h2>
        <div class="status-box">
          <label class="form-label fw-bold">Trạng thái hiện tại của tôi:</label>
          <select v-model="doctorStatus" @change="updateDoctorStatus" class="form-select status-select">
            <option value="Active">🟢 Hoạt động (Sẵn sàng)</option>
            <option value="Busy">🟡 Đang bận (Trong ca khám)</option>
            <option value="Inactive">🔴 Nghỉ (Không làm việc)</option>
          </select>
          <div class="text-primary small mt-2">
            <i class="bi bi-info-circle"></i> Thay đổi để bệnh nhân và nhân viên biết bạn đang ở đâu.
          </div>
        </div>
      </div>

      <div class="filter-subtle">
        <label class="form-label">Lọc danh sách theo:</label>
        <div class="d-flex gap-2">
          <select v-model="currentStatus" class="form-select form-select-sm border-0 bg-light" @change="loadAppointments">
            <option value="All">Tất cả lịch khám</option>
            <option value="CheckedIn,Confirmed">Đã check-in + Chờ khám</option>
            <option value="CheckedIn">Đã check-in</option>
            <option value="Confirmed">Chờ khám</option>
            <option value="Completed">Đã khám xong</option>
          </select>
          <button class="btn btn-light btn-sm text-muted" @click="loadAppointments">
            <i class="bi bi-arrow-clockwise"></i>
          </button>
        </div>
      </div>
    </div>

    <div v-if="error" class="alert alert-danger py-2 mt-2">{{ error }}</div>

    <div class="card mt-3">
      <div class="card-body p-0">
        <table class="table mb-0">
          <thead>
            <tr>
              <th>Bệnh nhân</th>
              <th>Điện thoại</th>
              <th>Mã bệnh nhân</th>
              <th>CCCD</th>
              <th>BHYT</th>
              <th>Ngày khám</th>
              <th>Trạng thái</th>
              <th class="text-end">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
              <td colspan="8" class="text-center py-4 text-muted">
                <span class="spinner-border spinner-border-sm me-2"></span>Đang tải...
              </td>
            </tr>
            <tr v-else-if="appointments.length === 0">
              <td colspan="8" class="text-center py-4 text-muted">Không có lịch khám</td>
            </tr>
            <tr v-else v-for="a in appointments" :key="a.id">
              <td class="fw-semibold">{{ a.fullName }}</td>
              <td>{{ a.phone }}</td>
              <td>{{ a.patientCode || '—' }}</td>
              <td>{{ a.citizenId || '—' }}</td>
              <td>{{ a.insuranceCardNumber || '—' }}</td>
              <td>{{ formatDateTime(a.appointmentDate, a.appointmentTime) }}</td>
              <td>
                <span :class="['badge', badgeClass(a.status)]">{{ statusLabel(a.status) }}</span>
              </td>
              <td class="text-end">
                <button class="btn btn-primary btn-sm" @click="goExamine(a.id)">
                  <i class="bi bi-stethoscope me-1"></i>
                  {{ a.status === 'Completed' ? 'Xem hồ sơ' : 'Khám' }}
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue"
import { useRouter } from "vue-router"
import api from "@/services/api"

const router = useRouter()
const appointments = ref<any[]>([])
const loading = ref(false)
const error = ref<string | null>(null)

const currentStatus = ref("All")
const doctorStatus = ref("Active")
const doctorId = localStorage.getItem("doctorId")

async function updateDoctorStatus() {
  if (!doctorId) return;
  try {
    await api.put(`/doctor/${doctorId}/status`, {
      Status: doctorStatus.value
    });
    localStorage.setItem("selectedDoctorStatus", doctorStatus.value);
  } catch (err: any) {
    error.value = "Không thể cập nhật trạng thái bác sĩ";
  }
}

const syncDoctorStatus = async () => {
  const cached = localStorage.getItem("selectedDoctorStatus");
  if (cached) doctorStatus.value = cached;

  if (!doctorId) return;
  try {
    const res = await api.get(`/doctor/profile`);
    const statusMap: any = { 0: 'Inactive', 1: 'Active', 2: 'Busy' };
    const dbStatus = typeof res.data.status === 'number' ? statusMap[res.data.status] : res.data.status;
    
    if (dbStatus) {
      doctorStatus.value = dbStatus;
      localStorage.setItem("selectedDoctorStatus", dbStatus);
    }
  } catch (err) {
    console.error("Lỗi đồng bộ trạng thái:", err);
  }
}

const loadAppointments = async () => {
  loading.value = true
  error.value = null
  try {
    const params = currentStatus.value === "All"
      ? {}
      : { status: currentStatus.value }

    const res = await api.get("/doctor/DoctorAppointments", { params })
    appointments.value = res.data ?? []
  } catch (err) {
    error.value = "Không tải được danh sách lịch khám."
  } finally {
    loading.value = false
  }
}

const goExamine = (id: string) => {
  router.push(`/doctor/examination/${id}`)
}

const statusLabel = (status: string) => {
  const labels: Record<string, string> = {
    CheckedIn: "Đã check-in",
    Confirmed: "Chờ khám",
    Completed: "Đã khám xong"
  }
  return labels[status] || status
}

const badgeClass = (status: string) => {
  if (status === "CheckedIn") return "bg-info-subtle text-info"
  if (status === "Confirmed") return "bg-warning-subtle text-warning"
  if (status === "Completed") return "bg-success-subtle text-success"
  return "bg-secondary-subtle text-secondary"
}

const formatDateTime = (dateStr: string, timeStr: string) => {
  if (!dateStr || !timeStr) return ""
  const date = new Date(dateStr)
  const [hours, minutes] = timeStr.split(":")
  const day = String(date.getDate()).padStart(2, "0")
  const month = String(date.getMonth() + 1).padStart(2, "0")
  const year = date.getFullYear()
  return `${day}/${month}/${year} ${hours}:${minutes}`
}

onMounted(() => {
  syncDoctorStatus()
  loadAppointments()
})
</script>

<style src="@/styles/layouts/doctor-appointments.css"></style>