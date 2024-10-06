namespace AviaskApi.Models.Result;

public enum SubscriptionStatus
{
    ACTIVE,
    CANCELLED
}

public record SubscriptionInformations(SubscriptionStatus Status, DateTimeOffset StartedAt, DateTimeOffset NextPayment);