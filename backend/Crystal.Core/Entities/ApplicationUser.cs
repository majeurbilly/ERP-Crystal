using Microsoft.AspNetCore.Identity;

namespace Crystal.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsActive { get; set; } = true;
    }
}