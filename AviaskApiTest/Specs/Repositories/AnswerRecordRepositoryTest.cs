using AviaskApi.Entities;
using AviaskApiTest.Abstractions;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace AviaskApiTest.Specs.Repositories;

public class AnswerRecordRepositoryTest : FunctionalTest
{
    private readonly AnswerRecordFactory _factory = new();
    private readonly Question _persistedQuestion;
    private readonly AnswerRecordRepository _repository;

    public AnswerRecordRepositoryTest(FunctionalTestWebApplicationFactory factory, ITestOutputHelper output) : base(
        factory, output)
    {
        _repository = new AnswerRecordRepository(DbContext);

        _persistedQuestion = new QuestionFactory().GetOne();

        DbContext.Questions.Add(_persistedQuestion);
        DbContext.QuestionAnswers.Add(_persistedQuestion.QuestionAnswers);
    }

    [Fact]
    public async Task GetAllAsync()
    {
        var persistedRecords = _factory.GetMany(5).Select(r =>
        {
            r.QuestionId = _persistedQuestion.Id;
            r.Question = _persistedQuestion;

            return r;
        });

        await DbContext.AddRangeAsync(persistedRecords);
        await DbContext.SaveChangesAsync();

        var result = await _repository.GetAllAsync();

        Assert.Equivalent(persistedRecords, result);
    }

    [Fact]
    public async Task CreateAsync()
    {
        var newRecord = _factory.GetOne();
        newRecord.QuestionId = _persistedQuestion.Id;
        newRecord.Question = _persistedQuestion;

        await _repository.CreateAsync(newRecord);

        Assert.Single(await DbContext.AnswerRecords.ToArrayAsync());
        Assert.Equal(newRecord, await DbContext.AnswerRecords.FirstAsync());
    }

    [Fact]
    public async Task GetByIdAsync()
    {
        var persistedRecord = _factory.GetOne();
        persistedRecord.Question = _persistedQuestion;
        persistedRecord.QuestionId = _persistedQuestion.Id;

        await _repository.CreateAsync(persistedRecord);

        var result = await _repository.GetByIdAsync(persistedRecord.Id);

        Assert.NotNull(result);
        Assert.Equal(persistedRecord, result);
    }

    [Fact]
    public async Task ExistsByIdAsync_NotExists()
    {
        Assert.False(await _repository.ExistsByIdAsync(new Guid()));
    }

    [Fact]
    public async Task ExistsByIdAsync_Exists()
    {
        var persistedRecord = _factory.GetOne();
        persistedRecord.Question = _persistedQuestion;
        persistedRecord.QuestionId = _persistedQuestion.Id;

        await _repository.CreateAsync(persistedRecord);

        Assert.True(await _repository.ExistsByIdAsync(persistedRecord.Id));
    }

    [Fact]
    public async Task UpdateAsync()
    {
        var persistedRecord = _factory.GetOne();
        persistedRecord.Question = _persistedQuestion;
        persistedRecord.QuestionId = _persistedQuestion.Id;

        await _repository.CreateAsync(persistedRecord);

        persistedRecord.Answered = "This answer";

        await _repository.UpdateAsync(persistedRecord);

        Assert.Equal("This answer", DbContext.AnswerRecords.First().Answered);
    }

    [Fact]
    public async Task DeleteAsync()
    {
        var persistedRecord = _factory.GetOne();
        persistedRecord.Question = _persistedQuestion;
        persistedRecord.QuestionId = _persistedQuestion.Id;

        await _repository.CreateAsync(persistedRecord);

        Assert.Single(DbContext.AnswerRecords.ToArray());

        await _repository.DeleteAsync(persistedRecord);

        Assert.Empty(DbContext.AnswerRecords.ToArray());
    }

    [Fact]
    public async Task GetAllByUserIdAsync_UserNotExists()
    {
        var persistedRecords = _factory.GetMany(10).ToArray();

        foreach (var persistedRecord in persistedRecords) await _repository.CreateAsync(persistedRecord);

        var result = await _repository.GetAllByUserIdAsync(new Guid());

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllByUserIdAsync_UserExists()
    {
        var user = new UserFactory().GetOne();
        var persistedRecords = _factory.GetMany(10).ToArray();

        persistedRecords[0].User = user;
        persistedRecords[1].User = user;
        persistedRecords[2].User = user;

        foreach (var persistedRecord in persistedRecords) await _repository.CreateAsync(persistedRecord);

        var result = await _repository.GetAllByUserIdAsync(user.Id);

        Assert.Equal(3, result.Length);
        Assert.Equivalent(persistedRecords[..3], result);
    }
}