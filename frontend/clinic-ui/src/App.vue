<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import GuestLayout from '@/layouts/GuestLayout.vue'
import AdminLayout from '@/layouts/AdminLayout.vue'
import DoctorLayout from '@/layouts/DoctorLayout.vue'
import CashierLayout from '@/layouts/CashierLayout.vue'
import AppToastContainer from '@/components/common/AppToastContainer.vue'

const route = useRoute()

// Get layout from route meta, default to 'guest'
const currentLayout = computed(() => {
  return (route.meta.layout as string) || 'guest'
})

// Layout components map
const layoutComponents = {
  guest: GuestLayout,
  admin: AdminLayout,
  dashboard: AdminLayout,
  doctor: DoctorLayout,
  cashier: CashierLayout
}

// Get current layout component
const LayoutComponent = computed(() => {
  const layout = currentLayout.value as keyof typeof layoutComponents
  return layoutComponents[layout] || layoutComponents.guest
})
</script>

<template>
  <component :is="LayoutComponent" />
  <AppToastContainer />
</template>

<style scoped>
</style>
