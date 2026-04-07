<script setup lang="ts">
const articles = [
  {
    title: 'Khai trương khu xét nghiệm nhanh',
    date: '02/2026',
    desc: 'Thêm dây chuyền xét nghiệm tự động, trả kết quả trong 2 giờ.',
    image:
      'https://images.unsplash.com/photo-1582719471384-894fbb16e074?auto=format&fit=crop&w=900&q=80'
  },
  {
    title: 'Gói khám sức khỏe tổng quát',
    date: '01/2026',
    desc: 'Ưu đãi 20% cho khách đặt lịch online, áp dụng đến 30/06/2026.',
    image:
      'https://images.unsplash.com/photo-1576091160399-112ba8d25d1d?auto=format&fit=crop&w=900&q=80'
  },
  {
    title: 'Bác sĩ mới gia nhập',
    date: '12/2025',
    desc: 'BS. Trần Thu Hà (Nhi) và BS. Lê Quỳnh Mai (RHM) chính thức nhận khám.',
    image:
      'https://images.unsplash.com/photo-1631217868264-e5b90bb7e133?auto=format&fit=crop&w=900&q=80'
  }
]
</script>

<template>
  <section class="news-section">
    <div class="container">
      <p class="news-eyebrow">Tin tức</p>
      <h3 class="news-heading mb-3">Tin tức &amp; cập nhật</h3>
      <div class="row g-3">
        <div v-for="a in articles" :key="a.title" class="col-lg-4 col-md-6">
          <div class="card h-100 shadow-sm news-card overflow-hidden">
            <img
              :src="a.image"
              class="card-img-top news-image"
              :alt="a.title"
              loading="lazy"
            />
            <div class="card-body">
              <div class="news-date small mb-1">{{ a.date }}</div>
              <h6 class="mb-1">{{ a.title }}</h6>
              <p class="text-muted small mb-0">{{ a.desc }}</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
.news-section {
  padding: var(--space-16) 0 var(--space-20);
  background: var(--color-bg-main);
  border-top: 1px solid var(--color-border);
}
.news-eyebrow {
  font-size: var(--font-size-xs);
  font-weight: var(--font-weight-semibold);
  letter-spacing: var(--letter-spacing-wide);
  text-transform: uppercase;
  color: var(--color-primary-dark);
  margin-bottom: var(--space-2);
}
.news-heading {
  font-size: var(--font-size-3xl);
  font-weight: var(--font-weight-bold);
  color: var(--color-text-primary);
}
.news-date {
  color: var(--color-primary-dark);
  font-weight: var(--font-weight-semibold);
}
.news-image {
  width: 100%;
  height: 200px;
  object-fit: cover;
  object-position: center;
  border-bottom: 1px solid var(--color-border);
}

@media (min-width: 992px) {
  .news-image {
    height: 220px;
  }
}

.news-card {
  border: 1px solid var(--color-border);
  border-radius: var(--radius-xl);
  transition: box-shadow var(--transition-base), transform var(--transition-base);
}
.news-card:hover {
  box-shadow: var(--shadow-md);
  transform: translateY(-2px);
}
.news-card h6 {
  color: var(--color-text-primary);
  font-weight: var(--font-weight-semibold);
}
</style>
