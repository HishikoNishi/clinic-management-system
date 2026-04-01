using System;
using System.ComponentModel.DataAnnotations;

public class UpdateStaffDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string Code { get; set; } = null!;

    [Required]
    public string FullName { get; set; } = null!;

    [Required]
    public string Role { get; set; } = null!;

    public bool IsActive { get; set; }

    public string? AvatarUrl { get; set; }
}