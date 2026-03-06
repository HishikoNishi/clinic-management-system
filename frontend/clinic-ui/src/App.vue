<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import GuestLayout from '@/layouts/GuestLayout.vue'
import AdminLayout from '@/layouts/AdminLayout.vue'

const route = useRoute()

// Get layout from route meta, default to 'guest'
const currentLayout = computed(() => {
  return (route.meta.layout as string) || 'guest'
})

// Layout components map
const layoutComponents = {
  guest: GuestLayout,
  admin: AdminLayout
}

// Get current layout component
const LayoutComponent = computed(() => {
  const layout = currentLayout.value as keyof typeof layoutComponents
  return layoutComponents[layout] || layoutComponents.guest
})
</script>

<template>
  <component :is="LayoutComponent" />
</template>

<style scoped>
</style>
