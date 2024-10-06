using AviaskApi.Entities;

namespace AviaskApi.Services.MockExaminer;

public interface IMockExaminer
{
    public Task<Guid?> GetNextQuestionAsync(MockExam mockExam);
    public Task StartMockExamTimerAsync(MockExam mockExam);
    public Task StartOngoingMockExamTimersAsync();
}