using AviaskApi.Entities;
using AviaskApi.Identity;
using AviaskApi.Models;
using AviaskApi.Models.Result;
using AviaskApi.Repositories;
using AviaskApi.Services.Filterable;
using AviaskApi.Services.Jwt;
using AviaskApi.Services.Mailer;
using AviaskApi.Services.RecoveryToken;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AviaskApi.Controllers;

[ApiController]
public class AuthenticationController : AviaskController<AuthenticationController>
{
    private readonly IValidator<LoginModel> _loginModelValidator;
    private readonly IMailerService _mailer;
    private readonly IValidator<PasswordResetModel> _passwordResetModelValidator;
    private readonly IRecoveryTokenService _recoveryToken;
    private readonly UserManager<AviaskUser> _userManager;
    private readonly UserRepository _userRepository;

    public AuthenticationController(IAviaskRepository<AviaskUser, Guid> userRepository, IJwtService jwt,
        UserManager<AviaskUser> userManager, IValidator<LoginModel> loginModelValidator,
        IValidator<PasswordResetModel> passwordResetModelValidator,
        IMailerService mailer, IRecoveryTokenService recoveryToken, ILogger<AuthenticationController> logger,
        IFilterableService filterable) : base(jwt, logger,
        filterable)
    {
        _userRepository = (UserRepository)userRepository;
        _userManager = userManager;
        _loginModelValidator = loginModelValidator;
        _passwordResetModelValidator = passwordResetModelValidator;
        _mailer = mailer;
        _recoveryToken = recoveryToken;
    }

    //  POST: api/users/auth_informations
    /// <summary>
    ///     Checks the JWT validity
    ///     If valid, returns details about the jwt holder
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("api/authentication/authenticate")]
    [AllowAnonymous]
    public async Task<IActionResult> Authenticate([FromBody] string token)
    {
        var (valid, userId) = await Jwt.IsTokenValidAsync(token);
        if (!valid) return BadRequest(new ApiErrorResponse("Invalid token"));

        var user = await _userRepository.GetByIdAsync(userId!.Value);
        if (user is null) return NotFound(new ApiErrorResponse("User not found"));

        return Ok(user.GetDetails());
    }

    //  POST: api/authentication/login
    [HttpPost]
    [Route("api/authentication/login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginModel login)
    {
        //  Validations
        var validationResult = await _loginModelValidator.ValidateAsync(login);
        if (!validationResult.IsValid)
            return BadRequest(new ApiErrorResponse(validationResult.Errors.First().ErrorMessage));

        var user = await _userRepository.GetByEmailAsync(login.Email);

        if (user == null) return BadRequest(new ApiErrorResponse("Invalid email and/or password."));

        if (!await _userManager.CheckPasswordAsync(user, login.Password))
            return BadRequest(new ApiErrorResponse("Invalid email and/or password."));

        return Ok(new AuthenticationResult(await Jwt.CreateTokenAsync(user)));
    }

    //  GET: api/authentication/refresh
    //  Remarks: The user should already be authenticated -> the old JWT must still be valid
    [HttpGet]
    [Route("api/authentication/refresh")]
    [Authorize(Policy = AuthorizationPolicyConstants.AuthenticatedUsers)]
    public async Task<AuthenticationResult> Refresh()
    {
        var currentUser = await CurrentUserAsync();

        var newToken = await Jwt.CreateTokenAsync(currentUser);

        return new AuthenticationResult(newToken);
    }

    //  POST: api/authentication/recovery
    [HttpPost]
    [Route("api/authentication/recovery")]
    [AllowAnonymous]
    public async Task<IActionResult> Recovery([FromBody] string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user is null) return NotFound(new ApiErrorResponse("Could not find the associated user."));

        var token = await _recoveryToken.CreateRecoveryToken(user.Id);

        //  TODO: Use a job instead
        Task.Run(async () => { await _mailer.SendResetPasswordEmailAsync(user, token.ToString()); });

        return Ok();
    }

    //  POST: api/authentication/passwordReset
    [HttpPost]
    [Route("api/authentication/passwordReset")]
    [AllowAnonymous]
    public async Task<IActionResult> PasswordReset([FromBody] PasswordResetModel model)
    {
        var modelValidation = await _passwordResetModelValidator.ValidateAsync(model);
        if (!modelValidation.IsValid)
            return BadRequest(new ApiErrorResponse(modelValidation.Errors.First().ErrorMessage));

        var token = await _recoveryToken.GetValidToken(model.Token);
        if (token is null) return NotFound(new ApiErrorResponse("Could not find the appropriate user."));

        await _userManager.RemovePasswordAsync(token.User);
        await _userManager.AddPasswordAsync(token.User, model.Password);

        await _recoveryToken.DeleteRecoveryToken(token);

        return Ok();
    }
}