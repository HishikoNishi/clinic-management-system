<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import InvoiceTable from '@/components/cashier/InvoiceTable.vue'
import InvoiceDetail from '@/components/cashier/InvoiceDetail.vue'
import { useInvoice } from '@/composables/useInvoice'
import { invoiceApi } from '@/services/invoiceApi'
import api from '@/services/api'

const filterStatus = ref<'all' | 'paid' | 'unpaid'>('all')
const filterType = ref<'all' | 'clinic' | 'drug'>('all')
const searchText = ref('')
const selectedId = ref<string | null>(null)
const prescriptionId = ref('')
const creatingDrug = ref(false)

const { list, invoice, listLoading, loading, error, fetchList, fetchInvoice } = useInvoice()

const loadData = async () => {
  const isPaid = filterStatus.value === 'paid' ? true : filterStatus.value === 'unpaid' ? false : undefined
  await fetchList({ isPaid })
  if (selectedId.value) await fetchInvoice(selectedId.value)
}

const handleView = async (id: string) => {
  selectedId.value = id
  await fetchInvoice(id)
}

const filteredList = () => {
  let data = list.value
  if (filterType.value !== 'all') {
    data = data.filter(i => (filterType.value === 'drug' ? i.invoiceType === 'Drug' : i.invoiceType !== 'Drug'))
  }
  if (!searchText.value.trim()) return data
  const term = searchText.value.toLowerCase()
  return data.filter(item =>
    item.patientName?.toLowerCase().includes(term) ||
    item.id.toLowerCase().includes(term) ||
    item.appointmentCode?.toLowerCase().includes(term)
  )
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
    error.value = err?.response?.data?.message || 'Khong tai duoc file PDF'
  }
}

const createDrugInvoice = async () => {
  if (!prescriptionId.value.trim()) {
    alert('Nhap PrescriptionId')
    return
  }
  creatingDrug.value = true
  error.value = ''
  try {
    const res = await invoiceApi.createDrugInvoice(prescriptionId.value.trim())
    const id = res?.id || res?.Id || res?.invoiceId
    await loadData()
    if (id) await handleView(id)
    alert('Tao hoa don thuoc thanh cong')
  } catch (err: any) {
    error.value = err?.response?.data?.message ?? 'Khong tao duoc hoa don thuoc'
  } finally {
    creatingDrug.value = false
  }
}

const lookupPrescription = async () => {
  if (!searchText.value.trim()) {
    alert('Nhap ma lich hen (AppointmentId hoac AppointmentCode) vao o tim kiem roi bam Lay don')
    return
  }
  try {
    const code = searchText.value.trim()
    let appointmentId = code
    if (!/^[0-9a-fA-F-]{8}/.test(code)) {
      const res = await invoiceApi.lookupAppointment(code)
      appointmentId = res?.id || res?.appointmentId || ''
    }
    if (!appointmentId) throw new Error('Khong tim thay lich hen')
    const { data } = await api.get(`/Prescription/by-appointment/${appointmentId}`)
    prescriptionId.value = data?.id || ''
    if (!prescriptionId.value) throw new Error('Khong thay PrescriptionId')
    alert('Da lay duoc PrescriptionId, bam Tao hoa don thuoc de tao')
  } catch (err: any) {
    error.value = err?.response?.data?.message || err.message || 'Khong tim duoc don thuoc'
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
        <h2 class="page-title mb-0">Danh sach hoa don</h2>
        <p class="page-subtitle">Tim theo ma hoa don / ma lich kham / ten benh nhan va loc theo trang thai, loai hoa don.</p>
      </div>
      <div class="d-flex gap-2 flex-wrap">
        <input v-model="searchText" type="search" class="form-control" placeholder="Tim theo ma / ten benh nhan" />
        <button class="btn btn-outline-secondary" :disabled="listLoading" @click="loadData">
          <span v-if="listLoading" class="spinner-border spinner-border-sm me-1" aria-hidden="true" />Lam moi
        </button>
      </div>
    </div>

    <div v-if="error" class="alert alert-danger py-2">{{ error }}</div>

    <div class="row g-3">
      <div class="col-lg-7">
        <div class="card shadow-sm h-100 page-card">
          <div class="card-body">
            <div class="d-flex justify-content-between align-items-center mb-2">
              <h5 class="card-title mb-0">Hoa don</h5>
              <span class="text-muted small">{{ filteredList().length }} muc</span>
            </div>
            <InvoiceTable :items="filteredList()" :loading="loading" @view="handleView" />
          </div>
        </div>
      </div>

      <div class="col-lg-5">
        <InvoiceDetail :invoice="invoice">
          <template #top>
            <div class="mb-3 pb-3 border-bottom">
              <h6 class="card-title mb-3">Bo loc</h6>
              <div class="row g-2">
                <div class="col-6">
                  <label class="form-label small">Trang thai</label>
                  <select v-model="filterStatus" class="form-select form-select-sm">
                    <option value="all">Tat ca</option>
                    <option value="paid">Da thanh toan</option>
                    <option value="unpaid">Chua thanh toan</option>
                  </select>
                </div>
                <div class="col-6">
                  <label class="form-label small">Loai hoa don</label>
                  <select v-model="filterType" class="form-select form-select-sm">
                    <option value="all">Moi loai</option>
                    <option value="clinic">Hoa don kham</option>
                    <option value="drug">Hoa don thuoc</option>
                  </select>
                </div>
              </div>
            </div>
          </template>

          <div v-if="invoice && !invoice.isPaid" class="alert alert-info mt-3 py-2 mb-0">
            Hoa don chua thanh toan. Vui long vao
            <router-link to="/cashier/invoices" class="fw-semibold">Hoa don & thanh toan</router-link>
            (hoa don kham) hoac
            <router-link to="/cashier/drug-invoices" class="fw-semibold">Hoa don thuoc</router-link>
            de thuc hien thanh toan.
          </div>

          <div v-if="invoice?.isPaid" class="mt-3">
            <button class="btn btn-outline-success btn-sm" @click="downloadPdf">Tai PDF hoa don</button>
          </div>
        </InvoiceDetail>
      </div>
    </div>
  </div>
</template>
