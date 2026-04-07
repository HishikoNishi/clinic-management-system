<script setup lang="ts">
import { computed, ref } from 'vue'
import type { PaymentMethod } from '@/services/invoiceApi'
import { formatCurrency } from '@/utils/format'

const props = defineProps<{
  appointmentId?: string
  invoiceAmount?: number | null
  isPaid?: boolean
  insuranceCoverPercent?: number
  loading?: boolean
}>()

const emit = defineEmits<{
  (e: 'create', payload: { amount: number }): void
  (e: 'recalc', payload: { insuranceCode?: string; insuranceCoverPercent?: number; surcharge?: number; discount?: number }): void
}>()

const amount = ref<number | null>(null)
const insuranceCode = ref('')
const insuranceCoverPercent = ref<number | null>(null)
const surcharge = ref<number>(0)
const discount = ref<number>(0)
const payMethod = ref<PaymentMethod>('cash')

const canCreate = computed(() =>
  !!props.appointmentId && !!amount.value && amount.value > 0 && !props.loading
)

const disabled = computed(() => props.isPaid || props.loading)
const insuranceLocked = computed(() => (props.insuranceCoverPercent ?? 0) > 0)

const handleCreate = () => {
  if (!amount.value || amount.value <= 0) return
  emit('create', { amount: amount.value })
}

const handleRecalc = () => {
  const payload: any = {
    surcharge: surcharge.value || 0,
    discount: discount.value || 0
  }
  if (!insuranceLocked.value) {
    payload.insuranceCode = insuranceCode.value || undefined
    if (insuranceCoverPercent.value && insuranceCoverPercent.value > 0) {
      payload.insuranceCoverPercent = insuranceCoverPercent.value / 100
    }
  }
  emit('recalc', payload)
}

const autofillFromInvoice = computed(() => formatCurrency(props.invoiceAmount ?? undefined))
</script>

<template>
  <div class="card shadow-sm h-100">
    <div class="card-body">
      <div class="d-flex justify-content-between align-items-center mb-3">
        <h5 class="card-title mb-0">Tạo/Tính hóa đơn</h5>
        <span class="text-muted small">Gắn với lịch hẹn</span>
      </div>

      <div class="mb-3">
        <label class="form-label">Số tiền</label>
        <div class="input-group">
          <input v-model.number="amount" type="number" min="0" class="form-control" :disabled="disabled" placeholder="Nhập số tiền" />
          <span class="input-group-text">VND</span>
        </div>
        <small class="text-muted">Nếu đã tính tự động: {{ autofillFromInvoice }}</small>
      </div>

      <div class="d-flex gap-2 mb-3">
        <button class="btn btn-primary" type="button" :disabled="!canCreate" @click="handleCreate">
          <span v-if="loading" class="spinner-border spinner-border-sm me-1" />Tạo hóa đơn
        </button>
        <button class="btn btn-outline-secondary" type="button" :disabled="disabled" @click="handleRecalc">
          <span v-if="loading" class="spinner-border spinner-border-sm me-1" />Tính tự động / Bảo hiểm
        </button>
      </div>

      <div class="fw-semibold mb-2">Bảo hiểm & phụ thu</div>
      <div class="row g-3">
        <div class="col-md-6">
          <label class="form-label">Mã bảo hiểm</label>
          <input v-model="insuranceCode" type="text" class="form-control" :disabled="disabled || insuranceLocked" />
          <small v-if="insuranceLocked" class="text-success">Đã áp dụng BH trước đó</small>
        </div>
        <div class="col-md-6">
          <label class="form-label">Bảo hiểm chi trả (%)</label>
          <input v-model.number="insuranceCoverPercent" type="number" min="0" max="100" class="form-control" :disabled="disabled || insuranceLocked" />
        </div>
        <div class="col-md-6">
          <label class="form-label">Phụ thu (VND)</label>
          <input v-model.number="surcharge" type="number" min="0" class="form-control" :disabled="disabled" />
        </div>
        <div class="col-md-6">
          <label class="form-label">Giảm trừ (VND)</label>
          <input v-model.number="discount" type="number" min="0" class="form-control" :disabled="disabled" />
        </div>
      </div>
    </div>
  </div>
</template>
