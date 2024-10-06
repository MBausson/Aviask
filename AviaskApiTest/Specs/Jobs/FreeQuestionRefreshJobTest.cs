using AviaskApi.Jobs;
using AviaskApi.Services.FreeQuestionsPool;
using Microsoft.Extensions.Logging;

namespace AviaskApiTest.Specs.Jobs;

public class FreeQuestionRefreshJobTest
{
    private readonly FreeQuestionsPoolService _freeQuestionsPoolService = new();
    private readonly FreeQuestionsRefreshJob _job;
    private readonly Mock<QuestionRepository> _mockQuestionsRepository;
    private readonly QuestionFactory _questionsFactory = new();

    public FreeQuestionRefreshJobTest()
    {
        var mockLogger = Mock.Of<ILogger<FreeQuestionsRefreshJob>>();
        _mockQuestionsRepository = new Mock<QuestionRepository>(null!);

        _job = new FreeQuestionsRefreshJob(_mockQuestionsRepository.Object, mockLogger,
            _freeQuestionsPoolService);
    }

    [Fact]
    public void GetRandomQuestions()
    {
        var questions = _questionsFactory.GetMany(60);
        var questionsId = questions.Select(q => q.Id);

        _mockQuestionsRepository.Setup(m => m.GetQuery()).Returns(questions.AsQueryable());

        var result = _job.GetRandomQuestions();

        //  Verifies that the returned Ids are all distinct
        Assert.Distinct(result);

        //  Verifies that the returned IDs are contained in our questions database
        Assert.All(result, r => questionsId.Contains(r));

        var resultQuestions = questions.Where(q => result.Contains(q.Id));
        var resultQuestionsPerCategory = resultQuestions.GroupBy(q => q.Category).ToArray();

        //  Verifies that the returned questions are grouped by at most 3 questions per category
        Assert.All(resultQuestionsPerCategory, c => Assert.True(3 >= c.Count()));
    }
}