import api from '@/services/api'

export interface PayOsCreateResponse {
  orderCode: string
  amount: number
  qrCodeUrl: string
  qrCode?: string
  qrCodeBase64?: string
  qrData?: string
  checkoutUrl: string
}

export const payosApi = {
  async createPayment(invoiceId: string): Promise<PayOsCreateResponse> {
    const token = localStorage.getItem('token')
    const { data } = await api.post(
      '/payos/create',
      { invoiceId },
      {
        headers: token ? { Authorization: `Bearer ${token}` } : undefined
      }
    )
    return data
  }
}
