using ClinicManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Api.Data
{
    public class ClinicDbContext : DbContext
    {
        public ClinicDbContext(DbContextOptions<ClinicDbContext> options) : base(options)
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

            // Configure Role entity
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Name)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.HasIndex(r => r.Name).IsUnique();
            });

            // Configure User entity
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

            // Seed roles
            var adminRole = new Role { Id = Guid.NewGuid(), Name = "Admin" };
            var doctorRole = new Role { Id = Guid.NewGuid(), Name = "Doctor" };
            var staffRole = new Role { Id = Guid.NewGuid(), Name = "Staff" };
            var guestRole = new Role { Id = Guid.NewGuid(), Name = "Guest" };

            modelBuilder.Entity<Role>().HasData(adminRole, doctorRole, staffRole, guestRole);

            // Seed admin user
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                PasswordHash = new Microsoft.AspNetCore.Identity.PasswordHasher<User>()
                    .HashPassword(null, "Admin@123"),
                RoleId = adminRole.Id
            };
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

                entity.HasIndex(a => a.AppointmentCode)
                      .IsUnique();

                entity.HasOne(a => a.Patient)
                      .WithMany(p => p.Appointments)
                      .HasForeignKey(a => a.PatientId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Doctor)
                      .WithMany()
                      .HasForeignKey(a => a.DoctorId)
                      .OnDelete(DeleteBehavior.SetNull);
            });


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



            modelBuilder.Entity<User>().HasData(adminUser);
        }
    }
}
