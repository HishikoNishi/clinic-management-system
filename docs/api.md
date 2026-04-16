# Clinic Management API

## Tổng quan

- Base URL: `http://<host>/api`
- Swagger (môi trường Development): `/swagger`
- Authentication: JWT Bearer token (`Authorization: Bearer <token>`)
- Role chính trong hệ thống: `Admin`, `Doctor`, `Staff`, `Cashier`, `Technician`, `Guest`

## Nhóm Auth & Security

- `POST /api/auth/login` - Đăng nhập bằng username/password
- `POST /api/auth/login-pin` - Đăng nhập nhanh (PIN)
- `POST /api/auth/refresh` - Làm mới access token bằng refresh token
- `GET /api/secure/public` - Endpoint public
- `GET /api/secure/authenticated` - Endpoint yêu cầu đăng nhập
- `GET /api/secure/admin` - Endpoint chỉ `Admin`

## Nhóm User/Admin

- `GET /api/admin` - Lấy danh sách user (Admin)
- `GET /api/admin/{id}` - Lấy chi tiết user
- `POST /api/admin` - Tạo user mới
- `PUT /api/admin/{id}` - Cập nhật user
- `DELETE /api/admin/{id}` - Xóa user
- `POST /api/admin/{id}/roles/{roleName}` - Gán role cho user
- `GET /api/admin/roles` - Lấy danh sách role
- `GET /api/admin/admindashboard/stats` - Thống kê dashboard
- `GET /api/admin/admindashboard/revenue` - Thống kê doanh thu

## Nhóm Bác sĩ (Doctor)

- `GET /api/doctor/departments` - Lấy khoa/phòng ban phục vụ màn hình doctor
- `GET /api/doctor` - Danh sách bác sĩ
- `GET /api/doctor/{id}` - Chi tiết bác sĩ
- `POST /api/doctor` - Tạo bác sĩ mới (Admin)
- `PUT /api/doctor/{id}` - Cập nhật hồ sơ bác sĩ
- `PUT /api/doctor/{id}/status` - Cập nhật trạng thái bác sĩ
- `DELETE /api/doctor/{id}` - Xóa bác sĩ
- `GET /api/doctor/by-department/{departmentId}` - Lấy bác sĩ theo khoa
- `GET /api/doctor/departments/{departmentId}/specialties` - Chuyên khoa theo khoa
- `GET /api/doctor/by-specialty/{specialtyId}` - Lấy bác sĩ theo chuyên khoa
- `GET /api/doctor/{id}/appointments` - Lịch hẹn theo bác sĩ
- `GET /api/doctor/profile` - Hồ sơ bác sĩ đang đăng nhập
- `PUT /api/doctor/profile` - Cập nhật hồ sơ bác sĩ đang đăng nhập
- `POST /api/doctor/{id}/avatar` - Upload avatar bác sĩ

## Nhóm Lịch bác sĩ (Doctor Schedules)

- `GET /api/doctorschedules/doctors/{doctorId}/weekly-template`
- `PUT /api/doctorschedules/doctors/{doctorId}/weekly-template`
- `GET /api/doctorschedules/doctors/{doctorId}`
- `PUT /api/doctorschedules/doctors/{doctorId}/day`
- `GET /api/doctorschedules/doctors/{doctorId}/work-summary`
- `GET /api/doctorschedules/doctors/{doctorId}/slot-impact`
- `GET /api/doctorschedules/available-doctors`
- `GET /api/doctorschedules/doctors/{doctorId}/available-slots`
- `POST /api/doctorschedules/reassign-slot`
- `DELETE /api/doctorschedules/{id}`
- `DELETE /api/doctorschedules/doctors/{doctorId}/day-override`

## Nhóm Đơn xin đổi/nhường ca

### Doctor Shift Requests

