using AviaskApi.Entities;
using AviaskApi.Models;
using Bogus;

namespace AviaskApiTest.Factories;

public class CreateQuestionReportModelFactory : Factory<CreateQuestionReportModel>
{
    public CreateQuestionReportModelFactory()
    {
        var questionFactory = new QuestionFactory();

        Faker = new Faker<CreateQuestionReportModel>()
            .RuleFor(c => c.Message, f => f.Lorem.Sentence(15))
            .RuleFor(c => c.Category, f => f.PickRandom<ReportCategory>())
            .RuleFor(c => c.QuestionId, _ => questionFactory.GetOne().Id);
    }

    protected override Faker<CreateQuestionReportModel> Faker { get; }
}