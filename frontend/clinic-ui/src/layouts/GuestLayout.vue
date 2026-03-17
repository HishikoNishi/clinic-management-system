<script setup lang="ts">
import AppNavbar from '@/components/layouts/AppNavbar.vue'
import AppFooter from '@/components/layouts/AppFooter.vue'
</script>

<template>
  <div class="guest-layout">
    <!-- Navigation -->
    <AppNavbar />

    <!-- Main Content -->
    <main class="main-content">
      <router-view />
    </main>

    <!-- Footer -->
    <AppFooter />
  </div>
</template>

<style src="@/styles/layouts/guest-layout.css" scoped></style>
