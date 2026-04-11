<script setup lang="ts">
import type { InvoiceDetail } from '@/services/invoiceApi'
import { formatCurrency, formatDateTime } from '@/utils/format'

const props = defineProps<{
  invoice: InvoiceDetail | null
}>()

const prettyType = (t: string) => {
  const map: Record<string, string> = {
    Consultation: 'Khám',
    Drug: 'Thuốc',
    Test: 'Xét nghiệm',
    Surcharge: 'Phí thu',
    Discount: 'Giảm trừ',
    Insurance: 'Bảo hiểm',
    Deposit: "Tạm ứng"
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
        <slot name="top" />
        <div class="small text-muted mb-3">
          Hóa đơn: <span class="fw-semibold">{{ invoice.id }}</span><br />
          Bệnh nhân: <span class="fw-semibold">{{ invoice.patientName || invoice.appointment?.patient?.fullName || '—' }}</span><br />
          <div>Mã BN: <span class="fw-semibold text-dark">{{ invoice.patientCode || invoice.appointment?.patient?.patientCode || '—' }}</span></div>
      <div>Số CCCD: <span class="fw-semibold text-dark">{{ invoice.citizenId || invoice.appointment?.patient?.citizenId || '—' }}</span></div>
      <div>Mã BHYT: <span class="fw-semibold text-dark">{{ invoice.insuranceCardNumber || invoice.appointment?.patient?.insuranceCardNumber || '—' }}</span></div>
          Lịch hẹn: <span class="text-monospace">{{ invoice.appointmentCode || invoice.appointment?.appointmentCode || '—' }}</span><br />
          Loại: <span class="badge bg-light text-dark">{{ invoice.invoiceType === 'Drug' ? 'Thuốc' : 'Khám' }}</span><br />
          <span v-if="invoice.insuranceCoverPercent && invoice.insuranceCoverPercent > 0">
            Bảo hiểm: <span class="badge bg-info text-dark">{{ Math.round(invoice.insuranceCoverPercent * 100) }}%</span>
            <span v-if="invoice.insurancePlanCode" class="text-muted">({{ invoice.insurancePlanCode }})</span><br />
          </span>
          Trạng thái: <span :class="`badge ${statusBadge(invoice.isPaid)}`">{{ invoice.isPaid ? 'Đã thanh toán' : 'Chưa thanh toán' }}</span><br />
          Ngày tạo: {{ formatDateTime(invoice.createdAt) }}<br />
          Ngày thanh toán: {{ formatDateTime(invoice.paymentDate) }}<br />
          Phương thức: {{ invoice.payments?.[0]?.method ?? "-" }}<br />
          Tạm ứng: {{ formatCurrency(invoice.totalDeposit ?? 0) }} | Còn thu: {{ formatCurrency(invoice.balanceDue ?? invoice.amount) }}
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
        <div class="fw-semibold">Tổng sau trừ tạm ứng: {{ formatCurrency(invoice.balanceDue ?? invoice.amount) }}</div>
        <div class="text-muted small">* Đã bao gồm phí dịch vụ, phí thu/giảm trừ và tạm ứng.</div>
        <slot />
      </div>
      <div v-else class="text-muted">Chọn một hóa đơn để xem chi tiết.</div>
    </div>
  </div>
</template>
