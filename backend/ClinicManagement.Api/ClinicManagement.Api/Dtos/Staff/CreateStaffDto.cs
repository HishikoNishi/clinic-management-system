using System;
using System.ComponentModel.DataAnnotations;
public class CreateStaffDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string Code { get; set; } = null!;

    [Required]
    public string FullName { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public string? Position { get; set; }  
    public string? AvatarUrl { get; set; }
    [Required]
    public string Role { get; set; } = null!;
}