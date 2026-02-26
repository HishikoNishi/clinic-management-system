import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const routes: RouteRecordRaw[] = [
  { path: '/', redirect: '/login' },

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

  // ✅ STAFF AREA
  {
    path: '/staff/appointments',
    name: 'StaffAppointment',
    component: () => import('@/views/staff/StaffAppointment.vue'),
    meta: { requiresAuth: true, role: 'Staff' }
  },  
  {
    path: '/staff/appointments/:id',
    name: 'StaffAppointmentDetail',
    component: () => import('@/views/staff/StaffAppointmentDetail.vue'),
    meta: { requiresAuth: true, role: 'Staff' }
  },

  {
    path: '/patients',
    name: 'PatientPage',
    component: () => import('@/views/PatientPage.vue'),
    meta: { requiresAuth: true, role: 'Staff' }
  },

  {
    path: '/doctors',
    name: 'DoctorPage',
    component: () => import('@/views/DoctorPage.vue'),
    meta: { requiresAuth: true, role: 'Staff' }
  },

  {
    path: '/doctorappointment',
    name: 'DoctorAppointment',
    component: () => import('@/views/DoctorAppointments.vue'),
    meta: { requiresAuth: true, role: 'Doctor' }
  },
  {
  path: '/doctor/appointments/:id',
  name: 'DoctorAppointmentDetail',
  component: () => import('@/views/DoctorAppointmentDetail.vue'),
  meta: { requiresRole: 'Doctor' }
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

  // ✅ check role
  if (to.meta.role) {
    if (authStore.role !== to.meta.role) {
      return { name: 'Dashboard' }
    }
  }
})

export default router