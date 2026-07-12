# Automation testing

## Chạy backend unit tests trong VS Code

Mở terminal tại thư mục gốc dự án rồi chạy:

```powershell
dotnet test .\backend\ClinicManagement.Api\ClinicManagement.Api.sln
```

Để chạy lặp khi đang lập trình:

```powershell
dotnet watch test --project .\backend\ClinicManagement.Api\ClinicManagement.Api.Tests
```

VS Code chỉ cần extension **C# Dev Kit** (hoặc **.NET Test Explorer**) để phát hiện và chạy từng test trong giao diện. Không cần cài Visual Studio IDE.

## Cấu trúc hiện tại

- `backend/ClinicManagement.Api/ClinicManagement.Api.Tests/Utils`: test các hàm thuần, nhanh.
- `backend/ClinicManagement.Api/ClinicManagement.Api.Tests/Services`: test logic service không cần database.

Một số service cần đọc/ghi dữ liệu được chạy với EF Core InMemory. Mỗi test tạo database riêng, không kết nối hay thay đổi SQL Server phát triển.

Test integration dùng SQL Server riêng và test E2E Playwright sẽ được thêm ở giai đoạn tiếp theo; chúng không sử dụng database phát triển.

## Frontend test environment

`VITE_API_BASE` là base URL cho frontend. Khi không khai báo, ứng dụng vẫn dùng `http://localhost:7235/api` như trước. Môi trường E2E sau này sẽ đặt biến này trỏ tới API test.
