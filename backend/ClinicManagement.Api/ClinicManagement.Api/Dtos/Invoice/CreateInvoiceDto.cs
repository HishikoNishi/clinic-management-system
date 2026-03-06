using System;
namespace ClinicManagement.Api.Dtos.Invoices
{
    public class CreateInvoiceDto
    {
        public Guid AppointmentId { get; set; }
        public decimal Amount { get; set; }
    }
}