<script setup lang="ts">
import { computed, ref } from 'vue'
import AppointmentLookup from '@/components/cashier/AppointmentLookup.vue'
import InvoiceForm from '@/components/cashier/InvoiceForm.vue'
import InvoiceDetail from '@/components/cashier/InvoiceDetail.vue'
import { useInvoice } from '@/composables/useInvoice'
import { usePayment } from '@/composables/usePayment'
import type { PaymentMethod } from '@/services/invoiceApi'
import { invoiceApi } from '@/services/invoiceApi'
import { payosApi, type PayOsCreateResponse } from '@/services/payosApi'
import { formatCurrency } from '@/utils/format'

const currentAppointment = ref<any | null>(null)
const payAmount = ref<number | null>(null)
const payMethod = ref<PaymentMethod>('cash')

const { invoice, loading, error, recalc, create, fetchInvoice } = useInvoice()
const { loading: payLoading, error: payError, message: payMessage, pay } = usePayment()
const payosData = ref<PayOsCreateResponse | null>(null)
const payosLoading = ref(false)
const showPayOsModal = ref(false)

const appointmentId = computed(() => currentAppointment.value?.id ?? '')
const isPaid = computed(() => invoice.value?.isPaid ?? false)

const balanceDue = computed(() => invoice.value?.balanceDue ?? invoice.value?.amount ?? 0)
const totalDeposit = computed(() => invoice.value?.totalDeposit ?? 0)
const insuranceCoverPercent = computed<number | null>(() =>
  invoice.value?.insuranceCoverPercent ?? null
)

const autofillPayAmount = computed(() => payAmount.value ?? balanceDue.value ?? 0)

const onAppointmentFound = async (appt: any) => {
  currentAppointment.value = appt
  if (appt?.id) {
    await recalc({ appointmentId: appt.id })
    payAmount.value = invoice.value?.balanceDue ?? invoice.value?.amount ?? null
  }
}

const handleCreate = async ({ amount }: { amount: number }) => {
  if (!appointmentId.value) return
  const res = await create({ appointmentId: appointmentId.value, amount })
  if (res?.invoiceId) {
    await fetchInvoice(res.invoiceId)
    payAmount.value = invoice.value?.balanceDue ?? invoice.value?.amount ?? amount
  }
}

const handleRecalc = async (payload: { insuranceCode?: string; insuranceCoverPercent?: number; surcharge?: number; discount?: number }) => {
  if (!appointmentId.value) return
  await recalc({
    appointmentId: appointmentId.value,
    ...payload,
    insuranceCoverPercent: payload.insuranceCoverPercent ?? undefined
  })
  payAmount.value = invoice.value?.balanceDue ?? invoice.value?.amount ?? payAmount.value
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

const openPayOs = async () => {
  if (!invoice.value?.id) return
  payosLoading.value = true
  payError.value = ''
  try {
    payosData.value = await payosApi.createPayment(invoice.value.id)
    showPayOsModal.value = true
  } catch (err: any) {
    payError.value = err?.response?.data?.message ?? 'Không tạo được QR PayOS'
  } finally {
    payosLoading.value = false
  }
}

const downloadPdf = async () => {
  if (!invoice.value?.id) return
  try {
    const { blob, headers } = await invoiceApi.downloadInvoicePdf(invoice.value.id)
    const contentDisposition = headers?.['content-disposition'] as string | undefined
    const fileNameMatch = contentDisposition?.match(/filename="?([^"]+)"?/)
    const fileName = fileNameMatch?.[1] || `invoice-${invoice.value.id}.pdf`
    const url = URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = fileName
    document.body.appendChild(link)
    link.click()
    link.remove()
    URL.revokeObjectURL(url)
  } catch (err: any) {
    payError.value = err?.response?.data?.message ?? 'Không tải được file PDF'
  }
}
</script>

<template>
  <div class="container py-4 page">
    <div class="page-header">
      <div>
        <div class="page-eyebrow">Cashier</div>
        <h2 class="page-title mb-0">Hóa đơn & thanh toán</h2>
        <p class="page-subtitle">Tra cứu lịch khám, tạo hóa đơn, tính lại và thanh toán nhanh.</p>
      </div>
      <span class="badge bg-primary-subtle text-primary border">Vai trò: Thu ngân</span>
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
          :insurance-cover-percent="insuranceCoverPercent ?? undefined"
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
        <div class="card shadow-sm h-100 page-card">
          <div class="card-body">
            <h5 class="card-title mb-3">Thanh toán</h5>
            <div v-if="invoice">
              <div class="mb-2 text-muted small">
                Tạm ứng đã thu: <span class="fw-semibold">{{ formatCurrency(totalDeposit) }}</span><br />
                Cần thanh toán: <span class="fw-semibold">{{ formatCurrency(balanceDue) }}</span>
              </div>
              <div class="mb-3">
                <label class="form-label">Số tiền thanh toán</label>
                <div class="input-group">
                  <input v-model.number="payAmount" type="number" min="0" class="form-control" :disabled="isPaid || payLoading" />
                  <span class="input-group-text">VND</span>
                </div>
                <small class="text-muted">Mặc định: {{ formatCurrency(balanceDue) }}</small>
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
              <button class="btn btn-outline-primary w-100 mt-2" :disabled="isPaid || payosLoading" @click="openPayOs">
                <span v-if="payosLoading" class="spinner-border spinner-border-sm me-1" />QR PayOS
              </button>
              <button
                v-if="isPaid"
                class="btn btn-outline-success w-100 mt-2"
                @click="downloadPdf"
              >
                Tải PDF hóa đơn
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

    <div v-if="showPayOsModal && payosData" class="modal-backdrop-custom">
      <div class="modal-card">
        <div class="modal-header d-flex justify-content-between align-items-center">
          <h6 class="mb-0">QR PayOS</h6>
          <button class="btn-close" aria-label="Close" @click="showPayOsModal=false"></button>
        </div>
        <div class="modal-body text-center">
          <p>Số tiền: <strong>{{ payosData.amount.toLocaleString('vi-VN') }}₫</strong></p>
          <div class="qr-box">
            <img
              v-if="payosData.qrCodeUrl || payosData.qrCodeBase64 || payosData.qrCode"
              :src="payosData.qrCodeUrl || (payosData.qrCodeBase64 ? `data:image/png;base64,${payosData.qrCodeBase64}` : payosData.qrCode)"
              alt="QR PayOS"
            />
            <div v-else class="text-muted small">Không có ảnh QR, bấm "Mở PayOS"</div>
          </div>
          <p class="small text-muted mt-2">Quét bằng app ngân hàng, hệ thống sẽ tự cập nhật khi PayOS gửi webhook.</p>
          <a :href="payosData.checkoutUrl" target="_blank" rel="noreferrer" class="btn btn-primary w-100 mt-2">Mở PayOS</a>
        </div>
      </div>
    </div>
  </div>
</template>


