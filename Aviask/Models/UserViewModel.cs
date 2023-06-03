using Microsoft.AspNetCore.Identity;

namespace Aviask.Models
{
    public class UserViewModel
    {
        public IdentityUser User { get; set; }
        public IList<string> Roles { get; set; }
    }
}
