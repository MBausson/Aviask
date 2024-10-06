using AviaskApi.Entities;
using AviaskApi.Identity;
using AviaskApi.Models.Details;
using AviaskApi.Models.Result;
using AviaskApi.Repositories;
using AviaskApi.Services.Filterable;
using AviaskApi.Services.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FromUri = System.Web.Http.FromUriAttribute;

namespace AviaskApi.Controllers;

[ApiController]
public class AnswerRecordsController : AviaskController<AnswerRecordsController>
{
    private readonly AnswerRecordRepository _answerRecordRepository;
    private readonly UserManager<AviaskUser> _userManager;
    private readonly UserRepository _userRepository;

    public AnswerRecordsController(IAviaskRepository<AnswerRecord, Guid> repository,
        IAviaskRepository<AviaskUser, Guid> userRepository, IJwtService jwt,
        UserManager<AviaskUser> userManager, ILogger<AnswerRecordsController> logger,
        IFilterableService filterable) : base(jwt, logger, filterable)
    {
        _answerRecordRepository = (AnswerRecordRepository)repository;
        _userRepository = (UserRepository)userRepository;
        _userManager = userManager;
    }

    [HttpGet]
    [Route("api/answerRecords/user/{userId}")]
    [Authorize(Policy = AuthorizationPolicyConstants.AuthenticatedUsers)]
    public async Task<IActionResult> Index([FromUri] Guid userId, [FromQuery] int? page = 1)
    {
        var targetUser = await _userRepository.GetByIdAsync(userId);
        if (targetUser is null) return NotFound(new ApiErrorResponse("Please specify a target user"));

        //  authenticatedUser should not be null since we are inside an Authorized action
        //  If it is null, it means our jwt was badly created
        var authenticatedUser = await CurrentUserAsync();

        if (!await CanUserAccessAnswerRecordsAsync(authenticatedUser, targetUser))
            return Unauthorized(new ApiErrorResponse("You cannot access this user's answer records"));

        if (page is null or < 1) page = 1;

        var query = (await _answerRecordRepository.GetAllByUserIdAsync(userId))
            .OrderByDescending(r => r.AnsweredAt)
            .Select(r => r.GetDetails())
            .AsQueryable();

        return Ok(new FilteredResult<AnswerRecordDetails>(Filterable.Paginate(query, page.Value), query.Count()));
    }

    [HttpGet]
    [Route("api/answerRecords/{id}")]
    [Authorize(Policy = AuthorizationPolicyConstants.AuthenticatedUsers)]
    public async Task<IActionResult> Show([FromQuery] Guid id)
    {
        var answerRecord = await _answerRecordRepository.GetByIdAsync(id);
        if (answerRecord is null) return NotFound(new ApiErrorResponse("Could not find requested answer record"));

        return Ok(answerRecord.GetDetails());
    }

    [HttpGet]
    [Route("api/answerRecords/extended/{id}")]
    [Authorize(Policy = AuthorizationPolicyConstants.Administration)]
    public async Task<IActionResult> ShowExtended([FromQuery] Guid id)
    {
        var answerRecord = await _answerRecordRepository.GetByIdAsync(id);
        if (answerRecord is null) return NotFound(new ApiErrorResponse("Could not find requested answer record"));

        return Ok(answerRecord);
    }

    [HttpDelete]
    [Route("api/answerRecords/{id}")]
    [Authorize(Policy = AuthorizationPolicyConstants.Administration)]
    public async Task<IActionResult> Delete([FromUri] Guid id)
    {
        var answerRecord = await _answerRecordRepository.GetByIdAsync(id);
        if (answerRecord is null) return NotFound(new ApiErrorResponse("Could not find requested answer record"));

        await _answerRecordRepository.DeleteAsync(answerRecord);

        return Ok();
    }

    private async Task<bool> CanUserAccessAnswerRecordsAsync(AviaskUser issuerUser, AviaskUser targetUser)
    {
        var authenticatedUserRole = (await _userManager.GetRolesAsync(issuerUser)).FirstOrDefault();

        //  Members can only see their answer history
        //  Admins can see everyone's answer history
        return authenticatedUserRole == "admin" || issuerUser!.Id == targetUser.Id;
    }
}