using System.Net;
using System.Net.Http.Json;
using AviaskApi.Entities;
using AviaskApi.Models.Result;
using AviaskApi.Services.FreeQuestionsPool;
using AviaskApiTest.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AviaskApiTest.Specs.Controllers;

public class QuestionsControllerTest : FunctionalTest
{
    public QuestionsControllerTest(FunctionalTestWebApplicationFactory factory, ITestOutputHelper output) : base(
        factory, output)
    {
    }

    [Fact]
    public async Task Index_ReturnsAllPublicQuestions()
    {
        var questions = new QuestionFactory().GetMany(10).ToList();
        var publicQuestionIds = questions[..6].Select(q => q.Id);
        questions[..6].ForEach(question => question.Visibility = QuestionVisibility.PUBLIC);

        foreach (var q in questions) await CreateQuestion(q);

        var response = await HttpClient.GetAsync("api/questions");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsAsync<FilteredResult<QuestionDetails>>();
        Assert.Equal(6, result.Elements.Count());
        Assert.Equal(6, result.TotalCount);
        Assert.All(result.Elements, q => Assert.Contains(q.Id, publicQuestionIds));
    }

    [Fact]
    public async Task Index_NonPremium_ReturnsFreeQuestions()
    {
        var questions = new QuestionFactory().GetMany(10).ToList();
        var freeQuestionIds = questions[..4].Select(q => q.Id);

        foreach (var question in questions) await CreateQuestion(question);

        await Authenticate();

        var pool = ServiceScope.ServiceProvider.GetRequiredService<IFreeQuestionsPoolService>();
        pool.SetPool(freeQuestionIds);

        var response = await HttpClient.GetAsync("api/questions");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsAsync<FilteredResult<QuestionDetails>>();
        Assert.Equal(4, result.Elements.Count());
        Assert.Equal(4, result.TotalCount);
        Assert.All(result.Elements, q => Assert.True(pool.IsQuestionInPool(q.Id)));
    }

    [Fact]
    public async Task Index_Premium_ReturnsAllQuestions()
    {
        var questions = new QuestionFactory().GetMany(10).ToList();
        foreach (var question in questions) await CreateQuestion(question);

        await Authenticate(premium: true);

        var response = await HttpClient.GetAsync("api/questions");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsAsync<FilteredResult<QuestionDetails>>();
        Assert.Equal(10, result.Elements.Count());
        Assert.Equal(10, result.TotalCount);
    }

