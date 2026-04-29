import api from '@/services/api'

export type PaymentMethod = 'cash' | 'banker' | 'card'

export interface InvoiceLine {
  id: string
  description: string
  itemType: string
  amount: number
  quantity?: number
}

export interface InvoiceDetail {
  id: string
  appointmentId: string
  appointmentCode?: string
  invoiceType?: string
  prescriptionId?: string | null
  patientName?: string
  amount: number
  totalDeposit?: number
  balanceDue?: number
  isPaid: boolean
  createdAt?: string
  paymentDate?: string
  insuranceCoverPercent?: number
  insurancePlanCode?: string
  patientCode?: string;      
  citizenId?: string;          
  insuranceCardNumber?: string;
  appointment?: {
    id: string
    appointmentCode?: string
    patient?: {
      fullName?: string
      phone?: string
      email?: string
      patientCode?: string;      
      citizenId?: string;          
      insuranceCardNumber?: string;
    }
  }
  payments?: {
    id: string
    amount: number
    depositAmount?: number
    isDeposit?: boolean
    method: PaymentMethod
    paymentDate: string
  }[]
  depositPayments?: {
    id: string
    amount: number
    method: PaymentMethod
    paymentDate: string
  }[]
  invoiceLines?: InvoiceLine[]
}

export interface InvoiceListItem {
  id: string
  appointmentId: string
  appointmentCode?: string
  invoiceType?: string
  prescriptionId?: string | null
  amount: number
  balanceDue?: number
  totalDeposit?: number
  isPaid: boolean
  createdAt?: string
  paymentDate?: string
  patientName?: string
  patientCode?: string;     
  citizenId?: string;        
  insuranceCardNumber?: string;
}

export const invoiceApi = {
  async lookupAppointment(code: string) {
    const { data } = await api.get(`/appointments/${code}`)
    return data
  },

  async createInvoice(payload: { appointmentId: string; amount: number }) {
    const { data } = await api.post('/invoicemanagement', payload)
    return data
  },

  async recalcInvoice(payload: {
    appointmentId: string
    insuranceCoverPercent?: number
    insuranceCode?: string | null
    surcharge?: number
    discount?: number
  }): Promise<InvoiceDetail> {
    const { data } = await api.post('/invoicemanagement/recalculate', payload)
    return data
  },

  async getInvoice(id: string): Promise<InvoiceDetail> {
    const { data } = await api.get(`/invoicemanagement/${id}`)
    return data
  },

  async listInvoices(params: { isPaid?: boolean; page?: number; pageSize?: number } = {}): Promise<InvoiceListItem[]> {
    const { data } = await api.get('/invoicemanagement/list', { params })
    return data
  },

  async createDrugInvoice(prescriptionId: string) {
    const { data } = await api.post('/invoicemanagement/drug', { prescriptionId })
    return data
  },

  async getDrugInvoiceByPrescription(prescriptionId: string) {
    const { data } = await api.get(`/invoicemanagement/drug/by-prescription/${prescriptionId}`)
    return data
  },

  async payInvoice(payload: { invoiceId: string; amount: number; method: PaymentMethod }) {
    const { data } = await api.post('/payment', payload)
    return data
  },

  async downloadInvoicePdf(invoiceId: string) {
    const { data, headers } = await api.get(`/invoicemanagement/${invoiceId}/pdf`, {
      responseType: 'blob'
    })
    return { blob: data as Blob, headers }
  },

  async validateInsurance(code: string) {
    const { data } = await api.get('/insurance/validate', { params: { code } })
    return data
  }
}
