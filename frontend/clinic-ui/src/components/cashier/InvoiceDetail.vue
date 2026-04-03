<script setup lang="ts">
import type { InvoiceDetail } from '@/services/invoiceApi'
import { formatCurrency, formatDateTime } from '@/utils/format'

const props = defineProps<{
  invoice: InvoiceDetail | null
}>()

const prettyType = (t: string) => {
  const map: Record<string, string> = {
    Consultation: 'Kh�m',
    Drug: 'Thu?c',
    Test: 'X�t nghi?m',
    Surcharge: 'Ph? thu',
    Discount: 'Gi?m tr?',
    Insurance: 'B?o hi?m',
    Deposit: "T?m ?ng"
  }
  return map[t] || t
}
const statusBadge = (isPaid: boolean) => isPaid ? 'bg-success' : 'bg-warning text-dark'
</script>

<template>
  <div class="card shadow-sm h-100">
    <div class="card-body">
      <h5 class="card-title mb-3">Chi tiết hóa đơn</h5>
      <div v-if="invoice">
        <div class="small text-muted mb-3">
          Hóa đơn: <span class="fw-semibold">{{ invoice.id }}</span><br />
          Bệnh nhân: <span class="fw-semibold">{{ invoice.patientName || invoice.appointment?.patient?.fullName || '—' }}</span><br />
          Lịch hẹn: <span class="text-monospace">{{ invoice.appointmentId }}</span><br />
          Trạng thái: <span :class="`badge ${statusBadge(invoice.isPaid)}`">{{ invoice.isPaid ? 'Đã thanh toán' : 'Chưa thanh toán' }}</span><br />
          Ngày tạo: {{ formatDateTime(invoice.createdAt) }}<br />
          Ngày thanh toán: {{ formatDateTime(invoice.paymentDate) }}<br />
          Phuong th?c: {{ invoice.payments?.[0]?.method ?? "�" }}<br />
          T?m ?ng: {{ formatCurrency(invoice.totalDeposit ?? 0) }} | C?n thu: {{ formatCurrency(invoice.balanceDue ?? invoice.amount) }}
        </div>

        <div class="mb-3">
          <div class="fw-semibold">Hạng mục:</div>
          <ul class="list-group list-group-flush">
            <li
              v-for="line in invoice.invoiceLines"
              :key="line.id"
              class="list-group-item d-flex justify-content-between"
            >
              <span>{{ line.description }} <span v-if="line.itemType" class="badge bg-light text-dark ms-1">{{ prettyType(line.itemType) }}</span></span>
              <span :class="line.amount < 0 ? 'text-success' : ''">{{ formatCurrency(line.amount) }}</span>
            </li>
          </ul>
        </div>
        <div class="fw-semibold">T?ng sau tr? t?m ?ng: {{ formatCurrency(invoice.balanceDue ?? invoice.amount) }}</div>
        <div class="text-muted small">* �� bao g?m ph� d?ch v?, ph? thu/gi?m tr? v� t?m ?ng.</div>
      </div>
      <div v-else class="text-muted">Chọn một hóa đơn để xem chi tiết.</div>
    </div>
  </div>
</template>
