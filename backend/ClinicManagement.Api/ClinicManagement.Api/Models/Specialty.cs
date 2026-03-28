using System;
namespace ClinicManagement.Api.Models
{
    using System;
    namespace ClinicManagement.Api.Models
    {
        public class Specialty
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = null!;
            public Guid DepartmentId { get; set; }
            public Department Department { get; set; } = null!;
        }

    }

}

