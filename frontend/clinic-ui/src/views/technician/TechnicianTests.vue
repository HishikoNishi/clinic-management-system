<template>
  <div class="tech-tests">
    <h2>Danh sách xét nghiệm</h2>

    <table class="test-table">
      <thead>
        <tr>
          <th>Test</th>
          <th>Medical Record ID</th>
          <th>Result</th>
          <th>Technician</th>
          <th></th>
        </tr>
      </thead>

      <tbody>
        <tr v-for="t in tests" :key="t.id">

          <td>{{ t.testName }}</td>

          <td>{{ t.medicalRecordId }}</td>

          <td>
            <input v-model="t.result" placeholder="Nhập kết quả..." />
          </td>

          <td>
            <input v-model="t.technicianName" placeholder="Tên KTV..." />
          </td>

          <td>
            <button @click="updateResult(t)">
              Lưu
            </button>
          </td>

        </tr>
      </tbody>
    </table>

    <p v-if="!tests.length">Không có xét nghiệm nào.</p>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue"
import api from "@/services/api"

const tests = ref<any[]>([])

/* LOAD TEST CHƯA CÓ RESULT */

const loadTests = async () => {
  try {

    const res = await api.get("/ClinicalTests")

    // chỉ lấy test chưa có kết quả
    tests.value = res.data.filter((t: any) => !t.result)

  } catch (err) {
    console.log(err)
  }
}

/* UPDATE RESULT */

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

<style scoped>
.tech-tests {
  padding: 20px;
}

.test-table {
  width: 100%;
  border-collapse: collapse;
}

.test-table th,
.test-table td {
  border: 1px solid #ccc;
  padding: 8px;
}
</style>