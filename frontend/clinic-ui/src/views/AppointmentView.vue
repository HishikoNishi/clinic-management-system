<script setup lang="ts">
import { ref, computed, onMounted } from "vue"
import api from "@/services/api"
import { useAuthStore } from "@/stores/auth"

const auth = useAuthStore()

/* SEARCH */

const searchCode = ref("")
const searchName = ref("")
const searchPhone = ref("")
const searchDate = ref("")

/* DATA */

const appointments = ref<any[]>([])

const statuses = [
  "All",
  "Pending",
  "Confirmed",
  "Completed",
  "Cancelled"
]

const currentStatus = ref("All")

/* LOAD APPOINTMENTS */

const loadAppointments = async () => {

  try {

    const res = await api.get("/doctor/DoctorAppointments")

    appointments.value = res.data || []

  } catch (err) {

    console.error("Load appointments error:", err)

  }

}

/* FILTER */

const filteredAppointments = computed(() => {

  return appointments.value.filter((a: any) => {

    const matchStatus =
      currentStatus.value === "All" ||
      a.status === currentStatus.value

    const matchCode =
      !searchCode.value ||
      a.appointmentCode
        ?.toLowerCase()
        .includes(searchCode.value.toLowerCase())

    const matchName =
      !searchName.value ||
      a.fullName
        ?.toLowerCase()
        .includes(searchName.value.toLowerCase())

    const matchPhone =
      !searchPhone.value ||
      a.phone?.includes(searchPhone.value)

    const matchDate =
      !searchDate.value ||
      a.appointmentDate?.startsWith(searchDate.value)

    return (
      matchStatus &&
      matchCode &&
      matchName &&
      matchPhone &&
      matchDate
    )

  })

})

/* CHANGE STATUS */

const changeStatus = (s: string) => {

  currentStatus.value = s

}

/* APPROVE */

const approveAppointment = async (appointmentId: string) => {

  const ok = confirm("Xác nhận lịch khám này?")

  if (!ok) return

  try {

    await api.patch(`/doctor/DoctorAppointments/${appointmentId}/approve`)

    alert("Đã xác nhận lịch khám")

    loadAppointments()

  } catch (err) {

    console.error(err)

  }

}

/* COMPLETE */

const completeAppointment = async (appointmentId: string) => {

  const ok = confirm("Đánh dấu lịch khám hoàn thành?")

  if (!ok) return

  try {

    await api.patch(`/doctor/DoctorAppointments/${appointmentId}/complete`)

    alert("Đã hoàn thành lịch khám")

    loadAppointments()

  } catch (err) {

    console.error(err)

  }

}

/* CANCEL */

const cancelAppointment = async (appointmentId: string) => {

  const ok = confirm("Bạn chắc chắn muốn hủy lịch khám?")

  if (!ok) return

  try {

    await api.patch(`/doctor/DoctorAppointments/${appointmentId}/cancel`)

    alert("Đã hủy lịch khám")

    loadAppointments()

  } catch (err) {

    console.error(err)

  }

}

/* STATUS LABEL */

const statusLabel = (status: string) => {

  const labels: Record<string, string> = {

    All: "Tất cả",
    Pending: "Chờ xử lý",
    Confirmed: "Đã xác nhận",
    Completed: "Hoàn thành",
    Cancelled: "Đã hủy"

  }

  return labels[status] || status

}

/* CLEAR FILTER */

const clearFilters = () => {

  searchCode.value = ""
  searchName.value = ""
  searchPhone.value = ""
  searchDate.value = ""

}

/* FORMAT DATE */

const formatDateTime = (dateStr: string, timeStr: string) => {

  if (!dateStr) return ""

  const date = new Date(dateStr)

  const [hours, minutes] = timeStr.split(":")

  const day = String(date.getDate()).padStart(2, "0")
  const month = String(date.getMonth() + 1).padStart(2, "0")
  const year = date.getFullYear()

  return `${day}/${month}/${year} ${hours}:${minutes}`

}

/* INIT */

onMounted(() => {

  loadAppointments()

})
</script>
<style src="@/styles/layouts/appointment.css"></style>