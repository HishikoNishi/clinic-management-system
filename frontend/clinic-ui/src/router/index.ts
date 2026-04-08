import { createRouter, createWebHistory, type RouteRecordRaw } from "vue-router"
import { useAuthStore } from "@/stores/auth"
import path from "path/win32"

const routes: RouteRecordRaw[] = [
  {
    path: "/",
    redirect: "/home"
  },

  {
    path: "/home",
    name: "GuestDashboard",
    component: () => import("@/views/GuestDashboard.vue")
  },

{
  path: "/login",
  name: "Login",
  component: () => import("@/views/Login.vue"),
  beforeEnter: () => {
    const authStore = useAuthStore()
    const pinOk = localStorage.getItem('pinAuthOk') === 'true'
    if (authStore.token) return true
    if (!pinOk) return "/home"
    return true
  }
},

  {
    path: "/dashboard",
    name: "Dashboard",
    component: () => import("@/views/admin/AdminDashboard.vue"),
    meta: { layout: "dashboard", requiresAuth: true, role: "Admin" }
  },

  {
    path: "/admin/users/create",
    name: "CreateUser",
    component: () => import("@/views/admin/CreateUserView.vue"),
    meta: { layout: "dashboard", requiresAuth: true, role: "Admin" }
  },

  {
    path: "/appointment",
    name: "Appointment",
    component: () => import("@/views/AppointmentView.vue"),
    meta: { layout: "dashboard", requiresAuth: true, role: ["Admin", "Staff", "Doctor"] }
  },

  {
    path: "/appointmentdetail",
    name: "AppointmentDetail",
    component: () => import("@/views/AppointmentDetail.vue")
  },

  {
    path: "/staff/appointments",
    name: "StaffAppointment",
    component: () => import("@/views/staff/StaffAppointment.vue"),
    meta: { layout: "dashboard", requiresAuth: true, role: ["Staff", "Admin"] }
  },

  {
    path: "/staff/appointments/:id",
    name: "StaffAppointmentDetail",
    component: () => import("@/views/staff/StaffAppointmentDetail.vue"),
    meta: { layout: "dashboard", requiresAuth: true, role: ["Staff", "Admin"] }
  },

  {
    path: "/doctor/appointments",
    name: "DoctorAppointment",
    component: () => import("@/views/doctor/DoctorAppointments.vue"),
    meta: { layout: "doctor", requiresAuth: true, role: "Doctor" }
  },

  // Backward compatibility for older doctor path
  {
    path: "/doctorappointment",
    redirect: "/doctor/appointments"
  },

  {
    path: "/doctor/patients",
    name: "DoctorPatients",
    component: () => import("@/views/doctor/DoctorPatients.vue"),
    meta: { layout: "doctor", requiresAuth: true, role: "Doctor" }
  },
  {
    path: "/doctor/patients/:id",
    name: "DoctorPatientDetail",
    component: () => import("@/views/doctor/DoctorPatientDetail.vue"),
    meta: { layout: "doctor", requiresAuth: true, role: "Doctor" }
  },

  {
    path: "/doctor/examination/:id",
    name: "DoctorExamination",
    component: () => import("@/views/doctor/DoctorExamination.vue"),
    meta: { layout: "doctor", requiresAuth: true, role: "Doctor" }
  },

  {
    path: "/doctor/appointments/:id",
    name: "DoctorAppointmentDetail",
    component: () => import("@/views/doctor/DoctorAppointmentDetail.vue"),
    meta: { layout: "doctor", requiresAuth: true, role: "Doctor" }
  },

  {
    path: "/doctor/profile",
    name: "DoctorProfile",
    component: () => import("@/views/doctor/DoctorProfile.vue"),
    meta: { layout: "doctor", requiresAuth: true, role: "Doctor" }
  },

  {
    path: "/doctors",
    name: "Doctors",
    component: () => import("@/views/admin/DoctorList.vue"),
    meta: { layout: "dashboard", requiresAuth: true, role: "Admin" }
  },
  {
    path: "/departments",
    name: "Departments",
    component: () => import("@/views/admin/DepartmentList.vue"),
    meta: { layout: "dashboard", requiresAuth: true, role: "Admin" }
  },
  {
    path: "/specialties",
    name: "Specialties",
    component: () => import("@/views/admin/SpecialtyList.vue"),
    meta: { layout: "dashboard", requiresAuth: true, role: "Admin" }
  },
  {
    path: '/doctors/:id',
    name: 'DoctorDetail',
    component: () => import('@/views/admin/DoctorDetail.vue'),
    meta: { layout: "dashboard", requiresAuth: true, role: "Admin" }
  },

{
    path: "/staff/profile",
    name: "StaffProfile",
    component: () => import("@/views/staff/StaffProfile.vue"),
    meta: { layout: "dashboard", requiresAuth: true, role: "Staff" }
  },

  {
    path: "/staff",
    name: "StaffList",
    component: () => import("@/views/admin/StaffList.vue"),
    meta: { layout: "dashboard", requiresAuth: true, role: "Admin" }
  },

  {
    path: "/staff/create",
    name: "CreateStaff",
    component: () => import("@/views/admin/StaffForm.vue"),
    meta: { layout: "dashboard", requiresAuth: true, role: "Admin" }
  },

  {
    path: "/staff/edit/:id",
    name: "EditStaff",
    component: () => import("@/views/admin/StaffForm.vue"),
    meta: { layout: "dashboard", requiresAuth: true, role: "Admin" }
  },

  {
    path: "/technician/tests",
    name: "TechnicianTests",
    component: () => import("@/views/technician/TechnicianTests.vue"),
    meta: { layout: "dashboard", requiresAuth: true, role: "Technician" }
  },
  {
    path: "/technician/tests/history",
    name: "TechnicianTestsHistory",
    component: () => import("@/views/technician/TechnicianTests.vue"),
    meta: { layout: "dashboard", requiresAuth: true, role: "Technician" }
  },
  {
    path: "/cashier/invoices",
    name: "CashierInvoices",
    component: () => import("@/views/cashier/CashierInvoices.vue"),
    meta: { layout: "cashier", requiresAuth: true, role: "Cashier" }
  },
  {
    path: "/cashier/invoices/list",
    name: "CashierInvoiceList",
    component: () => import("@/views/cashier/CashierInvoiceList.vue"),
    meta: { layout: "cashier", requiresAuth: true, role: "Cashier" }
  },
  {
    path: "/cashier/drug-invoices",
    name: "CashierDrugInvoices",
    component: () => import("@/views/cashier/CashierDrugInvoices.vue"),
    meta: { layout: "cashier", requiresAuth: true, role: "Cashier" }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

const roleFallback: Record<string, string> = {
  Admin: "/dashboard",
  Staff: "/staff/appointments",
  Doctor: "/doctor/appointments",
  Technician: "/technician/tests",
  Cashier: "/cashier/invoices",
}

router.beforeEach((to) => {
  const authStore = useAuthStore()
  const pinOk = localStorage.getItem('pinAuthOk') === 'true'

  if ((to.name === 'Login' || to.path === '/login') && !pinOk && !authStore.token) {
    return { path: '/home' }
  }

  if (to.meta.requiresAuth) {
    if (!authStore.token) {
      authStore.logout()
      return { name: "Login" }
    }
  }

 
  if (to.meta.role) {
    const roles = (Array.isArray(to.meta.role) ? to.meta.role : [to.meta.role]).map(r => String(r).toLowerCase())
    const userRole = (authStore.role || "").toLowerCase()

    if (!roles.includes(userRole)) {
      const fallback = authStore.role
        ? roleFallback[authStore.role]
        : "/home"

      return { path: fallback }
    }
  }

  return true
})

export default router
