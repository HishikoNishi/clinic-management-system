<template>
  <div class="audit-page container-fluid py-4">
    <div class="audit-hero card border-0 shadow-sm mb-3">
      <div class="card-body d-flex flex-wrap justify-content-between align-items-start gap-3">
        <div>
          <p class="audit-eyebrow mb-1">ADMIN</p>
          <h4 class="mb-1">Nhật ký thay đổi dữ liệu</h4>
          <p class="text-muted mb-0">Theo dõi toàn bộ thao tác tạo, cập nhật và xóa mềm trong hệ thống.</p>
        </div>
        <div class="audit-metrics text-end">
          <div class="metric-label">Tổng bản ghi</div>
          <div class="metric-value">{{ total.toLocaleString('vi-VN') }}</div>
          <div class="metric-sub">Trang {{ page }} / {{ totalPages }}</div>
        </div>
      </div>
    </div>

    <div class="card border-0 shadow-sm mb-3">
      <div class="card-body">
        <div class="row g-3 align-items-end">
          <div class="col-12 col-md-4 col-xl-3">
            <label class="form-label small mb-1">Entity</label>
            <input
              v-model="filters.entityName"
              class="form-control filter-control"
              placeholder="Ví dụ: Patient, Appointment"
              @keyup.enter="loadLogs(1)"
            />
          </div>

          <div class="col-6 col-md-3 col-xl-2">
            <label class="form-label small mb-1">Action</label>
            <select v-model="filters.action" class="form-select filter-control">
              <option value="">Tất cả</option>
              <option value="Create">Create</option>
              <option value="Update">Update</option>
              <option value="SoftDelete">SoftDelete</option>
              <option value="Delete">Delete</option>
            </select>
          </div>

          <div class="col-6 col-md-3 col-xl-2">
            <label class="form-label small mb-1">Từ ngày</label>
            <input v-model="filters.from" type="date" class="form-control filter-control" />
          </div>

          <div class="col-6 col-md-3 col-xl-2">
            <label class="form-label small mb-1">Đến ngày</label>
            <input v-model="filters.to" type="date" class="form-control filter-control" />
          </div>

          <div class="col-6 col-md-3 col-xl-3 d-flex gap-2 justify-content-end">
            <button class="btn btn-outline-secondary w-100" :disabled="loading" @click="resetFilters">Đặt lại</button>
            <button class="btn btn-primary w-100" :disabled="loading" @click="loadLogs(1)">
              <span v-if="loading" class="spinner-border spinner-border-sm me-1" />Lọc
            </button>
          </div>
        </div>
      </div>
    </div>

    <div class="card border-0 shadow-sm">
      <div class="card-body p-0">
        <div v-if="loading" class="p-4 text-muted">Đang tải dữ liệu...</div>

        <div v-else-if="!logs.length" class="p-4 text-center text-muted">
          Không có bản ghi audit phù hợp với bộ lọc.
        </div>

        <div v-else class="table-responsive">
          <table class="table align-middle mb-0 audit-table">
            <thead>
              <tr>
                <th style="width: 180px">Thời gian</th>
                <th style="width: 120px">Action</th>
                <th style="width: 170px">Entity</th>
                <th>Record ID</th>
                <th style="width: 190px">Người thao tác</th>
                <th style="width: 80px" class="text-center">Chi tiết</th>
              </tr>
            </thead>
            <tbody>
              <template v-for="log in logs" :key="log.id">
                <tr>
                  <td>{{ formatDate(log.changedAt) }}</td>
                  <td>
                    <span class="badge" :class="actionBadgeClass(log.action)">{{ log.action }}</span>
                  </td>
                  <td>
                    <span class="entity-pill">{{ log.entityName }}</span>
                  </td>
                  <td>
                    <code class="record-id">{{ log.recordId }}</code>
                  </td>
                  <td>{{ log.username || log.userId || 'system' }}</td>
                  <td class="text-center">
                    <button class="btn btn-sm btn-outline-primary" @click="toggleDetail(log.id)">
                      {{ expandedId === log.id ? 'Ẩn' : 'Xem' }}
                    </button>
                  </td>
                </tr>

                <tr v-if="expandedId === log.id" class="detail-row">
                  <td colspan="6">
                    <div class="row g-3">
                      <div class="col-12 col-xl-6">
                        <div class="detail-box">
                          <p class="detail-label mb-2">Before</p>
                          <pre class="detail-json mb-0">{{ pretty(log.beforeData) }}</pre>
                        </div>
                      </div>
                      <div class="col-12 col-xl-6">
                        <div class="detail-box">
                          <p class="detail-label mb-2">After</p>
                          <pre class="detail-json mb-0">{{ pretty(log.afterData) }}</pre>
                        </div>
                      </div>
                    </div>
                  </td>
                </tr>
              </template>
            </tbody>
          </table>
        </div>
      </div>

      <div class="card-footer bg-white border-0 pt-0">
        <div class="d-flex flex-wrap justify-content-between align-items-center gap-2">
          <small class="text-muted">Hiển thị {{ fromItem }} - {{ toItem }} / {{ total }}</small>

          <div class="d-flex align-items-center gap-2">
            <button class="btn btn-sm btn-outline-secondary" :disabled="page <= 1 || loading" @click="loadLogs(page - 1)">
              Trước
            </button>
            <span class="small fw-semibold">Trang {{ page }}</span>
            <button class="btn btn-sm btn-outline-secondary" :disabled="page >= totalPages || loading" @click="loadLogs(page + 1)">
              Sau
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import api from '@/services/api'

