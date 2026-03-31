using ClinicManagement.Api.Models;
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
        public DbSet<Staff> Staffs { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<Patient> Patients { get; set; } = null!;
        public DbSet<Invoice> Invoices { get; set; } = null!;
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PrescriptionDetail> PrescriptionDetails { get; set; }
        public DbSet<ClinicalTest> ClinicalTests { get; set; }
        public DbSet<EmailOtp> EmailOtps { get; set; }
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<InvoiceLine> InvoiceLines { get; set; } = null!;
        public DbSet<InsurancePlan> InsurancePlans { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<Specialty> Specialties { get; set; } = null!;

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

                entity.Property(u => u.Email)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(u => u.FullName)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(u => u.PhoneNumber)
                      .HasMaxLength(15);

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

                entity.Property(d => d.FullName)
                      .IsRequired()
                      .HasMaxLength(30);

                entity.Property(d => d.LicenseNumber)
                      .HasMaxLength(50);

                entity.HasIndex(d => d.Code).IsUnique();

                entity.HasOne(d => d.User)
                      .WithOne(u => u.Doctor)
                      .HasForeignKey<Doctor>(d => d.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(d => d.UserId).IsUnique();

                entity.HasOne(d => d.Department)
                      .WithMany(dep => dep.Doctors)
                      .HasForeignKey(d => d.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Specialty)
                      .WithMany()
                      .HasForeignKey(d => d.SpecialtyId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            /* ================================
             * DEPARTMENT
             * ================================ */
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Name).IsRequired().HasMaxLength(150);
                entity.HasIndex(d => d.Name).IsUnique();
            });

            /* ================================
             * SPECIALTY
             * ================================ */
            modelBuilder.Entity<Specialty>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Name).IsRequired().HasMaxLength(150);
                entity.HasOne(s => s.Department)
                      .WithMany(d => d.Specialties)
                      .HasForeignKey(s => s.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

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
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Amount)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();
                entity.HasOne(i => i.Appointment)
                      .WithOne()
                      .HasForeignKey<Invoice>(i => i.AppointmentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            /* ================================
           * Prescription
           * ================================ */

             modelBuilder.Entity<Prescription>()
                      .HasOne(p => p.MedicalRecord)
                      .WithMany()
                      .HasForeignKey(p => p.MedicalRecordId)
                      .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PrescriptionDetail>()
                .HasOne(d => d.Prescription)
                .WithMany(p => p.PrescriptionDetails)
                .HasForeignKey(d => d.PrescriptionId);

            modelBuilder.Entity<ClinicalTest>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.HasOne(t => t.MedicalRecord)
                      .WithMany()
                      .HasForeignKey(t => t.MedicalRecordId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<EmailOtp>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.Email).IsRequired().HasMaxLength(150);
                entity.Property(o => o.Code).IsRequired().HasMaxLength(6);
                entity.HasIndex(o => new { o.Email, o.IsUsed });
            });

            modelBuilder.Entity<InvoiceLine>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.Property(l => l.Description).IsRequired().HasMaxLength(255);
                entity.Property(l => l.ItemType).IsRequired().HasMaxLength(50);
                entity.Property(l => l.Amount).HasColumnType("decimal(18,2)");

                entity.HasOne(l => l.Invoice)
                      .WithMany(i => i.InvoiceLines)
                      .HasForeignKey(l => l.InvoiceId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<InsurancePlan>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Code).IsRequired().HasMaxLength(50);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
                entity.Property(p => p.CoveragePercent).HasColumnType("decimal(5,4)");
                entity.Property(p => p.ExpiryDate);
                entity.HasIndex(p => p.Code).IsUnique();
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Token).IsRequired().HasMaxLength(200);
                entity.HasIndex(r => r.Token).IsUnique();
                entity.HasOne(r => r.User)
                      .WithMany()
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            /* ================================
             * PAYMENT
             * ================================ */

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Amount)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();

                entity.Property(p => p.Method)
                      .HasConversion<string>() // enum lưu dạng string
                      .IsRequired();

                entity.Property(p => p.PaymentDate)
                      .IsRequired();

                entity.HasOne(p => p.Invoice)
                      .WithMany(i => i.Payments)
                      .HasForeignKey(p => p.InvoiceId)
                      .OnDelete(DeleteBehavior.Restrict);
            });


            /* ================================
             * DEPARTMENTS & SPECIALTIES SEED
             * ================================ */
            var depNoi   = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var depNgoai = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var depSan   = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var depNhi   = Guid.Parse("44444444-4444-4444-4444-444444444444");
            var depRang  = Guid.Parse("55555555-5555-5555-5555-555555555555");
            var depTmh   = Guid.Parse("66666666-6666-6666-6666-666666666666");
            var depKham  = Guid.Parse("77777777-7777-7777-7777-777777777777");

            modelBuilder.Entity<Department>().HasData(
                new Department { Id = depNoi, Name = "Khoa Nội" },
                new Department { Id = depNgoai, Name = "Khoa Ngoại" },
                new Department { Id = depSan, Name = "Khoa Sản" },
                new Department { Id = depNhi, Name = "Khoa Nhi" },
                new Department { Id = depRang, Name = "Răng Hàm Mặt" },
                new Department { Id = depTmh, Name = "Tai Mũi Họng" },
                new Department { Id = depKham, Name = "Khám tổng quát" }
            );

            modelBuilder.Entity<Specialty>().HasData(
                new Specialty { Id = Guid.Parse("aaaa1111-1111-1111-1111-111111111111"), Name = "Nội tổng quát", DepartmentId = depNoi },
                new Specialty { Id = Guid.Parse("aaaa2222-1111-1111-1111-111111111111"), Name = "Nội tim mạch", DepartmentId = depNoi },
                new Specialty { Id = Guid.Parse("aaaa3333-1111-1111-1111-111111111111"), Name = "Nội tiêu hóa", DepartmentId = depNoi },
                new Specialty { Id = Guid.Parse("bbbb1111-2222-2222-2222-222222222222"), Name = "Ngoại tổng quát", DepartmentId = depNgoai },
                new Specialty { Id = Guid.Parse("bbbb2222-2222-2222-2222-222222222222"), Name = "Chấn thương chỉnh hình", DepartmentId = depNgoai },
                new Specialty { Id = Guid.Parse("cccc1111-3333-3333-3333-333333333333"), Name = "Sản phụ khoa", DepartmentId = depSan },
                new Specialty { Id = Guid.Parse("cccc2222-3333-3333-3333-333333333333"), Name = "Khám thai", DepartmentId = depSan },
                new Specialty { Id = Guid.Parse("dddd1111-4444-4444-4444-444444444444"), Name = "Nhi tổng quát", DepartmentId = depNhi },
                new Specialty { Id = Guid.Parse("eeee1111-5555-5555-5555-555555555555"), Name = "Nha tổng quát", DepartmentId = depRang },
                new Specialty { Id = Guid.Parse("eeee2222-5555-5555-5555-555555555555"), Name = "Niềng răng", DepartmentId = depRang },
                new Specialty { Id = Guid.Parse("ffff1111-6666-6666-6666-666666666666"), Name = "Khám TMH", DepartmentId = depTmh },
                new Specialty { Id = Guid.Parse("aaaa4444-7777-7777-7777-777777777777"), Name = "Khám sức khỏe", DepartmentId = depKham },
                new Specialty { Id = Guid.Parse("aaaa5555-7777-7777-7777-777777777777"), Name = "Tiêm chủng", DepartmentId = depKham }
            );


            /* ================================
             * SEED DATA (GUID CỐ ĐỊNH)
             * ================================ */

            var adminRoleId  = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var doctorRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var staffRoleId  = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var guestRoleId  = Guid.Parse("44444444-4444-4444-4444-444444444444");
            var technicianRoleId = Guid.Parse("55555555-5555-5555-5555-555555555555");
            var cashierRoleId = Guid.Parse("66666666-6666-6666-6666-666666666666");

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = adminRoleId, Name = "Admin" },
                new Role { Id = doctorRoleId, Name = "Doctor" },
                new Role { Id = staffRoleId, Name = "Staff" },
                new Role { Id = guestRoleId, Name = "Guest" },
                new Role { Id = technicianRoleId, Name = "Technician" },
                new Role { Id = cashierRoleId, Name = "Cashier" }
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

            // Seed bảo hiểm mẫu
            modelBuilder.Entity<InsurancePlan>().HasData(
                new InsurancePlan { Id = Guid.Parse("77777777-0000-0000-0000-000000000001"), Code = "BHYT-A", Name = "BHYT Nhà nước A", CoveragePercent = 0.8m, Note = "Giảm 80% tổng phí (trừ phụ thu)", IsActive = true },
                new InsurancePlan { Id = Guid.Parse("77777777-0000-0000-0000-000000000002"), Code = "CORP-ACME", Name = "Bảo hiểm công ty ACME", CoveragePercent = 0.5m, Note = "Giảm 50%", IsActive = true },
                new InsurancePlan { Id = Guid.Parse("77777777-0000-0000-0000-000000000003"), Code = "VIP-GOLD", Name = "VIP Gold", CoveragePercent = 0.9m, Note = "Giảm 90%", IsActive = true }
            );
        }
    }
}
