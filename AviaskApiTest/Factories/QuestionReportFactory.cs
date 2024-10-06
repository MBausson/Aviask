using AviaskApi.Entities;
using Bogus;

namespace AviaskApiTest.Factories;

public class QuestionReportFactory : Factory<QuestionReport>
{
    public QuestionReportFactory()
    {
        var userFactory = new UserFactory();
        var questionFactory = new QuestionFactory();

        Faker = new Faker<QuestionReport>()
            .RuleFor(q => q.Id, f => f.Random.Guid())
            .RuleFor(q => q.Message, f => f.Lorem.Text())
            .RuleFor(q => q.Category, f => f.PickRandom<ReportCategory>())
            .RuleFor(q => q.State, f => f.PickRandom<ReportState>())
            .RuleFor(q => q.Issuer, _ => userFactory.GetOne())
            .RuleFor(q => q.IssuerId, (_, m) => m.Issuer.Id)
            .RuleFor(q => q.Question, _ => questionFactory.GetOne())
            .RuleFor(q => q.QuestionId, (_, m) => m.Question.Id);
    }

    protected override Faker<QuestionReport> Faker { get; }
}