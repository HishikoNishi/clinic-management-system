<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import InvoiceTable from '@/components/cashier/InvoiceTable.vue'
import InvoiceDetail from '@/components/cashier/InvoiceDetail.vue'
import { useInvoice } from '@/composables/useInvoice'
import { invoiceApi, type PaymentMethod } from '@/services/invoiceApi'
import { payosApi, type PayOsCreateResponse } from '@/services/payosApi'
import api from '@/services/api'

const filterStatus = ref<'all' | 'paid' | 'unpaid'>('all')
const searchText = ref('')
const selectedId = ref<string | null>(null)
const prescriptionId = ref('')
const creatingDrug = ref(false)
const payMethod = ref<PaymentMethod>('cash')
const payosLoading = ref(false)
const payosData = ref<PayOsCreateResponse | null>(null)
const showPayOsModal = ref(false)

const { list, invoice, listLoading, loading, error, fetchList, fetchInvoice } = useInvoice()

const loadData = async () => {
  const isPaid = filterStatus.value === 'paid' ? true : filterStatus.value === 'unpaid' ? false : undefined
  await fetchList({ isPaid })
  await restoreSelection()
}

const handleView = async (id: string) => {
  selectedId.value = id
  await fetchInvoice(id)
}

const openPayOs = async () => {
  if (!invoice.value) return
  payosLoading.value = true
  error.value = ''
  try {
    payosData.value = await payosApi.createPayment(invoice.value.id)
    showPayOsModal.value = true
  } catch (err: any) {
    error.value = err?.response?.data?.message ?? 'Không tạo được QR PayOS'
  } finally {
    payosLoading.value = false
  }
}

const filteredList = () => {
  let data = list.value.filter(i => i.invoiceType === 'Drug')
  if (!searchText.value.trim()) return data
  const term = searchText.value.toLowerCase()
  return data.filter(item =>
    item.patientName?.toLowerCase().includes(term) ||
    item.id.toLowerCase().includes(term) ||
    item.appointmentCode?.toLowerCase().includes(term)
  )
}

const createDrugInvoice = async () => {
  if (!prescriptionId.value.trim()) {
    alert('Nhập PrescriptionId')
    return
  }
  creatingDrug.value = true
  error.value = ''
  try {
    const res = await invoiceApi.createDrugInvoice(prescriptionId.value.trim())
    const id = (res?.id || res?.Id || res?.invoiceId)
    await loadData()
    if (id) await handleView(id)
    alert('Tạo hóa đơn thuốc thành công')
  } catch (err: any) {
    error.value = err?.response?.data?.message ?? 'Không tạo được hóa đơn thuốc'
  } finally {
    creatingDrug.value = false
  }
}

const lookupPrescription = async () => {
  if (!searchText.value.trim()) {
    alert('Nhập mã lịch hẹn (AppointmentId hoặc AppointmentCode) vào ô tìm kiếm rồi bấm Lấy đơn')
    return
  }
  try {
    const code = searchText.value.trim()
    let appointmentId = code
    if (!/^[0-9a-fA-F-]{8}/.test(code)) {
      const res = await invoiceApi.lookupAppointment(code)
      appointmentId = res?.id || res?.appointmentId || ''
    }
    if (!appointmentId) throw new Error('Không tìm thấy lịch hẹn')
    const { data } = await api.get(`/Prescription/by-appointment/${appointmentId}`)
    prescriptionId.value = data?.id || ''
    if (!prescriptionId.value) throw new Error('Không thấy PrescriptionId')
    alert('Đã lấy được PrescriptionId, bấm "Tạo hóa đơn thuốc" để tạo')
  } catch (err: any) {
    error.value = err?.response?.data?.message || err.message || 'Không tìm được đơn thuốc'
  }
}

const restoreSelection = async () => {
  const listByTab = filteredList()
  const targetId = selectedId.value && listByTab.some(i => i.id === selectedId.value)
    ? selectedId.value
    : listByTab[0]?.id
  if (targetId) {
    selectedId.value = targetId
    await fetchInvoice(targetId)
  } else {
    selectedId.value = null
  }
}

onMounted(loadData)
watch(filterStatus, loadData)
</script>

