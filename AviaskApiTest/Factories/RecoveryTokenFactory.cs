using AviaskApi.Entities;
using Bogus;

namespace AviaskApiTest.Factories;

public class RecoveryTokenFactory : Factory<RecoveryToken>
{
    public RecoveryTokenFactory()
    {
        Faker = new Faker<RecoveryToken>()
            .RuleFor(r => r.Id, f => f.Random.Guid())
            .RuleFor(r => r.ExpiresAt, f => f.Date.Future())
            .RuleFor(r => r.User, _ => new UserFactory().GetOne())
            .RuleFor(r => r.UserId, (_, m) => m.User.Id);
    }

    protected override Faker<RecoveryToken> Faker { get; }
}