using System.Net;
using AviaskApi.Entities;
using AviaskApi.Models.Result;
using AviaskApiTest.Abstractions;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace AviaskApiTest.Specs.Controllers;

[CollectionDefinition("Integration")]
public class AnswerRecordsControllerTest : FunctionalTest
{
    public AnswerRecordsControllerTest(FunctionalTestWebApplicationFactory factory, ITestOutputHelper output) : base(
        factory, output)
    {
    }

    [Fact]
    public async Task Index_ReturnsUnauthorized()
    {
        var response = await HttpClient.GetAsync("api/answerRecords/user");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Index_ReturnsNotFound()
    {
        await Authenticate();

        var response = await HttpClient.GetAsync($"api/answerRecords/user/{new Guid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Index_ReturnsUnauthorized_Member()
    {
        await Authenticate("member");

        var user = new UserFactory().GetOne();
        await CreateUser(user);

        var response = await HttpClient.GetAsync($"api/answerRecords/user/{user.Id}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Index_ReturnsRecords_Admin()
    {
        await Authenticate();

        var user = new UserFactory().GetOne();
        await CreateUser(user);

        var answerRecords = await CreateAnswerRecords(user);

        var response = await HttpClient.GetAsync($"api/answerRecords/user/{user.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsAsync<FilteredResult<AnswerRecordDetails>>();

        Assert.Equal(answerRecords.Count(), result.Elements.Count());
        Assert.All(result.Elements, a => Assert.Equal(user.Id, a.UserId));
    }

    [Fact]
    public async Task Index_ReturnsRecords()
    {
        var user = await Authenticate("member");

        var answerRecords = await CreateAnswerRecords(user);

        var response = await HttpClient.GetAsync($"api/answerRecords/user/{user.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsAsync<FilteredResult<AnswerRecordDetails>>();

        Assert.Equal(answerRecords.Count(), result.Elements.Count());
        Assert.All(result.Elements, a => Assert.Equal(user.Id, a.UserId));
    }

    [Fact]
    public async Task Show_ReturnsUnauthorized()
    {
        var response = await HttpClient.GetAsync($"api/answerRecords/{new Guid()}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Show_ReturnsNotFound()
    {
        await Authenticate();

        var response = await HttpClient.GetAsync($"api/answerRecords/{new Guid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    //  TODO: WTF ?
    // [Fact]
    // public async Task Show_ReturnsAnswerRecordDetails()
    // {
    //     var user = await Authenticate();
    //
    //     var answerRecord = (await CreateAnswerRecords(user, 1)).First();
    //
    //     var response = await HttpClient.GetAsync($"api/answerRecords/{answerRecord.Id}");
    //     Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    //
    //     var result = await response.Content.ReadAsAsync<AnswerRecordDetails>();
    //     Assert.Equal(answerRecord.Id, result.Id);
    // }

    [Fact]
    public async Task ShowExtended_ReturnsUnauthorized()
    {
        var response = await HttpClient.GetAsync($"api/answerRecords/extended/{new Guid()}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ShowExtended_ReturnsNotFound()
    {
        await Authenticate();

        var response = await HttpClient.GetAsync($"api/answerRecords/extended/{new Guid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsUnauthorized()
    {
        var response = await HttpClient.DeleteAsync($"api/answerRecords/{new Guid()}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound()
    {
        await Authenticate();

        var response = await HttpClient.DeleteAsync($"api/answerRecords/{new Guid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_DeletesAnswerRecord()
    {
        var user = await Authenticate();

        var answerRecords = await CreateAnswerRecords(user);

        var response = await HttpClient.DeleteAsync($"api/answerRecords/{answerRecords.First().Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.Equal(4, await DbContext.AnswerRecords.CountAsync());
        Assert.DoesNotContain(answerRecords.First().Id, await DbContext.AnswerRecords.Select(a => a.Id).ToArrayAsync());
    }

    private async Task<IEnumerable<AnswerRecord>> CreateAnswerRecords(AviaskUser user, int n = 5)
    {
        var questions = new QuestionFactory().GetMany(n).ToList();

        foreach (var question in questions)
        {
            question.Publisher = user;

            await CreateQuestion(question);
        }

        var answerRecords = new AnswerRecordFactory().GetMany(n).ToList();

        foreach (var answerRecord in answerRecords)
        {
            answerRecord.User = user;
            answerRecord.Question = questions[new Random().Next(questions.Count)];
        }

        await DbContext.AddRangeAsync(answerRecords);
        await DbContext.SaveChangesAsync();

        return answerRecords;
    }
}