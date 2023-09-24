using Microsoft.AspNetCore.Identity;

namespace AllUp.Models
{
    public class AppUser:IdentityUser
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public bool IsDeactive { get; set; }
        public bool IsMale { get; set; }
    }
}
