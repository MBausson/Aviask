using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AviaskApi.Entities;

public class RecoveryToken
{
    [Key] public Guid Id { get; set; }

    [ForeignKey(nameof(User))] public Guid UserId { get; set; }

    public AviaskUser User { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool IsExpired()
    {
        return DateTime.UtcNow > ExpiresAt;
    }
}