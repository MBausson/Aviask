using AviaskApi.Entities;
using Bogus;

namespace AviaskApiTest.Factories;

public class QuestionAnswersFactory : Factory<QuestionAnswers>
{
    public QuestionAnswersFactory()
    {
        Faker = new Faker<QuestionAnswers>()
            .RuleFor(o => o.Answer1, f => f.Vehicle.Manufacturer())
            .RuleFor(o => o.Answer2, f => f.Vehicle.Manufacturer())
            .RuleFor(o => o.Answer3, f => f.Vehicle.Manufacturer())
            .RuleFor(o => o.Answer4, f => f.Vehicle.Manufacturer())
            .RuleFor(o => o.CorrectAnswer, (f, m) =>
            {
                if (f.Random.Bool()) return m.Answer1;

                return m.Answer2;
            })
            .RuleFor(o => o.Explications, f => f.Lorem.Sentences(2));
    }

    protected override Faker<QuestionAnswers> Faker { get; }
}