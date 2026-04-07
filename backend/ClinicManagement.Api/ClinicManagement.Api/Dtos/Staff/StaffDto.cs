public class StaffDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Role { get; set; } = null!;
    public bool IsActive { get; set; }
    public string Username { get; set; } = null!;
    public Guid UserId { get; set; }
    public string ? AvatarUrl { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public DateTime CreatedAt { get; set; }
}
