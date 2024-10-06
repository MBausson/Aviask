using AviaskApi.Entities;
using AviaskApiTest.Abstractions;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace AviaskApiTest.Specs.Repositories;

public class MockExamRepositoryTest : FunctionalTest
{
    private readonly MockExamFactory _factory = new();
    private readonly MockExamRepository _repository;

    public MockExamRepositoryTest(FunctionalTestWebApplicationFactory factory, ITestOutputHelper output) : base(factory,
        output)
    {
        _repository = new MockExamRepository(DbContext);
    }

    [Fact]
    public async Task GetAllAsync()
    {
        var mockExams = _factory.GetMany(3);

        await DbContext.AddRangeAsync(mockExams);
        await DbContext.SaveChangesAsync();

        var result = await _repository.GetAllAsync();

        Assert.Equivalent(mockExams, result);
    }

    [Fact]
    public async Task GetOngoingByUserIdAsync()
    {
        var mockExamOngoing = _factory.GetOne();
        mockExamOngoing.Status = MockExamStatus.ONGOING;

        var mockExamFinished = _factory.GetOne();
        mockExamFinished.UserId = mockExamOngoing.UserId;
        mockExamFinished.Status = MockExamStatus.FINISHED;

        await DbContext.AddRangeAsync([mockExamOngoing, mockExamFinished, _factory.GetOne()]);
        await DbContext.SaveChangesAsync();

        var result = await _repository.GetOngoingByUserIdAsync(mockExamOngoing.UserId);

        Assert.Equivalent(mockExamOngoing, result);
    }

    [Fact]
    public async Task CreateAsync()
    {
        var mockExam = _factory.GetOne();

        Assert.Empty(DbContext.MockExams.ToArray());

        await _repository.CreateAsync(mockExam);

        Assert.Single(DbContext.MockExams.ToArray(), mockExam);
    }

    [Fact]
    public async Task GetByIdAsync()
    {
        var mockExams = _factory.GetMany(3);
        var firstMockExam = mockExams.First();

        await DbContext.AddRangeAsync(mockExams);
        await DbContext.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(firstMockExam.Id);

        Assert.Equivalent(firstMockExam, result);
    }

    [Fact]
    public async Task ExistsByIdAsync()
    {
        var mockExams = _factory.GetMany(3);
        var firstMockExam = mockExams.First();

        await DbContext.AddRangeAsync(mockExams);
        await DbContext.SaveChangesAsync();

        var result = await _repository.ExistsByIdAsync(firstMockExam.Id);

        Assert.True(result);
        Assert.False(await _repository.ExistsByIdAsync(new Guid()));
    }

    [Fact]
    public async Task UpdateAsync()
    {
        var mockExam = _factory.GetOne();
        mockExam.Status = MockExamStatus.ONGOING;

        await DbContext.AddAsync(mockExam);
        await DbContext.SaveChangesAsync();

        mockExam.Status = MockExamStatus.FINISHED;

        await _repository.UpdateAsync(mockExam);

        Assert.Equal(MockExamStatus.FINISHED, DbContext.MockExams.First().Status);
    }

    [Fact]
    public async Task DeleteAsync()
    {
        var mockExam = _factory.GetOne();

        await DbContext.AddAsync(mockExam);
        await DbContext.SaveChangesAsync();

        Assert.Equal(1, DbContext.MockExams.Count());

        await _repository.DeleteAsync(mockExam);

        Assert.Equal(0, DbContext.MockExams.Count());
    }

    [Fact]
    public async Task FinishMockExamAsync()
    {
        var mockExam = _factory.GetOne();

        mockExam.Status = MockExamStatus.ONGOING;
        mockExam.MaxQuestions = 20;

        await DbContext.AddAsync(mockExam);
        await DbContext.SaveChangesAsync();

        var answerRecords = new AnswerRecordFactory().GetMany(4);
        mockExam = await DbContext.MockExams.FirstOrDefaultAsync();

        foreach (var answerRecord in answerRecords)
        {
            answerRecord.MockExamId = mockExam.Id;
            answerRecord.IsCorrect = true;
        }

        await DbContext.AddRangeAsync(answerRecords);
        await DbContext.SaveChangesAsync();

        await _repository.FinishMockExamAsync(mockExam);

        mockExam = await DbContext.MockExams.FirstOrDefaultAsync();

        Assert.NotNull(mockExam);
        Assert.Equal(0.2, mockExam.CorrectnessRatio, 4);
        Assert.Equal(MockExamStatus.FINISHED, mockExam.Status);
        Assert.Equal(DateTime.UtcNow, mockExam.EndedAt, (time, dateTime) => time - dateTime < TimeSpan.FromSeconds(10));
    }
}