import { ref } from 'vue'
import { invoiceApi, type InvoiceDetail, type InvoiceListItem } from '@/services/invoiceApi'

export const useInvoice = () => {
  const invoice = ref<InvoiceDetail | null>(null)
  const list = ref<InvoiceListItem[]>([])
  const loading = ref(false)
  const listLoading = ref(false)
  const error = ref('')

  const fetchInvoice = async (id: string) => {
    loading.value = true
    error.value = ''
    try {
      invoice.value = await invoiceApi.getInvoice(id)
      return invoice.value
    } catch (err: any) {
      error.value = err?.response?.data?.message ?? 'Không tải được hóa đơn'
      return null
    } finally {
      loading.value = false
    }
  }

  const recalc = async (payload: {
    appointmentId: string
    insuranceCoverPercent?: number
    insuranceCode?: string | null
    surcharge?: number
    discount?: number
  }) => {
    loading.value = true
    error.value = ''
    try {
      invoice.value = await invoiceApi.recalcInvoice(payload)
      return invoice.value
    } catch (err: any) {
      error.value = err?.response?.data?.message ?? 'Không tính lại được hóa đơn'
      return null
    } finally {
      loading.value = false
    }
  }

  const create = async (payload: { appointmentId: string; amount: number }) => {
    loading.value = true
    error.value = ''
    try {
      return await invoiceApi.createInvoice(payload)
    } catch (err: any) {
      error.value = err?.response?.data?.message ?? 'Không tạo được hóa đơn'
      return null
    } finally {
      loading.value = false
    }
  }

  const fetchList = async (params: { isPaid?: boolean } = {}) => {
    listLoading.value = true
    error.value = ''
    try {
      list.value = await invoiceApi.listInvoices(params)
      return list.value
    } catch (err: any) {
      error.value = err?.response?.data?.message ?? 'Không tải được danh sách hóa đơn'
      return null
    } finally {
      listLoading.value = false
    }
  }

  return { invoice, list, loading, listLoading, error, fetchInvoice, recalc, create, fetchList }
}
