namespace AviaskApi.Models.Details;

public record AviaskUserDetails(
    Guid Id,
    string UserName,
    string? Role,
    DateTime CreatedAt,
    bool IsPremium
);