    [Fact]
    public async Task Show_ReturnsNotFound()
    {
        var response = await HttpClient.GetAsync($"api/question/{new Guid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Show_PrivateQuestion_ReturnsUnauthorized()
    {
        var question = new QuestionFactory().GetOne();
        question.Visibility = QuestionVisibility.PRIVATE;

        await CreateQuestion(question);

        var response = await HttpClient.GetAsync($"api/question/{question.Id}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Show_PrivateQuestion_ReturnsQuestion()
    {
        var question = new QuestionFactory().GetOne();

        await CreateQuestion(question);

        var pool = ServiceScope.ServiceProvider.GetRequiredService<IFreeQuestionsPoolService>();
        pool.SetPool([question.Id]);

        await Authenticate();

        var response = await HttpClient.GetAsync($"api/question/{question.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsAsync<QuestionDetails>();

        Assert.Equal(question.Id, result.Id);
    }

    [Fact]
    public async Task Show_PremiumQuestion_ReturnsUnauthorized()
    {
        var question = new QuestionFactory().GetOne();
        await CreateQuestion(question);

        await Authenticate();

        var response = await HttpClient.GetAsync($"api/question/{question.Id}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Show_PremiumQuestion_ReturnsQuestion()
    {
        var question = new QuestionFactory().GetOne();
        await CreateQuestion(question);

        await Authenticate(premium: true);

        var response = await HttpClient.GetAsync($"api/question/{question.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsAsync<QuestionDetails>();

        Assert.Equal(question.Id, result.Id);
    }

    [Fact]
    public async Task ShowExtended_ReturnsForbidden()
    {
        await Authenticate("member");

        var response = await HttpClient.GetAsync($"api/question/extended/{new Guid()}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ShowExtended_ReturnsNotFound()
    {
        await Authenticate();

        var response = await HttpClient.GetAsync($"api/question/extended/{new Guid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ShowExtended_ReturnsQuestion()
    {
        var question = new QuestionFactory().GetOne();
        await CreateQuestion(question);

        await Authenticate();

        var response = await HttpClient.GetAsync($"api/question/extended/{question.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsAsync<Question>();
        Assert.Equal(question.Id, result.Id);
    }

    [Fact]
    public async Task New_ReturnsForbidden()
    {
        await Authenticate("member");

        var response = await HttpClient.PostAsJsonAsync("api/questions", new { });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task New_FailsInvalidModel()
    {
        var question = new QuestionFactory().GetOne();
        question.Title = "";

        await Authenticate();

        var response = await HttpClient.PostAsJsonAsync("api/questions", question);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadAsAsync<ApiErrorResponse>();

        Assert.NotNull(result?.Message);
        Assert.NotEmpty(result.Message);
    }

    [Fact]
    public async Task New_FailsSameTitleExists()
    {
        var persistedQuestion = new QuestionFactory().GetOne();
        await CreateQuestion(persistedQuestion);

        var question = new QuestionFactory().GetOne();
        question.Title = persistedQuestion.Title;

        await Authenticate();

        var response = await HttpClient.PostAsJsonAsync("api/questions", question);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadAsAsync<ApiErrorResponse>();

        Assert.NotNull(result?.Message);
        Assert.NotEmpty(result.Message);
    }

    [Fact]
    public async Task New_CreatesQuestion()
    {
        var question = new QuestionFactory().GetOne();

        await Authenticate();

        var response = await HttpClient.PostAsJsonAsync("api/questions", question);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsAsync<QuestionDetails>();

        Assert.NotNull(result);
        Assert.Equal(1, DbContext.Questions.Count());
    }

    [Fact]
    public async Task Edit_ReturnsForbidden()
    {
        await Authenticate("member");

        var response = await HttpClient.PutAsJsonAsync($"api/question/{new Guid()}", new { });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Edit_ReturnsNotFound()
    {
        await Authenticate();

        var response = await HttpClient.PutAsJsonAsync($"api/question/{new Guid()}", new QuestionFactory().GetOne());
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var result = await response.Content.ReadAsAsync<ApiErrorResponse>();

        Assert.NotNull(result?.Message);
        Assert.NotEmpty(result.Message);
    }

    [Fact]
    public async Task Edit_FailsInvalidModel()
    {
        var question = new QuestionFactory().GetOne();
        await CreateQuestion(question);

        await Authenticate();

        var response = await HttpClient.PutAsJsonAsync($"api/question/{question.Id}", question);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Edit_FailsSameTitle()
    {
        var persistedQuestion = new QuestionFactory().GetOne();
        persistedQuestion.Title = "test";
        await CreateQuestion(persistedQuestion);

        var question = new QuestionFactory().GetOne();
        question.Title = "test";
        await CreateQuestion(question);

        await Authenticate();

        var response = await HttpClient.PutAsJsonAsync($"api/question/{question.Id}", question);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadAsAsync<ApiErrorResponse>();

        Assert.NotNull(result?.Message);
        Assert.NotEmpty(result.Message);
    }

    [Fact]
    public async Task GetIllustration_ReturnsNotFound()
    {
        var response = await HttpClient.GetAsync($"api/question/{new Guid()}/illustration");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetIllustration_ReturnsNothing_NoAttachment()
    {
        var question = new QuestionFactory().GetOne();
        await CreateQuestion(question);

        var response = await HttpClient.GetAsync($"api/question/{question.Id}/illustration");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetIllustration_ReturnsAttachment()
    {
        var attachment = new AttachmentFactory().GetOne();
        await CreateAttachment(attachment);

        var question = new QuestionFactory().GetOne();
        question.IllustrationId = attachment.Id;

        await CreateQuestion(question);

        var response = await HttpClient.GetAsync($"api/question/{question.Id}/illustration");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsByteArrayAsync();

        Assert.NotEmpty(result);
        Assert.Equivalent(attachment.Data, result);
    }

    [Fact]
    public async Task EditIllustration_ReturnsUnauthorized()
    {
        var response = await HttpClient.PatchAsJsonAsync($"api/question/{new Guid()}/illustration", new { });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task EditIllustration_ReturnsNotFound()
    {
        await Authenticate();

        var response = await HttpClient.PatchAsJsonAsync($"api/question/{new Guid()}/illustration", new { });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task EditIllustration_RemovesIllustration()
    {
        var attachment = new AttachmentFactory().GetOne();
        await CreateAttachment(attachment);

        var question = new QuestionFactory().GetOne();
        question.IllustrationId = attachment.Id;
        await CreateQuestion(question);

        await Authenticate();

        var response = await HttpClient.PatchAsJsonAsync($"api/question/{question.Id}/illustration", new { });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.Equal(0, DbContext.Attachments.Count());
    }

    [Fact]
    public async Task Destroy_ReturnsUnauthorized()
    {
        var response = await HttpClient.DeleteAsync($"api/question/{new Guid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Destroy_ReturnsNotFound()
    {
        await Authenticate();

        var response = await HttpClient.DeleteAsync($"api/question/{new Guid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Destroy_DeletesQuestion()
    {
        var question = await CreateQuestion(new QuestionFactory().GetOne());
        await Authenticate();

        var response = await HttpClient.DeleteAsync($"api/question/{question.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.Equal(0, await DbContext.Questions.CountAsync());
    }

    [Fact]
    public async Task Check_ReturnsNotFound()
    {
        var response = await HttpClient.GetAsync($"api/question/{new Guid()}/check/test");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Check_ReturnsUnauthorized()
    {
        var question = await CreateQuestion(new QuestionFactory().GetOne());

        var response =
            await HttpClient.GetAsync($"api/question/{question.Id}/check/{question.QuestionAnswers.Answer1}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Check_ReturnsCorrect()
    {
        var question = new QuestionFactory().GetOne();
        question.Visibility = QuestionVisibility.PUBLIC;
        await CreateQuestion(question);

        var response =
            await HttpClient.GetAsync($"api/question/{question.Id}/check/{question.QuestionAnswers.CorrectAnswer}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsAsync<CheckAnswerResult>();

        Assert.Equal(question.Id, result.QuestionId);
        Assert.True(result.IsCorrect);
        Assert.Equal(question.QuestionAnswers.Explications, result.Explications);
    }

    [Fact]
    public async Task Check_ReturnsIncorrect()
    {
        var question = new QuestionFactory().GetOne();
        question.Visibility = QuestionVisibility.PUBLIC;
        question.QuestionAnswers.CorrectAnswer = "--------";

        await CreateQuestion(question);

        var response =
            await HttpClient.GetAsync($"api/question/{question.Id}/check/{question.QuestionAnswers.Answer1}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsAsync<CheckAnswerResult>();

        Assert.Equal(question.Id, result.QuestionId);
        Assert.False(result.IsCorrect);
        Assert.Equal(question.QuestionAnswers.Explications, result.Explications);
    }

    [Fact]
    public async Task Check_CreatesAnswerRecord()
    {
        var user = await Authenticate();
        var question = await CreateQuestion(new QuestionFactory().GetOne());

        var pool = ServiceScope.ServiceProvider.GetRequiredService<IFreeQuestionsPoolService>();
        pool.SetPool([question.Id]);

        var response =
            await HttpClient.GetAsync($"api/question/{question.Id}/check/{question.QuestionAnswers.CorrectAnswer}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsAsync<CheckAnswerResult>();

        Assert.Equal(question.Id, result.QuestionId);
        Assert.True(result.IsCorrect);
        Assert.Equal(question.QuestionAnswers.Explications, result.Explications);

        var answerRecord = await DbContext.AnswerRecords.FirstOrDefaultAsync();

        Assert.NotNull(answerRecord);
        Assert.Equal(question.Id, answerRecord.QuestionId);
        Assert.Equal(user.Id, answerRecord.UserId);
        Assert.True(answerRecord.IsCorrect);
    }

    //  TODO test with mock exam

    [Fact]
    public async Task QuestionsCount_ReturnsUnauthorized()
    {
        var response = await HttpClient.GetAsync("api/questions/count");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task QuestionsCount_ReturnsQuestionsCount()
    {
        await Authenticate();

        var n = new Random().Next(5, 10);
        var questions = new QuestionFactory().GetMany(n);

        foreach (var question in questions) await CreateQuestion(question);

        var response = await HttpClient.GetAsync("api/questions/count");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsAsync<int>();

        Assert.Equal(result, n);
    }

    [Fact]
    public async Task Random_ReturnsUnauthorized()
    {
        var response = await HttpClient.GetAsync("api/questions/random");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Random_ReturnsRandomQuestion_NotPremium()
    {
        await Authenticate();

        var n = new Random().Next(5, 10);
        var questions = new QuestionFactory().GetMany(n).ToList();
        var freeQuestionIds = questions[..2].Select(q => q.Id).ToArray();

        foreach (var question in questions) await CreateQuestion(question);

        var pool = ServiceScope.ServiceProvider.GetRequiredService<IFreeQuestionsPoolService>();
        pool.SetPool(freeQuestionIds);

        var response = await HttpClient.GetAsync("api/questions/random");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsAsync<Guid>();

        Assert.Contains(result, questions.Select(q => q.Id));
        Assert.Contains(result, freeQuestionIds);
    }

    [Fact]
    public async Task Random_ReturnsRandomQuestion_Premium()
    {
        await Authenticate(premium: true);

        var n = new Random().Next(5, 10);
        var questions = new QuestionFactory().GetMany(n).ToList();

        foreach (var question in questions) await CreateQuestion(question);

        var response = await HttpClient.GetAsync("api/questions/random");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsAsync<Guid>();

        Assert.Contains(result, questions.Select(q => q.Id));
    }

    [Fact]
    public async Task Random_ReturnsRandomQuestion_Filtered()
    {
        await Authenticate(premium: true);

        var n = new Random().Next(5, 10);
        var questions = new QuestionFactory().GetMany(n).ToList();
        questions[0].Category = Category.AIR_LAW;
        questions[1..].ForEach(q => q.Category = Category.MASS_AND_BALANCE);

        foreach (var question in questions) await CreateQuestion(question);

        var response = await HttpClient.GetAsync($"api/questions/random?categories={Category.AIR_LAW}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsAsync<Guid>();
        var questionResult = await DbContext.Questions.FirstOrDefaultAsync(q => q.Id == questions[0].Id);

        Assert.NotNull(questionResult);
        Assert.Equal(Category.AIR_LAW, questionResult.Category);
    }

    private new async Task<Question> CreateQuestion(Question question)
    {
        await CreateUser(question.Publisher!);
        await base.CreateQuestion(question);

        return question;
    }

    private async Task<Attachment> CreateAttachment(Attachment attachment)
    {
        await DbContext.Attachments.AddAsync(attachment);
        await DbContext.SaveChangesAsync();

        return attachment;
    }
}