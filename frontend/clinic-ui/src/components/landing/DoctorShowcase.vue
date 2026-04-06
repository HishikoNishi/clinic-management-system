<script setup lang="ts">
import { ref, onMounted } from 'vue'
import api from '@/services/api'

const doctors = ref<any[]>([])
const loading = ref(false)
const fallbackPhotos = [
  "https://images.unsplash.com/photo-1550831107-1553da8c8464?auto=format&fit=crop&w=600&q=80",
  "https://images.unsplash.com/photo-1524504388940-b1c1722653e1?auto=format&fit=crop&w=600&q=80",
  "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=600&q=80",
  "https://images.unsplash.com/photo-1544723795-3fb6469f5b39?auto=format&fit=crop&w=600&q=80"
]

const loadDoctors = async () => {
  loading.value = true
  try {
    const res = await api.get('/Doctor')
    const list = Array.isArray(res.data) ? res.data : []
    doctors.value = list.slice(0, 4).map((d: any, idx: number) => ({
      ...d,
      avatarUrl: d.avatarUrl || fallbackPhotos[idx % fallbackPhotos.length],
      dept: d.departmentName || d.department?.name || 'Đa khoa',
      exp: d.licenseNumber ? `Mã GP: ${d.licenseNumber}` : '',
      tags: [d.specialtyName || d.specialty?.name || ''].filter(Boolean)
    }))
  } catch (err) {
    console.warn('Không tải được danh sách bác sĩ', err)
  } finally {
    loading.value = false
  }
}

onMounted(loadDoctors)
</script>

<template>
  <section class="doctor-section" id="doctors">
    <div class="container">
      <p class="doctor-eyebrow">Đội ngũ</p>
      <div class="d-flex justify-content-between align-items-end mb-4 flex-wrap gap-2">
        <div>
          <h2 class="doctor-heading mb-1">Chuyên gia y tế</h2>
          <p class="text-muted mb-0 doctor-lead">Bác sĩ nhiều kinh nghiệm, đồng hành cùng bạn tại từng chuyên khoa.</p>
        </div>
      </div>
      <div v-if="loading" class="text-muted small mb-2">Đang tải danh sách bác sĩ...</div>
      <div class="row g-3">
        <div v-for="doc in doctors" :key="doc.id || doc.code || doc.name" class="col-md-6 col-lg-3">
          <div class="card shadow-sm h-100 doctor-card">
            <img :src="doc.avatarUrl" class="card-img-top doctor-photo" alt="doctor photo" loading="lazy" />
            <div class="card-body">
              <div class="fw-semibold">{{ doc.fullName || doc.name }}</div>
              <div class="doctor-dept small mb-1">{{ doc.dept }}</div>
              <div class="text-muted small mb-2">{{ doc.exp || 'Bác sĩ giàu kinh nghiệm' }}</div>
              <div class="tag-list">
                <span v-for="tag in doc.tags" :key="tag" class="badge bg-light text-dark me-1 mb-1">{{ tag }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
.doctor-section {
  padding: var(--space-16) 0;
  background: var(--color-bg-main);
}
.doctor-eyebrow {
  font-size: var(--font-size-xs);
  font-weight: var(--font-weight-semibold);
  letter-spacing: var(--letter-spacing-wide);
  text-transform: uppercase;
  color: var(--color-primary-dark);
  margin-bottom: var(--space-2);
}
.doctor-heading {
  font-size: var(--font-size-4xl);
  font-weight: var(--font-weight-bold);
  color: var(--color-text-primary);
  line-height: var(--line-height-tight);
}
.doctor-lead {
  max-width: 36rem;
  line-height: var(--line-height-relaxed);
}

.doctor-dept {
  color: var(--color-primary-dark);
  font-weight: var(--font-weight-semibold);
}
.doctor-card {
  border: 1px solid var(--color-border);
  border-radius: var(--radius-xl);
  overflow: hidden;
  transition: box-shadow var(--transition-base), transform var(--transition-base);
}
.doctor-card:hover {
  box-shadow: var(--shadow-md);
  transform: translateY(-2px);
}
.doctor-photo {
  height: 180px;
  object-fit: cover;
  border-bottom: 1px solid var(--color-border);
}
.tag-list .badge {
  font-weight: var(--font-weight-medium);
  background: var(--color-bg-soft) !important;
  color: var(--color-text-primary) !important;
  border: 1px solid var(--color-border);
}
</style>
