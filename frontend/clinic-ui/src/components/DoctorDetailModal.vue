<script setup lang="ts">
import type { Doctor } from "@/services/doctorService"

defineProps<{ doctor: Doctor }>()
defineEmits(["close"])
</script>

<template>
  <div class="modal-backdrop d-flex align-items-center justify-content-center">
    <div class="bg-white p-4 rounded shadow" style="width:400px;">
      <h5>Doctor Detail</h5>

      <p><strong>Name:</strong> {{ doctor.  code }}</p>
      <p><strong>Specialization:</strong> {{ doctor.specialty}}</p>

      <div class="text-end">
        <button class="btn btn-primary" @click="$emit('close')">Close</button>
      </div>
    </div>
  </div>
</template>