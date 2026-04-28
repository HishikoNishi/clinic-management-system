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
        public DbSet<Medicine> Medicines { get; set; } = null!;
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; } = null!;
        public DbSet<DoctorWeeklySchedule> DoctorWeeklySchedules { get; set; } = null!;
        public DbSet<DoctorScheduleOverrideDay> DoctorScheduleOverrideDays { get; set; } = null!;
        public DbSet<DoctorShiftRequest> DoctorShiftRequests { get; set; } = null!;
        public DbSet<AppNotification> Notifications { get; set; } = null!;
        public DbSet<Room> Rooms { get; set; } = null!;
        public DbSet<QueueEntry> QueueEntries { get; set; } = null!;
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

            modelBuilder.Entity<DoctorSchedule>(entity =>
            {
                entity.HasKey(s => s.Id);

                entity.Property(s => s.WorkDate)
                      .IsRequired();

                entity.Property(s => s.ShiftCode)
                      .IsRequired()
                      .HasMaxLength(32);

                entity.Property(s => s.SlotLabel)
                      .IsRequired()
                      .HasMaxLength(64);

                entity.Property(s => s.StartTime)
                      .IsRequired();

                entity.Property(s => s.EndTime)
                      .IsRequired();

                entity.HasIndex(s => new { s.DoctorId, s.WorkDate, s.StartTime })
                      .IsUnique();

                entity.HasIndex(s => new { s.RoomId, s.WorkDate, s.StartTime })
                      .IsUnique()
                      .HasFilter("[RoomId] IS NOT NULL");

                entity.HasOne(s => s.Doctor)
                      .WithMany(d => d.Schedules)
                      .HasForeignKey(s => s.DoctorId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.Room)
                      .WithMany()
                      .HasForeignKey(s => s.RoomId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DoctorWeeklySchedule>(entity =>
            {
                entity.HasKey(s => s.Id);

                entity.Property(s => s.DayOfWeek)
                      .IsRequired();

                entity.Property(s => s.ShiftCode)
                      .IsRequired()
                      .HasMaxLength(32);

                entity.Property(s => s.SlotLabel)
                      .IsRequired()
                      .HasMaxLength(64);

                entity.HasIndex(s => new { s.DoctorId, s.DayOfWeek, s.StartTime })
                      .IsUnique();

                entity.HasIndex(s => new { s.RoomId, s.DayOfWeek, s.StartTime })
                      .IsUnique()
                      .HasFilter("[RoomId] IS NOT NULL");

                entity.HasOne(s => s.Doctor)
                      .WithMany(d => d.WeeklySchedules)
                      .HasForeignKey(s => s.DoctorId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.Room)
                      .WithMany()
                      .HasForeignKey(s => s.RoomId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DoctorScheduleOverrideDay>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.Property(o => o.WorkDate)
                      .IsRequired();

                entity.HasIndex(o => new { o.DoctorId, o.WorkDate })
                      .IsUnique();

                entity.HasOne(o => o.Doctor)
                      .WithMany(d => d.ScheduleOverrideDays)
                      .HasForeignKey(o => o.DoctorId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<DoctorShiftRequest>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.Property(r => r.RequestType)
                      .HasConversion<string>()
                      .IsRequired();

                entity.Property(r => r.Status)
                      .HasConversion<string>()
                      .IsRequired();

                entity.Property(r => r.WorkDate)
                      .IsRequired();

                entity.Property(r => r.ShiftCode)
                      .IsRequired()
                      .HasMaxLength(32);

                entity.Property(r => r.SlotLabel)
                      .IsRequired()
                      .HasMaxLength(64);

                entity.Property(r => r.Reason)
                      .IsRequired()
                      .HasMaxLength(1000);

                entity.Property(r => r.AdminNote)
                      .HasMaxLength(1000);

                entity.HasIndex(r => new { r.DoctorId, r.WorkDate, r.StartTime, r.Status });

                entity.HasOne(r => r.Doctor)
                      .WithMany(d => d.ShiftRequests)
                      .HasForeignKey(r => r.DoctorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.PreferredDoctor)
                      .WithMany(d => d.PreferredShiftRequests)
                      .HasForeignKey(r => r.PreferredDoctorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.ReplacementDoctor)
                      .WithMany(d => d.ReplacementShiftRequests)
                      .HasForeignKey(r => r.ReplacementDoctorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.ReviewedByUser)
                      .WithMany(u => u.ReviewedShiftRequests)
                      .HasForeignKey(r => r.ReviewedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<AppNotification>(entity =>
            {
                entity.HasKey(n => n.Id);

                entity.Property(n => n.Title)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(n => n.Message)
                      .IsRequired()
                      .HasMaxLength(1000);

                entity.Property(n => n.Type)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(n => n.Link)
                      .HasMaxLength(500);

                entity.HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });

                entity.HasOne(n => n.User)
                      .WithMany(u => u.Notifications)
                      .HasForeignKey(n => n.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.Property(r => r.Code)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.Property(r => r.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.HasIndex(r => r.Code)
                      .IsUnique();

                entity.HasOne(r => r.Department)
                      .WithMany()
                      .HasForeignKey(r => r.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<QueueEntry>(entity =>
            {
                entity.ToTable("Queues");

                entity.HasKey(q => q.Id);

                entity.Property(q => q.Status)
                      .HasConversion<string>()
                      .IsRequired();

                entity.Property(q => q.QueuedAt)
                      .IsRequired();

                entity.HasIndex(q => new { q.RoomId, q.QueuedAt });
                entity.HasIndex(q => q.AppointmentId);

                entity.HasOne(q => q.Appointment)
                      .WithMany(a => a.QueueEntries)
                      .HasForeignKey(q => q.AppointmentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(q => q.Room)
                      .WithMany(r => r.QueueEntries)
                      .HasForeignKey(q => q.RoomId)
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
                entity.Property(s => s.AvatarUrl)
                      .HasMaxLength(255);
                entity.Property(s => s.Position)
                      .HasMaxLength(100);

                entity.HasIndex(s => s.Code).IsUnique();

                entity.HasOne(s => s.User)
                      .WithOne(u => u.Staff)
                      .HasForeignKey<Staff>(s => s.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(s => s.UserId).IsUnique();

                entity.HasOne(s => s.Department)
                      .WithMany()
                      .HasForeignKey(s => s.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);
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

            modelBuilder.Entity<MedicalRecord>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.HasIndex(r => r.AppointmentId).IsUnique();
                entity.Property(r => r.Symptoms).HasMaxLength(1000);
                entity.Property(r => r.DetailedSymptoms).HasMaxLength(4000);
                entity.Property(r => r.PastMedicalHistory).HasMaxLength(4000);
                entity.Property(r => r.Allergies).HasMaxLength(2000);
                entity.Property(r => r.Occupation).HasMaxLength(200);
                entity.Property(r => r.Habits).HasMaxLength(2000);
                entity.Property(r => r.BloodPressure).HasMaxLength(20);
                entity.Property(r => r.HeightCm).HasColumnType("decimal(5,2)");
                entity.Property(r => r.WeightKg).HasColumnType("decimal(5,2)");
                entity.Property(r => r.Bmi).HasColumnType("decimal(5,2)");
                entity.Property(r => r.Temperature).HasColumnType("decimal(4,1)");
                entity.Property(r => r.InsuranceCoverPercent).HasColumnType("decimal(5,2)");
                entity.Property(r => r.Surcharge).HasColumnType("decimal(18,2)");
                entity.Property(r => r.Discount).HasColumnType("decimal(18,2)");
                entity.HasOne<Appointment>()
                      .WithMany()
                      .HasForeignKey(r => r.AppointmentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Medicine>(entity =>
            {
                entity.HasIndex(m => m.Name); 
                entity.Property(m => m.Price).HasColumnType("decimal(18,2)");
            });
            modelBuilder.Entity<Medicine>().HasData(
                new Medicine { Id = Guid.Parse("f001f001-1111-1111-1111-111111111111"), Name = "Paracetamol", DefaultDosage = "500mg", Unit = "Viên", Price = 2000, IsActive = true },
                new Medicine { Id = Guid.Parse("f002f002-2222-2222-2222-222222222222"), Name = "Panadol Extra", DefaultDosage = "500mg", Unit = "Viên", Price = 3500, IsActive = true },
                new Medicine { Id = Guid.Parse("f003f003-3333-3333-3333-333333333333"), Name = "Efferalgan", DefaultDosage = "500mg", Unit = "Viên", Price = 4000, IsActive = true },
                new Medicine { Id = Guid.Parse("f004f004-4444-4444-4444-444444444444"), Name = "Hapacol", DefaultDosage = "650mg", Unit = "Gói", Price = 2500, IsActive = true },
                new Medicine { Id = Guid.Parse("f005f005-5555-5555-5555-555555555555"), Name = "Amoxicillin", DefaultDosage = "500mg", Unit = "Viên", Price = 5000, IsActive = true },
                new Medicine { Id = Guid.Parse("f006f006-6666-6666-6666-666666666666"), Name = "Augmentin", DefaultDosage = "625mg", Unit = "Viên", Price = 18000, IsActive = true },
                new Medicine { Id = Guid.Parse("f007f007-7777-7777-7777-777777777777"), Name = "Cefalexin", DefaultDosage = "500mg", Unit = "Viên", Price = 4500, IsActive = true },
                new Medicine { Id = Guid.Parse("f008f008-8888-8888-8888-888888888888"), Name = "Azithromycin", DefaultDosage = "500mg", Unit = "Viên", Price = 25000, IsActive = true },
                new Medicine { Id = Guid.Parse("f009f009-9999-9999-9999-999999999999"), Name = "Gaviscon", DefaultDosage = "10ml", Unit = "Gói", Price = 15000, IsActive = true },
                new Medicine { Id = Guid.Parse("f010f010-0000-0000-0000-000000000010"), Name = "Maalox", DefaultDosage = "400mg", Unit = "Viên", Price = 3000, IsActive = true },
                new Medicine { Id = Guid.Parse("f011f011-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Name = "Berberin", DefaultDosage = "100mg", Unit = "Viên", Price = 500, IsActive = true },
                new Medicine { Id = Guid.Parse("f012f012-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Name = "Smecta", DefaultDosage = "3g", Unit = "Gói", Price = 5000, IsActive = true },
                new Medicine { Id = Guid.Parse("f013f013-cccc-cccc-cccc-cccccccccccc"), Name = "Decolgen", DefaultDosage = "N/A", Unit = "Vỉ", Price = 15000, IsActive = true },
                new Medicine { Id = Guid.Parse("f014f014-dddd-dddd-dddd-dddddddddddd"), Name = "Tiffy", DefaultDosage = "N/A", Unit = "Vỉ", Price = 12000, IsActive = true },
                new Medicine { Id = Guid.Parse("f015f015-eeee-eeee-eeee-eeeeeeeeeeee"), Name = "Siro Prospan", DefaultDosage = "100ml", Unit = "Chai", Price = 75000, IsActive = true },
                new Medicine { Id = Guid.Parse("f016f016-ffff-ffff-ffff-ffffffffffff"), Name = "Telfast", DefaultDosage = "180mg", Unit = "Viên", Price = 12000, IsActive = true },
                new Medicine { Id = Guid.Parse("f017f017-1212-1212-1212-121212121212"), Name = "Vitamin C", DefaultDosage = "500mg", Unit = "Viên sủi", Price = 3000, IsActive = true },
                new Medicine { Id = Guid.Parse("f018f018-2323-2323-2323-232323232323"), Name = "Enervon", DefaultDosage = "N/A", Unit = "Viên", Price = 2500, IsActive = true },
                new Medicine { Id = Guid.Parse("f019f019-3434-3434-3434-343434343434"), Name = "Zinc (Kẽm)", DefaultDosage = "20mg", Unit = "Viên", Price = 4000, IsActive = true },
                new Medicine { Id = Guid.Parse("f020f020-4545-4545-4545-454545454545"), Name = "Dầu cá Omega-3", DefaultDosage = "1000mg", Unit = "Viên", Price = 6000, IsActive = true }
            );
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.FullName).IsRequired().HasMaxLength(150);
                entity.Property(p => p.Gender).HasConversion<string>().IsRequired();

                entity.Property(p => p.PatientCode).HasMaxLength(20); 
                entity.Property(p => p.CitizenId).HasMaxLength(20);
                entity.Property(p => p.InsuranceCardNumber).HasMaxLength(20);

                entity.HasIndex(p => p.PatientCode).IsUnique();
                entity.HasIndex(p => p.CitizenId).IsUnique().HasFilter("[CitizenId] IS NOT NULL");
                entity.HasIndex(p => p.InsuranceCardNumber).IsUnique().HasFilter("[InsuranceCardNumber] IS NOT NULL");

            });
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Amount)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();
                entity.Property(i => i.TotalDeposit)
                      .HasColumnType("decimal(18,2)")
                      .HasDefaultValue(0m);
                entity.Property(i => i.BalanceDue)
                      .HasColumnType("decimal(18,2)")
                      .HasDefaultValue(0m);
                entity.Property(i => i.InvoiceType)
                      .HasConversion<string>()
                      .HasMaxLength(20)
                      .IsRequired()
                      .HasDefaultValue(InvoiceType.Clinic);
                entity.HasOne(i => i.Appointment)
                      .WithMany(a => a.Invoices)
                      .HasForeignKey(i => i.AppointmentId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(i => i.AppointmentId);
                entity.HasIndex(i => i.PrescriptionId);
            });
            /* ================================
           * Prescription
           * ================================ */

             modelBuilder.Entity<Prescription>()
                      .HasOne(p => p.MedicalRecord)
                      .WithMany()
                      .HasForeignKey(p => p.MedicalRecordId)
                      .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PrescriptionDetail>(entity =>
            {
                entity.Property(d => d.UnitPrice)
                      .HasColumnType("decimal(18,2)");

                entity.HasOne(d => d.Prescription)
                      .WithMany(p => p.PrescriptionDetails)
                      .HasForeignKey(d => d.PrescriptionId);
            });

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

                entity.Property(p => p.DepositAmount)
                      .HasColumnType("decimal(18,2)")
                      .HasDefaultValue(0m);

                entity.Property(p => p.IsDeposit)
                      .HasDefaultValue(false);

                entity.Property(p => p.AppointmentId)
                      .IsRequired();

                entity.Property(p => p.Method)
                      .HasConversion<string>()
                      .IsRequired();

                entity.Property(p => p.PaymentDate)
                      .IsRequired();

                entity.HasOne(p => p.Invoice)
                      .WithMany(i => i.Payments)
                      .HasForeignKey(p => p.InvoiceId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.Appointment)
                      .WithMany(a => a.Payments)
                      .HasForeignKey(p => p.AppointmentId)
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
            
            modelBuilder.Entity<Invoice>()
                .HasIndex(i => i.CreatedAt);

            modelBuilder.Entity<Invoice>()
                .HasIndex(i => i.InvoiceType);

            modelBuilder.Entity<Invoice>()
                .HasIndex(i => i.IsPaid);
        }
    }
}



