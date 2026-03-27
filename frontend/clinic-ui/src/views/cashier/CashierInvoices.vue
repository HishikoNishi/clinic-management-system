<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import api from '@/services/api'

type PaymentMethod = 'cash' | 'banker' | 'card'

const appointmentCode = ref('')
const appointment = ref<any | null>(null)
const appointmentLoading = ref(false)
const appointmentError = ref('')

const amount = ref<number | null>(null)
const invoice = ref<any | null>(null)
const invoiceLoading = ref(false)
const invoiceError = ref('')
const invoiceMessage = ref('')

const invoiceLookupId = ref('')

const paymentMethod = ref<PaymentMethod>('cash')
const payAmount = ref<number | null>(null)
const paymentLoading = ref(false)
const paymentError = ref('')
const paymentMessage = ref('')

// Billing adjustments (cashier side)
const insuranceCoverPercent = ref<number>(0)
const surcharge = ref<number>(0)
const discount = ref<number>(0)
const insuranceCode = ref<string>('')
const insuranceMessage = ref<string>('')

const canCreateInvoice = computed(() =>
  !!appointment.value?.id &&
  !!amount.value &&
  amount.value > 0 &&
  !invoiceLoading.value
)

const canPay = computed(() =>
  !!invoice.value?.id &&
  invoice.value.isPaid === false &&
  !!payAmount.value &&
  payAmount.value > 0 &&
  !paymentLoading.value
)

watch(invoice, (val) => {
  if (val?.amount) {
    payAmount.value = val.amount
  }
})

const resetPaymentState = () => {
  paymentError.value = ''
  paymentMessage.value = ''
}

const resetInvoiceState = () => {
  invoiceError.value = ''
  invoiceMessage.value = ''
}

const resetAdjustments = () => {
  insuranceCoverPercent.value = 0
  surcharge.value = 0
  discount.value = 0
  insuranceCode.value = ''
  insuranceMessage.value = ''
}

const fetchAppointment = async () => {
  appointmentError.value = ''
  appointment.value = null
  resetInvoiceState()
  resetPaymentState()

  if (!appointmentCode.value.trim()) {
    appointmentError.value = 'Nhập mã lịch hẹn trước khi tra cứu'
    return
  }

  appointmentLoading.value = true
  try {
    const { data } = await api.get(`/appointments/${appointmentCode.value.trim()}`)
    appointment.value = data

    if (!data.id || data.id === '00000000-0000-0000-0000-000000000000') {
      appointmentError.value = 'API chưa trả về AppointmentId, không thể tạo hóa đơn. Vui lòng dùng mã GUID lịch hẹn.'
    }
  } catch (err: any) {
    appointmentError.value = err?.response?.data ?? 'Không tìm thấy lịch hẹn'
  } finally {
    appointmentLoading.value = false
  }
}

const fetchInvoiceById = async (id?: string) => {
  const target = (id || invoiceLookupId.value || '').trim()
  resetInvoiceState()
  resetPaymentState()
  if (!target) {
    invoiceError.value = 'Nhập mã hóa đơn để tra cứu'
    return
  }

  invoiceLoading.value = true
  try {
    const { data } = await api.get(`/invoicemanagement/${target}`)
    invoice.value = data
    invoiceLookupId.value = data.id
    invoiceMessage.value = 'Đã tải hóa đơn'
  } catch (err: any) {
    invoiceError.value = err?.response?.data?.message ?? 'Không tìm thấy hóa đơn'
    invoice.value = null
  } finally {
    invoiceLoading.value = false
  }
}

const createInvoice = async () => {
  resetInvoiceState()
  resetPaymentState()

  if (!appointment.value?.id) {
    invoiceError.value = 'Thiếu AppointmentId, không thể tạo hóa đơn'
    return
  }

  if (!amount.value || amount.value <= 0) {
    invoiceError.value = 'Số tiền phải lớn hơn 0'
    return
  }

  invoiceLoading.value = true
  try {
    const payload = {
      appointmentId: appointment.value.id,
      amount: amount.value
    }
    const { data } = await api.post('/invoicemanagement', payload)
    invoiceMessage.value = data.message ?? 'Tạo hóa đơn thành công'
    if (data.invoiceId) {
      await fetchInvoiceById(data.invoiceId)
    }
  } catch (err: any) {
    invoiceError.value = err?.response?.data?.message ?? 'Tạo hóa đơn thất bại'
  } finally {
    invoiceLoading.value = false
  }
}

