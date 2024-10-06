using AviaskApi.Entities;
using AviaskApi.Models;
using AviaskApi.Services.MockExaminer;

namespace AviaskApiTest.Mocks;

public class MockExaminerMock(AviaskApiContext context) : IMockExaminer
{
    public async Task<Guid?> GetNextQuestionAsync(MockExam mockExam)
    {
        var question = new QuestionFactory().GetOne();
        question.Status = QuestionStatus.ACCEPTED;
        question.Category = mockExam.Category;

        await context.Questions.AddAsync(question);
        await context.SaveChangesAsync();

        return question.Id;

        // return mockExam.QuestionId;
    }

    public async Task StartMockExamTimerAsync(MockExam mockExam)
    {
    }

    public async Task StartOngoingMockExamTimersAsync()
    {
    }
}