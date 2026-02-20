using System;

namespace ClinicManagement.Api.Models
{
    public class Staff
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Code { get; set; } = string.Empty; //mã nhân viên (giúp quản lý dễ hơn).
        public string FullName{ get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;

        //Liên kết với user
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
