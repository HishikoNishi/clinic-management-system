import { ref } from 'vue'
import { invoiceApi, type PaymentMethod } from '@/services/invoiceApi'

export const usePayment = () => {
  const loading = ref(false)
  const error = ref('')
  const message = ref('')

  const pay = async (payload: { invoiceId: string; amount: number; method: PaymentMethod }) => {
    loading.value = true
    error.value = ''
    message.value = ''
    try {
      const data = await invoiceApi.payInvoice(payload)
      message.value = data.message ?? 'Thanh toán thành công'
      return data
    } catch (err: any) {
      error.value = err?.response?.data ?? 'Thanh toán thất bại'
      return null
    } finally {
      loading.value = false
    }
  }

  return { loading, error, message, pay }
}
