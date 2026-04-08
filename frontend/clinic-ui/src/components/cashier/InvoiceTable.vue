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
          <th>Bệnh nhân</th>
          <th>Mã lịch hẹn</th>
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
          <td class="small">{{ inv.patientName || '—' }}</td>
          <td class="small text-monospace">{{ inv.appointmentCode || '—' }}</td>
          <td class="small">
            <span class="badge bg-light text-dark">{{ inv.invoiceType === 'Drug' ? 'Thuốc' : 'Khám' }}</span>
          </td>
          <td class="text-end">{{ formatCurrency(inv.amount) }}</td>
          <td><span class="badge" :class="statusBadge(inv.isPaid)">{{ inv.isPaid ? 'Đã thanh toán' : 'Chưa thanh toán' }}</span></td>
          <td class="small">{{ formatDateTime(inv.createdAt) }}</td>
          <td class="text-end">
            <button class="btn btn-outline-primary btn-sm" :disabled="props.loading" @click="emit('view', inv.id)">Xem</button>
          </td>
        </tr>
        <tr v-if="!props.items.length">
          <td colspan="8" class="text-center text-muted">Chưa có hóa đơn</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
