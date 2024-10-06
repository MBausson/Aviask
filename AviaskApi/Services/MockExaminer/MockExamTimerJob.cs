using AviaskApi.Entities;
using AviaskApi.Repositories;
using Quartz;

namespace AviaskApi.Services.MockExaminer;

public class MockExamTimerJob : IJob
{
    private readonly ILogger<MockExamTimerJob> _logger;
    private readonly MockExamRepository _mockExamRepository;

    public MockExamTimerJob(IAviaskRepository<MockExam, Guid> repository, ILogger<MockExamTimerJob> logger)
    {
        _mockExamRepository = (MockExamRepository)repository;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var mockExamId = context.JobDetail.JobDataMap.GetGuidValue("mockExamId");

        await FinishMockExamAsync(mockExamId);
    }

    public async Task FinishMockExamAsync(Guid mockExamId)
    {
        var mockExam = await _mockExamRepository.GetByIdAsync(mockExamId);

        if (mockExam is null)
        {
            _logger.LogError($"Could not retrieve MockExam(Id = {mockExamId})");
            return;
        }

        if (mockExam.Status == MockExamStatus.FINISHED) return;

        await _mockExamRepository.FinishMockExamAsync(mockExam);
    }
}