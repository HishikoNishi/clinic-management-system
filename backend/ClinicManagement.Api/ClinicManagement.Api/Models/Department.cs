using System;
using System.Collections.Generic;

namespace ClinicManagement.Api.Models;

public class Department
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}