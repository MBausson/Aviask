using AviaskApi.Entities;

namespace AviaskApi.Models.Result;

public record AviaskUserExtended(
    Guid Id,
    string UserName,
    string Email,
    DateTime CreatedAt,
    string Role,
    bool IsPremium)
{
    public static AviaskUserExtended FromAviaskUser(AviaskUser user)
    {
        return new AviaskUserExtended(user.Id, user.UserName, user.Email, user.CreatedAt, user.Role!, user.IsPremium);
    }
}