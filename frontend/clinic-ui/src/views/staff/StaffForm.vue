<script setup lang="ts">
import { ref } from 'vue'

const name = ref('')
const email = ref('')
const role = ref('')
const isActive = ref(true)

const submitForm = () => {
  if (!name.value || !email.value || !role.value) {
    alert('Please fill all fields')
    return
  }

  console.log({
    name: name.value,
    email: email.value,
    role: role.value,
    isActive: isActive.value
  })

  alert('Staff created successfully!')
}
</script>

<template>
  <div class="p-6 max-w-lg mx-auto">
    <h2 class="text-2xl font-bold mb-6">Create Staff</h2>

    <form @submit.prevent="submitForm" class="space-y-4">

      <div>
        <label class="block font-medium mb-1">Name</label>
        <input v-model="name" class="border w-full p-2 rounded" />
      </div>

      <div>
        <label class="block font-medium mb-1">Email</label>
        <input v-model="email" type="email" class="border w-full p-2 rounded" />
      </div>

      <div>
        <label class="block font-medium mb-1">Role</label>
        <select v-model="role" class="border w-full p-2 rounded">
          <option value="">Select role</option>
          <option value="Doctor">Doctor</option>
          <option value="Nurse">Nurse</option>
          <option value="Admin">Admin</option>
        </select>
      </div>

      <div class="flex items-center space-x-2">
        <input type="checkbox" v-model="isActive" />
        <label>Active</label>
      </div>

      <button
        type="submit"
        class="bg-blue-600 text-white px-4 py-2 rounded w-full"
      >
        Save
      </button>

    </form>
  </div>
</template>
