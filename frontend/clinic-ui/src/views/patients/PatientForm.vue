<script setup>
import { ref, onMounted } from "vue"
import { useRoute, useRouter } from "vue-router"
import patientService from "@/services/patientService"

const route = useRoute()
const router = useRouter()
const id = route.params.id

const form = ref({
  fullName: "",
  dateOfBirth: "",
  gender: 0,
  phone: "",
  email: "",
  address: "",
  note: ""
})

const errors = ref({})

const loadPatient = async () => {
  if (id) {
    const res = await patientService.getById(id)
    form.value = res.data
  }
}

const validate = () => {
  errors.value = {}

  if (!form.value.fullName) {
    errors.value.fullName = "Full name is required"
  }

  if (!form.value.dateOfBirth) {
    errors.value.dateOfBirth = "Date of birth is required"
  }

  if (!form.value.gender && form.value.gender !== 0) {
    errors.value.gender = "Gender is required"
  }

  if (form.value.email && !/\S+@\S+\.\S+/.test(form.value.email)) {
    errors.value.email = "Invalid email format"
  }

  if (form.value.phone && !/^[0-9]{9,11}$/.test(form.value.phone)) {
    errors.value.phone = "Phone must be 9-11 digits"
  }

  return Object.keys(errors.value).length === 0
}

const submit = async () => {
  if (!validate()) return

  if (id) {
    await patientService.update(id, form.value)
  } else {
    await patientService.create(form.value)
  }

  router.push("/patients")
}

onMounted(loadPatient)
</script>

<template>
  <div class="p-6 max-w-lg mx-auto">
    <h2 class="text-xl font-bold mb-4">
      {{ id ? "Edit Patient" : "Add Patient" }}
    </h2>

    <!-- Full Name -->
    <input v-model="form.fullName" placeholder="Full Name" class="input" />
    <p v-if="errors.fullName" class="error">{{ errors.fullName }}</p>

    <!-- Date of Birth -->
    <input type="date" v-model="form.dateOfBirth" class="input" />
    <p v-if="errors.dateOfBirth" class="error">{{ errors.dateOfBirth }}</p>

    <!-- Gender -->
    <select v-model="form.gender" class="input">
      <option :value="0">Male</option>
      <option :value="1">Female</option>
    </select>
    <p v-if="errors.gender" class="error">{{ errors.gender }}</p>

    <!-- Phone -->
    <input v-model="form.phone" placeholder="Phone" class="input" />
    <p v-if="errors.phone" class="error">{{ errors.phone }}</p>

    <!-- Email -->
    <input v-model="form.email" placeholder="Email" class="input" />
    <p v-if="errors.email" class="error">{{ errors.email }}</p>

    <!-- Address -->
    <input v-model="form.address" placeholder="Address" class="input" />

    <!-- Note -->
    <textarea v-model="form.note" placeholder="Note" class="input"></textarea>

    <button
      @click="submit"
      class="bg-green-500 text-white px-4 py-2 rounded mt-3"
    >
      Save
    </button>
  </div>
</template>

<style>
.input {
  display: block;
  width: 100%;
  border: 1px solid #ccc;
  padding: 8px;
  margin-bottom: 6px;
  border-radius: 4px;
}

.error {
  color: red;
  font-size: 13px;
  margin-bottom: 10px;
}
</style>