const recalcInvoice = async () => {
  resetInvoiceState()
  resetPaymentState()

  if (!appointment.value?.id) {
    invoiceError.value = 'Chưa có lịch hẹn để tính hóa đơn'
    return
  }

  invoiceLoading.value = true
  try {
    const payload = {
      appointmentId: appointment.value.id,
      insuranceCoverPercent: (insuranceCoverPercent.value || 0) / 100,
      insuranceCode: insuranceCode.value || null,
      surcharge: surcharge.value || 0,
      discount: discount.value || 0
    }
    const { data } = await api.post('/invoicemanagement/recalculate', payload)
    invoice.value = data
    invoiceLookupId.value = data.id
    invoiceMessage.value = 'Đã tính lại hóa đơn'
    payAmount.value = data.amount
  } catch (err: any) {
    invoiceError.value = err?.response?.data?.message ?? 'Không tính lại được hóa đơn'
  } finally {
    invoiceLoading.value = false
  }
}

const validateInsurance = async () => {
  insuranceMessage.value = ''
  resetInvoiceState()
  if (!insuranceCode.value.trim()) {
    invoiceError.value = 'Nhập mã bảo hiểm trước khi kiểm tra'
    return
  }
  try {
    const { data } = await api.get('/insurance/validate', { params: { code: insuranceCode.value.trim() } })
    insuranceCoverPercent.value = Math.round((data.coveragePercent || 0) * 100)
    insuranceMessage.value = `${data.name} - Giảm ${insuranceCoverPercent.value}%`
    invoiceMessage.value = 'Đã áp dụng mã bảo hiểm'
  } catch (err: any) {
    invoiceError.value = err?.response?.data?.message ?? 'Mã bảo hiểm không hợp lệ'
  }
}

const payInvoice = async () => {
  resetPaymentState()

  if (!invoice.value?.id) {
    paymentError.value = 'Chưa có hóa đơn để thanh toán'
    return
  }

  if (!payAmount.value || payAmount.value <= 0) {
    paymentError.value = 'Số tiền thanh toán không hợp lệ'
    return
  }

  paymentLoading.value = true
  try {
    const payload = {
      invoiceId: invoice.value.id,
      amount: payAmount.value,
      method: paymentMethod.value
    }
    const { data } = await api.post('/payment', payload)
    paymentMessage.value = data.message ?? 'Thanh toán thành công'
    if (data.invoice) {
      invoice.value = data.invoice
    } else {
      invoice.value = { ...(invoice.value || {}), isPaid: true, paymentDate: new Date().toISOString() }
    }
  } catch (err: any) {
    paymentError.value = err?.response?.data ?? 'Thanh toán thất bại'
  } finally {
    paymentLoading.value = false
  }
}
</script>

