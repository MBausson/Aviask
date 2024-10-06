using AviaskApi.Entities;
using AviaskApi.Identity;
using AviaskApi.Models;
using AviaskApi.Models.Result;
using AviaskApi.Repositories;
using AviaskApi.Services.Filterable;
using AviaskApi.Services.Jwt;
using AviaskApi.Services.MockExaminer;
using AviaskApi.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AviaskApi.Controllers;

public class MockExamsController : AviaskController<MockExam>
{
    private readonly IMockExaminer _mockExaminer;
    private readonly MockExamRepository _repository;

    private readonly int MAX_ITEMS_PER_PAGE = 15;

    public MockExamsController(IJwtService jwt, ILogger<MockExam> logger, IFilterableService filterable,
        IAviaskRepository<MockExam, Guid> repository, IMockExaminer mockExaminer) : base(jwt,
        logger, filterable)
    {
        _repository = (MockExamRepository)repository;
        _mockExaminer = mockExaminer;
    }

    //  GET: api/mockExams
    [HttpGet]
    [Route("api/mockExams")]
    [Authorize(Policy = AuthorizationPolicyConstants.AuthenticatedUsers)]
    public async Task<FilteredResult<MockExam>> Index([FromQuery] int? page = 1)
    {
        if (page is null) page = 1;

        var currentUser = await CurrentUserAsync();

        var query = _repository
            .GetQuery()
            .Where(m => m.UserId == currentUser.Id && m.Status == MockExamStatus.FINISHED)
            .OrderByDescending(m => m.StartedAt);

        return new FilteredResult<MockExam>(Filterable.Paginate(query, page.Value, MAX_ITEMS_PER_PAGE), query.Count());
    }

    //  GET: api/mockExam/{id}
    [HttpGet]
    [Route("api/mockExam/{id}")]
    [Authorize(Policy = AuthorizationPolicyConstants.AuthenticatedUsers)]
    public async Task<IActionResult> Show(Guid id)
    {
        var currentUser = await CurrentUserAsync();
        var mockExam = await _repository.GetByIdAsync(id);

        if (mockExam is null) return NotFound(new ApiErrorResponse("Could not find the requested mock exam."));
        if (mockExam.UserId != currentUser.Id && !currentUser.IsAdmin())
            return Unauthorized(new ApiErrorResponse("You cannot access this mock exam"));

        return Ok(mockExam);
    }

    //  GET: api/mockExam
    [HttpGet]
    [Route("api/mockExam")]
    [Authorize(Policy = AuthorizationPolicyConstants.AuthenticatedUsers)]
    public async Task<IActionResult> Current()
    {
        var user = await CurrentUserAsync();
        var mockExam = await _repository.GetOngoingByUserIdAsync(user.Id);

        return mockExam is null ? NotFound() : Ok(mockExam);
    }

    //  POST: api/mockExams
    [HttpPost]
    [Route("api/mockExams")]
    [Authorize(Policy = AuthorizationPolicyConstants.AuthenticatedUsers)]
    public async Task<IActionResult> New([FromBody] CreateMockExamModel model)
    {
        var validation = await new CreateMockExamValidator().ValidateAsync(model);
        if (!validation.IsValid) return BadRequest(validation.Errors.Select(m => m.ErrorMessage));

        var user = await CurrentUserAsync();
        var ongoingMockExam = await _repository.GetOngoingByUserIdAsync(user.Id);

        if (ongoingMockExam is not null)
            return BadRequest(new ApiErrorResponse("You already have an ongoing mock exam"));

        var mockExam = MockExam.FromCreateModel(model, user);
        mockExam.QuestionId = await _mockExaminer.GetNextQuestionAsync(mockExam);

        await _repository.CreateAsync(mockExam);
        await _mockExaminer.StartMockExamTimerAsync(mockExam);

        return Ok(mockExam);
    }

    //  GET: api/mockExam/next
    [HttpGet]
    [Route("api/mockExam/next")]
    [Authorize(Policy = AuthorizationPolicyConstants.AuthenticatedUsers)]
    public async Task<IActionResult> NextQuestion()
    {
        var currentUser = await CurrentUserAsync();
        var mockExam = await _repository.GetOngoingByUserIdAsync(currentUser.Id);

        if (mockExam?.QuestionId is null) return NotFound(new ApiErrorResponse("Could not find a next question"));

        return Ok(mockExam.QuestionId);
    }

    //  PATCH: api/mockExam/stopCurrent
    [HttpPatch]
    [Route("api/mockExam/stop")]
    [Authorize(Policy = AuthorizationPolicyConstants.AuthenticatedUsers)]
    public async Task<IActionResult> StopCurrent()
    {
        var user = await CurrentUserAsync();
        var ongoingMockExam = await _repository.GetOngoingByUserIdAsync(user.Id);

        if (ongoingMockExam is null) return BadRequest(new ApiErrorResponse("You aren't currently doing a mock exam"));

        await _repository.FinishMockExamAsync(ongoingMockExam);

        return Ok();
    }
}