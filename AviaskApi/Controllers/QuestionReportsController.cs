using AviaskApi.Entities;
using AviaskApi.Identity;
using AviaskApi.Models;
using AviaskApi.Models.Details;
using AviaskApi.Models.Result;
using AviaskApi.Repositories;
using AviaskApi.Services.Filterable;
using AviaskApi.Services.Jwt;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using FromUri = System.Web.Http.FromUriAttribute;

namespace AviaskApi.Controllers;

[ApiController]
public class QuestionReportsController : AviaskController<QuestionReportsController>
{
    private readonly IValidator<CreateQuestionReportModel> _createQuestionReportModelValidator;
    private readonly QuestionReportRepository _questionReportRepository;

    public QuestionReportsController(IAviaskRepository<QuestionReport, Guid> questionReportRepository,
        IValidator<CreateQuestionReportModel> createQuestionReportModelValidator, IJwtService jwt,
        ILogger<QuestionReportsController> logger, IFilterableService filterable) : base(jwt, logger, filterable)
    {
        _questionReportRepository = (QuestionReportRepository)questionReportRepository;
        _createQuestionReportModelValidator = createQuestionReportModelValidator;
    }

    [HttpGet]
    [Route("/api/questionReports")]
    [Authorize(Policy = AuthorizationPolicyConstants.QuestionModeration)]
    public async Task<IActionResult> Index([FromQuery] int? page = 1)
    {
        page ??= 1;

        var result = (await _questionReportRepository.GetAllAsync())
            .OrderBy(q => q.State)
            .Select(q => q.GetDetails())
            .ToArray();

        return Ok(new FilteredResult<QuestionReportDetails>(Filterable.Paginate(result, page.Value), result.Length));
    }

    //  Add a detailed version to allow the issuer to see its own report
    [HttpGet]
    [Route("/api/questionReport/{id}")]
    [Authorize(Policy = AuthorizationPolicyConstants.QuestionModeration)]
    public async Task<IActionResult> Show([FromUri] Guid id)
    {
        var report = await _questionReportRepository.GetByIdAsync(id);

        if (report is null) return NotFound(new ApiErrorResponse("Could not find requested report"));

        return Ok(report);
    }

    [HttpPost]
    [Route("/api/questionReports")]
    [Authorize(Policy = AuthorizationPolicyConstants.AuthenticatedUsers)]
    public async Task<IActionResult> New([FromBody] CreateQuestionReportModel model)
    {
        //  Validation
        var validationResult = await _createQuestionReportModelValidator.ValidateAsync(model);
        if (!validationResult.IsValid)
            return BadRequest(new ApiErrorResponse(validationResult.Errors.First().ErrorMessage));

        var user = await CurrentUserAsync();
        var questionReport = QuestionReport.FromCreateModel(model);

        if (UserAlreadyReportedThis(user.Id, model.QuestionId))
            return BadRequest(new ApiErrorResponse("You already have a report pending for this question"));

        questionReport.State = ReportState.PENDING;
        questionReport.IssuerId = user.Id;

        await _questionReportRepository.CreateAsync(questionReport);

        return Ok();
    }

    [HttpPatch]
    [Route("/api/questionReport/{id}")]
    [Authorize(Policy = AuthorizationPolicyConstants.QuestionModeration)]
    public async Task<IActionResult> Edit([FromUri] Guid id, [FromQuery] ReportState state)
    {
        var questionReport = await _questionReportRepository.GetByIdAsync(id);

        if (questionReport is null) return NotFound(new ApiErrorResponse("Could not find requested report"));

        questionReport.State = state;
        await _questionReportRepository.UpdateAsync(questionReport);

        return Ok(questionReport.GetDetails());
    }

    [HttpDelete]
    [Route("/api/questionReport/{id}")]
    [Authorize(Policy = AuthorizationPolicyConstants.QuestionModeration)]
    public async Task<IActionResult> Delete([FromUri] Guid id)
    {
        var questionReport = await _questionReportRepository.GetByIdAsync(id);

        if (questionReport is null) return NotFound(new ApiErrorResponse("Could not find requested report"));

        await _questionReportRepository.DeleteAsync(questionReport);
        return Ok();
    }

    private bool UserAlreadyReportedThis(Guid userId, Guid questionId)
    {
        return !_questionReportRepository.GetQuery()
            .Where(q => q.QuestionId == questionId && q.IssuerId == userId && q.State == ReportState.PENDING)
            .IsNullOrEmpty();
    }
}