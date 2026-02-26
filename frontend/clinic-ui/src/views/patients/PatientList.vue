<script setup>
import { ref, onMounted, watch } from "vue"
import patientService from "@/services/patientService"
import { useRouter } from "vue-router"

const router = useRouter()
const patients = ref([])
const keyword = ref("")

const loadPatients = async () => {
  const res = await patientService.getAll()
  patients.value = res.data
}

const searchPatients = async () => {
  if (!keyword.value) {
    await loadPatients()
    return
  }

  const res = await patientService.search(keyword.value)
  patients.value = res.data
}

// debounce search (tránh spam API)
let timeout = null
watch(keyword, () => {
  clearTimeout(timeout)
  timeout = setTimeout(() => {
    searchPatients()
  }, 400)
})

const goToCreate = () => {
  router.push("/patients/create")
}

const goToEdit = (id) => {
  router.push(`/patients/edit/${id}`)
}

onMounted(loadPatients)
</script>

<template>
  <div class="p-6">
    <div class="flex justify-between mb-4 items-center">
      <h1 class="text-xl font-bold">Patient List</h1>
      <button
        @click="goToCreate"
        class="bg-blue-500 text-white px-4 py-2 rounded"
      >
        Add Patient
      </button>
    </div>

    <!-- SEARCH -->
    <input
      v-model="keyword"
      type="text"
      placeholder="Search by name, phone, email..."
      class="border border-gray-300 px-3 py-2 rounded w-full mb-4"
    />

    <table class="w-full border">
      <thead class="bg-gray-100">
        <tr>
          <th class="p-2">Name</th>
          <th class="p-2">Phone</th>
          <th class="p-2">Email</th>
          <th class="p-2">Action</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="p in patients" :key="p.id">
          <td class="p-2">{{ p.fullName }}</td>
          <td class="p-2">{{ p.phone }}</td>
          <td class="p-2">{{ p.email }}</td>
          <td class="p-2">
            <button
              @click="goToEdit(p.id)"
              class="text-blue-600 hover:underline"
            >
              Edit
            </button>
          </td>
        </tr>
      </tbody>
    </table>

    <!-- Empty state -->
    <div v-if="patients.length === 0" class="text-center mt-4 text-gray-500">
      No patients found.
    </div>
  </div>
</template>