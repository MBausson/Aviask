using System.Net;
using System.Net.Http.Json;
using AviaskApi.Entities;
using AviaskApi.Models;
using AviaskApi.Models.Result;
using AviaskApiTest.Abstractions;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace AviaskApiTest.Specs.Controllers;

public class MockExamsControllerTest : FunctionalTest
{
    private readonly MockExamFactory _factory = new();

    public MockExamsControllerTest(FunctionalTestWebApplicationFactory factory, ITestOutputHelper output) : base(
        factory, output)
    {
    }

    [Fact]
    public async Task Index_ReturnsUnauthorized()
    {
        var response = await HttpClient.GetAsync("api/mockExams");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Index_ReturnsMockExams()
    {
        var user = await Authenticate("member");
        var mockExams = _factory.GetMany(5).Select(m =>
        {
            m.User = user;
            m.UserId = user.Id;
            m.Status = MockExamStatus.FINISHED;

            return m;
        }).ToArray();

        mockExams[0].Status = MockExamStatus.ONGOING;

        await DbContext.MockExams.AddRangeAsync(mockExams);
        await DbContext.SaveChangesAsync();

        var response = await HttpClient.GetAsync("api/mockExams");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsAsync<FilteredResult<MockExam>>();
        Assert.Equal(4, result.TotalCount);
        Assert.All(result.Elements, m =>
        {
            Assert.Equal(MockExamStatus.FINISHED, m.Status);
            Assert.Equal(user.Id, m.UserId);
        });
    }

    [Fact]
    public async Task Show_ReturnsUnauthorized()
    {
        var response = await HttpClient.GetAsync($"api/mockExam/{new Guid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Show_ReturnsNotFound()
    {
        await Authenticate("member");

        var response = await HttpClient.GetAsync($"api/mockExam/{new Guid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Show_ReturnsUnauthorized_Member()
    {
        var user = await Authenticate("member");
        var mockExam = _factory.GetOne();

        await CreateMockExams(mockExam);

        var response = await HttpClient.GetAsync($"api/mockExam/{mockExam.Id}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Show_ReturnsMockExam()
    {
        var user = await Authenticate("member");

        var mockExam = _factory.GetOne();
        mockExam.User = user;
        mockExam.UserId = user.Id;

        await CreateMockExams(mockExam);

        var response = await HttpClient.GetAsync($"api/mockExam/{mockExam.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsAsync<MockExam>();
        Assert.Equal(mockExam.Id, result.Id);
    }

    [Fact]
    public async Task Current_ReturnsUnauthorized()
    {
        var response = await HttpClient.GetAsync("api/mockExam");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Current_ReturnsNotFound_NoMockExam()
    {
        await Authenticate("member");

        var response = await HttpClient.GetAsync("api/mockExam");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Current_ReturnsNotFound_FinishedMockExam()
    {
        var user = await Authenticate("member");

        var mockExam = _factory.GetOne();
        mockExam.Status = MockExamStatus.FINISHED;
        mockExam.UserId = user.Id;

        await CreateMockExams(mockExam);

        var response = await HttpClient.GetAsync("api/mockExam");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Current_ReturnsMockExam()
    {
        var user = await Authenticate("member");

        var mockExam = _factory.GetOne();
        mockExam.Status = MockExamStatus.ONGOING;
        mockExam.User = user;

        await CreateMockExams(mockExam);

        var response = await HttpClient.GetAsync("api/mockExam");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsAsync<MockExam>();
        Assert.Equal(mockExam.Id, result.Id);
    }

    [Fact]
    public async Task New_ReturnsUnauthorized()
    {
        var response = await HttpClient.PostAsJsonAsync("api/mockExams", new { });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task New_ReturnsBadRequest_InvalidModel()
    {
        await Authenticate("member");

        var createMockExamModel = new CreateMockExamModel
        {
            Category = Category.AIR_LAW,
            MaxQuestions = 155,
            TimeLimit = TimeSpan.FromMinutes(121)
        };

        var result = await HttpClient.PostAsJsonAsync("api/mockExams", createMockExamModel);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task New_ReturnsBadRequest_AlreadyOngoing()
    {
        var user = await Authenticate("member");

        var mockExam = _factory.GetOne();
        mockExam.Status = MockExamStatus.ONGOING;
        mockExam.User = user;

        await CreateMockExams(mockExam);

        var createMockExamModel = new CreateMockExamModel
        {
            Category = Category.AIR_LAW,
            MaxQuestions = 100,
            TimeLimit = TimeSpan.FromMinutes(60)
        };

        var result = await HttpClient.PostAsJsonAsync("api/mockExams", createMockExamModel);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task New_CreatesMockExam()
    {
        await Authenticate("member");

        var createMockExamModel = new CreateMockExamModel
        {
            Category = Category.AIR_LAW,
            MaxQuestions = 100,
            TimeLimit = TimeSpan.FromMinutes(60)
        };

        var question = new QuestionFactory().GetOne();
        question.Category = Category.AIR_LAW;
        question.Status = QuestionStatus.ACCEPTED;

        await CreateQuestion(question);

        var result = await HttpClient.PostAsJsonAsync("api/mockExams", createMockExamModel);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var response = await result.Content.ReadAsAsync<MockExam>();

        Assert.Equal(1, DbContext.MockExams.Count());
        Assert.Equal((await DbContext.MockExams.FirstAsync()).Id, response.Id);
    }

    [Fact]
    public async Task StopCurrent_ReturnsUnauthorized()
    {
        var response = await HttpClient.PatchAsJsonAsync("api/mockExam/stop", new { });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task StopCurrent_ReturnsBadRequest_NoOngoing()
    {
        await Authenticate("member");

        var response = await HttpClient.PatchAsJsonAsync("api/mockExam/stop", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task NextQuestion_ReturnsUnauthorized()
    {
        var response = await HttpClient.GetAsync("api/mockExam/next");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task NextQuestion_ReturnsNotFound_NoMockExam()
    {
        await Authenticate("member");

        var response = await HttpClient.GetAsync("api/mockExam/next");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // [Fact]
    // public async Task NextQuestion_ReturnsNotFound_NoQuestionLeft()
    // {
    //     var user = await Authenticate("member");
    //
    //     var response = await HttpClient.GetAsync("api/mockExam/next");
    //
    //     Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    // }

    // [Fact]
    // public async Task StopCurrent_StopsMockExam()
    // {
    //     var user = await Authenticate("member");
    //
    //     var mockExam = _factory.GetOne();
    //     mockExam.User = user;
    //     mockExam.Status = MockExamStatus.ONGOING;
    //
    //     await CreateMockExams(mockExam);
    //
    //     var response = await HttpClient.PatchAsJsonAsync($"api/mockExam/stop", new { });
    //     Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    //
    //     var persistedMockExam = await DbContext.MockExams.FirstAsync(m => m.Id == mockExam.Id);
    //     Assert.Equal(MockExamStatus.FINISHED, persistedMockExam.Status);
    // }

    private async Task CreateMockExams(params MockExam[] mockExams)
    {
        await DbContext.MockExams.AddRangeAsync(mockExams);
        await DbContext.SaveChangesAsync();
    }
}