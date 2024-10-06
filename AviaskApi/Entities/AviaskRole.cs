using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace AviaskApi.Entities;

public class AviaskUserRole : IdentityRole<Guid>
{
    [NotMapped] public static string[] AvailableRoles = ["member", "manager", "admin"];
}