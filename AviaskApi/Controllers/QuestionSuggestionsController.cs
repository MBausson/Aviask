using AviaskApi.Entities;
using AviaskApi.Identity;
using AviaskApi.Models.Details;
using AviaskApi.Models.Result;
using AviaskApi.Repositories;
using AviaskApi.Services.Filterable;
using AviaskApi.Services.Jwt;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AviaskApi.Controllers;

[ApiController]
public class QuestionSuggestionsController : AviaskController<QuestionSuggestionsController>
{
    private readonly QuestionRepository _questionRepository;
    private readonly IValidator<Question> _questionValidator;

    public QuestionSuggestionsController(IJwtService jwt, ILogger<QuestionSuggestionsController> logger,
        IAviaskRepository<Question, Guid> questionRepository,
        IValidator<Question> questionValidator, IFilterableService filterable) : base(jwt, logger, filterable)
    {
        _questionRepository = (QuestionRepository)questionRepository;
        _questionValidator = questionValidator;
    }

    //  GET: api/suggestions
    [HttpGet]
    [Route("api/suggestions")]
    [Authorize(Policy = AuthorizationPolicyConstants.QuestionModeration)]
    public async Task<FilteredResult<QuestionDetails>> Index([FromQuery] int? page = 1)
    {
        if (page is null || page < 1) page = 1;

        var query = (await _questionRepository.GetSuggestionsAsync())
            .Select(s => s.GetDetails())
            .AsQueryable();

        return new FilteredResult<QuestionDetails>(Filterable.Paginate(query, page.Value)!, query.Count());
    }

    //  GET: api/suggestion/${id}
    [HttpGet]
    [Route("api/suggestion/{id}")]
    [Authorize(Policy = AuthorizationPolicyConstants.QuestionModeration)]
    public async Task<IActionResult> Show(Guid id)
    {
        var suggestion = await _questionRepository.GetByIdAsync(id);

        if (suggestion is null) return NotFound(new ApiErrorResponse("Could not find request suggestion"));

        return Ok(suggestion);
    }

    //  POST: api/suggestions/suggest
    [HttpPost]
    [Route("api/suggestions/new")]
    [Authorize(Policy = AuthorizationPolicyConstants.AuthenticatedUsers)]
    public async Task<IActionResult> New([FromBody] Question question)
    {
        //  Validations
        var validationResult = await _questionValidator.ValidateAsync(question);
        if (!validationResult.IsValid)
            return BadRequest(new ApiErrorResponse(validationResult.Errors.First().ErrorMessage));

        //  Check question uniqueness by its title
        if (await _questionRepository.ExistsByTitleAsync(question.Title))
            return BadRequest(new ApiErrorResponse("A question with the same title already exists"));

        //  Retrieves the publisher
        var publisher = await CurrentUserAsync();

        question.Publisher = publisher;
        question.PublisherId = publisher.Id;

        //  Creates the suggestion
        question.Status = QuestionStatus.PENDING;

        await _questionRepository.CreateAsync(question);

        return Ok(question.GetDetails());
    }

    //  PUT: api/suggestion/{id}
    [HttpPut]
    [Route("api/suggestion/{id}")]
    [Authorize(Policy = AuthorizationPolicyConstants.QuestionModeration)]
    public async Task<IActionResult> Edit(Guid id, [FromBody] Question question)
    {
        //  Validations
        var validationResult = await _questionValidator.ValidateAsync(question);
        if (!validationResult.IsValid)
            return BadRequest(new ApiErrorResponse(validationResult.Errors.First().ErrorMessage));

        //  Suggestion fetching
        var suggestion = await _questionRepository.GetSuggestionByIdAsync(id);
        if (suggestion is null) return NotFound(new ApiErrorResponse("Could not find the requested suggestion"));

        //  Check that the question title doesn't already exist
        var questionsWithSameTitle = (await _questionRepository.GetByTitleAsync(question.Title)).Where(q => q.Id != id);

        if (!questionsWithSameTitle.IsNullOrEmpty())
            return BadRequest(new ApiErrorResponse("Another question with the same title exists"));

        suggestion = question;
        await _questionRepository.UpdateAsync(suggestion);

        return Ok(question.GetDetails());
    }

    //  PATCH: api/suggestion/{id}/status
    [HttpPatch]
    [Route("api/suggestion/{id}/{status}")]
    [Authorize(Policy = AuthorizationPolicyConstants.QuestionModeration)]
    public async Task<IActionResult> UpdateStatus(Guid id, QuestionStatus status)
    {
        var suggestion = await _questionRepository.GetSuggestionByIdAsync(id);

        if (suggestion is null) return NotFound(new ApiErrorResponse("Could not find request suggestion"));

        suggestion.Status = status;
        await _questionRepository.UpdateAsync(suggestion);

        return Ok();
    }
}