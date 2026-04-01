<script setup lang="ts">
import { ref, computed } from 'vue'

const currentYear = computed(() => new Date().getFullYear())
</script>

<template>
  <footer class="footer bg-light text-center py-3 mt-5 border-top fixed-bottom">
    <div class="container-fluid">
      <p class="mb-0 text-dark">
        <small>
          &copy; {{ currentYear }} Hệ thống quản lý phòng khám. Bảo lưu mọi quyền.
        </small>
      </p>
    </div>
  </footer>
</template>

<style scoped>
.footer {
  background-color: #f8f9fa !important;
  border-top: 1px solid #dee2e6;
  box-shadow: 0 -2px 4px rgba(0, 0, 0, 0.05);
}

small {
  font-size: 0.875rem;
  color: #6c757d;
}
</style>