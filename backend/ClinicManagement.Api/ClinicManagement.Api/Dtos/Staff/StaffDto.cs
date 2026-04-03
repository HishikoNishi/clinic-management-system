public class StaffDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Role { get; set; } = null!;
    public bool IsActive { get; set; }
    public string Username { get; set; } = null!;
    public string ? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}