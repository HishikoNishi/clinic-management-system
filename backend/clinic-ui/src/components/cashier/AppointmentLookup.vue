<script setup lang="ts">
import { ref } from 'vue'
import { useAppointment } from '@/composables/useAppointment'

const emit = defineEmits<{
  (e: 'found', data: any): void
}>()

const code = ref('')
const { appointment, loading, error, lookupByCode } = useAppointment()

const handleLookup = async () => {
  const data = await lookupByCode(code.value)
  if (data?.id) emit('found', data)
}
</script>

<template>
  <div class="card shadow-sm h-100">
    <div class="card-body">
      <div class="d-flex justify-content-between align-items-center mb-3">
        <h5 class="card-title mb-0">Tra cứu lịch hẹn</h5>
        <span class="text-muted small">Nhập mã lịch hẹn</span>
      </div>
      <div class="input-group mb-3">
        <input v-model="code" type="text" class="form-control" placeholder="Mã lịch hẹn (AppointmentCode)" />
        <button class="btn btn-outline-primary" type="button" :disabled="loading" @click="handleLookup">
          <span v-if="loading" class="spinner-border spinner-border-sm me-1" />Tra cứu
        </button>
      </div>
      <div v-if="error" class="alert alert-danger py-2">{{ error }}</div>

      <div v-if="appointment" class="appointment-card">
        <div class="d-flex justify-content-between">
          <div>
            <div class="fw-semibold">{{ appointment.fullName }}</div>
            <div class="text-muted small">{{ appointment.phone }} · {{ appointment.email || 'Không email' }}</div>
          </div>
          <span class="badge bg-info text-dark">{{ appointment.appointmentCode }}</span>
        </div>
        <div class="mt-2 text-muted small">
          Ngày {{ new Date(appointment.appointmentDate).toLocaleDateString() }} ·
          Giờ {{ appointment.appointmentTime }} · Trạng thái: {{ appointment.status }}
        </div>
        <div v-if="!appointment.id" class="alert alert-warning mt-2 mb-0 py-2">
          API chưa trả về <code>Id</code>; cần GUID lịch hẹn để tạo hóa đơn.
        </div>
      </div>
    </div>
  </div>
</template>
