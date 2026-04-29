# Database Documentation

## Tổng quan

- Database context: `ClinicDbContext`
- DB engine: SQL Server (EF Core Code First + Migration)
- Một số enum đang lưu dạng string: `AppointmentStatus`, `DoctorStatus`, `DoctorShiftRequestStatus`, `DoctorShiftRequestType`, `PaymentMethod`, `InvoiceType`, `QueueStatus`, `Gender`.
- Các bảng nghiệp vụ có áp dụng soft delete qua cột `IsDeleted` + query filter.

## Danh sách bảng chính

- `Roles`
- `Users`
- `Doctors`
- `Staffs`
- `Patients`
- `Departments`
- `Specialties`
- `Appointments`
- `MedicalRecords`
- `Prescriptions`
- `PrescriptionDetails`
- `ClinicalTests`
- `Medicines`
- `Invoices`
- `InvoiceLines`
- `Payments`
- `InsurancePlans`
- `RefreshTokens`
- `EmailOtps`
- `DoctorSchedules`
- `DoctorWeeklySchedules`
- `DoctorScheduleOverrideDays`
- `DoctorShiftRequests`
- `Notifications`
- `Rooms`
- `Queues` (entity `QueueEntry` map sang bảng `Queues`)

## Chi tiết schema (tóm tắt)

### 1) Roles

- PK: `Id` (`Guid`)
- Cột quan trọng: `Name` (unique, max 50)
- Quan hệ: 1 - n với `Users`

### 2) Users

- PK: `Id` (`Guid`)
- FK: `RoleId -> Roles.Id`
- Cột quan trọng: `Username` (unique), `PasswordHash`, `Email`, `FullName`, `PhoneNumber`, `IsActive`, `CreatedAt`
- Quan hệ:
  - 1 - 1 với `Doctors` qua `Doctors.UserId` (unique)
  - 1 - 1 với `Staffs` qua `Staffs.UserId` (unique)
  - 1 - n với `Notifications`
  - 1 - n với `DoctorShiftRequests` (vai trò reviewer)

### 3) Doctors

- PK: `Id` (`Guid`)
- FK:
  - `UserId -> Users.Id` (unique)
  - `DepartmentId -> Departments.Id`
  - `SpecialtyId -> Specialties.Id`
- Cột quan trọng: `Code` (unique), `FullName`, `LicenseNumber`, `Status`, `AvatarUrl`, `CreatedAt`, `IsDeleted`
- Quan hệ:
  - 1 - n `Appointments`
  - 1 - n `DoctorSchedules`
  - 1 - n `DoctorWeeklySchedules`
  - 1 - n `DoctorScheduleOverrideDays`
  - 1 - n `DoctorShiftRequests`
- Soft delete: `DELETE` API chuyển thành set `IsDeleted = true` (không xóa vật lý)

### 4) Staffs

- PK: `Id` (`Guid`)
- FK:
  - `UserId -> Users.Id` (unique)
  - `DepartmentId -> Departments.Id` (nullable)
- Cột quan trọng: `Code` (unique), `FullName`, `Role`, `Position`, `AvatarUrl`, `IsActive`, `IsDeleted`, `CreatedAt`, `UpdatedAt`

### 5) Patients

- PK: `Id` (`Guid`)
- Cột quan trọng:
  - `PatientCode` (unique, nullable)
  - `CitizenId` (unique filtered, nullable)
  - `InsuranceCardNumber` (unique filtered, nullable)
  - `FullName`, `DateOfBirth`, `Gender`, `Phone`, `Email`, `Address`, `Note`, `IsDeleted`, `CreatedAt`, `UpdatedAt`
- Quan hệ: 1 - n với `Appointments`

### 6) Departments

- PK: `Id` (`Guid`)
- Cột quan trọng: `Name` (unique), `Description`, `CreatedAt`, `IsDeleted`
- Quan hệ:
  - 1 - n với `Doctors`
  - 1 - n với `Specialties`
  - 1 - n với `Rooms`

### 7) Specialties

- PK: `Id` (`Guid`)
- FK: `DepartmentId -> Departments.Id`
- Cột quan trọng: `Name`, `IsDeleted`
- Quan hệ: n - 1 với `Departments`, 1 - n với `Doctors`

### 8) Appointments

