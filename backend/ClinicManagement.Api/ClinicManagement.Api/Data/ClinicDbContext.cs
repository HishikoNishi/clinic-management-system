<<<<<<< HEAD
using ClinicManagement.Api.Models;
=======
﻿using ClinicManagement.Api.Models;
>>>>>>> origin/feature/appointment-api-Nhan
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
        public DbSet<Doctor> Doctors { get; set; } = null!;
<<<<<<< HEAD
        public DbSet<Staff> Staffs { get; set; } = null!;
=======
>>>>>>> origin/feature/appointment-api-Nhan
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<Patient> Patients { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            /* ================================
             * ROLE
             * ================================ */
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.Property(r => r.Name)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.HasIndex(r => r.Name).IsUnique();
            });

            /* ================================
             * USER
             * ================================ */
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

            /* ================================
             * DOCTOR
             * ================================ */
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

                entity.HasIndex(d => d.Code).IsUnique();

                entity.HasOne(d => d.User)
                      .WithOne(u => u.Doctor)
                      .HasForeignKey<Doctor>(d => d.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(d => d.UserId).IsUnique();
            });

<<<<<<< HEAD
            /* ================================
             * STAFF
             * ================================ */
            modelBuilder.Entity<Staff>(entity =>
            {
                entity.HasKey(s => s.Id);

                entity.Property(s => s.Code)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.Property(s => s.FullName)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(s => s.Position)
                      .HasMaxLength(100);

                entity.HasIndex(s => s.Code).IsUnique();

                entity.HasOne(s => s.User)
                      .WithOne(u => u.Staff)
                      .HasForeignKey<Staff>(s => s.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(s => s.UserId).IsUnique();
            });
=======
           
>>>>>>> origin/feature/appointment-api-Nhan

            /* ================================
             * SEED DATA
             * ================================ */

<<<<<<< HEAD
            var adminRoleId  = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var doctorRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var staffRoleId  = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var guestRoleId  = Guid.Parse("44444444-4444-4444-4444-444444444444");
=======
            var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var doctorRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var staffRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var guestRoleId = Guid.Parse("44444444-4444-4444-4444-444444444444");
>>>>>>> origin/feature/appointment-api-Nhan

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = adminRoleId, Name = "Admin" },
                new Role { Id = doctorRoleId, Name = "Doctor" },
                new Role { Id = staffRoleId, Name = "Staff" },
                new Role { Id = guestRoleId, Name = "Guest" }
            );

            var adminUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

            var adminUser = new User
            {
                Id = adminUserId,
                Username = "admin",
                RoleId = adminRoleId
            };

            adminUser.PasswordHash =
                new PasswordHasher<User>().HashPassword(adminUser, "Admin@123");

            modelBuilder.Entity<User>().HasData(adminUser);

            /* ================================
             * APPOINTMENT
             * ================================ */
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

                entity.Property(a => a.AppointmentDate)
                      .IsRequired();

                entity.Property(a => a.AppointmentTime)
                      .IsRequired();

                entity.HasIndex(a => a.AppointmentCode)
                      .IsUnique();

                entity.HasOne(a => a.Patient)
                      .WithMany(p => p.Appointments)
.HasForeignKey(a => a.PatientId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Doctor)
                      .WithMany(d => d.Appointments)
                      .HasForeignKey(a => a.DoctorId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            /* ================================
             * PATIENT
             * ================================ */
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
        }
    }
}