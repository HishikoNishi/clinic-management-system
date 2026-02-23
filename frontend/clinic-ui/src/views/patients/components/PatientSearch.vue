<script setup>
import { ref, watch } from "vue"

const emit = defineEmits(["search"])

const keyword = ref("")

// emit khi gõ (debounce nhẹ 300ms)
let timeout = null

watch(keyword, (value) => {
  clearTimeout(timeout)
  timeout = setTimeout(() => {
    emit("search", value)
  }, 300)
})
</script>

<template>
  <div class="mb-4">
    <input
      v-model="keyword"
      type="text"
      placeholder="Search by name, phone, email..."
      class="w-full border border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-400"
    />
  </div>
</template>