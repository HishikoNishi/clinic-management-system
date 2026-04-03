using System;
using ClinicManagement.Api.Data;
using ClinicManagement.Api.Dtos.Appointments;
using ClinicManagement.Api.DTOs.Appointments;
using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicManagement.Api.Dtos.Appointments;

namespace ClinicManagement.Api.Controllers
{
    [ApiController]
    [Route("api/staff/[controller]")]
    [Authorize(Roles = "Staff")]
    public class StaffAppointmentsController : ControllerBase
    {
        private readonly ClinicDbContext _context;

        public StaffAppointmentsController(ClinicDbContext context)
        {
            _context = context;
        }

        // GET: api/staff/Appointments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppointmentDetailDto>>> GetAppointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
      
    .ThenInclude(d => d.Department)

                .Select(a => new AppointmentDetailDto
                {
                    Id = a.Id,
                    AppointmentCode = a.AppointmentCode,
                    FullName = a.Patient != null ? a.Patient.FullName : null,
                    Phone = a.Patient != null ? a.Patient.Phone : null,
                    Email = a.Patient != null ? a.Patient.Email : null,
                    DateOfBirth = a.Patient != null ? a.Patient.DateOfBirth : DateTime.MinValue,
                    Gender = a.Patient != null ? a.Patient.Gender.ToString() : null,
                    Address = a.Patient != null ? a.Patient.Address : null,
                    Reason = a.Reason,
                    Status = a.Status.ToString(),
                    AppointmentDate = a.AppointmentDate,
                    AppointmentTime = a.AppointmentTime,
                    CreatedAt = a.CreatedAt,
                    StatusDetail = new AppointmentStatusDto
                    {
                        Value = a.Status.ToString(),
                        DoctorId = (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Completed || a.Status == AppointmentStatus.CheckedIn) && a.Doctor != null
                            ? a.Doctor.Id
                            : null,
                        DoctorName = (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Completed || a.Status == AppointmentStatus.CheckedIn) && a.Doctor != null
                            ? a.Doctor.FullName
                            : null,
                        DoctorCode = (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Completed || a.Status == AppointmentStatus.CheckedIn) && a.Doctor != null
                            ? a.Doctor.Code
                            : null,
                        DoctorDepartmentName = (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Completed || a.Status == AppointmentStatus.CheckedIn) && a.Doctor != null
                            ? a.Doctor.Department.Name
                            : null
                    }


                })
                .ToListAsync();

