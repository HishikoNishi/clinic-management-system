using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;

namespace ClinicManagement.Api.Data
{
    public class ClinicDbContext : DbContext
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;
        private bool _isSavingAuditLogs;
        public ClinicDbContext(DbContextOptions<ClinicDbContext> options)
            : base(options)
        {
        }
        public ClinicDbContext(
            DbContextOptions<ClinicDbContext> options,
            IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Doctor> Doctors { get; set; } = null!;
        public DbSet<Staff> Staffs { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<Patient> Patients { get; set; } = null!;
        public DbSet<Invoice> Invoices { get; set; } = null!;
        public DbSet<MedicalRecord> MedicalRecords { get; set; } = null!;
        public DbSet<Prescription> Prescriptions { get; set; } = null!;
        public DbSet<PrescriptionDetail> PrescriptionDetails { get; set; } = null!;
        public DbSet<ClinicalTest> ClinicalTests { get; set; } = null!;
        public DbSet<EmailOtp> EmailOtps { get; set; } = null!;
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
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        // Central persistence pipeline:
        // 1) convert hard-delete to soft-delete when entity has IsDeleted
        // 2) capture before/after snapshots for audit logs
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (_isSavingAuditLogs)
            {
                return await base.SaveChangesAsync(cancellationToken);
            }

            ApplySoftDeleteRules();
            var auditDrafts = BuildAuditLogDrafts();

            var result = await base.SaveChangesAsync(cancellationToken);
            await PersistAuditLogsAsync(auditDrafts, cancellationToken);
            return result;
        }

        public override int SaveChanges()
        {
            if (_isSavingAuditLogs)
            {
                return base.SaveChanges();
            }

            ApplySoftDeleteRules();
            var auditDrafts = BuildAuditLogDrafts();

            var result = base.SaveChanges();
            PersistAuditLogs(auditDrafts);
            return result;
        }

        // Generic soft-delete rule so controllers do not duplicate delete logic.
        private void ApplySoftDeleteRules()
        {
            foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted))
            {
                var isDeletedProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "IsDeleted" && p.Metadata.ClrType == typeof(bool));
                if (isDeletedProp == null) continue;

