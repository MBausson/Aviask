using System.Web.Http.Description;
using AviaskApi.Entities;
using AviaskApi.Identity;
using AviaskApi.Models;
using AviaskApi.Models.Details;
using AviaskApi.Models.Result;
using AviaskApi.Repositories;
using AviaskApi.Services.Filterable;
using AviaskApi.Services.Jwt;
using AviaskApi.Services.StripeService;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FromUri = System.Web.Http.FromUriAttribute;

namespace AviaskApi.Controllers;

[ApiController]
public class UsersController : AviaskController<UsersController>
{
    private readonly AnswerRecordRepository _answerRecordRepository;
    private readonly IValidator<CreateUserModel> _createUserValidator;
    private readonly IValidator<EditUserModel> _editUserValidator;
    private readonly QuestionRepository _questionRepository;
    private readonly IStripeService _stripeService;
    private readonly UserManager<AviaskUser> _userManager;
    private readonly UserRepository _userRepository;

    public UsersController(IAviaskRepository<AviaskUser, Guid> userRepository,
        IAviaskRepository<AnswerRecord, Guid> answerRecordRepository,
        IJwtService jwt, UserManager<AviaskUser> userManager, IValidator<CreateUserModel> crateUserValidator,
        IValidator<EditUserModel> editUserValidator, IAviaskRepository<Question, Guid> questionRepository,
        ILogger<UsersController> logger,
        IFilterableService filterable, IStripeService stripeService) : base(jwt, logger, filterable)
    {
        _userRepository = (UserRepository)userRepository;
        _answerRecordRepository = (AnswerRecordRepository)answerRecordRepository;
        _userManager = userManager;
        _createUserValidator = crateUserValidator;
        _editUserValidator = editUserValidator;
        _stripeService = stripeService;
        _questionRepository = (QuestionRepository)questionRepository;
    }

    [HttpGet]
    [Route("/api/users")]
    [Authorize(Policy = AuthorizationPolicyConstants.Administration)]
    [ResponseType(typeof(AviaskUserExtended[]))]
    public async Task<FilteredResult<AviaskUserExtended>> Index([FromQuery] int? page = 1)
    {
        if (page is null) page = 1;

        var users = (await _userRepository.GetAllAsync())
            .Select(u =>
            {
                u.Role = _userManager.GetRolesAsync(u).Result.First();
                return u;
            })
            .ToArray();


        return new FilteredResult<AviaskUserExtended>(
            Filterable.Paginate(users.Select(AviaskUserExtended.FromAviaskUser), page.Value),
            users.Length);
    }

    //  GET: api/user/{Id}
    [HttpGet]
    [Route("api/user/{Id}")]
    [ResponseType(typeof(UserProfileResult))]
    public async Task<IActionResult> Show(Guid? id = null)
    {
        if (id is null)
        {
            var currentUser = await TryCurrentUserAsync();

            if (currentUser is null)
                return NotFound(new ApiErrorResponse("Could not find the profile you were looking for"));

            id = currentUser.Id;
        }

        var user = await _userRepository.GetByIdAsync(id.Value);

        if (user is null) return NotFound(new ApiErrorResponse("Could not find the profile you were looking for"));

        return Ok(new UserProfileResult(
            user.GetDetails(),
            GetAcceptedSuggestionsCount(user),
            GetAnswerRecordsCount(user))
        );
    }

    //  GET: api/user/extended/{Id}
    [HttpGet]
    [Route("api/user/extended/{Id}")]
    [Authorize(Policy = AuthorizationPolicyConstants.Administration)]
    [ResponseType(typeof(AviaskUser))]
    public async Task<IActionResult> ShowExtended([FromUri] Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user is null) return NotFound(new ApiErrorResponse("Could not find the profile you were looking for"));

