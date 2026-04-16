# API Examples

Tài liệu này cung cấp payload mẫu để FE tích hợp nhanh với các luồng chính.

## 1) Login

### Request

`POST /api/auth/login`

```json
{
  "username": "admin",
  "password": "Admin@123"
}
```

### Response 200

```json
{
  "token": "<JWT_ACCESS_TOKEN>",
  "username": "admin",
  "doctorId": null,
  "role": "Admin",
  "expiresAt": "2026-04-16T10:30:00Z",
  "refreshToken": "<REFRESH_TOKEN>",
  "refreshExpiresAt": "2026-04-23T09:30:00Z"
}
```

### Response lỗi 401

```json
{
  "code": "unauthorized",
  "message": "Invalid credentials.",
  "details": null,
  "timestamp": "2026-04-16T09:31:00Z"
}
```

## 2) Refresh token

### Request

`POST /api/auth/refresh`

```json
{
  "refreshToken": "<REFRESH_TOKEN>"
}
```

### Response 200

```json
{
  "token": "<NEW_JWT_ACCESS_TOKEN>",
  "username": "admin",
  "doctorId": null,
  "role": "Admin",
  "expiresAt": "2026-04-16T11:30:00Z",
  "refreshToken": "<NEW_REFRESH_TOKEN>",
  "refreshExpiresAt": "2026-04-23T10:30:00Z"
}
```

## 3) OTP + Đặt lịch online

### 3.1 Gửi OTP

`POST /api/email/send-otp`

```json
{
  "email": "patient@example.com"
}
```

Response 200:

```json
{
  "message": "OTP đã được gửi"
}
```

### 3.2 Xác thực OTP

`POST /api/email/verify-otp`

```json
{
  "email": "patient@example.com",
  "code": "123456"
}
```

Response 200:

```json
{
  "message": "Xác thực OTP thành công"
}
```

Response lỗi 400:

```json
{
  "code": "otp_invalid",
  "message": "OTP sai hoặc đã hết hạn",
  "details": null,
  "timestamp": "2026-04-16T09:45:00Z"
}
```

### 3.3 Tạo lịch hẹn

`POST /api/appointments`

```json
{
  "fullName": "Nguyen Van A",
  "phone": "0901234567",
  "dateOfBirth": "1996-05-10T00:00:00",
  "gender": "Male",
  "email": "patient@example.com",
  "address": "Thu Duc, HCM",
  "citizenId": "012345678901",
  "insuranceCardNumber": "BHYT001122",
  "appointmentDate": "2026-04-20T00:00:00",
  "appointmentTime": "09:00:00",
  "doctorId": "11111111-2222-3333-4444-555555555555",
  "reason": "Dau bung"
}
```

Response 200 (rút gọn):

```json
{
  "id": "f32ac6f1-6f6d-4a59-bf5f-cddccf2f8d47",
  "appointmentCode": "AB12CD",
  "fullName": "Nguyen Van A",
  "status": "Confirmed",
  "appointmentDate": "2026-04-20T00:00:00",
  "appointmentTime": "09:00:00",
  "statusDetail": {
    "value": "Confirmed",
    "doctorId": "11111111-2222-3333-4444-555555555555",
    "doctorName": "Dr. Tran B"
  }
}
```

## 4) Check-in tại quầy (Staff)

### Request

`POST /api/staff/staffappointments/checkin`

Header:

- `Authorization: Bearer <token staff/admin>`

```json
{
  "appointmentId": "f32ac6f1-6f6d-4a59-bf5f-cddccf2f8d47",
  "doctorId": "11111111-2222-3333-4444-555555555555",
  "roomId": "9a9a9a9a-1111-2222-3333-444444444444",
  "isInpatient": true,
  "depositAmount": 200000,
  "method": "Cash",
  "insuranceCode": "BHYT-A",
  "insuranceCoverPercent": 0.8
}
```

### Response 200

```json
{
  "message": "Checked in successfully",
  "status": "CheckedIn",
  "totalDeposit": 200000,
  "depositPaymentId": "0dd7ab0f-9102-4d33-88cb-a1a4a1821f30"
}
```

