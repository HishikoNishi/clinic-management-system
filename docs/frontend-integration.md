# Frontend Integration Guide

## Mục tiêu

Tài liệu này hướng dẫn FE tích hợp API theo code hiện tại, tập trung vào:

- Auth + refresh token
- Chuẩn gọi API và xử lý lỗi
- Mapping trạng thái nghiệp vụ quan trọng
- Các flow màn hình chính

## 1) API Client chuẩn

- Base URL: `http://<host>/api`
- Header mặc định:
  - `Content-Type: application/json`
  - `Authorization: Bearer <accessToken>` (trừ endpoint public)

Gợi ý tách 2 client:

- `publicClient`: không gắn token
- `authClient`: auto gắn token + interceptor refresh

## 2) Auth Flow trên FE

- Login:
  - `POST /api/auth/login`
  - lưu `token`, `refreshToken`, `expiresAt`, `refreshExpiresAt`, `role`
- Refresh:
  - `POST /api/auth/refresh`
  - trigger khi `401` hoặc trước khi token hết hạn
- Logout:
  - clear toàn bộ auth state ở FE

Khuyến nghị:

- Chỉ retry request 1 lần sau refresh để tránh loop vô hạn.
- Nếu refresh fail, chuyển về màn hình login ngay.

## 3) Chuẩn lỗi API (đã áp dụng ở các controller chính)

Một số endpoint trả lỗi theo format:

```json
{
  "code": "bad_request",
  "message": "Human readable message",
  "details": {},
  "timestamp": "2026-04-16T12:34:56Z"
}
```

FE nên parse theo thứ tự ưu tiên:

1. `message`
2. fallback message theo `code`
3. fallback mặc định: `Đã có lỗi xảy ra`

## 4) Enum/Status FE cần map

- `AppointmentStatus`: `Pending`, `Confirmed`, `CheckedIn`, `Completed`, `Cancelled`, `NoShow`
- `DoctorStatus`: `Active`, `Busy`, `Inactive`
- `QueueStatus`: `Waiting`, `InProgress`, `Done`, `Skipped`
- `InvoiceType`: `Clinic`, `Drug`

Gợi ý tạo file FE:

- `src/constants/status.ts`
- `src/utils/status-label.ts`

## 5) API theo màn hình chính

### Đặt lịch online (public)

1. `POST /api/email/send-otp`
2. `POST /api/email/verify-otp`
3. `POST /api/appointments`
4. `GET /api/appointments/{code}` (màn tra cứu)

### Lễ tân / Staff

- Danh sách lịch: `GET /api/staff/staffappointments`
- Walk-in: `POST /api/staff/staffappointments/walk-in`
- Gán bác sĩ: `POST /api/staff/staffappointments/assign-doctor`
- Check-in: `POST /api/staff/staffappointments/checkin`
- Queue check-in: `POST /api/roomqueues/check-in`

### Bác sĩ

- Lịch bác sĩ: `GET /api/doctor/doctorappointments`
- Dữ liệu khám: `GET /api/doctor/doctorappointments/{id}/examination`
- Hoàn tất khám: `PATCH /api/doctor/doctorappointments/{id}/complete`
- Kê đơn: nhóm `api/prescription`
- Cận lâm sàng: nhóm `api/clinicaltests`

### Thu ngân / Billing

- Tạo hóa đơn: `POST /api/invoicemanagement`
- Danh sách hóa đơn: `GET /api/invoicemanagement/list`
- Thanh toán: `PUT /api/invoicemanagement/{id}/pay`
- Payment record: `POST /api/payment`
- Bảo hiểm: `GET /api/insurance/validate`

## 6) Cách tổ chức FE service layer

Đề xuất module:

- `auth.api.ts`
- `appointments.api.ts`
- `staff-appointments.api.ts`
- `doctor.api.ts`
- `queue.api.ts`
- `billing.api.ts`
- `medical-record.api.ts`

Nguyên tắc:

- UI không gọi endpoint trực tiếp.
- Mọi parse lỗi/response đặt ở service layer.

## 7) Checklist tích hợp nhanh

- [ ] Login + refresh token hoạt động ổn định
- [ ] 401 auto refresh 1 lần, không loop
- [ ] Parse lỗi theo `message`/`code`
- [ ] Mapping status đầy đủ ở badge/filter
- [ ] Các flow chính chạy end-to-end:
  - [ ] đặt lịch online
  - [ ] check-in staff
  - [ ] bác sĩ hoàn tất khám
  - [ ] tạo hóa đơn + thanh toán