<template>
  <div class="container py-4">
    <div class="d-flex align-items-center justify-content-between mb-3">
      <h2 class="mb-0">Hóa đơn & thanh toán</h2>
      <span class="badge bg-primary">Vai trò: Cashier</span>
    </div>

    <div class="row g-3">
      <div class="col-lg-6">
        <div class="card shadow-sm h-100">
          <div class="card-body">
            <div class="d-flex justify-content-between align-items-center mb-3">
              <h5 class="card-title mb-0">Tra cứu lịch hẹn</h5>
              <span class="text-muted small">Nhập mã lịch hẹn</span>
            </div>
            <div class="input-group mb-3">
              <input
                v-model="appointmentCode"
                type="text"
                class="form-control"
                placeholder="Mã lịch hẹn (AppointmentCode)"
              />
              <button
                class="btn btn-outline-primary"
                type="button"
                :disabled="appointmentLoading"
                @click="fetchAppointment"
              >
                <span v-if="appointmentLoading" class="spinner-border spinner-border-sm me-1" />
                Tra cứu
              </button>
            </div>
            <div v-if="appointmentError" class="alert alert-danger py-2">
              {{ appointmentError }}
            </div>
            <div v-if="appointment" class="appointment-card">
              <div class="d-flex justify-content-between">
                <div>
                  <div class="fw-semibold">{{ appointment.fullName }}</div>
                  <div class="text-muted small">{{ appointment.phone }} · {{ appointment.email || 'Không email' }}</div>
                </div>
                <span class="badge bg-info text-dark">{{ appointment.appointmentCode }}</span>
              </div>
              <div class="mt-2 text-muted small">
                Ngày {{ new Date(appointment.appointmentDate).toLocaleDateString() }} ·
                Giờ {{ appointment.appointmentTime }} · Trạng thái: {{ appointment.status }}
              </div>
              <div v-if="!appointment.id" class="alert alert-warning mt-2 mb-0 py-2">
                API chưa trả về <code>Id</code>; cần cung cấp GUID lịch hẹn để tạo hóa đơn.
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="col-lg-6">
        <div class="card shadow-sm h-100">
          <div class="card-body">
            <div class="d-flex justify-content-between align-items-center mb-3">
              <h5 class="card-title mb-0">Tạo hóa đơn</h5>
              <span class="text-muted small">Gắn với lịch hẹn</span>
            </div>
            <div class="mb-3">
              <label class="form-label">Số tiền</label>
              <div class="input-group">
                <input
                  v-model.number="amount"
                  type="number"
                  min="0"
                  class="form-control"
                  placeholder="Nhập số tiền"
                />
                <span class="input-group-text">VND</span>
              </div>
            </div>
            <div class="d-flex gap-2 mb-3">
              <button
                class="btn btn-primary"
                type="button"
                :disabled="!canCreateInvoice"
                @click="createInvoice"
              >
                <span v-if="invoiceLoading" class="spinner-border spinner-border-sm me-1" />
                Tạo hóa đơn
              </button>
              <button
                class="btn btn-outline-secondary"
                type="button"
                :disabled="invoiceLoading"
                @click="resetInvoiceState"
              >
                Xóa thông báo
              </button>
            </div>

            <hr />
            <div class="fw-semibold mb-2">Bảo hiểm & phụ thu (Cashier nhập)</div>
            <div class="row g-3">
              <div class="col-md-6">
                <label class="form-label">Mã bảo hiểm</label>
                <div class="input-group">
                  <input v-model="insuranceCode" type="text" class="form-control" placeholder="Nhập mã BHYT / bảo hiểm" />
                  <button class="btn btn-outline-primary" type="button" :disabled="invoiceLoading" @click="validateInsurance">Kiểm tra</button>
                </div>
              </div>
              <div class="col-md-3">
                <label class="form-label">Bảo hiểm chi trả (%)</label>
                <input v-model.number="insuranceCoverPercent" type="number" min="0" max="100" class="form-control" />
              </div>
              <div class="col-md-3">
                <label class="form-label">Phụ thu (VND)</label>
                <input v-model.number="surcharge" type="number" min="0" class="form-control" />
              </div>
              <div class="col-md-3">
                <label class="form-label">Giảm trừ (VND)</label>
                <input v-model.number="discount" type="number" min="0" class="form-control" />
              </div>
            </div>
            <div class="d-flex gap-2 mt-3">
              <button class="btn btn-outline-primary" type="button" :disabled="invoiceLoading" @click="recalcInvoice">
                <span v-if="invoiceLoading" class="spinner-border spinner-border-sm me-1" />
                Tính lại theo BH / phụ thu
              </button>
              <button class="btn btn-outline-secondary" type="button" @click="resetAdjustments">
                Đặt lại
              </button>
            </div>

            <div v-if="invoiceMessage" class="alert alert-success mt-3 py-2">
              {{ invoiceMessage }}
            </div>
            <div v-if="insuranceMessage" class="alert alert-info mt-2 py-2">
              {{ insuranceMessage }}
            </div>
            <div v-if="invoiceError" class="alert alert-danger mt-3 py-2">
              {{ invoiceError }}
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="row g-3 mt-1">
      <div class="col-lg-5">
        <div class="card shadow-sm h-100">
          <div class="card-body">
            <div class="d-flex justify-content-between align-items-center mb-3">
              <h5 class="card-title mb-0">Tra cứu hóa đơn</h5>
              <span class="text-muted small">Nhập mã hóa đơn (GUID)</span>
            </div>
            <div class="input-group mb-3">
              <input
                v-model="invoiceLookupId"
                type="text"
                class="form-control"
                placeholder="Mã hóa đơn"
              />
              <button
                class="btn btn-outline-primary"
                type="button"
                :disabled="invoiceLoading"
                @click="fetchInvoiceById()"
              >
                <span v-if="invoiceLoading" class="spinner-border spinner-border-sm me-1" />
                Tải hóa đơn
              </button>
            </div>
            <div v-if="invoiceError" class="alert alert-danger py-2">
              {{ invoiceError }}
            </div>
          </div>
        </div>
      </div>

      <div class="col-lg-7">
        <div class="card shadow-sm h-100">
          <div class="card-body">
            <div class="d-flex justify-content-between align-items-center mb-3">
              <h5 class="card-title mb-0">Chi tiết & thanh toán</h5>
              <span v-if="invoice?.isPaid" class="badge bg-success">Đã thanh toán</span>
              <span v-else class="badge bg-warning text-dark">Chưa thanh toán</span>
            </div>

            <div v-if="invoice" class="mb-3">
              <div class="row small text-muted">
                <div class="col-md-6">
                  <div>Mã hóa đơn: <span class="fw-semibold">{{ invoice.id }}</span></div>
                  <div>Số tiền: <span class="fw-semibold">{{ invoice.amount }}</span></div>
                  <div>Ngày tạo: {{ invoice.createdAt ? new Date(invoice.createdAt).toLocaleString() : '—' }}</div>
                </div>
                <div class="col-md-6">
                  <div>Trạng thái: {{ invoice.isPaid ? 'Đã thanh toán' : 'Chưa thanh toán' }}</div>
                  <div>Ngày thanh toán: {{ invoice.paymentDate ? new Date(invoice.paymentDate).toLocaleString() : '—' }}</div>
                  <div>Mã lịch hẹn: {{ invoice.appointmentId }}</div>
                </div>
              </div>
              <hr />
              <div v-if="invoice.invoiceLines?.length">
                <div class="fw-semibold mb-2">Chi tiết tính tiền</div>
                <div class="table-responsive">
                  <table class="table table-sm align-middle">
                    <thead>
                      <tr>
                        <th>Mục</th>
                        <th class="text-end">Số tiền</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="line in invoice.invoiceLines" :key="line.id">
                        <td>{{ line.description }}</td>
                        <td class="text-end" :class="line.amount < 0 ? 'text-success' : ''">
                          {{ line.amount }}
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>

              <div class="row g-3 align-items-end">
                <div class="col-md-4">
                  <label class="form-label">Số tiền thanh toán</label>
                  <div class="input-group">
                    <input
                      v-model.number="payAmount"
                      type="number"
                      min="0"
                      class="form-control"
                    />
                    <span class="input-group-text">VND</span>
                  </div>
                </div>
                <div class="col-md-4">
                  <label class="form-label">Phương thức</label>
                  <select v-model="paymentMethod" class="form-select">
                    <option value="cash">Tiền mặt</option>
                    <option value="banker">Chuyển khoản</option>
                    <option value="card">Thẻ</option>
                  </select>
                </div>
                <div class="col-md-4 d-flex gap-2">
                  <button
                    class="btn btn-primary flex-grow-1"
                    type="button"
                    :disabled="!canPay"
                    @click="payInvoice"
                  >
                    <span v-if="paymentLoading" class="spinner-border spinner-border-sm me-1" />
                    Thanh toán
                  </button>
                  <button class="btn btn-outline-secondary" type="button" @click="resetPaymentState">
                    Xóa
                  </button>
                </div>
              </div>
            </div>

            <div v-else class="text-muted small">
              Chưa có hóa đơn nào được chọn.
            </div>

            <div v-if="paymentMessage" class="alert alert-success mt-3 py-2">
              {{ paymentMessage }}
            </div>
            <div v-if="paymentError" class="alert alert-danger mt-3 py-2">
              {{ paymentError }}
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style src="@/styles/layouts/cashier.css"></style>
