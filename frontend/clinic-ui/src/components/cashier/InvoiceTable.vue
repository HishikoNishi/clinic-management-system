<script setup lang="ts">
import type { InvoiceListItem } from '@/services/invoiceApi'
import { formatCurrency, formatDateTime } from '@/utils/format'

const props = defineProps<{
  items: InvoiceListItem[]
  loading?: boolean
}>()

const emit = defineEmits<{
  (e: 'view', id: string): void
}>()

const statusBadge = (isPaid: boolean) => (isPaid ? 'bg-success' : 'bg-warning text-dark')
</script>
<template>
  <div class="table-responsive">
    <table class="table table-sm align-middle">
      <thead>
        <tr>
          <th>Hóa đơn</th>
          <th>Bệnh nhân / Thông tin</th> <th>Mã lịch hẹn</th>
          <th>Loại</th>
          <th class="text-end">Tổng tiền</th>
          <th>Trạng thái</th>
          <th>Ngày tạo</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="inv in props.items" :key="inv.id">
          <td class="small text-monospace">{{ inv.id.slice(0, 8) }}...</td>
          
          <td>
            <div class="fw-bold">{{ inv.patientName || '—' }}</div>
            <div class="tech-sub-info" style="font-size: 0.75rem; color: #6c757d;">
              <span v-if="inv.patientCode">
                <i class="bi bi-person-badge"></i> {{ inv.patientCode }}
              </span>
              <span v-if="inv.citizenId" class="ms-2">
                | CCCD: {{ inv.citizenId }}
              </span>
              <div v-if="inv.insuranceCardNumber">
                <i class="bi bi-shield-check"></i> BHYT: {{ inv.insuranceCardNumber }}
              </div>
            </div>
          </td>

          <td class="small text-monospace">{{ inv.appointmentCode || '—' }}</td>
          <td class="small">
            <span class="badge bg-light text-dark">{{ inv.invoiceType === 'Drug' ? 'Thuốc' : 'Khám' }}</span>
          </td>
          <td class="text-end fw-semibold">{{ formatCurrency(inv.amount) }}</td>
          <td>
            <span class="badge" :class="statusBadge(inv.isPaid)">
              {{ inv.isPaid ? 'Đã thanh toán' : 'Chưa thanh toán' }}
            </span>
          </td>
          <td class="small">{{ formatDateTime(inv.createdAt) }}</td>
          <td class="text-end">
            <button 
              class="btn btn-outline-primary btn-sm" 
              :disabled="props.loading" 
              @click="emit('view', inv.id)"
            >
              Xem
            </button>
          </td>
        </tr>
        
        <tr v-if="!props.items.length">
          <td colspan="8" class="text-center text-muted py-4">Chưa có hóa đơn nào phù hợp.</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>