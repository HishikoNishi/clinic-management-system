# 🏥 Clinic Management System

A full-stack web application for managing clinic operations, including patient records, appointments, medical records, invoices, insurance, and online payments.

> 🎓 Graduation Project - FPT Polytechnic Da Nang

---

# 📌 Overview

Clinic Management System is a team project developed to improve clinic management by digitizing the entire workflow, from appointment booking to medical records and online payment.

The system supports multiple user roles including Administrator, Staff, Doctor, Technician, Cashier, and Patient.

---

# ✨ Features

## Authentication
- JWT Authentication
- Role-based Authorization
- Refresh Token
- Email OTP Verification

## Administration
- User Management
- Role Management
- Staff Management
- Doctor Management
- Department Management
- Specialty Management

## Patient Management
- Patient Registration
- Medical Record Management
- Appointment Scheduling
- Clinical Test Management
- Prescription Management

## Payment
- Online Payment with PayOS
- Invoice Generation
- PDF Invoice Export (QuestPDF)

## Others
- Audit Logging
- Validation
- Error Handling
- RESTful API

---

# 🛠 Technologies

## Backend

- ASP.NET Core
- Entity Framework Core
- SQL Server
- JWT Authentication
- RESTful API

## Frontend

- Vue.js
- Bootstrap
- Axios

## Tools

- Git
- GitHub
- QuestPDF
- PayOS API

---

# 🗄 Database

- SQL Server
- Entity Framework Core
- Code First
- Transaction
- Foreign Key Constraints

---

# 👥 Team

This project was developed by a team as a graduation project at **FPT Polytechnic Da Nang**.

## My Role

**Backend Developer (Core Developer)**

I was responsible for designing and implementing the core backend components of the system.

### My Contributions

- Designed and developed RESTful APIs using ASP.NET Core.
- Designed the database structure with Entity Framework Core.
- Implemented JWT Authentication & Authorization.
- Integrated PayOS API for online payments.
- Developed PDF invoice generation using QuestPDF.
- Implemented Audit Logging to track system changes.
- Optimized SQL queries and handled database transactions.
- Collaborated with frontend developers to integrate APIs.
- Fixed backend bugs and optimized system performance.

---

# 🏗 Architecture

```
Vue.js
    │
 REST API
    │
ASP.NET Core
    │
Entity Framework Core
    │
SQL Server
```

---

# 🎬 Demo

## 🔐 Authentication Flow
![Login Demo](./gifs/login.gif)

## 📅 Appointment Booking
![Booking Demo](./gifs/booking.gif)

## 💳 Payment Flow
![Payment Demo](<img width="1080" height="521" alt="Cashier" src="https://github.com/user-attachments/assets/391024df-77e3-42b1-9b81-59f94deaa7c9" />)

# 🚀 Getting Started

## Clone

```bash
git clone https://github.com/your-username/ClinicManagementSystem.git
```

---

## Backend

```bash
cd Backend

dotnet restore

dotnet ef database update

dotnet run
```

---

## Frontend

```bash
cd Frontend

npm install

npm run dev
```

---

# 🔐 User Roles

- Administrator
- Doctor
- Staff
- Technician
- Cashier
- Patient

---

# 📂 Project Structure

```
ClinicManagementSystem

│
├── Backend
│     ├── Controllers
│     ├── Services
│     ├── DTOs
│     ├── Models
│     ├── Repositories
│     └── Data
│
├── Frontend
│     ├── Components
│     ├── Pages
│     ├── Services
│     └── Router
│
└── Database
```

---

# 📈 Future Improvements

- Docker Deployment
- Unit Testing
- CI/CD Pipeline
- Redis Cache
- SignalR Notification
- Multi-language Support

---

# 📄 License

This project was developed for educational purposes at **FPT Polytechnic Da Nang**.

---

# 👨‍💻 Author

Backend Core Developer

**Gia Bảo**

GitHub:
https://github.com/HishikoNishi

Email:
tranggiabao2007@gmail.com
