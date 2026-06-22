using Crystal.Core;
using Microsoft.AspNetCore.Identity;

namespace Crystal.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsActive { get; set; } = true;

        public string DynamicRoleId { get; set; } = ApplicationRoles.Employee;
        public DynamicRole? DynamicRole { get; set; }

        public EmployeeProfile? EmployeeProfile { get; set; }
    }
}