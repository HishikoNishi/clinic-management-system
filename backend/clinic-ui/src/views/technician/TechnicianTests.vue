<template>
  <div class="tech-tests">
    <div class="card shadow-sm">
      <div class="card-header d-flex justify-content-between align-items-center">
        <div>
          <h5 class="mb-0">Danh sách xét nghiệm</h5>
          <small class="text-muted">Các yêu cầu bác sĩ gửi xuống cho kỹ thuật viên</small>
        </div>
        <button class="btn btn-outline-secondary btn-sm" @click="loadTests">
          <i class="bi bi-arrow-clockwise me-1"></i> Làm mới
        </button>
      </div>

      <div class="card-body p-0">
        <div v-if="tests.length === 0" class="p-3 text-center text-muted">
          Chưa có yêu cầu xét nghiệm.
        </div>

        <div v-else class="table-responsive">
          <table class="table table-striped align-middle mb-0">
            <thead class="table-light">
              <tr>
                <th>Tên xét nghiệm</th>
                <th>Medical Record</th>
                <th>Trạng thái</th>
                <th style="width: 26%;">Kết quả</th>
                <th style="width: 18%;">Kỹ thuật viên</th>
                <th style="width: 90px;"></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="t in tests" :key="t.id">
                <td class="fw-semibold">{{ t.testName }}</td>
                <td class="text-monospace small">{{ t.medicalRecordId }}</td>
                <td>
                  <span :class="['badge', t.status === 'Completed' ? 'bg-success-subtle text-success' : 'bg-warning-subtle text-warning']">
                    {{ t.status === 'Completed' ? 'Đã có kết quả' : 'Chờ thực hiện' }}
                  </span>
                </td>
                <td>
                  <input
                    v-model="t.result"
                    class="form-control form-control-sm"
                    :disabled="t.status === 'Completed'"
                    placeholder="Nhập kết quả..."
                  />
                </td>
                <td>
                  <input
                    v-model="t.technicianName"
                    class="form-control form-control-sm"
                    :disabled="t.status === 'Completed'"
                    placeholder="Tên KTV..."
                  />
                </td>
                <td class="text-end">
                  <button
                    v-if="t.status !== 'Completed'"
                    class="btn btn-primary btn-sm"
                    @click.stop="updateResult(t)"
                  >
                    <i class="bi bi-save me-1"></i>Lưu
                  </button>
                  <span v-else class="text-muted small">Đã lưu</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue"
import api from "@/services/api"

const tests = ref<any[]>([])

const loadTests = async () => {
  try {
    const res = await api.get("/ClinicalTests")
    const normalized = res.data.map((t: any) => ({
      ...t,
      status: t.status || (t.result ? "Completed" : "Pending")
    }))
    tests.value = normalized.sort((a: any, b: any) => {
      if (a.status === b.status) return 0
      return a.status === "Pending" ? -1 : 1
    })
  } catch (err) {
    console.log(err)
  }
}

const updateResult = async (test: any) => {
  if (!test.result || !test.technicianName) {
    alert("Nhập đầy đủ thông tin")
    return
  }

  await api.patch(`/ClinicalTests/${test.id}/result`, {
    result: test.result,
    technicianName: test.technicianName
  })

  alert("Đã lưu kết quả")
  loadTests()
}

onMounted(() => {
  loadTests()
})
</script>

<style src="@/styles/layouts/technician-tests.css"></style>
