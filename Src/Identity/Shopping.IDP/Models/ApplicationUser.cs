using Microsoft.AspNetCore.Identity;

namespace Shopping.IDP.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
