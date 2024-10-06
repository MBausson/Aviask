using AviaskApiTest.Abstractions;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace AviaskApiTest.Specs.Repositories;

public class QuestionRepositoryTest : FunctionalTest
{
    private readonly QuestionFactory _questionFactory = new();
    private readonly QuestionRepository _repository;

    public QuestionRepositoryTest(FunctionalTestWebApplicationFactory factory, ITestOutputHelper output) : base(factory,
        output)
    {
        _repository = new QuestionRepository(DbContext);
    }

    [Fact]
    public async Task GetQuery()
    {
        var questions = _questionFactory.GetMany(5);

        foreach (var question in questions) await _repository.CreateAsync(question);

        var result = _repository.GetQuery();

        Assert.Equivalent(questions, result);
    }

    [Fact]
    public async Task CreateAsync()
    {
        var question = _questionFactory.GetOne();

        await _repository.CreateAsync(question);

        Assert.Equal(1, await DbContext.Questions.CountAsync());
        Assert.Equal(question, await DbContext.Questions.FirstAsync());

        Assert.Equal(1, await DbContext.QuestionAnswers.CountAsync());
        Assert.Equal(question.QuestionAnswers, await DbContext.QuestionAnswers.FirstAsync());
    }

    [Fact]
    public async Task DeleteAsync()
    {
        var persistedQuestion = _questionFactory.GetOne();

        await DbContext.Questions.AddAsync(persistedQuestion);
        await DbContext.QuestionAnswers.AddAsync(persistedQuestion.QuestionAnswers);
        await DbContext.SaveChangesAsync();

        Assert.Single(DbContext.Questions.ToArray());

        await _repository.DeleteAsync(persistedQuestion);

        Assert.Empty(await DbContext.Questions.ToArrayAsync());
        Assert.Empty(await DbContext.QuestionAnswers.ToArrayAsync());
    }

    [Fact]
    public async Task ExistsByIdAsync_NotExists()
    {
        Assert.False(await _repository.ExistsByIdAsync(new Guid()));
    }

    [Fact]
    public async Task ExistsByIdAsync_Exists()
    {
        var persistedQuestion = _questionFactory.GetOne();

        await _repository.CreateAsync(persistedQuestion);

        Assert.True(await _repository.ExistsByIdAsync(persistedQuestion.Id));
    }

    [Fact]
    public async Task GetAllAsync()
    {
        var persistedQuestion = _questionFactory.GetMany(5).ToArray();

        await DbContext.Questions.AddRangeAsync(persistedQuestion);
        await DbContext.QuestionAnswers.AddRangeAsync(persistedQuestion.Select(q => q.QuestionAnswers));
        await DbContext.SaveChangesAsync();

        var result = await _repository.GetAllAsync();

        Assert.Equivalent(persistedQuestion, result);
    }

    [Fact]
    public async Task GetByIdAsync()
    {
        var persistedQuestion = _questionFactory.GetOne();

        await _repository.CreateAsync(persistedQuestion);

        Assert.Equal(persistedQuestion, await _repository.GetByIdAsync(persistedQuestion.Id));
    }

    [Fact]
    public async Task UpdateAsync()
    {
        var persistedQuestion = _questionFactory.GetOne();

        await _repository.CreateAsync(persistedQuestion);

        persistedQuestion.Title = "New title";
        await _repository.UpdateAsync(persistedQuestion);

        Assert.Equal(persistedQuestion, DbContext.Questions.First());
    }

    [Fact]
    public async Task ExistsByTitleAsync_NotExists()
    {
        Assert.False(await _repository.ExistsByTitleAsync("Title"));
    }

    [Fact]
    public async Task ExistsByTitleAsync_Exists()
    {
        var persistedQuestion = _questionFactory.GetOne();

        await _repository.CreateAsync(persistedQuestion);

        Assert.True(await _repository.ExistsByTitleAsync(persistedQuestion.Title));
    }

    [Fact]
    public async Task GetByTitleAsync_NotExists()
    {
        Assert.Empty(await _repository.GetByTitleAsync("Title"));
    }

    [Fact]
    public async Task GetByTitleAsync_Exists()
    {
        var persistedQuestions = _questionFactory.GetMany(3).ToArray();
        persistedQuestions[0].Title = "CommonTitle";
        persistedQuestions[1].Title = "CommonTitle";
        persistedQuestions[2].Title = "ABCD";

        foreach (var persistedQuestion in persistedQuestions) await _repository.CreateAsync(persistedQuestion);

        var result = await _repository.GetByTitleAsync("CommonTitle");

        Assert.Equal(2, result.Count());
        Assert.Equivalent(persistedQuestions[..2], result);
    }
}