# Core Workflows

## 1) Đặt lịch online (Public)

1. Người dùng nhập email -> gửi OTP  
   `POST /api/email/send-otp`
2. Xác thực OTP  
   `POST /api/email/verify-otp`
3. Tạo lịch hẹn  
   `POST /api/appointments`
4. Hệ thống trả `appointmentCode` để tra cứu
5. Người dùng tra cứu lịch  
   `GET /api/appointments/{code}`

## 2) Walk-in tại quầy (Staff)

1. Staff tạo lịch trực tiếp  
   `POST /api/staff/staffappointments/walk-in`
2. Có thể gán bác sĩ ngay hoặc để pending
3. Khi bệnh nhân đến:
   - check-in nghiệp vụ staff  
     `POST /api/staff/staffappointments/checkin`
   - hoặc check-in vào queue theo phòng  
     `POST /api/roomqueues/check-in`

## 3) Quy trình khám bác sĩ

1. Bác sĩ mở danh sách lịch  
   `GET /api/doctor/doctorappointments`
2. Gọi bệnh nhân từ queue  
   `POST /api/roomqueues/rooms/{roomId}/next`
3. Lưu hồ sơ khám  
   `POST /api/medical-record` hoặc `POST /api/medical-record/examination`
4. (Nếu cần) chỉ định cận lâm sàng  
   `POST /api/clinicaltests`
5. Kê đơn thuốc  
   `POST /api/prescription`
6. Hoàn tất khám  
   `PATCH /api/doctor/doctorappointments/{id}/complete`
7. Đánh dấu queue done  
   `POST /api/roomqueues/{queueId}/done`

## 4) Cận lâm sàng (Technician/Doctor)

1. Tạo chỉ định test từ bác sĩ
2. Kỹ thuật viên bắt đầu test  
   `PATCH /api/clinicaltests/{id}/start`
3. Trả kết quả  
   `PATCH /api/clinicaltests/{id}/result`
4. Bác sĩ xem lại trong hồ sơ khám

## 5) Hóa đơn và thanh toán (Cashier/Admin)

1. Tạo hóa đơn khám  
   `POST /api/invoicemanagement`
2. Nếu có đơn thuốc, tạo hóa đơn thuốc  
   `POST /api/invoicemanagement/drug`
3. Thu tiền / ghi nhận thanh toán  
   `PUT /api/invoicemanagement/{id}/pay` hoặc `POST /api/payment`
4. Có thể dùng PayOS:
   - tạo payment link: `POST /api/payos/create`
   - nhận callback: `POST /api/payos/webhook`

## 6) Quản lý đổi ca bác sĩ

1. Doctor tạo đơn đổi/nhường ca  
   `POST /api/doctor/shift-requests`
2. Admin xem danh sách pending  
   `GET /api/admin/shift-requests`
3. Admin duyệt/từ chối:
   - `POST /api/admin/shift-requests/{id}/approve`
   - `POST /api/admin/shift-requests/{id}/reject`
4. Doctor nhận notification trong hệ thống

## 7) Các điểm cần kiểm thử E2E

- Đặt lịch online + OTP + email confirm
- Walk-in không tạo trùng slot cho cùng bệnh nhân
- Queue không gọi nhầm bệnh nhân/nhầm bác sĩ
- Hoàn tất khám chỉ sau khi có hồ sơ khám
- Hóa đơn + payment + trạng thái paid khớp dữ liệu
