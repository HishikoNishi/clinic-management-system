import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    redirect: '/login' // 🔥 mặc định về login
  },
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/Login.vue')
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: () => import('@/views/Dashboard.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/doctors',
    name: 'Doctors',
    component: () => import('@/views/DoctorList.vue'),
    meta: { requiresAuth: true }
  },
  {
  path: '/medical-records',
  component: () => import('@/views/MedicalRecord.vue')
}
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to) => {
  const authStore = useAuthStore()

  // Nếu route cần đăng nhập mà chưa có token
  if (to.meta.requiresAuth && !authStore.token) {
    return { name: 'Login' }
  }

  // Nếu đã login mà cố vào login lại
  if (to.name === 'Login' && authStore.token) {
    return { name: 'Dashboard' }
  }
})

export default router