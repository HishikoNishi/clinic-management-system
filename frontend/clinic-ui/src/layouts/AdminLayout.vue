<script setup lang="ts">
import AppNavbar from '@/components/layouts/AppNavbar.vue'
</script>

<template>
  <div class="admin-layout">
    <!-- Navigation -->
    <AppNavbar />

    <!-- Main Content -->
    <main class="main-content">
      <div class="container-fluid py-4">
        <router-view />
      </div>
    </main>
  </div>
</template>

<style scoped>
.admin-layout {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
  background-color: #f8f9fa;
}

.main-content {
  margin-top: 70px;
  flex: 1;
}

@media (max-width: 991px) {
  .main-content {
    margin-top: 60px;
  }
}
</style>
