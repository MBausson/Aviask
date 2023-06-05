using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aviask.Models
{
    public class UserInformations
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public IdentityUser User { get; set; }

        public int QuestionAnswered { get; set; }
        public int QuestionAnsweredCorrectly { get; set; }
    }
}
