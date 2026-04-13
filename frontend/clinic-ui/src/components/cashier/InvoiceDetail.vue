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
  <div class="card border-0 shadow-sm h-100 invoice-card">
    <div class="card-header bg-white py-3 border-bottom-0">
      <div class="d-flex justify-content-between align-items-center">
        <h5 class="mb-0 fw-bold text-primary">
          <i class="bi bi-receipt-cutoff me-2"></i>Chi tiết hóa đơn
        </h5>
        <div v-if="invoice" :class="`status-pill ${invoice.isPaid ? 'paid' : 'unpaid'}`">
          {{ invoice.isPaid ? '● Đã thanh toán' : '● Chờ thanh toán' }}
        </div>
      </div>
    </div>

    <div class="card-body pt-0">
      <div v-if="invoice">
        <slot name="top" />

       
     <div class="mb-4 p-3 bg-light rounded-3 admin-info">
  <!-- Tên bệnh nhân nổi bật -->
  <div class="mb-3" >
  
    <div class="h4 fw-bold"> 
       <div class="label">Bệnh nhân</div>
      {{ invoice.patientName || invoice.appointment?.patient?.fullName || '—' }}
    </div>
  </div>

  <!-- Các thông tin khác nhỏ hơn -->
  <div class="row g-3">
    <div class="col-6 col-md-4">
      <div class="label">ID Hóa đơn</div>
      <div class="value  text-dark">{{ invoice.id }}</div>
    </div>
    <div class="col-6 col-md-4">
      <div class="label">Mã bệnh nhân</div>
      <div class="value  text-dark">{{ invoice.patientCode || invoice.appointment?.patient?.patientCode || '—' }}</div>
    </div>
    <div class="col-6 col-md-4">
      <div class="label">Mã lịch khám</div>
      <div class="value  text-dark">{{ invoice.appointmentCode || invoice.appointment?.appointmentCode || '—' }}</div>
    </div>
    <div class="col-6 col-md-4">
      <div class="label">Ngày tạo</div>
      <div class="value">{{ formatDateTime(invoice.createdAt) }}</div>
    </div>
    <div class="col-6 col-md-4">
      <div class="label">Số CCCD</div>
      <div class="value text-dark">{{ invoice.citizenId || invoice.appointment?.patient?.citizenId || '—' }}</div>
    </div>
    <div class="col-6 col-md-4">
      <div class="label">Mã BHYT</div>
      <div class="value text-truncate">{{ invoice.insuranceCardNumber || invoice.appointment?.patient?.insuranceCardNumber || '—' }}</div>
    </div>
    <div class="col-6 col-md-4">
      <div class="label">Loại hóa đơn</div>
      <div class="value">
        <span :class="['badge', invoice.invoiceType === 'Drug' ? 'bg-info-subtle text-info' : 'bg-primary-subtle text-primary']">
          {{ invoice.invoiceType === 'Drug' ? 'Toa thuốc' : 'Dịch vụ khám' }}
        </span>
      </div>
    </div>
    <div class="col-6 col-md-4" v-if="invoice.insuranceCoverPercent">
      <div class="label">BHYT Chi trả</div>
      <div class="value fw-bold text-success">{{ Math.round(invoice.insuranceCoverPercent * 100) }}%</div>
    </div>
  </div>
</div>

        <div class="table-responsive">
          <table class="table table-hover align-middle border-light">
            <thead class="bg-light-subtle text-muted small text-uppercase">
              <tr>
                <th style="width: 40%">Nội dung</th>
                <th class="text-center">SL/Liều</th>
                <th class="text-end">Thành tiền</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="line in invoice.invoiceLines" :key="line.id">
                <td>
                  <div class="fw-bold text-dark">{{ line.description }}</div>
                  <small class="text-muted text-uppercase" style="font-size: 0.7rem;">
                    {{ prettyType(line.itemType) }}
                  </small>
                </td>
                <td class="text-center">
                  <div v-if="line.itemType === 'Drug'">
                    <span class="fw-bold">{{ (line as any).duration ?? 1 }}</span>
                    <div class="small text-muted text-truncate" style="max-width: 80px;">{{ (line as any).dosage ?? '—' }}</div>
                  </div>
                  <span v-else class="text-muted">—</span>
                </td>
                <td class="text-end">
                  <span :class="line.amount < 0 ? 'text-success fw-bold' : 'fw-bold text-dark'">
                    {{ formatCurrency(line.amount) }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div class="mt-4 p-3 rounded-3 border-top border-2 border-primary-subtle bg-white">
          <div class="d-flex justify-content-between mb-2">
            <span class="text-muted">Tạm ứng đã thu:</span>
            <span class="fw-bold">{{ formatCurrency(invoice.totalDeposit ?? 0) }}</span>
          </div>
          <div class="d-flex justify-content-between align-items-center mb-0">
            <span class="h6 mb-0 fw-bold">Tổng tiền phải trả:</span>
            <span class="h4 mb-0 fw-bold text-danger">{{ formatCurrency(invoice.balanceDue ?? invoice.amount) }}</span>
          </div>
        </div>

        <slot />
      </div>

      <div v-else class="text-center py-5">
        <i class="bi bi-file-earmark-text display-1 text-light"></i>
        <p class="text-muted mt-3">Chọn một hóa đơn từ danh sách để xem chi tiết.</p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.invoice-card {
  border-radius: 12px;
}
.admin-info .label {
  font-size: 0.75rem;
  color: #6c757d;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}
.admin-info .value {
  font-size: 0.9rem;
}
.status-pill {
  padding: 4px 12px;
  border-radius: 50px;
  font-size: 0.8rem;
  font-weight: 600;
}
.status-pill.paid {
  background-color: #d1e7dd;
  color: #0f5132;
}
.status-pill.unpaid {
  background-color: #fff3cd;
  color: #856404;
}
.table thead th {
  border-bottom-width: 1px;
  font-weight: 600;
  padding: 12px 8px;
}
.table tbody td {
  padding: 12px 8px;
}
.admin-info .label {
  font-size: 0.75rem;
  color: #6c757d;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}
.admin-info .value {
  font-size: 0.9rem;

}
.admin-info .h4 {
  margin: 0;
  text-align: center;
}

</style>