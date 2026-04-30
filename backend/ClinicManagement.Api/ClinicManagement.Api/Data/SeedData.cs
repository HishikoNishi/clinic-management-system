using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicManagement.Api.Data
{
    public static class SeedData
    {
        // Idempotent demo seed: safe to run on every startup.
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
            var passwordHasher = new PasswordHasher<User>();

            var roles = await EnsureRolesAsync(context);
            var departments = await EnsureDepartmentsAsync(context);
            var specialties = await EnsureSpecialtiesAsync(context, departments);
            var rooms = await EnsureRoomsAsync(context, departments);

            await EnsureInsurancePlansAsync(context);
            await EnsureMedicinesAsync(context);

            var users = await EnsureUsersAsync(context, roles, passwordHasher);
            var doctors = await EnsureDoctorsAsync(context, users, departments, specialties);
            await EnsureStaffProfilesAsync(context, users, departments);
            await EnsureWeeklySchedulesAsync(context, doctors, rooms);

            var patients = await EnsurePatientsAsync(context);
            var appointments = await EnsureAppointmentsAsync(context, patients, doctors);
            await EnsureQueueForCheckedInAsync(context, appointments, rooms);
            await EnsureBillingDemoAsync(context, appointments);

            await context.SaveChangesAsync();
        }

        private static async Task<Dictionary<string, Role>> EnsureRolesAsync(ClinicDbContext context)
        {
            var roleNames = new[] { "Admin", "Doctor", "Staff", "Technician", "Cashier", "Guest" };
            var roleMap = new Dictionary<string, Role>(StringComparer.OrdinalIgnoreCase);

            foreach (var name in roleNames)
            {
                var role = await context.Roles.FirstOrDefaultAsync(r => r.Name == name);
                if (role == null)
                {
                    role = new Role { Id = Guid.NewGuid(), Name = name };
                    context.Roles.Add(role);
                }
                roleMap[name] = role;
            }

            await context.SaveChangesAsync();
            return roleMap;
        }

        private static async Task<Dictionary<string, Department>> EnsureDepartmentsAsync(ClinicDbContext context)
        {
            var seeds = new[]
            {
                new
                {
                    Key = "GEN",
                    Name = "Nội tổng quát",
                    Description = "Khám ngoại trú tổng quát",
                    LegacyNames = new[] { "General Medicine" }
                },
                new
                {
                    Key = "SUR",
                    Name = "Ngoại khoa",
                    Description = "Tư vấn phẫu thuật",
                    LegacyNames = new[] { "Surgery" }
                },
                new
                {
                    Key = "PED",
                    Name = "Nhi khoa",
                    Description = "Chăm sóc nhi khoa",
                    LegacyNames = new[] { "Pediatrics" }
                },
                new
                {
                    Key = "ENT",
                    Name = "Tai Mũi Họng",
                    Description = "Khám chuyên khoa Tai Mũi Họng",
                    LegacyNames = new[] { "ENT" }
                },
                new
                {
                    Key = "OBS",
                    Name = "Sản phụ khoa",
                    Description = "Khám và theo dõi sản phụ khoa",
                    LegacyNames = new[] { "Obstetrics" }
                },
                new
                {
                    Key = "DIA",
                    Name = "Cận lâm sàng",
                    Description = "Xét nghiệm và chẩn đoán hình ảnh",
                    LegacyNames = new[] { "Diagnostics" }
                },
            };

            var map = new Dictionary<string, Department>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in seeds)
            {
                var dep = await context.Departments
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(d => d.Name == item.Name || item.LegacyNames.Contains(d.Name));

                if (dep == null)
                {
                    dep = new Department
                    {
                        Id = Guid.NewGuid(),
                        Name = item.Name,
                        Description = item.Description,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };
                    context.Departments.Add(dep);
                }
                else
                {
                    dep.Description = item.Description;
                    dep.IsDeleted = false;
                }

                map[item.Key] = dep;
            }

            await context.SaveChangesAsync();
            return map;
        }

        private static async Task<Dictionary<string, Specialty>> EnsureSpecialtiesAsync(
            ClinicDbContext context,
            Dictionary<string, Department> departments)
        {
            var seeds = new[]
            {
                new { Key = "GEN-CORE", Name = "Nội tổng quát", DepartmentKey = "GEN" },
                new { Key = "GEN-CARD", Name = "Tim mạch", DepartmentKey = "GEN" },
                new { Key = "SUR-CORE", Name = "Ngoại tổng quát", DepartmentKey = "SUR" },
                new { Key = "PED-CORE", Name = "Nhi khoa tổng quát", DepartmentKey = "PED" },
                new { Key = "ENT-CORE", Name = "Tai Mũi Họng", DepartmentKey = "ENT" },
                new { Key = "OBS-CORE", Name = "Sản phụ khoa", DepartmentKey = "OBS" }
            };

            var map = new Dictionary<string, Specialty>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in seeds)
            {
                var departmentId = departments[item.DepartmentKey].Id;
                var specialty = await context.Specialties
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(s => s.Name == item.Name && s.DepartmentId == departmentId);

                if (specialty == null)
                {
                    specialty = new Specialty
                    {
                        Id = Guid.NewGuid(),
                        Name = item.Name,
                        DepartmentId = departmentId,
                        IsDeleted = false
                    };
                    context.Specialties.Add(specialty);
                }
                else
                {
                    specialty.IsDeleted = false;
                }

                map[item.Key] = specialty;
            }

            await context.SaveChangesAsync();
            return map;
        }

        private static async Task<Dictionary<string, Room>> EnsureRoomsAsync(
            ClinicDbContext context,
            Dictionary<string, Department> departments)
        {
            var seeds = new[]
            {
                new { Code = "RM-GEN-01", Name = "Phòng tổng quát 01", DepartmentKey = "GEN" },
                new { Code = "RM-SUR-01", Name = "Phòng phẫu thuật 01", DepartmentKey = "SUR" },
                new { Code = "RM-PED-01", Name = "Phòng nhi khoa 01", DepartmentKey = "PED" },
                new { Code = "RM-ENT-01", Name = "Phòng Tai Mũi Họng 01", DepartmentKey = "ENT" },
                new { Code = "RM-OBS-01", Name = "Phòng Sản phụ khoa 01", DepartmentKey = "OBS" },
                new { Code = "RM-DIA-01", Name = "Phòng chẩn đoán 01", DepartmentKey = "DIA" }
            };

            var map = new Dictionary<string, Room>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in seeds)
            {
                var room = await context.Rooms
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(r => r.Code == item.Code);

                if (room == null)
                {
                    room = new Room
                    {
                        Id = Guid.NewGuid(),
                        Code = item.Code,
                        Name = item.Name,
                        DepartmentId = departments[item.DepartmentKey].Id,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    context.Rooms.Add(room);
                }
                else
                {
                    room.Name = item.Name;
                    room.DepartmentId = departments[item.DepartmentKey].Id;
                    room.IsActive = true;
                    room.IsDeleted = false;
                }

                map[item.Code] = room;
            }

            await context.SaveChangesAsync();
            return map;
        }

        private static async Task EnsureMedicinesAsync(ClinicDbContext context)
        {
            var seeds = new[]
            {
                new { Name = "Paracetamol", Dosage = "500mg", Unit = "viên", Price = 3000m },
                new { Name = "Amoxicillin", Dosage = "500mg", Unit = "viên", Price = 7000m },
                new { Name = "Azithromycin", Dosage = "500mg", Unit = "viên", Price = 22000m },
                new { Name = "Cetirizine", Dosage = "10mg", Unit = "viên", Price = 4000m },
                new { Name = "Omeprazole", Dosage = "20mg", Unit = "viên", Price = 6000m },
                new { Name = "Methylprednisolone", Dosage = "16mg", Unit = "viên", Price = 8000m },
                new { Name = "Ibuprofen", Dosage = "400mg", Unit = "viên", Price = 5000m },
                new { Name = "Vitamin C", Dosage = "500mg", Unit = "viên", Price = 2500m },
                new { Name = "ORS", Dosage = "N/A", Unit = "gói", Price = 5000m },
                new { Name = "Dextromethorphan", Dosage = "15mg", Unit = "viên", Price = 4500m },
                new { Name = "Ambroxol", Dosage = "30mg", Unit = "viên", Price = 5000m },
                new { Name = "Loratadine", Dosage = "10mg", Unit = "viên", Price = 4500m },
                new { Name = "Furosemide", Dosage = "20mg", Unit = "ống", Price = 12000m },
                new { Name = "Atropine", Dosage = "0.25mg", Unit = "ống", Price = 9000m },
                new { Name = "Vitamin E", Dosage = "400IU", Unit = "viên nang", Price = 5000m },
                new { Name = "Omega-3", Dosage = "1000mg", Unit = "viên nang", Price = 8000m },
                new { Name = "Omeprazole DR", Dosage = "20mg", Unit = "viên nang", Price = 7000m },
                new { Name = "Povidone Iodine 10%", Dosage = "10%", Unit = "chai", Price = 25000m },
                new { Name = "Hydrogen Peroxide 3%", Dosage = "3%", Unit = "chai", Price = 15000m },
                new { Name = "Normal Saline 0.9%", Dosage = "0.9%", Unit = "chai", Price = 12000m },
                new { Name = "Multivitamin Syrup", Dosage = "N/A", Unit = "chai", Price = 45000m },
                new { Name = "ORS Glucose", Dosage = "N/A", Unit = "gói", Price = 6000m },
                new { Name = "Berberin", Dosage = "N/A", Unit = "gói", Price = 8000m }
            };

            foreach (var item in seeds)
            {
                var medicine = await context.Medicines
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(m => m.Name == item.Name);

                if (medicine == null)
                {
                    medicine = new Medicine
                    {
                        Id = Guid.NewGuid(),
                        Name = item.Name,
                        DefaultDosage = item.Dosage,
                        Unit = item.Unit,
                        Price = item.Price,
                        IsActive = true,
                        IsDeleted = false
                    };
                    context.Medicines.Add(medicine);
                }
                else
                {
                    medicine.DefaultDosage = item.Dosage;
                    medicine.Unit = item.Unit;
                    medicine.Price = item.Price;
                    medicine.IsActive = true;
                    medicine.IsDeleted = false;
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task EnsureInsurancePlansAsync(ClinicDbContext context)
        {
            var seeds = new[]
            {
                new { Code = "BHYT-80", Name = "BHYT 80", Cover = 0.80m, Note = "Muc huong BHYT 80%" },
                new { Code = "BHYT-50", Name = "Bao hiem doanh nghiep 50", Cover = 0.50m, Note = "Muc huong doanh nghiep" },
                new { Code = "BHYT-90", Name = "BHYT uu tien 90", Cover = 0.90m, Note = "Muc huong uu tien" }
            };

            foreach (var item in seeds)
            {
                var plan = await context.InsurancePlans
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(p => p.Code == item.Code);

                if (plan == null)
                {
                    plan = new InsurancePlan
                    {
                        Id = Guid.NewGuid(),
                        Code = item.Code,
                        Name = item.Name,
                        CoveragePercent = item.Cover,
                        Note = item.Note,
                        IsActive = true,
                        IsDeleted = false,
                        ExpiryDate = DateTime.UtcNow.AddYears(2)
                    };
                    context.InsurancePlans.Add(plan);
                }
                else
                {
                    plan.Name = item.Name;
                    plan.CoveragePercent = item.Cover;
                    plan.Note = item.Note;
                    plan.IsActive = true;
                    plan.IsDeleted = false;
                    plan.ExpiryDate ??= DateTime.UtcNow.AddYears(2);
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task<Dictionary<string, User>> EnsureUsersAsync(
            ClinicDbContext context,
            Dictionary<string, Role> roles,
            PasswordHasher<User> passwordHasher)
        {
            var seeds = new[]
            {
                new { Username = "admin", FullName = "Nguyễn Minh Quân", Email = "admin@clinic.local", Phone = "0900000000", Role = "Admin", Password = "Admin@123" },
                new { Username = "staff", FullName = "Trần Thị Thu Hà", Email = "staff@clinic.local", Phone = "0900000001", Role = "Staff", Password = "Staff@123" },
                new { Username = "cashier", FullName = "Lê Hoàng Nam", Email = "cashier@clinic.local", Phone = "0900000002", Role = "Cashier", Password = "Cashier@123" },
                new { Username = "tech", FullName = "Phạm Gia Bảo", Email = "tech@clinic.local", Phone = "0900000003", Role = "Technician", Password = "Tech@123" },
                new { Username = "doctor.gen", FullName = "Bác sĩ Vũ Đức Anh", Email = "doc.gen@clinic.local", Phone = "0900000011", Role = "Doctor", Password = "Doctor@123" },
                new { Username = "doctor.sur", FullName = "Bác sĩ Đoàn Ngọc Linh", Email = "doc.sur@clinic.local", Phone = "0900000012", Role = "Doctor", Password = "Doctor@123" },
                new { Username = "doctor.ped", FullName = "Bác sĩ Nguyễn Thị Mai", Email = "doc.ped@clinic.local", Phone = "0900000013", Role = "Doctor", Password = "Doctor@123" },
                new { Username = "doctor.ent", FullName = "Bác sĩ Trần Quốc Huy", Email = "doc.ent@clinic.local", Phone = "0900000014", Role = "Doctor", Password = "Doctor@123" },
                new { Username = "doctor.obs", FullName = "Bác sĩ Hoàng Như Quỳnh", Email = "doc.obs@clinic.local", Phone = "0900000015", Role = "Doctor", Password = "Doctor@123" }
            };

            var map = new Dictionary<string, User>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in seeds)
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.Username == item.Username);
                if (user == null)
                {
                    user = new User
                    {
                        Id = Guid.NewGuid(),
                        Username = item.Username,
                        FullName = item.FullName,
                        Email = item.Email,
                        PhoneNumber = item.Phone,
                        RoleId = roles[item.Role].Id,
                        IsActive = true
                    };
                    user.PasswordHash = passwordHasher.HashPassword(user, item.Password);
                    context.Users.Add(user);
                }
                else
                {
                    user.FullName = item.FullName;
                    user.Email = item.Email;
                    user.PhoneNumber = item.Phone;
                    user.RoleId = roles[item.Role].Id;
                    user.IsActive = true;
                }

                map[item.Username] = user;
            }

            await context.SaveChangesAsync();
            return map;
        }

        private static async Task<Dictionary<string, Doctor>> EnsureDoctorsAsync(
            ClinicDbContext context,
            Dictionary<string, User> users,
            Dictionary<string, Department> departments,
            Dictionary<string, Specialty> specialties)
        {
            var seeds = new[]
            {
                new { Username = "doctor.gen", Code = "DOC-DEMO-01", Department = "GEN", Specialty = "GEN-CORE", License = "LIC-DEMO-001" },
                new { Username = "doctor.sur", Code = "DOC-DEMO-02", Department = "SUR", Specialty = "SUR-CORE", License = "LIC-DEMO-002" },
                new { Username = "doctor.ped", Code = "DOC-DEMO-03", Department = "PED", Specialty = "PED-CORE", License = "LIC-DEMO-003" },
                new { Username = "doctor.ent", Code = "DOC-DEMO-04", Department = "ENT", Specialty = "ENT-CORE", License = "LIC-DEMO-004" },
                new { Username = "doctor.obs", Code = "DOC-DEMO-05", Department = "OBS", Specialty = "OBS-CORE", License = "LIC-DEMO-005" }
            };

            var map = new Dictionary<string, Doctor>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in seeds)
            {
                var user = users[item.Username];
                var doctor = await context.Doctors
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(d => d.UserId == user.Id);

                if (doctor == null)
                {
                    doctor = new Doctor
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        Code = item.Code,
                        FullName = user.FullName,
                        DepartmentId = departments[item.Department].Id,
                        SpecialtyId = specialties[item.Specialty].Id,
                        LicenseNumber = item.License,
                        Status = DoctorStatus.Active,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    context.Doctors.Add(doctor);
                }
                else
                {
                    doctor.Code = item.Code;
                    doctor.FullName = user.FullName;
                    doctor.DepartmentId = departments[item.Department].Id;
                    doctor.SpecialtyId = specialties[item.Specialty].Id;
                    doctor.LicenseNumber = item.License;
                    doctor.Status = DoctorStatus.Active;
                    doctor.IsDeleted = false;
                }

                map[item.Code] = doctor;
            }

            await context.SaveChangesAsync();
            return map;
        }

        private static async Task EnsureStaffProfilesAsync(
            ClinicDbContext context,
            Dictionary<string, User> users,
            Dictionary<string, Department> departments)
        {
            var seeds = new[]
            {
                new { Username = "staff", Code = "ST-DEMO-01", Role = "Staff", Department = "GEN" },
                new { Username = "cashier", Code = "CA-DEMO-01", Role = "Cashier", Department = "DIA" },
                new { Username = "tech", Code = "TE-DEMO-01", Role = "Technician", Department = "DIA" }
            };

            foreach (var item in seeds)
            {
                var user = users[item.Username];
                var staff = await context.Staffs
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(s => s.UserId == user.Id);

                if (staff == null)
                {
                    staff = new Staff
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        Code = item.Code,
                        FullName = user.FullName,
                        Role = item.Role,
                        DepartmentId = departments[item.Department].Id,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    context.Staffs.Add(staff);
                }
                else
                {
                    staff.Code = item.Code;
                    staff.FullName = user.FullName;
                    staff.Role = item.Role;
                    staff.DepartmentId = departments[item.Department].Id;
                    staff.IsActive = true;
                    staff.IsDeleted = false;
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task EnsureWeeklySchedulesAsync(
            ClinicDbContext context,
            Dictionary<string, Doctor> doctors,
            Dictionary<string, Room> rooms)
        {
            var mapping = new[]
            {
                new { DoctorCode = "DOC-DEMO-01", RoomCode = "RM-GEN-01" },
                new { DoctorCode = "DOC-DEMO-02", RoomCode = "RM-SUR-01" },
                new { DoctorCode = "DOC-DEMO-03", RoomCode = "RM-PED-01" },
                new { DoctorCode = "DOC-DEMO-04", RoomCode = "RM-ENT-01" },
                new { DoctorCode = "DOC-DEMO-05", RoomCode = "RM-OBS-01" }
            };

            var weekdaySeeds = new[]
            {
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday
            };

            var slotSeeds = new[]
            {
                new { ShiftCode = "morning", Label = "09:00 - 09:30", Start = new TimeSpan(9, 0, 0), End = new TimeSpan(9, 30, 0) },
                new { ShiftCode = "morning", Label = "09:30 - 10:00", Start = new TimeSpan(9, 30, 0), End = new TimeSpan(10, 0, 0) },
                new { ShiftCode = "afternoon", Label = "14:00 - 14:30", Start = new TimeSpan(14, 0, 0), End = new TimeSpan(14, 30, 0) },
                new { ShiftCode = "afternoon", Label = "14:30 - 15:00", Start = new TimeSpan(14, 30, 0), End = new TimeSpan(15, 0, 0) }
            };

            foreach (var item in mapping)
            {
                var doctor = doctors[item.DoctorCode];
                var room = rooms[item.RoomCode];

                foreach (var day in weekdaySeeds)
                {
                    foreach (var slot in slotSeeds)
                    {
                        var exists = await context.DoctorWeeklySchedules.AnyAsync(s =>
                            s.DoctorId == doctor.Id &&
                            s.DayOfWeek == day &&
                            s.StartTime == slot.Start);

                        if (exists) continue;

                        context.DoctorWeeklySchedules.Add(new DoctorWeeklySchedule
                        {
                            Id = Guid.NewGuid(),
                            DoctorId = doctor.Id,
                            RoomId = room.Id,
                            DayOfWeek = day,
                            ShiftCode = slot.ShiftCode,
                            SlotLabel = slot.Label,
                            StartTime = slot.Start,
                            EndTime = slot.End,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task<Dictionary<string, Patient>> EnsurePatientsAsync(ClinicDbContext context)
        {
            var seeds = new[]
            {
                new { Code = "PAT-DEMO-001", Name = "Nguyễn Thành An", Gender = Gender.Male, Phone = "0911000001", Citizen = "100000000001", Insurance = "BHYT-80-0001" },
                new { Code = "PAT-DEMO-002", Name = "Trần Ngọc Bích", Gender = Gender.Female, Phone = "0911000002", Citizen = "100000000002", Insurance = "BHYT-80-0002" },
                new { Code = "PAT-DEMO-003", Name = "Lê Minh Cường", Gender = Gender.Male, Phone = "0911000003", Citizen = "100000000003", Insurance = "BHYT-50-0003" },
                new { Code = "PAT-DEMO-004", Name = "Phạm Thùy Duyên", Gender = Gender.Female, Phone = "0911000004", Citizen = "100000000004", Insurance = "BHYT-90-0004" },
                new { Code = "PAT-DEMO-005", Name = "Hoàng Bảo Giang", Gender = Gender.Female, Phone = "0911000005", Citizen = "100000000005", Insurance = "BHYT-80-0005" },
                new { Code = "PAT-DEMO-006", Name = "Đỗ Quang Hải", Gender = Gender.Male, Phone = "0911000006", Citizen = "100000000006", Insurance = "BHYT-50-0006" }
            };

            var map = new Dictionary<string, Patient>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < seeds.Length; i++)
            {
                var item = seeds[i];
                var patient = await context.Patients
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(p => p.PatientCode == item.Code);

                if (patient == null)
                {
                    patient = new Patient
                    {
                        Id = Guid.NewGuid(),
                        PatientCode = item.Code,
                        FullName = item.Name,
                        DateOfBirth = new DateTime(1990, 1, 1).AddDays(i * 150),
                        Gender = item.Gender,
                        Phone = item.Phone,
                        Email = $"{item.Code.ToLowerInvariant()}@demo.local",
                        Address = "Demo Address",
                        CitizenId = item.Citizen,
                        InsuranceCardNumber = item.Insurance,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    context.Patients.Add(patient);
                }
                else
                {
                    patient.FullName = item.Name;
                    patient.Phone = item.Phone;
                    patient.Gender = item.Gender;
                    patient.CitizenId = item.Citizen;
                    patient.InsuranceCardNumber = item.Insurance;
                    patient.IsDeleted = false;
                }

                map[item.Code] = patient;
            }

            await context.SaveChangesAsync();
            return map;
        }

        private static async Task<Dictionary<string, Appointment>> EnsureAppointmentsAsync(
            ClinicDbContext context,
            Dictionary<string, Patient> patients,
            Dictionary<string, Doctor> doctors)
        {
            var staffId = await context.Staffs
                .Where(s => s.Role == "Staff" && !s.IsDeleted)
                .Select(s => (Guid?)s.Id)
                .FirstOrDefaultAsync();

            var today = DateTime.Today;
            var seeds = new[]
            {
                new { Code = "AP-DEMO-001", Patient = "PAT-DEMO-001", Doctor = "DOC-DEMO-01", Date = today.AddDays(1), Time = new TimeSpan(9, 0, 0), Status = AppointmentStatus.Confirmed, Reason = "General check" },
                new { Code = "AP-DEMO-002", Patient = "PAT-DEMO-002", Doctor = "DOC-DEMO-02", Date = today, Time = new TimeSpan(9, 30, 0), Status = AppointmentStatus.CheckedIn, Reason = "Abdominal pain" },
                new { Code = "AP-DEMO-003", Patient = "PAT-DEMO-003", Doctor = "DOC-DEMO-03", Date = today.AddDays(-1), Time = new TimeSpan(14, 0, 0), Status = AppointmentStatus.Completed, Reason = "Pediatric fever" },
                new { Code = "AP-DEMO-004", Patient = "PAT-DEMO-004", Doctor = "DOC-DEMO-04", Date = today.AddDays(-1), Time = new TimeSpan(14, 30, 0), Status = AppointmentStatus.Completed, Reason = "ENT pain" },
                new { Code = "AP-DEMO-005", Patient = "PAT-DEMO-005", Doctor = "DOC-DEMO-05", Date = today, Time = new TimeSpan(10, 0, 0), Status = AppointmentStatus.Pending, Reason = "Prenatal consult" },
                new { Code = "AP-DEMO-006", Patient = "PAT-DEMO-006", Doctor = "DOC-DEMO-01", Date = today.AddDays(-2), Time = new TimeSpan(9, 0, 0), Status = AppointmentStatus.NoShow, Reason = "Follow-up" }
            };

            var map = new Dictionary<string, Appointment>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in seeds)
            {
                var appt = await context.Appointments.FirstOrDefaultAsync(a => a.AppointmentCode == item.Code);
                if (appt == null)
                {
                    appt = new Appointment
                    {
                        Id = Guid.NewGuid(),
                        AppointmentCode = item.Code,
                        PatientId = patients[item.Patient].Id,
                        DoctorId = doctors[item.Doctor].Id,
                        StaffId = staffId,
                        AppointmentDate = item.Date,
                        AppointmentTime = item.Time,
                        Status = item.Status,
                        Reason = item.Reason,
                        CreatedAt = item.Date.AddDays(-1)
                    };
                    if (item.Status == AppointmentStatus.CheckedIn || item.Status == AppointmentStatus.Completed)
                    {
                        appt.CheckedInAt = item.Date.Add(item.Time).AddMinutes(-10).ToUniversalTime();
                        appt.CheckInChannel = "Seed";
                    }
                    context.Appointments.Add(appt);
                }
                else
                {
                    appt.PatientId = patients[item.Patient].Id;
                    appt.DoctorId = doctors[item.Doctor].Id;
                    appt.StaffId = staffId;
                    appt.AppointmentDate = item.Date;
                    appt.AppointmentTime = item.Time;
                    appt.Status = item.Status;
                    appt.Reason = item.Reason;
                    if (item.Status == AppointmentStatus.CheckedIn || item.Status == AppointmentStatus.Completed)
                    {
                        appt.CheckedInAt ??= item.Date.Add(item.Time).AddMinutes(-10).ToUniversalTime();
                        appt.CheckInChannel ??= "Seed";
                    }
                }

                map[item.Code] = appt;
            }

            await context.SaveChangesAsync();
            return map;
        }

        private static async Task EnsureQueueForCheckedInAsync(
            ClinicDbContext context,
            Dictionary<string, Appointment> appointments,
            Dictionary<string, Room> rooms)
        {
            var appt = appointments["AP-DEMO-002"];
            var room = rooms["RM-SUR-01"];

            var exists = await context.QueueEntries.AnyAsync(q => q.AppointmentId == appt.Id && q.Status != QueueStatus.Skipped);
            if (exists) return;

            var dayStart = DateTime.Today;
            var dayEnd = dayStart.AddDays(1);
            var max = await context.QueueEntries
                .Where(q => q.RoomId == room.Id && q.QueuedAt >= dayStart && q.QueuedAt < dayEnd)
                .Select(q => (int?)q.QueueNumber)
                .MaxAsync() ?? 0;

            context.QueueEntries.Add(new QueueEntry
            {
                Id = Guid.NewGuid(),
                AppointmentId = appt.Id,
                RoomId = room.Id,
                QueueNumber = max + 1,
                Status = QueueStatus.Waiting,
                IsPriority = true,
                QueuedAt = DateTime.Now
            });

            await context.SaveChangesAsync();
        }

        private static async Task EnsureBillingDemoAsync(
            ClinicDbContext context,
            Dictionary<string, Appointment> appointments)
        {
            await EnsureClinicInvoiceAsync(context, appointments["AP-DEMO-003"], 320000m, true, DateTime.Today.AddDays(-1).AddHours(10));
            await EnsureClinicInvoiceAsync(context, appointments["AP-DEMO-004"], 480000m, true, DateTime.Today.AddDays(-1).AddHours(11));
            await EnsureClinicInvoiceAsync(context, appointments["AP-DEMO-001"], 300000m, false, DateTime.Today.AddHours(9));

            var appt = appointments["AP-DEMO-004"];
            var record = await context.MedicalRecords.FirstOrDefaultAsync(r => r.AppointmentId == appt.Id);
            if (record == null)
            {
                record = new MedicalRecord
                {
                    Id = Guid.NewGuid(),
                    AppointmentId = appt.Id,
                    DoctorId = appt.DoctorId!.Value,
                    PatientId = appt.PatientId,
                    Symptoms = "Đau vùng xoang mũi",
                    Diagnosis = "Viêm xoang",
                    Treatment = "Điều trị bằng thuốc",
                    Note = "Hồ sơ khởi tạo",
                    InsuranceCoverPercent = 0.5m,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                };
                context.MedicalRecords.Add(record);
                await context.SaveChangesAsync();
            }

            var hasTest = await context.ClinicalTests.AnyAsync(t => t.MedicalRecordId == record.Id);
            if (!hasTest)
            {
                context.ClinicalTests.Add(new ClinicalTest
                {
                    MedicalRecordId = record.Id,
                    TestName = "X-Quang",
                    Status = "Hoàn thành",
                    Result = "Không phát hiện tổn thương cấp tính",
                    TechnicianName = "Kỹ thuật viên Demo",
                    ResultAt = DateTime.UtcNow.AddDays(-1)
                });
                await context.SaveChangesAsync();
            }

            var prescription = await context.Prescriptions
                .Include(p => p.PrescriptionDetails)
                .FirstOrDefaultAsync(p => p.MedicalRecordId == record.Id);

            if (prescription == null)
            {
                prescription = new Prescription
                {
                    Id = Guid.NewGuid(),
                    MedicalRecordId = record.Id,
                    Note = "Seed prescription",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                };
                context.Prescriptions.Add(prescription);
                await context.SaveChangesAsync();
                prescription.PrescriptionDetails = new List<PrescriptionDetail>();
            }

            if (!prescription.PrescriptionDetails.Any())
            {
                context.PrescriptionDetails.AddRange(
                    new PrescriptionDetail
                    {
                        Id = Guid.NewGuid(),
                        PrescriptionId = prescription.Id,
                        MedicineName = "Paracetamol",
                        Dosage = "1 viên",
                        Frequency = "2/day",
                        Duration = 3,
                        TotalQuantity = 6,
                        UnitPrice = 3000m
                    },
                    new PrescriptionDetail
                    {
                        Id = Guid.NewGuid(),
                        PrescriptionId = prescription.Id,
                        MedicineName = "Cetirizine",
                        Dosage = "1 viên",
                        Frequency = "1/day",
                        Duration = 5,
                        TotalQuantity = 5,
                        UnitPrice = 4000m
                    }
                );
                await context.SaveChangesAsync();
            }

            var drugInvoice = await context.Invoices.FirstOrDefaultAsync(i => i.PrescriptionId == prescription.Id);
            if (drugInvoice == null)
            {
                var amount = await context.PrescriptionDetails
                    .Where(d => d.PrescriptionId == prescription.Id)
                    .SumAsync(d => d.UnitPrice * d.TotalQuantity);

                drugInvoice = new Invoice
                {
                    Id = Guid.NewGuid(),
                    AppointmentId = appt.Id,
                    PrescriptionId = prescription.Id,
                    InvoiceType = InvoiceType.Drug,
                    Amount = amount,
                    TotalDeposit = 0m,
                    BalanceDue = amount,
                    IsPaid = false,
                    CreatedAt = DateTime.UtcNow
                };
                context.Invoices.Add(drugInvoice);
                await context.SaveChangesAsync();

                var details = await context.PrescriptionDetails
                    .Where(d => d.PrescriptionId == prescription.Id)
                    .ToListAsync();

                context.InvoiceLines.AddRange(
                    details.Select(d => new InvoiceLine
                    {
                        Id = Guid.NewGuid(),
                        InvoiceId = drugInvoice.Id,
                        Description = $"Drug: {d.MedicineName}",
                        ItemType = "Drug",
                        Amount = d.UnitPrice * d.TotalQuantity,
                        Dosage = d.Dosage,
                        Duration = d.Duration
                    })
                );
                await context.SaveChangesAsync();
            }
        }

        private static async Task EnsureClinicInvoiceAsync(
            ClinicDbContext context,
            Appointment appointment,
            decimal amount,
            bool paid,
            DateTime paymentDate)
        {
            var invoice = await context.Invoices
                .Include(i => i.InvoiceLines)
                .FirstOrDefaultAsync(i => i.AppointmentId == appointment.Id && i.InvoiceType == InvoiceType.Clinic);

            if (invoice == null)
            {
                invoice = new Invoice
                {
                    Id = Guid.NewGuid(),
                    AppointmentId = appointment.Id,
                    InvoiceType = InvoiceType.Clinic,
                    Amount = amount,
                    TotalDeposit = 0m,
                    BalanceDue = amount,
                    IsPaid = paid,
                    CreatedAt = paymentDate.AddMinutes(-10),
                    PaymentDate = paid ? paymentDate : null
                };
                context.Invoices.Add(invoice);
                await context.SaveChangesAsync();

                context.InvoiceLines.Add(new InvoiceLine
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = invoice.Id,
                    Description = "Phí khám và tư vấn",
                    ItemType = "Consultation",
                    Amount = amount
                });
            }
            else
            {
                invoice.Amount = amount;
                invoice.BalanceDue = amount;
                invoice.IsPaid = paid;
                invoice.PaymentDate = paid ? paymentDate : null;
            }

            var hasPayment = await context.Payments.AnyAsync(p => p.InvoiceId == invoice.Id && !p.IsDeposit);
            if (paid && !hasPayment)
            {
                context.Payments.Add(new Payment
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = invoice.Id,
                    AppointmentId = appointment.Id,
                    Amount = amount,
                    DepositAmount = 0m,
                    IsDeposit = false,
                    Method = PaymentMethod.cash,
                    PaymentDate = paymentDate
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
