<script setup lang="ts">
import { computed, ref } from 'vue'
import AppointmentLookup from '@/components/cashier/AppointmentLookup.vue'
import InvoiceForm from '@/components/cashier/InvoiceForm.vue'
import InvoiceDetail from '@/components/cashier/InvoiceDetail.vue'
import { useInvoice } from '@/composables/useInvoice'
import { usePayment } from '@/composables/usePayment'
import type { PaymentMethod } from '@/services/invoiceApi'
import { formatCurrency } from '@/utils/format'

const currentAppointment = ref<any | null>(null)
const payAmount = ref<number | null>(null)
const payMethod = ref<PaymentMethod>('cash')

const { invoice, loading, error, recalc, create, fetchInvoice } = useInvoice()
const { loading: payLoading, error: payError, message: payMessage, pay } = usePayment()

const appointmentId = computed(() => currentAppointment.value?.id ?? '')
const isPaid = computed(() => invoice.value?.isPaid ?? false)

const autofillPayAmount = computed(() => payAmount.value ?? invoice.value?.amount ?? 0)

const onAppointmentFound = async (appt: any) => {
  currentAppointment.value = appt
  if (appt?.id) {
    await recalc({ appointmentId: appt.id })
    payAmount.value = invoice.value?.amount ?? null
  }
}

const handleCreate = async ({ amount }: { amount: number }) => {
  if (!appointmentId.value) return
  const res = await create({ appointmentId: appointmentId.value, amount })
  if (res?.invoiceId) {
    await fetchInvoice(res.invoiceId)
    payAmount.value = invoice.value?.amount ?? amount
  }
}

const handleRecalc = async (payload: { insuranceCode?: string; insuranceCoverPercent?: number; surcharge?: number; discount?: number }) => {
  if (!appointmentId.value) return
  await recalc({
    appointmentId: appointmentId.value,
    ...payload
  })
  payAmount.value = invoice.value?.amount ?? payAmount.value
}

const handlePay = async () => {
  if (!invoice.value?.id || !autofillPayAmount.value) return
  const res = await pay({
    invoiceId: invoice.value.id,
    amount: autofillPayAmount.value,
    method: payMethod.value
  })
  if (res?.invoice) {
    invoice.value = res.invoice
  }
}
</script>

<template>
  <div class="container py-4">
    <div class="d-flex align-items-center justify-content-between mb-3">
      <h2 class="mb-0">Hóa đơn & thanh toán</h2>
      <span class="badge bg-primary">Vai trò: Cashier</span>
    </div>

    <div class="row g-3">
      <div class="col-lg-6">
        <AppointmentLookup @found="onAppointmentFound" />
      </div>

      <div class="col-lg-6">
        <InvoiceForm
          :appointment-id="appointmentId"
          :invoice-amount="invoice?.amount ?? null"
          :is-paid="isPaid"
          :loading="loading"
          @create="handleCreate"
          @recalc="handleRecalc"
        />
      </div>
    </div>

    <div class="row g-3 mt-1">
      <div class="col-lg-7">
        <InvoiceDetail :invoice="invoice" />
      </div>
      <div class="col-lg-5">
        <div class="card shadow-sm h-100">
          <div class="card-body">
            <h5 class="card-title mb-3">Thanh toán</h5>
            <div v-if="invoice">
              <div class="mb-2 text-muted small">
                Số tiền cần thanh toán: <span class="fw-semibold">{{ formatCurrency(invoice.amount) }}</span>
              </div>
              <div class="mb-3">
                <label class="form-label">Số tiền thanh toán</label>
                <div class="input-group">
                  <input v-model.number="payAmount" type="number" min="0" class="form-control" :disabled="isPaid || payLoading" />
                  <span class="input-group-text">VND</span>
                </div>
                <small class="text-muted">Mặc định: {{ formatCurrency(invoice.amount) }}</small>
              </div>
              <div class="mb-3">
                <label class="form-label">Phương thức</label>
                <select v-model="payMethod" class="form-select" :disabled="isPaid || payLoading">
                  <option value="cash">Tiền mặt</option>
                  <option value="banker">Chuyển khoản</option>
                  <option value="card">Thẻ</option>
                </select>
              </div>
              <button class="btn btn-primary w-100" :disabled="isPaid || payLoading" @click="handlePay">
                <span v-if="payLoading" class="spinner-border spinner-border-sm me-1" />Thanh toán
              </button>
              <div v-if="payMessage" class="alert alert-success mt-2 py-2">{{ payMessage }}</div>
              <div v-if="payError" class="alert alert-danger mt-2 py-2">{{ payError }}</div>
            </div>
            <div v-else class="text-muted small">Chưa có hóa đơn để thanh toán.</div>
          </div>
        </div>
      </div>
    </div>

    <div v-if="error" class="alert alert-danger mt-3 py-2">{{ error }}</div>
  </div>
</template>