- PK: `Id` (`Guid`)
- FK:
  - `PatientId -> Patients.Id` (restrict)
  - `DoctorId -> Doctors.Id` (nullable, set null khi xóa doctor)
  - `StaffId -> Staffs.Id` (nullable)
- Cột quan trọng:
  - `AppointmentCode` (unique)
  - `AppointmentDate`, `AppointmentTime`
  - `Reason`, `Status`, `CreatedAt`
  - `CheckedInAt`, `CheckInChannel`
- Quan hệ:
  - 1 - n với `Invoices`
  - 1 - n với `Payments`
  - 1 - n với `Queues`

### 9) MedicalRecords

- PK: `Id` (`Guid`)
- FK:
  - `AppointmentId -> Appointments.Id` (cascade)
  - (logic level) `DoctorId`, `PatientId` lưu trực tiếp Guid
- Cột khám lâm sàng nổi bật:
  - Triệu chứng: `Symptoms`, `DetailedSymptoms`, `PastMedicalHistory`, `Allergies`, `Habits`
  - Sinh hiệu: `HeightCm`, `WeightKg`, `Bmi`, `HeartRate`, `BloodPressure`, `Temperature`, `Spo2`
  - Kết luận: `Diagnosis`, `Treatment`, `Note`
  - Bảo hiểm: `InsurancePlanCode`, `InsuranceCoverPercent`, `Surcharge`, `Discount`
  - `CreatedAt`

### 10) Prescriptions

- PK: `Id` (`Guid`)
- FK: `MedicalRecordId -> MedicalRecords.Id` (cascade)
- Cột: `Note`, `CreatedAt`
- Quan hệ: 1 - n với `PrescriptionDetails`

### 11) PrescriptionDetails

- PK: `Id` (`Guid`)
- FK: `PrescriptionId -> Prescriptions.Id`
- Cột: `MedicineId` (nullable), `MedicineName`, `Dosage`, `Frequency`, `Duration`, `TotalQuantity`, `UnitPrice`

### 12) ClinicalTests

- PK: `Id` (`int`)
- FK: `MedicalRecordId -> MedicalRecords.Id` (cascade)
- Cột: `TestName`, `Result`, `TechnicianName`, `Status`, `ResultAt`, `OrderedByDoctorId`, `CreatedAt`

### 13) Medicines

- PK: `Id` (`Guid`)
- Cột: `Name` (indexed), `DefaultDosage`, `Unit`, `Price` (`decimal(18,2)`), `IsActive`, `IsDeleted`
- Có dữ liệu seed mặc định danh mục thuốc

### 14) Invoices

- PK: `Id` (`Guid`)
- FK:
  - `AppointmentId -> Appointments.Id`
  - `PrescriptionId` (nullable)
- Cột quan trọng:
  - `InvoiceType` (string enum, default `Clinic`)
  - `Amount`, `TotalDeposit`, `BalanceDue`
  - `IsPaid`, `CreatedAt`, `PaymentDate`
- Index:
  - `AppointmentId`
  - `PrescriptionId`
  - `CreatedAt`
  - `InvoiceType`
  - `IsPaid`
- Quan hệ:
  - 1 - n với `InvoiceLines`
  - 1 - n với `Payments`

### 15) InvoiceLines

- PK: `Id` (`Guid`)
- FK: `InvoiceId -> Invoices.Id` (cascade)
- Cột: `Description`, `ItemType`, `Amount`, `Dosage`, `Duration`

### 16) Payments

- PK: `Id` (`Guid`)
- FK:
  - `InvoiceId -> Invoices.Id` (nullable, set null khi invoice bị xóa)
  - `AppointmentId -> Appointments.Id`
- Cột: `Amount`, `DepositAmount`, `IsDeposit`, `Method`, `PaymentDate`

### 17) InsurancePlans

- PK: `Id` (`Guid`)
- Cột: `Code` (unique), `Name`, `CoveragePercent`, `Note`, `IsActive`, `ExpiryDate`, `IsDeleted`
- Có dữ liệu seed gói bảo hiểm mẫu

### 18) RefreshTokens

- PK: `Id` (`Guid`)
- FK: `UserId -> Users.Id` (cascade)
- Cột: `Token` (unique), `ExpiresAt`, `CreatedAt`, `IsRevoked`

### 19) EmailOtps

