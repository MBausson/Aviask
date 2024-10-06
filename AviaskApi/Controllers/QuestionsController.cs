using AviaskApi.Entities;
using AviaskApi.Identity;
using AviaskApi.Models.Details;
using AviaskApi.Models.Result;
using AviaskApi.Repositories;
using AviaskApi.Services.Attachment;
using AviaskApi.Services.Filterable;
using AviaskApi.Services.FreeQuestionsPool;
using AviaskApi.Services.HtmlContentSanitizer;
using AviaskApi.Services.Jwt;
using AviaskApi.Services.MockExaminer;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AviaskApi.Controllers;

[ApiController]
public class QuestionsController : AviaskController<QuestionsController>
{
    private readonly AnswerRecordRepository _answerRecordRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly IFreeQuestionsPoolService _freeQuestions;
    private readonly IHtmlContentSanitizerService _htmlContentSanitizerService;
    private readonly IMockExaminer _mockExaminer;
    private readonly MockExamRepository _mockExamRepository;
    private readonly QuestionRepository _questionRepository;
    private readonly IValidator<Question> _questionValidator;

    public QuestionsController(IAviaskRepository<Question, Guid> questionRepository, IJwtService jwt,
        IAviaskRepository<AnswerRecord, Guid> answerRecordRepository, IFreeQuestionsPoolService freeQuestions,
        IAttachmentService attachmentService, IAviaskRepository<MockExam, Guid> mockExamRepository,
        IHtmlContentSanitizerService htmlContentSanitizerService, IMockExaminer mockExaminer,
        ILogger<QuestionsController> logger,
        IValidator<Question> questionValidator, IFilterableService filterableService) : base(jwt, logger,
        filterableService)
    {
        _questionRepository = (QuestionRepository)questionRepository;
        _answerRecordRepository = (AnswerRecordRepository)answerRecordRepository;
        _mockExamRepository = (MockExamRepository)mockExamRepository;
        _questionValidator = questionValidator;
        _freeQuestions = freeQuestions;
        _attachmentService = attachmentService;
        _htmlContentSanitizerService = htmlContentSanitizerService;
        _mockExaminer = mockExaminer;
    }

    //  GET: api/questions?categoryFilter={categoryFilter}&page={page}&sourceFilter={source}&titleFilter={title}
    [HttpGet]
    [Route("api/questions")]
    [AllowAnonymous]
    public async Task<IActionResult> Index([FromQuery] Category[]? categoryFilter = default,
        [FromQuery] int? page = 1,
        [FromQuery] string titleFilter = "", [FromQuery] string sourceFilter = "")
    {
        if (categoryFilter is null || categoryFilter.Contains(Category.NULL)) categoryFilter = Array.Empty<Category>();

        if (page is null or < 1) page = 1;

        var currentUser = await TryCurrentUserAsync();
        IQueryable<Question> query;

        if (currentUser is null)
            query = _questionRepository.GetQuery().Where(q => q.Visibility == QuestionVisibility.PUBLIC);
        else if (currentUser.IsPremium)
            query = _questionRepository.GetVisibleQuestionsQuery();
        else
            query = await _questionRepository.GetFreeQuestionsAsync(_freeQuestions);

        //  Category filtering
        if (!categoryFilter.IsNullOrEmpty()) query = GetCategoryFilteredQuestionsQuery(query, categoryFilter);

        //  Title filtering
        if (!titleFilter.IsNullOrEmpty()) query = GetTitleFilteredQuestionsQuery(query, titleFilter.ToLower());

        //  Source filtering
        if (!sourceFilter.IsNullOrEmpty()) query = GetSourceFilteredQuestionQuery(query, sourceFilter.ToLower());

        query = query.OrderByDescending(q => q.PublishedAt);

        return Ok(new FilteredResult<QuestionDetails>(
            Filterable.Paginate(query.Select(q => q.GetDetails()), page.Value)!,
            query.Count()));
    }

