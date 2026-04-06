<template>
  <div class="doctor-container">
    <div class="header">
      <div>
        <h2>Lịch khám của tôi</h2>
         <!-- Chọn trạng thái của bác sĩ -->
        <div class="mb-3">
          <label class="form-label fw-semibold">Trạng thái của tôi:</label>
                    <disable class="form-label fw-semibold">(Cật nhật thay đổi trạng thái hoạt động, đang bận khám, hay nghỉ)</disable >

          <select v-model="doctorStatus" @change="updateDoctorStatus" class="form-select form-select-sm" style="max-width:200px">
            <option value="Active">Hoạt động</option>
            <option value="Busy">Đang khám</option>
            <option value="Inactive">Không hoạt động</option>
          </select>
        </div>
        <p class="text-muted mb-0">Hiển thị lịch đã được phân cho bác sĩ (Đã check-in / Chờ khám).</p>
      </div>
         

      <div class="filter d-flex gap-2">
        
       <label class="form-label fw-semibold">Lọc/tìm theo trạng thái:</label>
            
        <select v-model="currentStatus" class="form-select form-select-sm" @change="loadAppointments">
          <option value="CheckedIn,Confirmed">Đã check-in + Chờ khám</option>
          <option value="CheckedIn">Đã check-in</option>
          <option value="Confirmed">Chờ khám</option>
          <option value="Completed">Đã khám xong</option>
          <option value="All">Tất cả</option>
        </select>
        <button class="btn btn-outline-secondary btn-sm" @click="loadAppointments">
          <i class="bi bi-arrow-clockwise"></i> Làm mới
        </button>
      </div>
    </div>

    <div v-if="error" class="alert alert-danger py-2">{{ error }}</div>

    <div class="card">
      <div class="card-body p-0">
        <table class="table mb-0">
          <thead>
            <tr>
              <th>Bệnh nhân</th>
              <th>Điện thoại</th>
              <th>Ngày khám</th>
              <th>Trạng thái</th>
              <th class="text-end">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
              <td colspan="5" class="text-center py-4 text-muted">
                <span class="spinner-border spinner-border-sm me-2"></span>Đang tải...
              </td>
            </tr>
            <tr v-else-if="appointments.length === 0">
              <td colspan="5" class="text-center py-4 text-muted">Không có lịch khám</td>
            </tr>
            <tr v-else v-for="a in appointments" :key="a.id">
              <td class="fw-semibold">{{ a.fullName }}</td>
              <td>{{ a.phone }}</td>
              <td>{{ formatDateTime(a.appointmentDate, a.appointmentTime) }}</td>
              <td>
                <span :class="['badge', badgeClass(a.status)]">{{ statusLabel(a.status) }}</span>
              </td>
              <td class="text-end">
                <button class="btn btn-primary btn-sm" @click="goExamine(a.id)">
                  <i class="bi bi-stethoscope me-1"></i> Khám
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
const currentStatus = ref("CheckedIn,Confirmed")
// trạng thái của bác sĩ
const doctorStatus = ref("Active")
const doctorId = localStorage.getItem("doctorId")
async function updateDoctorStatus() {
  if (!doctorId) return;
  try {
    await api.put(`/doctor/${doctorId}/status`, {
      Status: doctorStatus.value
    });
    
    // LƯU VÀO CACHE: Để khi chuyển trang quay lại sẽ thấy ngay
    localStorage.setItem("selectedDoctorStatus", doctorStatus.value);
    
    console.log("Đã lưu trạng thái vào máy:", doctorStatus.value);
  } catch (err: any) {
    error.value = "Không thể cập nhật trạng thái";
  }
}

const loadAppointments = async () => {
  loading.value = true
  error.value = null
  appointments.value = []
  try {
    const params = currentStatus.value === "All"
      ? {}
      : { status: currentStatus.value }

    const res = await api.get("/doctor/DoctorAppointments", { params })
    appointments.value = res.data ?? []
  } catch (err) {
    console.error(err)
    error.value = "Không tải được dữ liệu. Vui lòng thử lại."
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
const mapStatus = (s: number | string) => {
  // Ép kiểu hoặc kiểm tra kỹ String/Number
  if (s === 1 || s === "Active") return "Active";
  if (s === 2 || s === "Busy") return "Busy";
  if (s === 0 || s === "Inactive") return "Inactive";
  return "Active"; // Chỉ để Active nếu thực sự không khớp cái nào
}
const loadDoctorStatus = async () => {
  // BƯỚC 1: Lấy từ bộ nhớ tạm của trình duyệt trước
  const savedStatus = localStorage.getItem("selectedDoctorStatus");
  if (savedStatus) {
    doctorStatus.value = savedStatus;
  }

  if (!doctorId) return;

  // BƯỚC 2: Đồng bộ lại với Database để đảm bảo chính xác
  try {
    const res = await api.get(`/doctor/${doctorId}/profile`);
    const statusFromDb = mapStatus(res.data.status);
    
    doctorStatus.value = statusFromDb;
    localStorage.setItem("selectedDoctorStatus", statusFromDb); // Cập nhật lại cache
  } catch (err) {
    console.error("Lỗi đồng bộ trạng thái:", err);
  }
}
onMounted(() => {
  loadDoctorStatus()   // lấy trạng thái thực tế từ backend
  loadAppointments()
})

</script>

<style src="@/styles/layouts/doctor-appointments.css"></style>