- PK: `Id` (`Guid`)
- Cột: `Email`, `Code`, `ExpiredAt`, `IsUsed`, `CreatedAt`, `VerifiedAt`
- Index: `(Email, IsUsed)`

### 20) DoctorSchedules

- PK: `Id` (`Guid`)
- FK:
  - `DoctorId -> Doctors.Id` (cascade)
  - `RoomId -> Rooms.Id` (nullable)
- Cột: `WorkDate`, `ShiftCode`, `SlotLabel`, `StartTime`, `EndTime`, `IsActive`, `CreatedAt`
- Unique index:
  - `(DoctorId, WorkDate, StartTime)`
  - `(RoomId, WorkDate, StartTime)` với filter `RoomId IS NOT NULL`

### 21) DoctorWeeklySchedules

- PK: `Id` (`Guid`)
- FK:
  - `DoctorId -> Doctors.Id` (cascade)
  - `RoomId -> Rooms.Id` (nullable)
- Cột: `DayOfWeek`, `ShiftCode`, `SlotLabel`, `StartTime`, `EndTime`, `IsActive`, `CreatedAt`
- Unique index:
  - `(DoctorId, DayOfWeek, StartTime)`
  - `(RoomId, DayOfWeek, StartTime)` với filter `RoomId IS NOT NULL`

### 22) DoctorScheduleOverrideDays

- PK: `Id` (`Guid`)
- FK: `DoctorId -> Doctors.Id` (cascade)
- Cột: `WorkDate`, `CreatedAt`
- Unique index: `(DoctorId, WorkDate)`

### 23) DoctorShiftRequests

- PK: `Id` (`Guid`)
- FK:
  - `DoctorId -> Doctors.Id`
  - `PreferredDoctorId -> Doctors.Id` (nullable)
  - `ReplacementDoctorId -> Doctors.Id` (nullable)
  - `ReviewedByUserId -> Users.Id` (nullable)
- Cột: `RequestType`, `Status`, `WorkDate`, `ShiftCode`, `SlotLabel`, `StartTime`, `EndTime`, `Reason`, `AdminNote`, `ReviewedAt`, `CreatedAt`
- Index: `(DoctorId, WorkDate, StartTime, Status)`

### 24) Notifications

- PK: `Id` (`Guid`)
- FK: `UserId -> Users.Id` (cascade)
- Cột: `Title`, `Message`, `Type`, `IsRead`, `Link`, `CreatedAt`
- Index: `(UserId, IsRead, CreatedAt)`

### 25) Rooms

- PK: `Id` (`Guid`)
- FK: `DepartmentId -> Departments.Id`
- Cột: `Code` (unique), `Name`, `IsActive`, `IsDeleted`, `CreatedAt`
- Quan hệ: 1 - n với `Queues`

### 26) Queues

- Bảng vật lý: `Queues` (entity `QueueEntry`)
- PK: `Id` (`Guid`)
- FK:
  - `AppointmentId -> Appointments.Id` (cascade)
  - `RoomId -> Rooms.Id`
- Cột: `QueueNumber`, `Status`, `IsPriority`, `QueuedAt`, `CalledAt`
- Index:
  - `(RoomId, QueuedAt)`
  - `AppointmentId`

## Seed Data hiện có

- `Roles`: Admin, Doctor, Staff, Guest, Technician, Cashier
- `Users`: có sẵn tài khoản admin seed (`username: admin`)
- `Departments` + `Specialties`: đã seed danh mục khoa/chuyên khoa cơ bản
- `Medicines`: seed danh mục thuốc mẫu
- `InsurancePlans`: seed gói bảo hiểm mẫu

## Lưu ý triển khai

- Ứng dụng có chạy `Database.Migrate()` khi startup.
- Có đoạn SQL startup để bổ sung các cột mới cho bảng `MedicalRecords` và `InvoiceLines` nếu DB cũ chưa có.
- Có đoạn SQL startup để bổ sung cột `IsDeleted` cho các bảng mới áp dụng soft delete (`Doctors`, `Staffs`, `Departments`, `Specialties`, `Rooms`, `Medicines`, `InsurancePlans`) nếu DB cũ chưa có.
- Một số quan hệ dùng `DeleteBehavior.Restrict` để tránh xóa dây chuyền ngoài ý muốn ở dữ liệu vận hành.