<template>
  <div class="container py-4 page">
    <div class="page-header">
      <div>
        <div class="page-eyebrow">Cashier</div>
        <h2 class="page-title mb-0">Hóa đơn thuốc</h2>
        <p class="page-subtitle">Danh sách, tạo và thanh toán hóa đơn thuốc.</p>
      </div>
      <span class="badge bg-primary-subtle text-primary border">Thu ngân</span>
    </div>

    <div v-if="error" class="alert alert-danger py-2">{{ error }}</div>

    <div class="drug-toolbar mb-3">
      <div class="drug-toolbar-left">
        <input
          v-model="searchText"
          type="search"
          class="form-control"
          placeholder="Tìm theo mã / tên bệnh nhân"
        />
        <select v-model="filterStatus" class="form-select">
          <option value="all">Tất cả</option>
          <option value="paid">Đã thanh toán</option>
          <option value="unpaid">Chưa thanh toán</option>
        </select>
        <button class="btn btn-outline-secondary" :disabled="listLoading" @click="loadData">
          <span v-if="listLoading" class="spinner-border spinner-border-sm me-1" aria-hidden="true" />Làm mới
        </button>
      </div>

      <div class="drug-toolbar-right">
        <button class="btn btn-outline-primary" :disabled="creatingDrug" @click="lookupPrescription">
          Lấy đơn từ lịch hẹn
        </button>
        <input
          v-model="prescriptionId"
          type="text"
          class="form-control"
          placeholder="PrescriptionId"
        />
        <button class="btn btn-primary" :disabled="creatingDrug || !prescriptionId" @click="createDrugInvoice">
          <span v-if="creatingDrug" class="spinner-border spinner-border-sm me-1" aria-hidden="true"></span>
          Tạo hóa đơn thuốc
        </button>
      </div>
    </div>
    <div class="alert alert-info py-2">
      Nhập mã lịch hẹn vào ô tìm kiếm rồi bấm "Lấy đơn từ lịch hẹn" để tự động điền mã hóa đơn thuốc.
    </div>

    <div class="row g-3">
      <div class="col-lg-7">
        <div class="card shadow-sm h-100 page-card">
          <div class="card-body">
            <div class="d-flex justify-content-between align-items-center mb-2">
              <h5 class="card-title mb-0">Danh sách hóa đơn thuốc</h5>
              <span class="text-muted small">{{ filteredList().length }} mục</span>
            </div>
            <InvoiceTable :items="filteredList()" :loading="loading" @view="handleView" />
          </div>
        </div>
      </div>

      <div class="col-lg-5">
      <InvoiceDetail :invoice="invoice">
        <div v-if="invoice && !invoice.isPaid" class="mt-3 pt-3 border-top">
          <div class="d-flex flex-wrap gap-2 align-items-center">
            <div class="fw-semibold">Thanh toán hóa đơn thuốc</div>
            <select v-model="payMethod" class="form-select form-select-sm" style="width: 140px;">
              <option value="cash">Tiền mặt</option>
              <option value="banker">Chuyển khoản</option>
              <option value="card">Thẻ</option>
            </select>
            <button class="btn btn-outline-primary btn-sm" :disabled="payosLoading" @click="openPayOs">
              <span v-if="payosLoading" class="spinner-border spinner-border-sm me-1" aria-hidden="true"></span>
              QR PayOS
            </button>
          </div>
          </div>
        </InvoiceDetail>
      </div>
    </div>

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

<style scoped>
.drug-toolbar {
  display: grid;
  grid-template-columns: 1fr;
  gap: 0.75rem;
}

.drug-toolbar-left,
.drug-toolbar-right {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  align-items: center;
}

.drug-toolbar-left .form-control {
  min-width: 260px;
  flex: 1 1 300px;
}

.drug-toolbar-left .form-select {
  min-width: 150px;
  flex: 0 0 170px;
}

.drug-toolbar-right .form-control {
  min-width: 200px;
  flex: 1 1 220px;
}

@media (min-width: 1200px) {
  .drug-toolbar {
    grid-template-columns: 1fr auto;
    align-items: center;
  }
}
</style>
