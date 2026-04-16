# Roles and Permissions

## Vai trò hiện có

- `Admin`
- `Doctor`
- `Staff`
- `Cashier`
- `Technician`
- `Guest`

## Ma trận quyền nghiệp vụ (tóm tắt)

## Admin

- Quản trị user/role: `api/admin/*`
- Quản lý danh mục:
  - `api/departments/*`
  - `api/specialties/*`
  - `api/medicines/*`
- Quản lý bác sĩ/nhân viên:
  - `api/doctor/*` (create/update/delete)
  - `api/staffs/*`
- Duyệt đổi ca:
  - `api/admin/shift-requests/*`
- Dashboard:
  - `api/admin/admindashboard/*`

## Doctor

- Xem/cập nhật profile doctor:
  - `GET/PUT /api/doctor/profile`
- Quản lý lịch khám cá nhân:
  - `api/doctor/doctorappointments/*`
- Medical record / prescription / clinical test:
  - `api/medical-record/*`
  - `api/prescription/*`
  - `api/clinicaltests/*` (tuỳ endpoint)
- Queue tại phòng bác sĩ:
  - `GET /api/roomqueues/doctor/rooms`
  - `POST /api/roomqueues/rooms/{roomId}/next`
  - `POST /api/roomqueues/{queueId}/done|skip`
- Đơn đổi ca:
  - `api/doctor/shift-requests/*`

## Staff

- Quản lý lịch quầy/lễ tân:
  - `api/staff/staffappointments/*`
- Quản lý check-in queue:
  - `POST /api/roomqueues/check-in`
  - `GET /api/roomqueues/rooms*`
- Quản lý bệnh nhân:
  - `GET/PUT /api/patients/*` (theo policy trong controller)
- Cập nhật profile staff:
  - `GET/PUT /api/staffs/profile`

## Cashier

- Hóa đơn/thanh toán:
  - `api/invoicemanagement/*`
  - `api/payment`
  - `api/insurance/validate`
  - `api/payos/*`

## Technician

- Theo seed role đã có trong hệ thống.
- Luồng cận lâm sàng hiện đang đi qua `ClinicalTestsController`; nếu phân quyền chi tiết cho technician cần bổ sung thêm `[Authorize(Roles=...)]` theo endpoint thực tế.

## Guest

- Vai trò mặc định thấp nhất.
- Chủ yếu cho các endpoint public không cần token.

## Endpoint Public quan trọng

- `POST /api/email/send-otp`
- `POST /api/email/verify-otp`
- `POST /api/appointments`
- `GET /api/appointments/patient-lookup`
- `GET /api/appointments/{code}`
- `POST /api/appointments/search`
- `POST /api/appointments/cancel`
- `GET /api/roomqueues/patient/{appointmentCode}`

## Lưu ý cho FE authorization

- FE nên ẩn menu theo role claim từ token, nhưng backend mới là nguồn quyết định cuối.
- Khi backend trả `401/403`, FE cần điều hướng:
  - `401`: thử refresh token -> fail thì về login
  - `403`: hiển thị “không có quyền truy cập”
