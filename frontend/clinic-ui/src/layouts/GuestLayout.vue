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

<style scoped>
.guest-layout {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

.main-content {
  margin-bottom: 80px;
  flex: 1;
}

@media (max-width: 991px) {
  .main-content {
    margin-top: 60px;
  }
}
</style>
