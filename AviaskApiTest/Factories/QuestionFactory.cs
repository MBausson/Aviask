using AviaskApi.Entities;
using Bogus;
using Microsoft.VisualBasic;

namespace AviaskApiTest.Factories;

public class QuestionFactory : Factory<Question>
{
    public QuestionFactory()
    {
        var userFactory = new UserFactory();

        var questionAnswerFactory = new QuestionAnswersFactory();

        Faker = new Faker<Question>()
            .RuleFor(o => o.Id, f => f.Random.Uuid())
            .RuleFor(o => o.Title, f => f.Name.FullName())
            .RuleFor(o => o.Description, f => Strings.Join(f.Lorem.Words(12)))
            .RuleFor(o => o.Category, f => f.PickRandom<Category>())
            .RuleFor(o => o.Source, f => f.Company.CompanyName())
            .RuleFor(o => o.QuestionAnswers, _ => questionAnswerFactory.GetOne())
            .RuleFor(o => o.QuestionAnswersId, (_, m) => m.QuestionAnswers.Id)
            .RuleFor(o => o.Publisher, _ => userFactory.GetOne())
            .RuleFor(o => o.PublisherId, (_, m) => m.Publisher!.Id)
            .RuleFor(o => o.PublishedAt, f => f.Date.Recent().ToUniversalTime())
            .RuleFor(o => o.Visibility, _ => QuestionVisibility.PRIVATE)
            .RuleFor(o => o.Status, _ => QuestionStatus.ACCEPTED);
    }

    protected override Faker<Question> Faker { get; }
}