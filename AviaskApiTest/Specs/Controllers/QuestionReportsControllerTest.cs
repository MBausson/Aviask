using System.Net;
using System.Net.Http.Json;
using AviaskApi.Entities;
using AviaskApi.Models;
using AviaskApi.Models.Result;
using AviaskApiTest.Abstractions;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace AviaskApiTest.Specs.Controllers;

public class QuestionReportsControllerTest : FunctionalTest
{
    public QuestionReportsControllerTest(FunctionalTestWebApplicationFactory factory, ITestOutputHelper output) :
        base(factory, output)
    {
    }

    [Fact]
    public async Task Index_ReturnsForbidden()
    {
        await Authenticate("member");

        var response = await HttpClient.GetAsync("api/questionReports");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Index_ReturnsReports()
    {
        await Authenticate();

        var question = await CreateQuestion(new QuestionFactory().GetOne());
        var reports = new QuestionReportFactory().GetMany(4).ToList();
        var reportIds = reports.Select(r => r.Id);

        foreach (var report in reports)
        {
            report.Question = question;
            report.QuestionId = question.Id;

            await CreateReport(report);
        }

        var response = await HttpClient.GetAsync("api/questionReports");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsAsync<FilteredResult<QuestionReportDetails>>();

        Assert.Equal(4, result.TotalCount);
        Assert.Equal(4, result.Elements.Count());
        Assert.All(result.Elements, r => Assert.Contains(r.Id, reportIds));
    }

    [Fact]
    public async Task Show_ReturnsForbidden()
    {
        await Authenticate("member");

        var response = await HttpClient.GetAsync($"api/questionReport/{new Guid()}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Show_ReturnsNotFound()
    {
        await Authenticate();

        var response = await HttpClient.GetAsync($"api/questionReports/{new Guid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Show_ReturnsReport()
    {
        await Authenticate();

        var report = await CreateReport(new QuestionReportFactory().GetOne());

        var response = await HttpClient.GetAsync($"api/questionReport/{report.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsAsync<QuestionReportDetails>();

        Assert.Equal(report.Id, result.Id);
    }

    [Fact]
    public async Task New_ReturnsUnauthorized()
    {
        var response = await HttpClient.PostAsJsonAsync("api/questionReports", new { });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task New_FailsInvalidModel()
    {
        await Authenticate();

        var model = new CreateQuestionReportModelFactory().GetOne();
        model.Message = "";

        var response = await HttpClient.PostAsJsonAsync("api/questionReports", model);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadAsAsync<ApiErrorResponse>();

        Assert.NotNull(result?.Message);
        Assert.NotEmpty(result.Message);
        Assert.Empty(await DbContext.QuestionReports.ToArrayAsync());
    }

    [Fact]
    public async Task New_FailsAlreadyReported()
    {
        var user = await Authenticate();

        var persistedReport = new QuestionReportFactory().GetOne();
        persistedReport.IssuerId = user.Id;
        persistedReport.Issuer = user;
        persistedReport.State = ReportState.PENDING;

        persistedReport = await CreateReport(persistedReport);

        var model = new CreateQuestionReportModel
        {
            Category = persistedReport.Category,
            QuestionId = persistedReport.QuestionId,
            Message = "A report message"
        };

        var response = await HttpClient.PostAsJsonAsync("api/questionReports", model);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadAsAsync<ApiErrorResponse>();

        Assert.NotNull(result?.Message);
        Assert.NotEmpty(result.Message);
        Assert.Equal(1, await DbContext.QuestionReports.CountAsync());
    }

    [Fact]
    public async Task New_CreatesReport()
    {
        await Authenticate();

        var report = await CreateReport(new QuestionReportFactory().GetOne());
        var model = new CreateQuestionReportModel
        {
            Category = ReportCategory.FEEDBACK,
            Message = "Testing testing",
            QuestionId = report.QuestionId
        };

        var response = await HttpClient.PostAsJsonAsync("api/questionReports", model);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Edit_ReturnsForbidden()
    {
        await Authenticate("member");

        var response = await HttpClient.PatchAsJsonAsync($"api/questionReport/{new Guid()}", new { });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Edit_ReturnsNotFound()
    {
        await Authenticate();

        var response = await HttpClient.PatchAsJsonAsync($"api/questionReport/{new Guid()}", new { });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Edit_UpdatesState()
    {
        await Authenticate();

        var report = new QuestionReportFactory().GetOne();
        report.State = ReportState.PENDING;
        await CreateReport(report);

        var response =
            await HttpClient.PatchAsJsonAsync($"api/questionReport/{report.Id}?state={ReportState.DECLINED}", new { });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsAsync<QuestionReportDetails>();

        Assert.Equal(report.Id, result.Id);
        Assert.Equal(ReportState.DECLINED, result.State);
    }

    [Fact]
    public async Task Delete_ReturnsForbidden()
    {
        await Authenticate("member");

        var result = await HttpClient.DeleteAsync($"api/questionReport/{new Guid()}");

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound()
    {
        await Authenticate();

        var result = await HttpClient.DeleteAsync($"api/questionReport/{new Guid()}");

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task Delete_DestroysReport()
    {
        await Authenticate();

        var report = await CreateReport(new QuestionReportFactory().GetOne());

        var result = await HttpClient.DeleteAsync($"api/questionReport/{report.Id}");
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        Assert.Empty(await DbContext.QuestionReports.ToArrayAsync());
    }

    private async Task<QuestionReport> CreateReport(QuestionReport report)
    {
        await DbContext.QuestionReports.AddAsync(report);
        await DbContext.SaveChangesAsync();

        return report;
    }
}