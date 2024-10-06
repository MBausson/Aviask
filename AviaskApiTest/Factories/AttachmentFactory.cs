using AviaskApi.Entities;
using Bogus;

namespace AviaskApiTest.Factories;

public class AttachmentFactory : Factory<Attachment>
{
    public AttachmentFactory()
    {
        Faker = new Faker<Attachment>()
            .RuleFor(i => i.Id, f => f.Random.Guid())
            .RuleFor(i => i.Data, _ => [0xF])
            .RuleFor(i => i.FileType, _ => "image/png");
    }

    protected override Faker<Attachment> Faker { get; }
}