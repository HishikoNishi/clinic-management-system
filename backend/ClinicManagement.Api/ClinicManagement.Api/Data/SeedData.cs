using System;
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
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();

            var roles = await context.Roles.AsNoTracking().ToListAsync();
            if (!roles.Any()) return;

            var doctorRoleId = roles.First(r => r.Name == "Doctor").Id;
            var staffRoleId = roles.First(r => r.Name == "Staff").Id;
            var technicianRoleId = roles.First(r => r.Name == "Technician").Id;
            var cashierRoleId = roles.First(r => r.Name == "Cashier").Id;

            // Fixed IDs from seed
            var depNoi = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var depNgoai = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var depSan = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var depNhi = Guid.Parse("44444444-4444-4444-4444-444444444444");
            var depRang = Guid.Parse("55555555-5555-5555-5555-555555555555");
            var depTmh = Guid.Parse("66666666-6666-6666-6666-666666666666");
            var depKham = Guid.Parse("77777777-7777-7777-7777-777777777777");

            var specNoi = Guid.Parse("aaaa1111-1111-1111-1111-111111111111");
            var specNgoai = Guid.Parse("bbbb1111-2222-2222-2222-222222222222");
            var specSan = Guid.Parse("cccc1111-3333-3333-3333-333333333333");
            var specNhi = Guid.Parse("dddd1111-4444-4444-4444-444444444444");
            var specRang = Guid.Parse("eeee1111-5555-5555-5555-555555555555");
            var specTmh = Guid.Parse("ffff1111-6666-6666-6666-666666666666");

            var passwordHasher = new PasswordHasher<User>();

            // Doctors
            var doctorSeeds = new[]
            {
                new { Username = "doctor1", FullName = "Bac si Noi 1", Email = "doctor1@example.com", Phone = "0900000001", Code = "DOC001", Dep = depNoi, Spec = specNoi, License = "LIC-0001" },
                new { Username = "doctor2", FullName = "Bac si Ngoai 1", Email = "doctor2@example.com", Phone = "0900000002", Code = "DOC002", Dep = depNgoai, Spec = specNgoai, License = "LIC-0002" },
                new { Username = "doctor3", FullName = "Bac si San 1", Email = "doctor3@example.com", Phone = "0900000003", Code = "DOC003", Dep = depSan, Spec = specSan, License = "LIC-0003" },
                new { Username = "doctor4", FullName = "Bac si Nhi 1", Email = "doctor4@example.com", Phone = "0900000004", Code = "DOC004", Dep = depNhi, Spec = specNhi, License = "LIC-0004" },
                new { Username = "doctor5", FullName = "Bac si Rang Ham Mat 1", Email = "doctor5@example.com", Phone = "0900000005", Code = "DOC005", Dep = depRang, Spec = specRang, License = "LIC-0005" },
                new { Username = "doctor6", FullName = "Bac si Tai Mui Hong 1", Email = "doctor6@example.com", Phone = "0900000006", Code = "DOC006", Dep = depTmh, Spec = specTmh, License = "LIC-0006" },
            };

            foreach (var docSeed in doctorSeeds)
            {
                var user = await EnsureUserAsync(context, docSeed.Username, docSeed.FullName, docSeed.Email, docSeed.Phone,
                    doctorRoleId, passwordHasher, "Doctor@123");

                if (!await context.Doctors.AnyAsync(d => d.Code == docSeed.Code))
                {
                    context.Doctors.Add(new Doctor
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        Code = docSeed.Code,
                        FullName = docSeed.FullName,
                        DepartmentId = docSeed.Dep,
                        SpecialtyId = docSeed.Spec,
                        LicenseNumber = docSeed.License,
                        Status = DoctorStatus.Active
                    });
                }
            }

            // Staff (front desk)
            var staffUser = await EnsureUserAsync(context, "staff1", "Le tan 1", "staff1@example.com", "0900000002",
                staffRoleId, passwordHasher, "Staff@123");

            if (!await context.Staffs.AnyAsync(s => s.Role == "Staff"))
            {
                context.Staffs.Add(new Staff
                {
                    Id = Guid.NewGuid(),
                    UserId = staffUser.Id,
                    Code = "ST001",
                    FullName = staffUser.FullName,
                    Role = "Staff",
                    DepartmentId = depKham,
                    IsActive = true
                });
            }

            // Technician
            var techUser = await EnsureUserAsync(context, "tech1", "Ky thuat vien 1", "tech1@example.com", "0900000003",
                technicianRoleId, passwordHasher, "Tech@123");

            if (!await context.Staffs.AnyAsync(s => s.Role == "Technician"))
            {
                context.Staffs.Add(new Staff
                {
                    Id = Guid.NewGuid(),
                    UserId = techUser.Id,
                    Code = "TE001",
                    FullName = techUser.FullName,
                    Role = "Technician",
                    DepartmentId = depNoi,
                    IsActive = true
                });
            }

            // Cashier
            var cashierUser = await EnsureUserAsync(context, "cashier1", "Thu ngan 1", "cashier1@example.com", "0900000004",
                cashierRoleId, passwordHasher, "Cashier@123");

            if (!await context.Staffs.AnyAsync(s => s.Role == "Cashier"))
            {
                context.Staffs.Add(new Staff
                {
                    Id = Guid.NewGuid(),
                    UserId = cashierUser.Id,
                    Code = "CA001",
                    FullName = cashierUser.FullName,
                    Role = "Cashier",
                    DepartmentId = depKham,
                    IsActive = true
                });
            }

            // Patient + sample appointment
            if (!await context.Patients.AnyAsync())
            {
                var patientId = Guid.NewGuid();
                context.Patients.Add(new Patient
                {
                    Id = patientId,
                    FullName = "Nguyen Van A",
                    DateOfBirth = new DateTime(1995, 1, 1),
                    Gender = Gender.Male,
                    Phone = "0911111111",
                    Email = "patient1@example.com",
                    Address = "HCMC"
                });

                var doctorId = await context.Doctors.Select(d => d.Id).FirstOrDefaultAsync();
                var staffId = await context.Staffs.Where(s => s.Role == "Staff").Select(s => s.Id).FirstOrDefaultAsync();

                if (doctorId != Guid.Empty)
                {
                    context.Appointments.Add(new Appointment
                    {
                        Id = Guid.NewGuid(),
                        PatientId = patientId,
                        DoctorId = doctorId,
                        StaffId = staffId,
                        AppointmentDate = DateTime.Today,
                        AppointmentTime = new TimeSpan(9, 0, 0),
                        AppointmentCode = "APSEED01",
                        Reason = "Kham tong quat (seed)",
                        Status = AppointmentStatus.Pending
                    });
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task<User> EnsureUserAsync(
            ClinicDbContext context,
            string username,
            string fullName,
            string email,
            string phone,
            Guid roleId,
            PasswordHasher<User> passwordHasher,
            string password)
        {
            var existing = await context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (existing != null) return existing;

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                FullName = fullName,
                Email = email,
                PhoneNumber = phone,
                RoleId = roleId,
                IsActive = true
            };
            user.PasswordHash = passwordHasher.HashPassword(user, password);
            context.Users.Add(user);
            return user;
        }
    }
}
