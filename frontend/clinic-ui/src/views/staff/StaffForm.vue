<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getRoles, type Role } from '@/services/roleService'
import { getUsers, type User } from '@/services/userService'


const name = ref('')
const email = ref('')
const roleId = ref('')
const isActive = ref(true)
const users = ref<User[]>([])
const userId = ref('')
const roles = ref<Role[]>([])

onMounted(async () => {
  try {
    roles.value = await getRoles()
  } catch (error) {
    console.error('Failed to load roles:', error)
  }
})

const submitForm = () => {
  if (!name.value || !email.value || !roleId.value || !userId.value) {
    alert('Please fill all fields')
    return
  }

  console.log({
    name: name.value,
    email: email.value,
    roleId: roleId.value,
    userId: userId.value,
    isActive: isActive.value
  })
onMounted(async () => {
  try {
    roles.value = await getRoles()
    users.value = await getUsers()
  } catch (error) {
    console.error('Failed to load data:', error)
  }
})


  alert('Staff form submitted!')
}
</script>

<template>
  <div class="p-6 max-w-lg mx-auto">
    <h2 class="text-2xl font-bold mb-6">Create Staff</h2>

    <form @submit.prevent="submitForm" class="space-y-4">

      <!-- Name -->
      <div>
        <label class="block font-medium mb-1">Name</label>
        <input v-model="name" class="border w-full p-2 rounded" />
      </div>

      <!-- Email -->
      <div>
        <label class="block font-medium mb-1">Email</label>
        <input v-model="email" type="email" class="border w-full p-2 rounded" />
      </div>

      <!-- Role Dropdown -->
      <div>
        <label class="block font-medium mb-1">Role</label>
        <select v-model="roleId" class="border w-full p-2 rounded">
          <option value="">Select role</option>
          <option
            v-for="r in roles"
            :key="r.id"
            :value="r.id"
          >
            {{ r.name }}
          </option>
        </select>
      </div>

      <!-- Status -->
      <div class="flex items-center space-x-2">
        <input type="checkbox" v-model="isActive" />
        <label>Active</label>
      </div>

      <!-- Submit -->
      <button
        type="submit"
        class="bg-blue-600 text-white px-4 py-2 rounded w-full"
      >
        Save
      </button>
<!-- Assign User -->
<div>
  <label class="block font-medium mb-1">Assign User</label>
  <select v-model="userId" class="border w-full p-2 rounded">
    <option value="">Select user</option>
    <option
      v-for="u in users"
      :key="u.id"
      :value="u.id"
    >
      {{ u.username }}
    </option>
  </select>
</div>

    </form>
  </div>
</template>
