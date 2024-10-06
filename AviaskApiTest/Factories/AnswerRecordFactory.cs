using AviaskApi.Entities;
using Bogus;

namespace AviaskApiTest.Factories;

public class AnswerRecordFactory : Factory<AnswerRecord>
{
    public AnswerRecordFactory()
    {
        var questionFactory = new QuestionFactory();
        var userFactory = new UserFactory();

        Faker = new Faker<AnswerRecord>()
            .RuleFor(o => o.Id, f => f.Random.Guid())
            .RuleFor(o => o.Question, _ => questionFactory.GetOne())
            .RuleFor(o => o.QuestionId, (_, m) => m.Question.Id)
            .RuleFor(o => o.User, _ => userFactory.GetOne())
            .RuleFor(o => o.UserId, (_, m) => m.User.Id)
            .RuleFor(o => o.Answered,
                (f, m) => f.Random.CollectionItem(m.Question.QuestionAnswers.GetPossibleAnswers()))
            .RuleFor(o => o.IsCorrect, (_, m) => m.Question.QuestionAnswers.CorrectAnswer == m.Answered)
            .RuleFor(o => o.AnsweredAt, f => f.Date.Recent().ToUniversalTime());
    }

    protected override Faker<AnswerRecord> Faker { get; }
}