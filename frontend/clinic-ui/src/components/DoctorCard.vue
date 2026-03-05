<script setup lang="ts">
import type { Doctor } from "@/services/doctorService"

defineProps<{ doctor: Doctor }>()
defineEmits(["edit", "delete", "view"])
</script>

<template>
  <div class="card shadow-sm p-3 h-100">
    <h5>{{ doctor.code }}</h5>
    <h5 class="text-muted">{{ doctor.specialty }}</h5>

    <div class="d-flex justify-content-between">
      <button class="btn btn-sm btn-outline-primary" @click="$emit('view', doctor)">
        View
      </button>

      <div>
        <button class="btn btn-sm btn-outline-dark me-2" @click="$emit('edit', doctor)">
          Edit
        </button>
        <button class="btn btn-sm btn-outline-danger" @click="$emit('delete', doctor.id)">
          Delete
        </button>
      </div>
    </div>
  </div>
</template>