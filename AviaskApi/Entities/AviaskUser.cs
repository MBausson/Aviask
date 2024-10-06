using System.ComponentModel.DataAnnotations.Schema;
using AviaskApi.Models.Details;
using Microsoft.AspNetCore.Identity;

namespace AviaskApi.Entities;

public class AviaskUser : IdentityUser<Guid>, IEntityDetails<AviaskUserDetails>
{
    public override string? UserName { get; set; }

    public override string? Email { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsPremium { get; set; }

    [NotMapped] public string? Role { get; set; }

    public ICollection<Question> Publications { get; } = [];

    public AviaskUserDetails GetDetails()
    {
        return new AviaskUserDetails(Id, UserName!, Role, CreatedAt, IsPremium);
    }

    public bool IsAdmin()
    {
        return Role?.ToLower() == "admin";
    }
}