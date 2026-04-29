<template>
  <div class="app-toast-container">
    <div
      v-for="toast in toastItems"
      :key="toast.id"
      class="app-toast"
      :class="`app-toast-${toast.type}`"
    >
      <div class="app-toast-message">{{ toast.message }}</div>
      <button class="app-toast-close" type="button" @click="toastStore.remove(toast.id)">×</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useToast, type ToastMessage } from '@/composables/useToast'

const toastStore = useToast()
const toastItems = computed<ToastMessage[]>(() => toastStore.toasts.value)
</script>

<style scoped>
.app-toast-container {
  position: fixed;
  top: 16px;
  right: 16px;
  z-index: 2100;
  display: flex;
  flex-direction: column;
  gap: 10px;
  width: min(380px, calc(100vw - 24px));
}

.app-toast {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 8px;
  border-radius: 10px;
  padding: 10px 12px;
  color: #fff;
  box-shadow: 0 10px 24px rgba(15, 23, 42, 0.22);
}

.app-toast-message {
  font-size: 14px;
  line-height: 1.35;
  white-space: pre-line;
}

.app-toast-close {
  border: 0;
  background: transparent;
  color: inherit;
  font-size: 20px;
  line-height: 1;
  cursor: pointer;
  padding: 0 2px;
}

.app-toast-success { background: #16a34a; }
.app-toast-error { background: #dc2626; }
.app-toast-warning { background: #d97706; }
.app-toast-info { background: #2563eb; }
</style>
