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

    [Required]
    public string Role { get; set; } = null!;
}