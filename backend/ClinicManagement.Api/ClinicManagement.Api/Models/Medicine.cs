using System;
namespace ClinicManagement.Api.Models
{
    public class Medicine
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!; // Tên thuốc (Paracetamol...)
        public string? DefaultDosage { get; set; } // Hàm lượng mặc định (500mg)
        public string Unit { get; set; } = "Viên"; // Đơn vị: Viên, Gói, Chai
        public decimal Price { get; set; } // GIÁ TIỀN để tính hóa đơn
        public bool IsActive { get; set; } = true;
    }
}

