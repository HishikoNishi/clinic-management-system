import { ref } from 'vue'
import { invoiceApi } from '@/services/invoiceApi'

export const useAppointment = () => {
  const appointment = ref<any | null>(null)
  const loading = ref(false)
  const error = ref('')

  const lookupByCode = async (code: string) => {
    error.value = ''
    appointment.value = null
    if (!code.trim()) {
      error.value = 'Nhập mã lịch hẹn'
      return null
    }
    loading.value = true
    try {
      const data = await invoiceApi.lookupAppointment(code.trim())
      appointment.value = data
      return data
    } catch (err: any) {
      error.value = err?.response?.data ?? 'Không tìm thấy lịch hẹn'
      return null
    } finally {
      loading.value = false
    }
  }

  return { appointment, loading, error, lookupByCode }
}
