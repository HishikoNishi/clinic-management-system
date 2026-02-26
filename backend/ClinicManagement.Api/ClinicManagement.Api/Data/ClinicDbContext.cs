using ClinicManagement.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;

namespace ClinicManagement.Api.Data
{
    public class ClinicDbContext : DbContext
    {
        public ClinicDbContext(DbContextOptions<ClinicDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<Patient> Patients { get; set; } = null!;
        public DbSet<Doctor> Doctors => Set<Doctor>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ================= ROLE =================
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(r => r.Name).IsUnique();
            });

            // ================= USER =================
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.Property(u => u.Username)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.PasswordHash)
                    .IsRequired();

                entity.HasIndex(u => u.Username).IsUnique();

                entity.HasOne(u => u.RoleNavigation)
                    .WithMany(r => r.Users)
                    .HasForeignKey(u => u.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ================= DOCTOR =================
            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasKey(d => d.Id);

                entity.Property(d => d.Code)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.Property(d => d.Specialty)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(d => d.LicenseNumber)
                      .HasMaxLength(50);

                entity.HasOne(d => d.User)
                      .WithOne(u => u.Doctor)
                      .HasForeignKey<Doctor>(d => d.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(d => d.UserId).IsUnique();
            });

            // ================= APPOINTMENT =================
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(a => a.Id);

                entity.Property(a => a.Reason)
                      .HasMaxLength(500);

                entity.Property(a => a.Status)
                      .HasConversion<string>();

                entity.Property(a => a.AppointmentCode)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.HasIndex(a => a.AppointmentCode).IsUnique();

                entity.HasOne(a => a.Patient)
                      .WithMany(p => p.Appointments)
                      .HasForeignKey(a => a.PatientId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Doctor)
                      .WithMany(d => d.Appointments)
                      .HasForeignKey(a => a.DoctorId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ================= PATIENT =================
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.FullName)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(p => p.Gender)
                      .HasConversion<string>()
                      .IsRequired();
            });

            // ================= SEED DATA =================
            var hasher = new PasswordHasher<User>();

            // Fixed GUIDs cho Role
            var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var doctorRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var staffRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var guestRoleId = Guid.Parse("44444444-4444-4444-4444-444444444444");

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = adminRoleId, Name = "Admin" },
                new Role { Id = doctorRoleId, Name = "Doctor" },
                new Role { Id = staffRoleId, Name = "Staff" },
                new Role { Id = guestRoleId, Name = "Guest" }
            );

            // Fixed GUIDs cho User
            var adminUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            var staffUserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
            var doctorUser1Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
            var doctorUser2Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
            var doctorUser3Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");

            var adminUser = new User
            {
                Id = adminUserId,
                Username = "admin",
                PasswordHash = hasher.HashPassword(null, "Admin@123"),
                RoleId = adminRoleId
            };

            var staffUser = new User
            {
                Id = staffUserId,
                Username = "staff1",
                PasswordHash = hasher.HashPassword(null, "Staff@123"),
                RoleId = staffRoleId
            };

            var doctorUser1 = new User
            {
                Id = doctorUser1Id,
                Username = "doctor1",
                PasswordHash = hasher.HashPassword(null, "Doctor@123"),
                RoleId = doctorRoleId
            };

            var doctorUser2 = new User
            {
                Id = doctorUser2Id,
                Username = "doctor2",
                PasswordHash = hasher.HashPassword(null, "Doctor@123"),
                RoleId = doctorRoleId
            };

            var doctorUser3 = new User
            {
                Id = doctorUser3Id,
                Username = "doctor3",
                PasswordHash = hasher.HashPassword(null, "Doctor@123"),
                RoleId = doctorRoleId
            };

            modelBuilder.Entity<User>().HasData(adminUser, staffUser, doctorUser1, doctorUser2, doctorUser3);

            // Doctors (GUID cố định)
            modelBuilder.Entity<Doctor>().HasData(
                new Doctor
                {
                    Id = Guid.Parse("99999999-0000-0000-0000-000000000001"),
                    Code = "BS001",
                    Specialty = "Nội tổng quát",
                    LicenseNumber = "LIC001",
                    Status = DoctorStatus.Active,
                    UserId = doctorUser1Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Doctor
                {
                    Id = Guid.Parse("99999999-0000-0000-0000-000000000002"),
                    Code = "BS002",
                    Specialty = "Da liễu",
                    LicenseNumber = "LIC002",
                    Status = DoctorStatus.Active,
                    UserId = doctorUser2Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Doctor
                {
                    Id = Guid.Parse("99999999-0000-0000-0000-000000000003"),
                    Code = "BS003",
                    Specialty = "Tai mũi họng",
                    LicenseNumber = "LIC003",
                    Status = DoctorStatus.Active,
                    UserId = doctorUser3Id,
                    CreatedAt = DateTime.UtcNow
                }
            );
        }
    }
}
