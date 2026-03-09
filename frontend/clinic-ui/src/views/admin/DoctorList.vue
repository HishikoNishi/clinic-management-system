<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { doctorService, type Doctor, type CreateDoctorDto } from '@/services/doctorService'

const doctors = ref<Doctor[]>([])
const loading = ref(false)
const showModal = ref(false)
const editingId = ref<string | null>(null)

/* ========================= */
/* ======= FORM DATA ======= */
/* ========================= */

const form = reactive<CreateDoctorDto>({
  username: '',
  password: '',
  fullName: '',
  code: '',
  specialty: '',
  licenseNumber: ''
})

/* ========================= */

async function loadDoctors() {
  loading.value = true
  doctors.value = await doctorService.getAll()
  loading.value = false
}

function openCreate() {
  editingId.value = null
  form.username = ''
  form.password = ''
  form.fullName = ''
  form.code = ''
  form.specialty = ''
  form.licenseNumber = ''
  showModal.value = true
}

function openEdit(doctor: Doctor) {
  editingId.value = doctor.id
  form.username = '' // không edit username
  form.password = '' // không edit password
  form.code = doctor.code
  form.specialty = doctor.specialty
  form.licenseNumber = doctor.licenseNumber || ''
  showModal.value = true
}

async function save() {
  try {
    if (editingId.value) {
      await doctorService.update(editingId.value, {
        code: form.code,
        specialty: form.specialty,
        licenseNumber: form.licenseNumber,
        status: 1
      })
    } else {
      await doctorService.create(form)
    }

    showModal.value = false
    await loadDoctors()

  } catch (error: any) {

    if (error.response?.data?.errors) {
      const errors = Object.values(error.response.data.errors)
        .flat()
        .join("\n")

      alert(errors)
      return
    }

    if (error.response?.data?.message) {
      alert(error.response.data.message)
      return
    }

    if (typeof error.response?.data === "string") {
      alert(error.response.data)
      return
    }

    alert("Something went wrong")
  }
}

async function remove(id: string) {
  if (confirm("Are you sure?")) {
    await doctorService.delete(id)
    await loadDoctors()
  }
}

onMounted(loadDoctors)
</script>

<template>
  <div class="doctor-page">
    <div class="container">
      <div class="header">
        <h2>Doctor Management</h2>
        <button class="btn-primary" @click="openCreate">
          + Add Doctor
        </button>
      </div>

      <div class="card">
        <table class="doctor-table">
          <thead>
            <tr>
              <th>Code</th>
              <th>Specialty</th>
              <th>License</th>
              <th>Status</th>
              <th style="text-align:right">Action</th>
            </tr>
          </thead>

          <tbody>
            <tr v-for="d in doctors" :key="d.id">
              <td>{{ d.code }}</td>
              <td>{{ d.specialty }}</td>
              <td>{{ d.licenseNumber || '-' }}</td>
              <td>
                <span class="badge-active">
                  Active
                </span>
              </td>
              <td class="actions">
                <button class="btn-edit" @click="openEdit(d)">
                  Edit
                </button>
                <button class="btn-delete" @click="remove(d.id)">
                  Delete
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- MODAL -->
    <div v-if="showModal" class="modal">
      <div class="modal-content">
        <h3>{{ editingId ? "Edit Doctor" : "Create Doctor" }}</h3>

        <div v-if="!editingId">
          <input v-model="form.username" placeholder="Username" />
          <input v-model="form.password" type="password" placeholder="Password" />
        </div>
        <input v-model="form.fullName" placeholder="Full Name" />
        <input v-model="form.code" placeholder="Code" />
        <input v-model="form.specialty" placeholder="Specialty" />
        <input v-model="form.licenseNumber" placeholder="License Number" />

        <div class="modal-actions">
          <button class="btn-primary" @click="save">Save</button>
          <button class="btn-cancel" @click="showModal = false">Cancel</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.doctor-page {
  min-height: 100vh;
  padding: 40px 60px;
  background: linear-gradient(135deg, #4f46e5, #9333ea);
}

.container {
  max-width: 1200px;
  margin: auto;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
  color: white;
}

.card {
  background: white;
  border-radius: 20px;
  padding: 25px;
  box-shadow: 0 20px 50px rgba(0,0,0,0.2);
}

.doctor-table {
  width: 100%;
  border-collapse: collapse;
}

.doctor-table th {
  text-align: left;
  padding: 14px;
  font-weight: 600;
  color: #555;
  border-bottom: 1px solid #eee;
}

.doctor-table td {
  padding: 14px;
  border-bottom: 1px solid #f3f3f3;
}

.doctor-table tr:hover {
  background: #f9f9ff;
}

.actions {
  text-align: right;
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

.badge-active {
  background: #dcfce7;
  color: #16a34a;
  padding: 6px 12px;
  border-radius: 999px;
  font-size: 13px;
  font-weight: 600;
}

.btn-primary {
  background: linear-gradient(135deg, #6366f1, #8b5cf6);
  border: none;
  padding: 10px 18px;
  border-radius: 999px;
  color: white;
  cursor: pointer;
  font-weight: 600;
}

.btn-primary:hover {
  opacity: 0.9;
}

.btn-edit {
  background: #e0e7ff;
  color: #3730a3;
  border: none;
  padding: 6px 14px;
  border-radius: 999px;
  cursor: pointer;
}

.btn-delete {
  background: #fee2e2;
  color: #b91c1c;
  border: none;
  padding: 6px 14px;
  border-radius: 999px;
  cursor: pointer;
}

.modal {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.4);
  display: flex;
  justify-content: center;
  align-items: center;
}

.modal-content {
  background: white;
  padding: 30px;
  width: 350px;
  border-radius: 20px;
  display: flex;
  flex-direction: column;
  gap: 14px;
  box-shadow: 0 20px 60px rgba(0,0,0,0.3);
}

.modal-content input {
  padding: 10px;
  border-radius: 10px;
  border: 1px solid #ddd;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

.btn-cancel {
  background: #eee;
  border: none;
  padding: 10px 16px;
  border-radius: 999px;
  cursor: pointer;
}
</style>