- `GET /api/doctor/shift-requests/my` - Danh sách đơn của bác sĩ hiện tại
- `GET /api/doctor/shift-requests/pending-count` - Số đơn chờ duyệt
- `POST /api/doctor/shift-requests` - Tạo đơn đổi/nhường ca
- `GET /api/doctor/notifications` - Danh sách thông báo bác sĩ
- `POST /api/doctor/notifications/{id}/read` - Đánh dấu đã đọc

### Admin Shift Requests

- `GET /api/admin/shift-requests` - Danh sách đơn
- `GET /api/admin/shift-requests/pending-count` - Số đơn pending
- `GET /api/admin/shift-requests/{id}` - Chi tiết đơn
- `POST /api/admin/shift-requests/{id}/approve` - Duyệt đơn
- `POST /api/admin/shift-requests/{id}/reject` - Từ chối đơn

## Nhóm Staff

- `GET /api/staffs` - Danh sách nhân viên
- `GET /api/staffs/{id}` - Chi tiết nhân viên
- `POST /api/staffs` - Tạo nhân viên
- `PUT /api/staffs/{id}` - Cập nhật nhân viên
- `DELETE /api/staffs/{id}` - Xóa nhân viên
- `GET /api/staffs/profile` - Hồ sơ staff hiện tại
- `PUT /api/staffs/profile` - Cập nhật hồ sơ staff hiện tại
- `POST /api/staffs/{id}/avatar` - Upload avatar staff

## Nhóm Patient

- `GET /api/patients` - Danh sách bệnh nhân
- `GET /api/patients/{id}` - Chi tiết bệnh nhân
- `GET /api/patients/my` - Thông tin bệnh nhân theo tài khoản hiện tại
- `PUT /api/patients/{id}` - Cập nhật bệnh nhân

## Nhóm Appointment

### Public/General Appointment

- `POST /api/appointments` - Tạo lịch hẹn
- `GET /api/appointments/patient-lookup` - Tra cứu bệnh nhân
- `GET /api/appointments/{code}` - Tra cứu lịch theo mã
- `POST /api/appointments/search` - Tìm kiếm lịch hẹn
- `POST /api/appointments/cancel` - Hủy lịch
- `GET /api/appointments/no-show` - Danh sách no-show
- `GET /api/appointments/by-status` - Lọc theo trạng thái

### Staff Appointments

- `POST /api/staff/staffappointments/walk-in` - Tạo lịch khám walk-in
- `GET /api/staff/staffappointments` - Danh sách lịch cho staff
- `GET /api/staff/staffappointments/filter` - Lọc lịch
- `GET /api/staff/staffappointments/specialties-by-department/{departmentId}`
- `GET /api/staff/staffappointments/by-specialty/{specialtyId}`
- `POST /api/staff/staffappointments/assign-doctor` - Gán bác sĩ
- `POST /api/staff/staffappointments/checkin` - Check-in
- `GET /api/staff/staffappointments/{id}` - Chi tiết lịch

### Doctor Appointments

- `GET /api/doctor/doctorappointments` - Lịch của bác sĩ
- `GET /api/doctor/doctorappointments/{id}` - Chi tiết lịch
- `GET /api/doctor/doctorappointments/{id}/examination` - Dữ liệu khám
- `PATCH /api/doctor/doctorappointments/{id}/complete` - Hoàn tất khám

## Nhóm Queue/Phòng khám

- `GET /api/roomqueues/rooms` - Danh sách phòng + queue
- `GET /api/roomqueues/rooms/{roomId}` - Queue chi tiết 1 phòng
- `GET /api/roomqueues/doctor/rooms` - Phòng theo bác sĩ
- `POST /api/roomqueues/check-in` - Check-in vào hàng đợi
- `POST /api/roomqueues/rooms/{roomId}/next` - Gọi số tiếp theo
- `POST /api/roomqueues/{queueId}/done` - Hoàn thành lượt
- `POST /api/roomqueues/{queueId}/skip` - Bỏ qua lượt
- `GET /api/roomqueues/patient/{appointmentCode}` - Trạng thái queue theo mã lịch

