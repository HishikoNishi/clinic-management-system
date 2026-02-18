using ClinicManagement.Api.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
namespace ClinicManagement.Api.Data
{
    public class ApplicationDbContext
    {
        public DbSet<Patient> Patients { get; set; }    
    }
}