        return Ok(user);
    }

    //  POST: api/users
    [HttpPost]
    [Route("api/users")]
    [AllowAnonymous]
    public async Task<IActionResult> New([FromBody] CreateUserModel userModel)
    {
        //  Validations
        var validationResult = await _createUserValidator.ValidateAsync(userModel);
        if (!validationResult.IsValid)
            return BadRequest(new ApiErrorResponse(validationResult.Errors.First().ErrorMessage));

        var user = new AviaskUser
        {
            UserName = userModel.Username,
            Email = userModel.Email
        };

        //  Username & emails should be unique
        if (await _userRepository.ExistsByEmailAsync(userModel.Email) ||
            await _userRepository.ExistsByUsernameAsync(userModel.Username))
            return BadRequest(new ApiErrorResponse("This email/username is already taken"));

        var creationResult = (await _userRepository.CreateUserAsync(user, userModel.Password)).ToArray();
        if (creationResult.Length != 0) return BadRequest("Could not create a new user");

        var newUser = await _userRepository.GetByEmailAsync(userModel.Email);

        return Ok(new AuthenticationResult(await Jwt.CreateTokenAsync(newUser!)));
    }

    //  GET: api/user/${Id}/update
    [HttpGet]
    [Route("/api/user/{Id}/update")]
    [Authorize(Policy = AuthorizationPolicyConstants.Administration)]
    [ResponseType(typeof(EditUserModel))]
    public async Task<IActionResult> Update([FromUri] Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user is null) return NotFound(new ApiErrorResponse("Could find the user you were looking for."));

        return Ok(new EditUserModel(user.UserName, user.Email, user.Role!, user.IsPremium));
    }

    //  PUT: api/user/{Id}
    [HttpPut]
    [Route("api/user/{Id}")]
    [Authorize(Policy = AuthorizationPolicyConstants.Administration)]
    public async Task<IActionResult> Edit([FromUri] Guid id, [FromBody] EditUserModel editModel)
    {
        //  Validation
        var validationResult = await _editUserValidator.ValidateAsync(editModel);
        if (!validationResult.IsValid)
            return BadRequest(new ApiErrorResponse(validationResult.Errors.First().ErrorMessage));

        var user = await _userRepository.GetByIdAsync(id);
        if (user is null) return NotFound(new ApiErrorResponse("Could not find the user you were looking for."));

        if (await UserAlreadyExistsAsync(editModel.Username, editModel.Email, user.Id))
            return BadRequest(new ApiErrorResponse("This username/email is already taken"));

        user.UserName = editModel.Username;
        user.Email = editModel.Email;

        if (user.Role != editModel.Role) await ChangeUserRole(user, editModel.Role);
        if (user.IsPremium != editModel.IsPremium) await CancelIfSubscribed(user, editModel.IsPremium);

        user.IsPremium = editModel.IsPremium;

        await _userRepository.UpdateAsync(user);

        return Ok();
    }

    //  DELETE: api/user/{Id}
    [HttpDelete]
    [Route("api/user/{Id}")]
    [Authorize(Policy = AuthorizationPolicyConstants.Administration)]
    public async Task<IActionResult> Destroy([FromUri] Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user is null) return NotFound(new ApiErrorResponse("Could not find the user you were looking for."));

        await _userRepository.DeleteAsync(user);

        return Ok();
    }

    //  Retrieves user statistics
    //  If no userId is provided, the user is the current authenticated user
    //  Only admin can specify a userId
    [HttpGet]
    [Route("/api/user/statistics")]
    [Authorize(Policy = AuthorizationPolicyConstants.AuthenticatedUsers)]
    public async Task<IActionResult> Statistics([FromQuery] Guid? id = null)
    {
        var currentUser = await CurrentUserAsync();
        var requestedUser = id is not null ? await GetUserIfAdminAsync(currentUser, id.Value) : currentUser;

        if (requestedUser is null)
            return NotFound(
                new ApiErrorResponse("The requested user does not exist or you don't have sufficient permissions."));

        return Ok(await GetUserStatistics(requestedUser));
    }

    [HttpGet]
    [Route("/api/user/publications")]
    [Authorize(Policy = AuthorizationPolicyConstants.AuthenticatedUsers)]
    public async Task<FilteredResult<QuestionDetails>> Publications([FromQuery] int? page = null)
    {
        var user = await CurrentUserAsync();

        if (page is null or < 1) page = 1;

        var query = _questionRepository
            .GetQuery()
            .Where(q => q.PublisherId == user.Id)
            .Select(q => q.GetDetails())
            .ToArray()
            .OrderByDescending(q => q!.Status).ThenByDescending(q => q!.PublishedAt);

        return new FilteredResult<QuestionDetails>(Filterable.Paginate(query, page.Value)!, query.Count());
    }

    [HttpGet]
    [Route("/api/users/leaderboard")]
    public async Task<CurrentLeaderboard> Leaderboard()
    {
        var currentUser = await TryCurrentUserAsync();
        var topUsers = await _userRepository.GetMostPublicationUsers(20);

        var leaderboard = new CurrentLeaderboard
        {
            Users = topUsers.Select(LeaderboardUser.FromData).ToArray(),
            CurrentUserCount = currentUser?.Publications.Count ?? 0
        };

        return leaderboard;
    }

    private async Task<UserStatistics> GetUserStatistics(AviaskUser user)
    {
        var userRecords = await _answerRecordRepository.GetAllByUserIdAsync(user.Id);

        var strongestCategories = GetUserReadyForExamCategories(userRecords);
        var weakestCategories = GetUserWeakestCategories(userRecords);

        var dateFilter = DateTime.Now.AddDays(-30);

        var last30daysCorrectAnswers = userRecords
            .Where(ar => ar.IsCorrect)
            .Where(ar => ar.AnsweredAt > dateFilter)
            .GroupBy(ar => ar.AnsweredAt.Date)
            .Select(group => new DayActivity(group.Key, group.Count()))
            .OrderBy(stat => stat.Day)
            .ToArray();

        var last30daysWrongAnswers = userRecords
            .Where(ar => !ar.IsCorrect)
            .Where(ar => ar.AnsweredAt > dateFilter)
            .GroupBy(ar => ar.AnsweredAt.Date)
            .Select(group => new DayActivity(group.Key, group.Count()))
            .OrderBy(stat => stat.Day)
            .ToArray();

        var totalCategories = userRecords
            .GroupBy(ar => ar.Question.Category)
            .Select(group =>
                new CategoryStatistics(group.Key,
                    (float)group.Count(ar => ar.IsCorrect) / group.Count(), group.Count()))
            .ToArray();

        return new UserStatistics
        {
            ReadyForExamCategories = strongestCategories,
            WeakestCategories = weakestCategories,
            Last30DaysCorrectAnswers = last30daysCorrectAnswers,
            Last30DaysWrongAnswers = last30daysWrongAnswers,
            TotalCategories = totalCategories
        };
    }

    private CategoryStatistics[] GetUserReadyForExamCategories(AnswerRecord[] userRecords)
    {
        return userRecords
            .Where(r => r.IsCorrect)
            .GroupBy(r => r.Question.Category)
            .Select(group => new CategoryStatistics(group.Key,
                (float)group.Count() / userRecords.Count(ar => ar.Question.Category == group.Key), group.Count()))
            .Where(stat => stat.CorrectnessRatio >= 0.75)
            .Take(3)
            .OrderByDescending(stat => stat.CorrectnessRatio)
            .ThenByDescending(stat => stat.AnswerCount)
            .ToArray();
    }

    private CategoryStatistics[] GetUserWeakestCategories(AnswerRecord[] userRecords)
    {
        return userRecords
            .Where(ar => !ar.IsCorrect)
            .GroupBy(ar => ar.Question.Category)
            .Select(group => new CategoryStatistics(
                group.Key,
                1.0f - (float)group.Count() / userRecords.Count(ar => ar.Question.Category == group.Key),
                group.Count()
            ))
            .Where(stat => stat.CorrectnessRatio < 0.5)
            .OrderByDescending(stat => stat.CorrectnessRatio)
            .ThenByDescending(stat => stat.AnswerCount)
            .Take(3)
            .ToArray();
    }

    private async Task ChangeUserRole(AviaskUser user, string role)
    {
        var userRoles = await _userManager.GetRolesAsync(user);
        foreach (var userRole in userRoles) await _userManager.RemoveFromRoleAsync(user, userRole);

        await _userManager.AddToRoleAsync(user, role);
    }

    private async Task CancelIfSubscribed(AviaskUser user, bool newIsPremium)
    {
        if (!newIsPremium) await _stripeService.StopCurrentSubscription(user);

        Logger.LogInformation($"User '{user.Id}' new IsPremium -> {newIsPremium}");
    }

    private int GetAnswerRecordsCount(AviaskUser user)
    {
        return _answerRecordRepository.GetQuery().Count(u => u.UserId == user.Id);
    }

    private int GetAcceptedSuggestionsCount(AviaskUser user)
    {
        return _questionRepository
            .GetQuery()
            .Where(q => q.PublisherId == user.Id)
            .Count(q => q.Status == QuestionStatus.ACCEPTED);
    }

    private async Task<AviaskUser?> GetUserIfAdminAsync(AviaskUser currentUser, Guid id)
    {
        currentUser.Role = (await _userManager.GetRolesAsync(currentUser)).First();

        if (currentUser.Role != "admin") return null;

        return await _userRepository.GetByIdAsync(id);
    }

    //  Tells if a user already exist with the same email/username
    //  Ignores users with `ignoredId`
    private async Task<bool> UserAlreadyExistsAsync(string username, string email, Guid ignoredId)
    {
        var sameUsernameEmailUsers = _userRepository.GetQuery()
            .Where(u => u.Id != ignoredId && (u.Email == email || u.UserName == username));

        return sameUsernameEmailUsers.Count() != 0;
    }
}
