<script setup lang="ts">
const highlights = [
  { title: 'Trang thiết bị', desc: 'Máy siêu âm, X-quang kỹ thuật số, xét nghiệm tự động', icon: 'bi-hospital' },
  { title: 'Quy trình nhanh', desc: 'Đặt lịch online, lấy mã khám, check-in trong 1 phút', icon: 'bi-speedometer' },
  { title: 'An toàn & bảo mật', desc: 'OTP email, phân quyền theo vai trò, lưu vết thanh toán', icon: 'bi-shield-check' }
]
</script>

<template>
  <section class="facility-section">
    <div class="container">
      <div class="row g-3">
        <div v-for="h in highlights" :key="h.title" class="col-lg-4 col-md-6">
          <div class="card h-100 shadow-sm highlight-card">
            <div class="card-body">
              <div class="d-flex align-items-start gap-3">
                <div class="icon-wrap"><i :class="`bi ${h.icon}`"></i></div>
                <div>
                  <h5 class="mb-1">{{ h.title }}</h5>
                  <p class="text-muted mb-0 small">{{ h.desc }}</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
.facility-section {
  padding: var(--space-12) 0 var(--space-16);
  background: var(--color-bg-soft);
}
.highlight-card {
  border: 1px solid var(--color-border);
  border-radius: var(--radius-xl);
  transition: box-shadow var(--transition-base), transform var(--transition-base);
}
.highlight-card:hover {
  box-shadow: var(--shadow-md);
  transform: translateY(-2px);
}
.icon-wrap {
  width: 48px;
  height: 48px;
  border-radius: var(--radius-lg);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  background: var(--color-primary-light);
  color: var(--color-primary-dark);
  font-size: 1.25rem;
}
.highlight-card h5 {
  color: var(--color-text-primary);
  font-weight: var(--font-weight-semibold);
}
</style>
