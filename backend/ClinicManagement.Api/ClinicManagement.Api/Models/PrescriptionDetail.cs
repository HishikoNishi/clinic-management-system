using ClinicManagement.Api.Models;

public class PrescriptionDetail
{
    public Guid Id { get; set; }
    public Guid PrescriptionId { get; set; }
    public Guid? MedicineId { get; set; } // Link tới danh mục gốc
    public string MedicineName { get; set; } // Lưu tên tại thời điểm kê đơn
    public string Dosage { get; set; } // Cách dùng: 1 viên/lần
    public string Frequency { get; set; } // Tần suất: 2 lần/ngày
    public int Duration { get; set; } // Số ngày uống
    public int TotalQuantity { get; set; } // Tổng số lượng: ví dụ 10 viên
    public decimal UnitPrice { get; set; } // Giá tại thời điểm kê đơn

    public Prescription Prescription { get; set; }
}