namespace Aviask.Models
{
    public class UserViewModel
    {
        public readonly static string[] AvailableRoles = new string[] { "user", "manager", "admin" };

        public IList<string> Roles { get; set; }
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
    }
}