interface AuditLogItem {
  id: string
  entityName: string
  action: string
  recordId: string
  changedAt: string
  userId?: string | null
  username?: string | null
  beforeData?: string | null
  afterData?: string | null
}

const loading = ref(false)
const logs = ref<AuditLogItem[]>([])
const page = ref(1)
const pageSize = 20
const total = ref(0)
const expandedId = ref<string | null>(null)

const filters = reactive({
  entityName: '',
  action: '',
  from: '',
  to: ''
})

const totalPages = computed(() => Math.max(1, Math.ceil(total.value / pageSize)))
const fromItem = computed(() => (total.value === 0 ? 0 : (page.value - 1) * pageSize + 1))
const toItem = computed(() => Math.min(page.value * pageSize, total.value))

const formatDate = (value: string) => (value ? new Date(value).toLocaleString('vi-VN') : '')

const actionBadgeClass = (action: string) => {
  if (action === 'Create') return 'text-bg-success'
  if (action === 'Update') return 'text-bg-primary'
  if (action === 'SoftDelete' || action === 'Delete') return 'text-bg-danger'
  return 'text-bg-secondary'
}

const pretty = (value: string | null | undefined) => {
  if (!value) return 'null'
  try {
    return JSON.stringify(JSON.parse(value), null, 2)
  } catch {
    return value
  }
}

const buildParams = (targetPage: number) => ({
  page: targetPage,
  pageSize,
  entityName: filters.entityName || undefined,
  action: filters.action || undefined,
  from: filters.from || undefined,
  to: filters.to || undefined
})

const loadLogs = async (targetPage = 1) => {
  try {
    loading.value = true
    expandedId.value = null
    const { data } = await api.get('/AuditLogs', { params: buildParams(targetPage) })
    logs.value = data?.items || []
    total.value = data?.total || 0
    page.value = data?.page || targetPage
  } finally {
    loading.value = false
  }
}

const resetFilters = async () => {
  filters.entityName = ''
  filters.action = ''
  filters.from = ''
  filters.to = ''
  await loadLogs(1)
}

const toggleDetail = (id: string) => {
  expandedId.value = expandedId.value === id ? null : id
}

loadLogs()
</script>

<style scoped>
.audit-page {
  --audit-primary: #1d4ed8;
  --audit-surface: #f8fbff;
}

.audit-hero {
  background: linear-gradient(135deg, #f8fbff 0%, #eef4ff 100%);
}

.audit-eyebrow {
  font-size: 0.72rem;
  font-weight: 700;
  letter-spacing: 0.08em;
  color: #4f46e5;
}

.metric-label {
  color: #64748b;
  font-size: 0.8rem;
}

.metric-value {
  color: #0f172a;
  font-size: 1.6rem;
  font-weight: 700;
  line-height: 1;
}

.metric-sub {
  color: #64748b;
  font-size: 0.8rem;
}

.audit-table thead th {
  background: #f8fafc;
  color: #475569;
  border-bottom: 1px solid #e2e8f0;
  font-weight: 600;
  font-size: 0.84rem;
  white-space: nowrap;
}

.audit-table tbody td {
  border-color: #edf2f7;
}

.filter-control {
  min-height: calc(1.5em + 0.75rem + 2px);
  line-height: 1.5;
}

.form-select.filter-control {
  padding-top: 0.375rem;
  padding-bottom: 0.375rem;
}

.entity-pill {
  display: inline-flex;
  align-items: center;
  padding: 0.15rem 0.6rem;
  border-radius: 999px;
  background: #eef2ff;
  color: #3730a3;
  font-size: 0.78rem;
  font-weight: 600;
}

.record-id {
  font-size: 0.78rem;
  color: #0f172a;
  word-break: break-all;
}

.detail-row td {
  background: var(--audit-surface);
}

.detail-box {
  border: 1px solid #e2e8f0;
  border-radius: 0.75rem;
  background: #ffffff;
  padding: 0.75rem;
}

.detail-label {
  font-size: 0.78rem;
  font-weight: 700;
  color: #475569;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.detail-json {
  max-height: 240px;
  overflow: auto;
  background: #0f172a;
  color: #e2e8f0;
  border-radius: 0.5rem;
  padding: 0.75rem;
  font-size: 0.77rem;
  line-height: 1.45;
}

@media (max-width: 992px) {
  .audit-metrics {
    text-align: left !important;
  }
}
</style>