    //  GET: api/question/{Id}
    [HttpGet]
    [Route("api/question/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> Show(Guid id)
    {
        var question = await _questionRepository.GetByIdVisibleAsync(id);
        if (question is null) return NotFound(new ApiErrorResponse("Could not find request question"));

        var currentUser = await TryCurrentUserAsync();
        if (!CanUserAccessQuestion(currentUser, question))
            return Unauthorized(new ApiErrorResponse("You cannot access this question"));

        return Ok(question.GetDetails());
    }

    //  GET: api/question/extended/{Id}
    [HttpGet]
    [Route("api/question/extended/{id}")]
    [Authorize(Policy = AuthorizationPolicyConstants.QuestionModeration)]
    public async Task<IActionResult> ShowExtended(Guid id)
    {
        var question = await _questionRepository.GetByIdVisibleAsync(id);

        if (question is null) return NotFound(new ApiErrorResponse("Could not find request question"));

        return Ok(question);
    }

    //  POST: api/questions
    [HttpPost]
    [Route("api/questions/")]
    [Authorize(Policy = AuthorizationPolicyConstants.QuestionModeration)]
    public async Task<IActionResult> New([FromBody] Question question)
    {
        question.Sanitize(_htmlContentSanitizerService);

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
        question.Status = QuestionStatus.ACCEPTED;

        await _questionRepository.CreateAsync(question);

        return Ok(question.GetDetails());
    }

    //  PUT: api/question/{Id}
    [HttpPut]
    [Route("api/question/{id}")]
    [Authorize(Policy = AuthorizationPolicyConstants.QuestionModeration)]
    public async Task<IActionResult> Edit(Guid id, [FromBody] Question question)
    {
        question.Sanitize(_htmlContentSanitizerService);

        //  Validations
        var validationResult = await _questionValidator.ValidateAsync(question);
        if (!validationResult.IsValid)
            return BadRequest(new ApiErrorResponse(validationResult.Errors.First().ErrorMessage));

        if (!await _questionRepository.ExistsByIdAsync(id))
            return NotFound(new ApiErrorResponse("Could not find request question"));

        //  Check that the question title doesn't already exist
        var questionsWithSameTitle = (await _questionRepository.GetByTitleAsync(question.Title)).Where(q => q.Id != id);

        if (!questionsWithSameTitle.IsNullOrEmpty())
            return BadRequest(new ApiErrorResponse("Another question with the same title exists"));

        await _questionRepository.UpdateAsync(question);

        return Ok(question.GetDetails());
    }

    //  GET: api/question/{id}/illustration
    [HttpGet]
    [Route("/api/question/{id}/illustration")]
    public async Task<IActionResult> GetIllustration(Guid id)
    {
        var question = await _questionRepository.GetByIdAsync(id);

        if (question is null) return NotFound(new ApiErrorResponse("Could not find request question"));
        if (question.IllustrationId is null) return NoContent();

        var illustration = await _attachmentService.GetAttachmentByIdAsync(question.IllustrationId.Value);

        if (illustration is null) return NoContent();

        return File(illustration.Data, illustration.FileType);
    }

    //  PATCH: /api/question/{id}/illustration
    [HttpPatch]
    [Route("/api/question/{id}/illustration")]
    [Authorize(Policy = AuthorizationPolicyConstants.QuestionModeration)]
    public async Task<IActionResult> EditIllustration(Guid id, [FromForm] IFormFile? file = null)
    {
        var question = await _questionRepository.GetByIdAsync(id);

        if (question is null) return NotFound(new ApiErrorResponse("Could not find requested question"));

        Attachment? illustration;

        //  If no file is provided, remove the current illustration
        if (file is null)
        {
            if (question.IllustrationId is null) return Ok();

            illustration = await _attachmentService.GetAttachmentByIdAsync(question.IllustrationId.Value);

            if (illustration is null) return Ok();

            question.IllustrationId = null;

            await _questionRepository.UpdateAsync(question);
            await _attachmentService.DeleteAttachmentAsync(illustration);

            return Ok();
        }

        //  File validation
        if (!_attachmentService.ValidateFile(file, 3000, "image/png", "image/jpg", "image/jpeg"))
            return BadRequest(new ApiErrorResponse("The illustration does not match size and type requirements"));

        //  Converts the file to an attachment
        //  If the question already had an attachment, update it
        illustration = await _attachmentService.FileToAttachmentAsync(file);

        //  IDK why I did that
        //  I don't touch for now, it seems to work fine
        if (question.IllustrationId is not null) illustration.Id = illustration.Id;

        await _attachmentService.CreateOrUpdateAsync(illustration);

        question.IllustrationId = illustration.Id;
        await _questionRepository.UpdateAsync(question);

        return Ok(question.IllustrationId);
    }

    //  DELETE: api/question/{Id}
    [HttpDelete]
    [Route("/api/question/{id}")]
    [Authorize(Policy = AuthorizationPolicyConstants.QuestionModeration)]
    public async Task<IActionResult> Destroy(Guid id)
    {
        var question = await _questionRepository.GetByIdAsync(id);

        if (question is null) return NotFound(new ApiErrorResponse("Could not find request question"));

        await _questionRepository.DeleteAsync(question);
        return Ok();
    }

    //  GET: api/question/{Id}/check/{answer}
    [HttpGet]
    [Route("api/question/{Id}/check/{answer}")]
    [AllowAnonymous]
    public async Task<IActionResult> Check(Guid id, string answer)
    {
        var question = await _questionRepository.GetByIdAsync(id);
        if (question == null) return NotFound(new ApiErrorResponse("Could not find requested question"));

        var currentUser = await TryCurrentUserAsync();
        if (!CanUserAccessQuestion(currentUser, question)) return Unauthorized("You cannot access this question");

        if (currentUser is not null) await AddAnswerRecord(currentUser, question, answer);

        return Ok(new CheckAnswerResult(
            id,
            question.IsCorrect(answer),
            question.QuestionAnswers.CorrectAnswer,
            question.QuestionAnswers.Explications)
        );
    }

    //  GET: api/questions/count
    [HttpGet]
    [Route("api/questions/count")]
    [Authorize(Policy = AuthorizationPolicyConstants.AuthenticatedUsers)]
    public async Task<int> QuestionsCount()
    {
        return await _questionRepository.GetQuestionsCountCached();
    }

    //  GET: api/questions/random
    [HttpGet]
    [Route("api/questions/random")]
    [Authorize(Policy = AuthorizationPolicyConstants.AuthenticatedUsers)]
    public async Task<IActionResult> Random([FromQuery] Category[]? categories = null)
    {
        var currentUser = await CurrentUserAsync();
        var freeQuestionIds = _freeQuestions.GetQuestionsIds();

        var query = currentUser.IsPremium
            ? _questionRepository.GetQuery()
            : _questionRepository.GetQuery().Where(q => freeQuestionIds.Contains(q.Id));

        if (!categories.IsNullOrEmpty()) query = query.Where(q => categories!.Contains(q.Category));

        var randomIndex = new Random().Next(query.Count());

        return Ok(query.Skip(randomIndex).Select(q => q.Id).First());
    }

    private IQueryable<Question> GetCategoryFilteredQuestionsQuery(IQueryable<Question> query, Category[] categories)
    {
        return query.Where(q => categories.Contains(q.Category));
    }

    private IQueryable<Question> GetTitleFilteredQuestionsQuery(IQueryable<Question> query, string titleFilter)
    {
        var fragments = titleFilter.ToLower().Split(" ");

        return query.Where(q => fragments.All(f => q.Title.ToLower().Contains(f)));
    }

    private IQueryable<Question> GetSourceFilteredQuestionQuery(IQueryable<Question> query, string sourceFilter)
    {
        return query.Where(q => q.Source.ToLower().Contains(sourceFilter));
    }

    private bool CanUserAccessQuestion(AviaskUser? user, Question question)
    {
        if (user is null) return question.Visibility == QuestionVisibility.PUBLIC;

        if (user.IsPremium) return true;

        return _freeQuestions.IsQuestionInPool(question.Id);
    }

    private async Task AddAnswerRecord(AviaskUser user, Question question, string answer)
    {
        var answerRecord = AnswerRecord.FromQuestionAndAnswer(question, user, answer);
        var mockExam = await _mockExamRepository.GetOngoingByUserIdAsync(user.Id);

        var isPartOfMockExam = mockExam is not null && mockExam.QuestionId == question.Id;

        if (!isPartOfMockExam)
        {
            await _answerRecordRepository.CreateAsync(answerRecord);
            return;
        }

        await UpdateMockExam(answerRecord, mockExam);
    }

    private async Task UpdateMockExam(AnswerRecord answerRecord, MockExam mockExam)
    {
        answerRecord.MockExam = mockExam;
        await _answerRecordRepository.CreateAsync(answerRecord);

        var nextQuestionId = await _mockExaminer.GetNextQuestionAsync(mockExam);

        if (CanMockExamContinue(mockExam, nextQuestionId))
        {
            mockExam.QuestionId = nextQuestionId;
            await _mockExamRepository.UpdateAsync(mockExam);

            return;
        }

        await _mockExamRepository.FinishMockExamAsync(mockExam);
    }

    private bool CanMockExamContinue(MockExam mockExam, Guid? nextQuestionId)
    {
        return nextQuestionId is not null && mockExam.HasNextQuestion() && mockExam.HasTimeRemaining();
    }
}