## Nhóm Medical Record / Prescription / Clinical Test

- `POST /api/medical-record` - Tạo hồ sơ bệnh án
- `POST /api/medical-record/examination` - Lưu kết quả khám chi tiết
- `GET /api/medical-record/patient/{patientId}` - Lịch sử bệnh án theo bệnh nhân
- `GET /api/medical-record/{recordId}` - Chi tiết bệnh án

- `POST /api/prescription` - Tạo đơn thuốc
- `GET /api/prescription` - Danh sách đơn thuốc
- `GET /api/prescription/{id}` - Chi tiết đơn thuốc
- `GET /api/prescription/by-appointment/{appointmentId}` - Đơn thuốc theo lịch hẹn
- `PUT /api/prescription/{id}` - Cập nhật đơn thuốc
- `DELETE /api/prescription/{id}` - Xóa đơn thuốc

- `POST /api/clinicaltests` - Tạo chỉ định cận lâm sàng
- `PATCH /api/clinicaltests/{id}/start` - Bắt đầu thực hiện
- `PATCH /api/clinicaltests/{id}/result` - Trả kết quả
- `GET /api/clinicaltests` - Danh sách cận lâm sàng
- `GET /api/clinicaltests/medical-record/{medicalRecordId}` - Theo bệnh án
- `GET /api/clinicaltests/pending-patients` - BN đang chờ CLS
- `GET /api/clinicaltests/pending-patients/by-department` - BN chờ theo khoa

## Nhóm Billing / Invoice / Payment

- `POST /api/invoicemanagement` - Tạo hóa đơn khám
- `GET /api/invoicemanagement/{id}` - Chi tiết hóa đơn
- `GET /api/invoicemanagement/list` - Danh sách hóa đơn
- `PUT /api/invoicemanagement/{id}/pay` - Ghi nhận thanh toán hóa đơn
- `POST /api/invoicemanagement/recalculate` - Tính lại giá trị hóa đơn
- `POST /api/invoicemanagement/drug` - Tạo hóa đơn thuốc
- `GET /api/invoicemanagement/drug/by-prescription/{id}` - Lấy hóa đơn thuốc theo đơn

- `POST /api/payment` - Tạo payment (cashier/admin)
- `GET /api/insurance/validate` - Kiểm tra thẻ/bảo hiểm

## Nhóm PayOS

- `POST /api/payos/create` - Tạo link thanh toán PayOS
- `POST /api/payos/webhook` - Webhook callback từ PayOS
- `GET /api/payos/webhook` - Endpoint test/verify webhook

## Nhóm Danh mục

- `GET /api/departments`
- `GET /api/departments/{id}`
- `POST /api/departments` (Admin)
- `PUT /api/departments/{id}` (Admin)
- `DELETE /api/departments/{id}` (Admin)

- `GET /api/specialties` (Admin)
- `GET /api/specialties/{id}` (Admin)
- `POST /api/specialties` (Admin)
- `PUT /api/specialties/{id}` (Admin)
- `DELETE /api/specialties/{id}` (Admin)

- `GET /api/medicines`
- `GET /api/medicines/suggest`
- `POST /api/medicines` (Admin)
- `PUT /api/medicines/{id}` (Admin)
- `PATCH /api/medicines/{id}/toggle` (Admin)
- `DELETE /api/medicines/{id}` (Admin)

## Nhóm Email OTP

- `POST /api/email/send-otp` - Gửi OTP email
- `POST /api/email/verify-otp` - Xác thực OTP

## Ghi chú

- Các endpoint có role cụ thể được enforce bằng `[Authorize(Roles = ...)]` trong controller.
- Enum trong response/request đang được serialize dạng string (đã bật `JsonStringEnumConverter`).
- Danh sách trên phản ánh đúng route từ source code hiện tại; chi tiết DTO request/response nên tham chiếu thêm Swagger.
