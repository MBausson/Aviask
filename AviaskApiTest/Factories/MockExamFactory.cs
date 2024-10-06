using AviaskApi.Entities;
using Bogus;

namespace AviaskApiTest.Factories;

public class MockExamFactory : Factory<MockExam>
{
    public MockExamFactory()
    {
        Faker = new Faker<MockExam>()
            .RuleFor(m => m.Id, _ => new Guid())
            .RuleFor(m => m.StartedAt, f => f.Date.Past().ToUniversalTime())
            .RuleFor(m => m.Status, _ => MockExamStatus.FINISHED)
            .RuleFor(m => m.MaxQuestions, f => f.Random.ArrayElement([20, 50, 100]))
            .RuleFor(m => m.AnswerRecords, (_, m) => new AnswerRecordFactory().GetMany(m.MaxQuestions))
            .RuleFor(m => m.MaxDuration,
                f => f.Random.ArrayElement([
                    TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(60), TimeSpan.FromMinutes(120)
                ]))
            .RuleFor(m => m.Category, f => f.Random.Enum<Category>())
            .RuleFor(m => m.CorrectnessRatio,
                (_, m) => (float)m.AnswerRecords.Count(a => a.IsCorrect) / m.AnswerRecords.Count)
            .RuleFor(m => m.User, _ => new UserFactory().GetOne())
            .RuleFor(m => m.UserId, (_, m) => m.User.Id);
    }

    protected override Faker<MockExam> Faker { get; }
}