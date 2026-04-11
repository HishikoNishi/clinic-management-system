import api from "@/services/api"

export interface MedicineItem {
  id: string
  name: string
  defaultDosage?: string | null
  unit: string
  price: number
  isActive: boolean
}

export interface UpsertMedicinePayload {
  name: string
  defaultDosage?: string | null
  unit: string
  price: number
  isActive: boolean
}

export const medicineApi = {
  async list(includeInactive = true): Promise<MedicineItem[]> {
    const { data } = await api.get("/Medicines", { params: { includeInactive } })
    return data ?? []
  },
  async create(payload: UpsertMedicinePayload) {
    const { data } = await api.post("/Medicines", payload)
    return data
  },
  async update(id: string, payload: UpsertMedicinePayload) {
    const { data } = await api.put(`/Medicines/${id}`, payload)
    return data
  },
  async toggle(id: string) {
    const { data } = await api.patch(`/Medicines/${id}/toggle`)
    return data
  }
}
