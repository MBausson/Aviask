using AviaskApi.Entities;
using Bogus;

namespace AviaskApiTest.Factories;

public class UserFactory : Factory<AviaskUser>
{
    public UserFactory()
    {
        Faker = new Faker<AviaskUser>()
            .RuleFor(o => o.Id, f => f.Random.Guid())
            .RuleFor(o => o.UserName, f => f.Internet.UserName())
            .RuleFor(o => o.Email, f => f.Internet.Email())
            .RuleFor(o => o.CreatedAt, f => f.Date.Recent().ToUniversalTime())
            .RuleFor(o => o.Role, _ => "member")
            .RuleFor(o => o.IsPremium, _ => false)
            .RuleFor(o => o.SecurityStamp, f => f.Random.Guid().ToString());
    }

    protected override Faker<AviaskUser> Faker { get; }
}