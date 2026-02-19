<script setup lang="ts">
import { ref } from 'vue'

const name = ref('')
const email = ref('')
const role = ref('')
const isActive = ref(true)

const submitForm = () => {
  console.log({
    name: name.value,
    email: email.value,
    role: role.value,
    isActive: isActive.value
  })
}
</script>

<template>
  <div class="p-6 max-w-md">
    <h2 class="text-xl font-bold mb-4">Create Staff</h2>

    <form @submit.prevent="submitForm" class="space-y-4">

      <div>
        <label>Name</label>
        <input v-model="name" class="border w-full p-2 rounded" />
      </div>

      <div>
        <label>Email</label>
        <input v-model="email" type="email" class="border w-full p-2 rounded" />
      </div>

      <div>
        <label>Role</label>
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
        class="bg-blue-600 text-white px-4 py-2 rounded"
      >
        Save
      </button>

    </form>
  </div>
</template>