            return Ok(appointments);
        }


        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<AppointmentDetailDto>>> GetAppointmentsByStatus([FromQuery] AppointmentStatus status)
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User) // thêm include User
                .Where(a => a.Status == status)
                .Select(a => new AppointmentDetailDto
                {
                    Id = a.Id,
                    AppointmentCode = a.AppointmentCode,
                    FullName = a.Patient != null ? a.Patient.FullName : null,
                    Phone = a.Patient != null ? a.Patient.Phone : null,
                    Email = a.Patient != null ? a.Patient.Email : null,
                    DateOfBirth = a.Patient != null ? a.Patient.DateOfBirth : DateTime.MinValue,
                    Gender = a.Patient != null ? a.Patient.Gender.ToString() : null,
                    Address = a.Patient != null ? a.Patient.Address : null,
                    Reason = a.Reason,
                    AppointmentDate = a.AppointmentDate,
                    AppointmentTime = a.AppointmentTime,
                    CreatedAt = a.CreatedAt,

                    StatusDetail = new AppointmentStatusDto
                    {
                        Value = a.Status.ToString(),
                        DoctorId = (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Completed || a.Status == AppointmentStatus.CheckedIn) && a.Doctor != null
                            ? a.Doctor.Id
                            : null,
                        DoctorName = (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Completed || a.Status == AppointmentStatus.CheckedIn) && a.Doctor != null
                            ? a.Doctor.FullName
                            : null,
                        DoctorCode = (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Completed || a.Status == AppointmentStatus.CheckedIn) && a.Doctor != null
                            ? a.Doctor.Code
                            : null,
                        DoctorDepartmentName = (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Completed || a.Status == AppointmentStatus.CheckedIn) && a.Doctor != null
                            ? a.Doctor.Department.Name
                            : null
                    }

                })
                .ToListAsync();

            return Ok(appointments);
        }
        // StaffAppointmentsController.cs
        [HttpGet("specialties-by-department/{departmentId}")]
        public async Task<IActionResult> GetSpecialtiesByDepartment(Guid departmentId)
        {
            // giả sử bạn có DbSet<Specialty> với DepartmentId
            var specialties = await _context.Specialties
                .Where(s => s.DepartmentId == departmentId)
                .Select(s => new
                {
                    s.Id,
                    s.Name
                })
                .ToListAsync();

            return Ok(specialties);
        }
        [HttpGet("by-specialty/{specialtyId}")]
        public async Task<IActionResult> GetDoctorsBySpecialty(Guid specialtyId)
        {
            var doctors = await _context.Doctors
                .Include(d => d.Department)
                .Include(d => d.Specialty)
                .Where(d => d.SpecialtyId == specialtyId
                            && d.Status == DoctorStatus.Active)
                .Select(d => new
                {
                    d.Id,
                    d.FullName,
                    d.Code,
                    d.SpecialtyId,
                    SpecialtyName = d.Specialty.Name,
                    d.DepartmentId,
                    DepartmentName = d.Department.Name
                })
                .ToListAsync();

            return Ok(doctors);
        }

        [HttpPost("assign-doctor")]
        public async Task<IActionResult> AssignDoctor([FromBody] AssignDoctorDto dto)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == dto.AppointmentId);

            if (appointment == null)
                return NotFound("Appointment not found");

            var doctor = await _context.Doctors
       .FirstOrDefaultAsync(d => d.Id == dto.DoctorId
                              && d.Status == DoctorStatus.Active);

            if (doctor == null)
                return BadRequest("Doctor is not available");
            var isBusy = await _context.Appointments.AnyAsync(a =>
    a.DoctorId == dto.DoctorId &&
    a.AppointmentDate == appointment.AppointmentDate &&
    a.AppointmentTime == appointment.AppointmentTime &&
    a.Status == AppointmentStatus.Confirmed
);

            if (isBusy)
                return BadRequest("Doctor is already booked at this time");

            appointment.DoctorId = doctor.Id;
            appointment.Status = AppointmentStatus.Confirmed;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Assigned doctor successfully",
                doctorCode = doctor.Code
            });
        }

        [HttpPost("checkin")]
        public async Task<IActionResult> CheckIn([FromBody] CheckInRequestDto dto)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == dto.AppointmentId);

            if (appointment == null) return NotFound("Appointment not found");
            if (appointment.Status == AppointmentStatus.Cancelled || appointment.Status == AppointmentStatus.Completed)
                return BadRequest("Cannot check-in cancelled/completed appointment");

            // Optional assign doctor during check-in
            if (dto.DoctorId.HasValue)
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == dto.DoctorId && d.Status == DoctorStatus.Active);
                if (doctor == null) return BadRequest("Doctor is not available");
                appointment.DoctorId = doctor.Id;
            }
            else if (appointment.DoctorId == null)
            {
                return BadRequest("Please assign a doctor before check-in");
            }

            appointment.Status = AppointmentStatus.CheckedIn;
            appointment.CheckedInAt = DateTime.UtcNow;
            appointment.CheckInChannel = "Staff";

            Payment? depositPayment = null;
            // create deposit
            if (dto.DepositAmount > 0)
            {
                // nếu đã có invoice cho appointment thì gắn luôn
                var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.AppointmentId == appointment.Id);

                depositPayment = new Payment
                {
                    AppointmentId = appointment.Id,
                    InvoiceId = invoice?.Id,
                    Amount = dto.DepositAmount,
                    DepositAmount = dto.DepositAmount,
                    IsDeposit = true,
                    Method = dto.Method,
                    PaymentDate = DateTime.UtcNow
                };

                _context.Payments.Add(depositPayment);
            }

            await _context.SaveChangesAsync();

            var totalDeposit = await _context.Payments
                .Where(p => p.AppointmentId == appointment.Id && p.IsDeposit)
                .SumAsync(p => p.Amount);

            return Ok(new
            {
                message = "Checked in successfully",
                appointment.Status,
                totalDeposit,
                depositPaymentId = depositPayment?.Id
            });
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentDetailDto>> GetAppointmentDetail(Guid id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Department)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            var dto = new AppointmentDetailDto
            {
                Id = appointment.Id,
                AppointmentCode = appointment.AppointmentCode,
                FullName = appointment.Patient?.FullName ?? string.Empty,
                Phone = appointment.Patient?.Phone ?? string.Empty,
                Email = appointment.Patient?.Email,
                DateOfBirth = appointment.Patient?.DateOfBirth ?? DateTime.MinValue,
                Gender = appointment.Patient?.Gender.ToString() ?? string.Empty,
                Address = appointment.Patient?.Address,
                Reason = appointment.Reason,
                Status = appointment.Status.ToString(),
                AppointmentDate = appointment.AppointmentDate,
                AppointmentTime = appointment.AppointmentTime,
                CreatedAt = appointment.CreatedAt,
                StatusDetail = new AppointmentStatusDto
                {
                    Value = appointment.Status.ToString(),
                    DoctorName = (appointment.Status == AppointmentStatus.Confirmed || appointment.Status == AppointmentStatus.Completed || appointment.Status == AppointmentStatus.CheckedIn)
                        ? appointment.Doctor?.FullName
                        : null,
                    DoctorCode = (appointment.Status == AppointmentStatus.Confirmed || appointment.Status == AppointmentStatus.Completed || appointment.Status == AppointmentStatus.CheckedIn)
                        ? appointment.Doctor?.Code
                        : null,
                    DoctorDepartmentName = (appointment.Status == AppointmentStatus.Confirmed || appointment.Status == AppointmentStatus.Completed || appointment.Status == AppointmentStatus.CheckedIn)
                        ? appointment.Doctor?.Department?.Name
                        : null
                }
            };

            return Ok(dto);
        }



    }
}
