using AviaskApi.Entities;
using AviaskApiTest.Abstractions;
using Xunit.Abstractions;

namespace AviaskApiTest.Specs.Repositories;

public class QuestionReportRepositoryTest : FunctionalTest
{
    private readonly QuestionReportFactory _factory = new();
    private readonly Question _persistedQuestion;
    private readonly QuestionReportRepository _repository;

    public QuestionReportRepositoryTest(FunctionalTestWebApplicationFactory factory, ITestOutputHelper output) : base(
        factory, output)
    {
        _repository = new QuestionReportRepository(DbContext);
        _persistedQuestion = new QuestionFactory().GetOne();

        DbContext.Add(_persistedQuestion);
        DbContext.Add(_persistedQuestion.QuestionAnswers);
    }

    [Fact]
    public async Task GetAllAsync()
    {
        var persistedReports = _factory.GetMany(5).Select(r =>
        {
            r.Question = _persistedQuestion;
            return r;
        });

        foreach (var persistedReport in persistedReports) await _repository.CreateAsync(persistedReport);

        var result = await _repository.GetAllAsync();

        Assert.Equal(5, result.Length);
        Assert.Equivalent(persistedReports, result);
    }

    [Fact]
    public async Task CreateAsync()
    {
        var newReport = _factory.GetOne();
        newReport.Question = _persistedQuestion;

        await _repository.CreateAsync(newReport);

        Assert.Single(DbContext.QuestionReports.ToArray());
        Assert.Equal(newReport, DbContext.QuestionReports.First());
    }

    [Fact]
    public async Task GetByIdAsync_NotExists()
    {
        Assert.Null(await _repository.GetByIdAsync(new Guid()));
    }

    [Fact]
    public async Task GetByIdAsync_Exists()
    {
        var persistedReport = _factory.GetOne();
        persistedReport.Question = _persistedQuestion;

        await _repository.CreateAsync(persistedReport);

        var result = await _repository.GetByIdAsync(persistedReport.Id);

        Assert.NotNull(result);
        Assert.Equal(persistedReport, result);
    }

    [Fact]
    public async Task ExistsByIdAsync_NotExists()
    {
        Assert.False(await _repository.ExistsByIdAsync(new Guid()));
    }

    [Fact]
    public async Task ExistsByIdAsync_Exists()
    {
        var persistedReport = _factory.GetOne();
        persistedReport.Question = _persistedQuestion;

        await _repository.CreateAsync(persistedReport);

        Assert.True(await _repository.ExistsByIdAsync(persistedReport.Id));
    }

    [Fact]
    public async Task UpdateAsync()
    {
        var persistedReport = _factory.GetOne();
        persistedReport.Question = _persistedQuestion;

        await _repository.CreateAsync(persistedReport);

        persistedReport.Message = "New message !!!";

        await _repository.UpdateAsync(persistedReport);

        Assert.Equal("New message !!!", DbContext.QuestionReports.First().Message);
    }

    [Fact]
    public async Task DeleteAsync()
    {
        var persistedReport = _factory.GetOne();
        persistedReport.Question = _persistedQuestion;

        await _repository.CreateAsync(persistedReport);

        Assert.Single(DbContext.QuestionReports.ToArray());

        await _repository.DeleteAsync(persistedReport);

        Assert.Empty(DbContext.QuestionReports.ToArray());
    }
}