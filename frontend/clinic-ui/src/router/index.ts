import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/stores/auth.ts'

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    redirect: '/login'
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
    path: '/appointment',
    name: 'Appointment',
    component: () => import('@/views/AppointmentView.vue')
  },
  {
    path: '/appointmentdetail',
    name: 'AppointmentDetail',
    component: () => import('@/views/AppointmentDetail.vue')

  },
  {
  path: '/staff',
  name: 'StaffDashboard',
  component: () => import('@/views/StaffDashboard.vue'),
  meta: {
    requiresAuth: true,
    role: 'Staff'

 
  }
}
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to) => {
  const authStore = useAuthStore()

  // ✅ check login
  if (to.meta.requiresAuth) {
    if (!authStore.token || authStore.isTokenExpired()) {
      authStore.logout()
      return { name: 'Login' }
    }
  }

  // ✅ check ROLE
  if (to.meta.role) {
if (authStore.role !== to.meta.role) {
      return { name: 'Dashboard' }
    }
  }
})

export default router
