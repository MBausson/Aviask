using AviaskApi.Entities;
using AviaskApi.Models;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace AviaskApi.Services.MockExaminer;

public class MockExaminer(AviaskApiContext context, ISchedulerFactory schedulerFactory, ILogger<IMockExaminer> logger)
    : IMockExaminer
{
    private IQueryable<Question> VisibleQuestions => context.Questions.Where(q => q.Status == QuestionStatus.ACCEPTED);

    public async Task<Guid?> GetNextQuestionAsync(MockExam mockExam)
    {
        var doneQuestionIds = mockExam.AnswerRecords.Select(a => a.QuestionId);
        var availableQuestions =
            VisibleQuestions.Where(q => q.Category == mockExam.Category && !doneQuestionIds.Contains(q.Id));

        return (await availableQuestions.OrderBy(q => new Guid()).FirstOrDefaultAsync())?.Id;
    }

    public async Task StartMockExamTimerAsync(MockExam mockExam)
    {
        ArgumentNullException.ThrowIfNull(mockExam);

        var data = new JobDataMap
        {
            { "mockExamId", mockExam.Id }
        };

        var job = JobBuilder.Create<MockExamTimerJob>()
            .UsingJobData(data)
            .Build();

        var trigger = TriggerBuilder.Create()
            .StartAt(DateTimeOffset.Now + RemainingTime(mockExam) + TimeSpan.FromSeconds(1))
            .Build();

        var scheduler = await schedulerFactory.GetScheduler();
        await scheduler.ScheduleJob(job, trigger);
    }

    public async Task StartOngoingMockExamTimersAsync()
    {
        var ongoingMockExams = await context.MockExams
            .Where(m => m.Status == MockExamStatus.ONGOING)
            .ToListAsync();

        foreach (var ongoingMockExam in ongoingMockExams) await StartMockExamTimerAsync(ongoingMockExam);
    }

    private TimeSpan RemainingTime(MockExam mockExam)
    {
        var result = mockExam.StartedAt + mockExam.MaxDuration - DateTimeOffset.Now;

        return result < TimeSpan.Zero ? TimeSpan.Zero : result;
    }
}