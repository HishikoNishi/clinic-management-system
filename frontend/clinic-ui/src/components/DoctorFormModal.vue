<script setup lang="ts">
import { ref, watch } from "vue"
import type { Doctor } from "@/services/doctorService"

const props = defineProps<{
  doctor: Doctor | null
  isEdit: boolean
}>()

const emit = defineEmits(["close", "submit"])

const form = ref({
  name: "",
  specialization: ""
})

watch(
  () => props.doctor,
  (val) => {
    if (val) {
      form.value = {
        name: val.code,
        specialization: val.specialty
      }
    } else {
      form.value = { name: "", specialization: "" }
    }
  },
  { immediate: true }
)

const submit = () => {
  emit("submit", form.value)
}
</script>

<template>
  <div class="modal-backdrop d-flex align-items-center justify-content-center">
    <div class="bg-white p-4 rounded shadow" style="width:400px;">
      <h5>{{ isEdit ? "Edit Doctor" : "Add Doctor" }}</h5>

      <input v-model="form.name" class="form-control mb-3" placeholder="Name" />
      <input v-model="form.specialization" class="form-control mb-3" placeholder="Specialization" />

      <div class="text-end">
        <button class="btn btn-secondary me-2" @click="$emit('close')">Cancel</button>
        <button class="btn btn-primary" @click="submit">Save</button>
      </div>
    </div>
  </div>
</template>