## 5) Tạo hồ sơ khám + đơn thuốc

### 5.1 Tạo hồ sơ khám

`POST /api/medical-record`

```json
{
  "appointmentId": "f32ac6f1-6f6d-4a59-bf5f-cddccf2f8d47",
  "doctorId": "11111111-2222-3333-4444-555555555555",
  "patientId": "a1111111-b222-c333-d444-eeeeeeeeeeee",
  "symptoms": "Sot, ho",
  "diagnosis": "Viem hong cap",
  "treatment": "Dieu tri ngoai tru",
  "note": "Tai kham sau 3 ngay"
}
```

Response 200 (rút gọn):

```json
{
  "id": "9a3a6f6d-5712-4ac9-9d9a-8180c4ab2f10",
  "appointmentId": "f32ac6f1-6f6d-4a59-bf5f-cddccf2f8d47",
  "diagnosis": "Viem hong cap"
}
```

### 5.2 Tạo đơn thuốc

`POST /api/prescription`

```json
{
  "medicalRecordId": "9a3a6f6d-5712-4ac9-9d9a-8180c4ab2f10",
  "note": "Uong sau an",
  "prescriptionDetails": [
    {
      "medicineId": "f001f001-1111-1111-1111-111111111111",
      "medicineName": "Paracetamol",
      "dosage": "1 vien/lan",
      "frequency": "2 lan/ngay",
      "duration": 5,
      "totalQuantity": 10,
      "unitPrice": 2000
    }
  ]
}
```

Response 200 (rút gọn):

```json
{
  "id": "f31f2f6d-77b2-4ba8-8fd8-3f07c4725bc1",
  "medicalRecordId": "9a3a6f6d-5712-4ac9-9d9a-8180c4ab2f10",
  "prescriptionDetails": [
    {
      "medicineName": "Paracetamol",
      "totalQuantity": 10
    }
  ]
}
```

## 6) Tạo hóa đơn + thanh toán

### 6.1 Tạo hóa đơn khám

`POST /api/invoicemanagement`

```json
{
  "appointmentId": "f32ac6f1-6f6d-4a59-bf5f-cddccf2f8d47"
}
```

Response 200 (rút gọn):

```json
{
  "id": "c4c9a77c-2a9a-4f6c-9f6f-4fc6a4d3672a",
  "appointmentId": "f32ac6f1-6f6d-4a59-bf5f-cddccf2f8d47",
  "invoiceType": "Clinic",
  "amount": 350000,
  "isPaid": false
}
```

### 6.2 Thanh toán hóa đơn

`PUT /api/invoicemanagement/{id}/pay`

```json
{
  "method": "Cash",
  "amount": 350000
}
```

Response 200:

```json
{
  "message": "Payment successful",
  "invoiceId": "c4c9a77c-2a9a-4f6c-9f6f-4fc6a4d3672a",
  "isPaid": true
}
```

## 7) Queue nhanh cho màn hình gọi số

### Check-in vào queue

`POST /api/roomqueues/check-in`

```json
{
  "appointmentId": "f32ac6f1-6f6d-4a59-bf5f-cddccf2f8d47",
  "roomId": "9a9a9a9a-1111-2222-3333-444444444444"
}
```

Response 200 (rút gọn):

```json
{
  "id": "bfec0628-26a1-43c2-9d86-83cc4ee93261",
  "queueNumber": 15,
  "status": "Waiting",
  "roomName": "Phong 01"
}
```

### Gọi số tiếp theo (Doctor)

`POST /api/roomqueues/rooms/{roomId}/next`

Response 200 (rút gọn):

```json
{
  "id": "bfec0628-26a1-43c2-9d86-83cc4ee93261",
  "queueNumber": 15,
  "status": "InProgress",
  "fullName": "Nguyen Van A"
}
```

## Ghi chú

- Payload thực tế có thể có thêm field theo DTO hiện tại; kiểm tra Swagger để lấy đầy đủ schema.
- Các ví dụ ưu tiên cho FE flow, không thay thế tài liệu chi tiết kỹ thuật.
