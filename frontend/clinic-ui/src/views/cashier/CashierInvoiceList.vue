<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import InvoiceTable from '@/components/cashier/InvoiceTable.vue'
import InvoiceDetail from '@/components/cashier/InvoiceDetail.vue'
import { useInvoice } from '@/composables/useInvoice'

const filterStatus = ref<'all' | 'paid' | 'unpaid'>('all')
const searchText = ref('')
const selectedId = ref<string | null>(null)

const { list, invoice, listLoading, loading, error, fetchList, fetchInvoice } = useInvoice()

const loadData = async () => {
  const isPaid = filterStatus.value === 'paid' ? true : filterStatus.value === 'unpaid' ? false : undefined
  await fetchList({ isPaid })
  if (selectedId.value) {
    await fetchInvoice(selectedId.value)
  }
}

const handleView = async (id: string) => {
  selectedId.value = id
  await fetchInvoice(id)
}

const filteredList = () => {
  if (!searchText.value.trim()) return list.value
  const term = searchText.value.toLowerCase()
  return list.value.filter(item =>
    item.patientName?.toLowerCase().includes(term) ||
    item.id.toLowerCase().includes(term) ||
    item.appointmentCode?.toLowerCase().includes(term)
  )
}

onMounted(loadData)
watch(filterStatus, loadData)
</script>

<template>
  <div class="container py-4">
    <div class="d-flex align-items-center justify-content-between mb-3">
      <h2 class="mb-0">Danh sách hóa đơn</h2>
      <div class="d-flex gap-2">
        <input v-model="searchText" type="search" class="form-control" placeholder="Tìm theo mã / tên bệnh nhân" @input="filteredList" />
        <select v-model="filterStatus" class="form-select">
          <option value="all">Tất cả</option>
          <option value="paid">Đã thanh toán</option>
          <option value="unpaid">Chưa thanh toán</option>
        </select>
        <button class="btn btn-outline-secondary" :disabled="listLoading" @click="loadData">
          <span v-if="listLoading" class="spinner-border spinner-border-sm me-1" />Làm mới
        </button>
      </div>
    </div>

    <div v-if="error" class="alert alert-danger py-2">{{ error }}</div>

    <div class="row g-3">
      <div class="col-lg-7">
        <div class="card shadow-sm h-100">
          <div class="card-body">
            <div class="d-flex justify-content-between align-items-center mb-2">
              <h5 class="card-title mb-0">Hóa đơn</h5>
              <span class="text-muted small">{{ filteredList().length }} mục</span>
            </div>
            <InvoiceTable :items="filteredList()" :loading="loading" @view="handleView" />
          </div>
        </div>
      </div>

      <div class="col-lg-5">
        <InvoiceDetail :invoice="invoice" />
      </div>
    </div>
  </div>
</template>
