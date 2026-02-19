<script setup lang="ts">
import { ref, onMounted } from 'vue'
import type { Staff } from '@/types/staff'
import { getStaffs } from '@/services/staffService'

const staffs = ref<Staff[]>([])

const loadStaffs = async () => {
  staffs.value = await getStaffs()
}

onMounted(() => {
  loadStaffs()
})
</script>

<template>
  <div class="p-6">
    <h1 class="text-2xl font-bold mb-4">Staff Management</h1>

    <router-link to="/staff/create">
  <button class="bg-blue-600 text-white px-4 py-2 rounded mb-4">
    Create Staff
  </button>
</router-link>


    <table class="w-full border">
      <thead class="bg-gray-100">
        <tr>
          <th class="p-2">Name</th>
          <th>Email</th>
          <th>Role</th>
          <th>Status</th>
          <th>Action</th>
        </tr>
      </thead>

      <tbody>
        <tr
          v-for="staff in staffs"
          :key="staff.id"
          class="border-t text-center"
        >
          <td class="p-2">{{ staff.name }}</td>
          <td>{{ staff.email }}</td>
          <td>{{ staff.role }}</td>
          <td>
            <span
              :class="[
                'px-2 py-1 rounded text-white',
                staff.isActive ? 'bg-green-500' : 'bg-red-500'
              ]"
            >
              {{ staff.isActive ? 'Active' : 'Inactive' }}
            </span>
          </td>
          <td>
            <button class="text-blue-600">Edit</button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