                entry.State = EntityState.Modified;
                isDeletedProp.CurrentValue = true;
            }
        }

        private List<AuditLogDraft> BuildAuditLogDrafts()
        {
            var logs = new List<AuditLogDraft>();
            var now = DateTime.UtcNow;
            var userId = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var username = _httpContextAccessor?.HttpContext?.User?.Identity?.Name;

            foreach (var entry in ChangeTracker.Entries())
            {
                if (!IsAuditableEntry(entry)) continue;
                //**Soft delete generic** 
                var action = entry.State switch
                {
                    EntityState.Added => "Create",
                    EntityState.Modified => IsSoftDelete(entry) ? "SoftDelete" : "Update",
                    EntityState.Deleted => "Delete",
                    _ => string.Empty
                };

                if (string.IsNullOrEmpty(action)) continue;

                var key = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
                var (recordId, needsGeneratedKey) = ResolveRecordId(key);

                var beforeData = entry.State switch
                {
                    EntityState.Added => null,
                    EntityState.Modified => SerializeValues(entry.OriginalValues),
                    EntityState.Deleted => SerializeValues(entry.OriginalValues),
                    _ => null
                };

                var afterData = entry.State switch
                {
                    EntityState.Added => SerializeValues(entry.CurrentValues),
                    EntityState.Modified => SerializeValues(entry.CurrentValues),
                    EntityState.Deleted => null,
                    _ => null
                };

                logs.Add(new AuditLogDraft(
                    entry,
                    new AuditLog
                {
                    Id = Guid.NewGuid(),
                    Action = action,
                    EntityName = entry.Metadata.ClrType.Name,
                    RecordId = recordId,
                    UserId = userId,
                    Username = username,
                    BeforeData = beforeData,
                    AfterData = afterData,
                    ChangedAt = now
                    },
                    needsGeneratedKey));
            }

            return logs;
        }

        private async Task PersistAuditLogsAsync(List<AuditLogDraft> drafts, CancellationToken cancellationToken)
        {
            if (drafts.Count == 0) return;

            FinalizeRecordIds(drafts);

            _isSavingAuditLogs = true;
            try
            {
                AuditLogs.AddRange(drafts.Select(d => d.Log));
                await base.SaveChangesAsync(cancellationToken);
            }
            finally
            {
                _isSavingAuditLogs = false;
            }
        }

        private void PersistAuditLogs(List<AuditLogDraft> drafts)
        {
            if (drafts.Count == 0) return;

            FinalizeRecordIds(drafts);

            _isSavingAuditLogs = true;
            try
            {
                AuditLogs.AddRange(drafts.Select(d => d.Log));
                base.SaveChanges();
            }
            finally
            {
                _isSavingAuditLogs = false;
            }
        }

        private static (string recordId, bool needsGeneratedKey) ResolveRecordId(PropertyEntry? keyProperty)
        {
            if (keyProperty == null) return ("[no-key]", false);

            var current = keyProperty.CurrentValue?.ToString();
            if (!string.IsNullOrWhiteSpace(current) && !keyProperty.IsTemporary)
            {
                return (current, false);
            }

            var original = keyProperty.OriginalValue?.ToString();
            if (!string.IsNullOrWhiteSpace(original))
            {
                return (original, false);
            }

            return ("[pending-generated-key]", true);
        }

        private static void FinalizeRecordIds(IEnumerable<AuditLogDraft> drafts)
        {
            foreach (var draft in drafts.Where(d => d.NeedsGeneratedKey))
            {
                var key = draft.Entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
                var recordId = key?.CurrentValue?.ToString() ?? key?.OriginalValue?.ToString();
                draft.Log.RecordId = string.IsNullOrWhiteSpace(recordId) ? "[unresolved-key]" : recordId;
            }
        }

        private static bool IsAuditableEntry(EntityEntry entry)
        {
            if (entry.Entity is AuditLog) return false;
            if (entry.State != EntityState.Added && entry.State != EntityState.Modified && entry.State != EntityState.Deleted)
                return false;
            return true;
        }

        private static bool IsSoftDelete(EntityEntry entry)
        {
            if (entry.State != EntityState.Modified) return false;
            var prop = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "IsDeleted" && p.Metadata.ClrType == typeof(bool));
            if (prop == null) return false;
            return prop.OriginalValue is bool oldVal && prop.CurrentValue is bool newVal && !oldVal && newVal;
        }

        private static string SerializeValues(PropertyValues values)
        {
            var dict = new Dictionary<string, object?>();
            foreach (var property in values.Properties)
            {
                dict[property.Name] = NormalizeAuditValue(values[property.Name]);
            }

            try
            {
                return JsonSerializer.Serialize(dict);
            }
            catch (NotSupportedException)
            {
                return JsonSerializer.Serialize(dict.ToDictionary(x => x.Key, x => x.Value?.ToString()));
            }
        }

        private static object? NormalizeAuditValue(object? value)
        {
            return value switch
            {
                null => null,
                DateTime dt => dt.ToString("O"),
                DateTimeOffset dto => dto.ToString("O"),
                TimeSpan ts => ts.ToString("c"),
                byte[] bytes => Convert.ToBase64String(bytes),
                Enum e => e.ToString(),
                _ => value
            };
        }

        private sealed record AuditLogDraft(EntityEntry Entry, AuditLog Log, bool NeedsGeneratedKey);

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
                entity.HasQueryFilter(d => !d.IsDeleted);

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
                entity.HasQueryFilter(r => !r.IsDeleted);

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
                entity.HasQueryFilter(d => !d.IsDeleted);
                entity.Property(d => d.Name).IsRequired().HasMaxLength(150);
                entity.HasIndex(d => d.Name).IsUnique();
            });

            /* ================================
             * SPECIALTY
             * ================================ */
            modelBuilder.Entity<Specialty>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.HasQueryFilter(s => !s.IsDeleted);
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
                entity.HasQueryFilter(s => !s.IsDeleted);

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
                entity.HasQueryFilter(m => !m.IsDeleted);
            });
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.FullName).IsRequired().HasMaxLength(150);
                entity.Property(p => p.Gender).HasConversion<string>().IsRequired();
                entity.HasQueryFilter(p => !p.IsDeleted);

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
                entity.HasQueryFilter(p => !p.IsDeleted);
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

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Action).IsRequired().HasMaxLength(30);
                entity.Property(a => a.EntityName).IsRequired().HasMaxLength(120);
                entity.Property(a => a.RecordId).IsRequired().HasMaxLength(100);
                entity.Property(a => a.UserId).HasMaxLength(100);
                entity.Property(a => a.Username).HasMaxLength(200);
                entity.Property(a => a.BeforeData).HasColumnType("nvarchar(max)");
                entity.Property(a => a.AfterData).HasColumnType("nvarchar(max)");
                entity.Property(a => a.ChangedAt).IsRequired();

                entity.HasIndex(a => a.ChangedAt);
                entity.HasIndex(a => new { a.EntityName, a.Action });
            });


            modelBuilder.Entity<Invoice>()
                .HasIndex(i => i.CreatedAt);

            modelBuilder.Entity<Invoice>()
                .HasIndex(i => i.InvoiceType);

            modelBuilder.Entity<Invoice>()
                .HasIndex(i => i.IsPaid);

            EnsureSoftDeleteQueryFilters(modelBuilder);
        }

        private static void EnsureSoftDeleteQueryFilters(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (entityType.ClrType == null) continue;

                var isDeletedProperty = entityType.FindProperty("IsDeleted");
                if (isDeletedProperty == null || isDeletedProperty.ClrType != typeof(bool))
                {
                    continue;
                }

                if (entityType.GetQueryFilter() == null)
                {
                    throw new InvalidOperationException(
                        $"Entity '{entityType.ClrType.Name}' has IsDeleted but no global query filter.");
                }
            }
        }
    }
}






