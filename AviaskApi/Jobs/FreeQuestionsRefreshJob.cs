using AviaskApi.Entities;
using AviaskApi.Repositories;
using AviaskApi.Services.FreeQuestionsPool;
using Quartz;

namespace AviaskApi.Jobs;

public class FreeQuestionsRefreshJob : IJob
{
    private readonly IFreeQuestionsPoolService _freeQuestions;
    private readonly ILogger<FreeQuestionsRefreshJob> _logger;
    private readonly QuestionRepository _questionRepository;

    public FreeQuestionsRefreshJob(IAviaskRepository<Question, Guid> questionRepository,
        ILogger<FreeQuestionsRefreshJob> logger, IFreeQuestionsPoolService freeQuestions)
    {
        _questionRepository = (QuestionRepository)questionRepository;
        _logger = logger;
        _freeQuestions = freeQuestions;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _freeQuestions.SetPool(GetRandomQuestions());

        _logger.LogInformation($"{nameof(FreeQuestionsRefreshJob)} :: Refreshed questions selection");
    }

    //  We want only 2 questions of each category
    public List<Guid> GetRandomQuestions()
    {
        var questionsPerCategory = _questionRepository
            .GetQuery()
            .Where(q => q.Status == QuestionStatus.ACCEPTED)
            .Select(q => new { q.Category, q.Id })
            .GroupBy(q => q.Category)
            .ToArray();

        var random = new Random();
        var availableQuestionIds = new List<Guid>();

        foreach (var questionCategory in questionsPerCategory)
        {
            var shuffledItems = questionCategory.OrderBy(_ => random.Next()).ToList();

            availableQuestionIds.AddRange(shuffledItems.Take(2).Select(q => q.Id));
        }

        return availableQuestionIds;
    }
}