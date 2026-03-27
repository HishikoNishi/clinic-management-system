import { createRouter, createWebHistory, type RouteRecordRaw } from "vue-router"
import { useAuthStore } from "@/stores/auth"

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
    component: () => import("@/views/Login.vue")
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
    component: () => import("@/views/AppointmentView.vue")
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
    meta: { layout: "dashboard", requiresAuth: true, role: "Staff" }
  },

  {
    path: "/staff/appointments/:id",
    name: "StaffAppointmentDetail",
    component: () => import("@/views/staff/StaffAppointmentDetail.vue"),
    meta: { layout: "dashboard", requiresAuth: true, role: "Staff" }
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
    path: "/doctors",
    name: "Doctors",
    component: () => import("@/views/admin/DoctorList.vue"),
    meta: { layout: "dashboard", requiresAuth: true, role: "Admin" }
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
    path: "/cashier/invoices",
    name: "CashierInvoices",
    component: () => import("@/views/cashier/CashierInvoices.vue"),
    meta: { layout: "cashier", requiresAuth: true, role: "Cashier" }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to) => {
  const authStore = useAuthStore()

  const roleFallback: Record<string, string> = {
    Admin: "/dashboard",
    Staff: "/staff/appointments",
    Doctor: "/doctor/appointments",
    Cashier: "/cashier/invoices"
  }


  if (to.meta.requiresAuth) {
    if (!authStore.token) {
      authStore.logout()
      return { name: "Login" }
    }
  }

 
  if (to.meta.role) {
    const roles = Array.isArray(to.meta.role)
      ? to.meta.role
      : [to.meta.role]

    if (!roles.includes(authStore.role || "")) {
      const fallback = authStore.role
        ? roleFallback[authStore.role]
        : "/home"

      return { path: fallback }
    }
  }

  return true
})

export default router
