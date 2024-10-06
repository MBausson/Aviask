using System.Net;
using AviaskApi.Entities;
using AviaskApi.Models;
using AviaskApi.Models.Result;
using AviaskApi.Services.Jwt;
using AviaskApiTest.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AviaskApiTest.Specs.Controllers;

public class AuthenticationControllerTest : FunctionalTest
{
    private readonly CreateUserModel _dummyUserCredentials = new("dummy", "test@example.com", "testing");

    public AuthenticationControllerTest(FunctionalTestWebApplicationFactory factory, ITestOutputHelper output) :
        base(factory, output)
    {
    }

    [Fact]
    public async Task Login_FailsInvalidModel()
    {
        var model = new LoginModel("test.com", "testing");

        var response = await HttpClient.PostAsJsonAsync("api/authentication/login", model);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadAsAsync<ApiErrorResponse>();

        Assert.NotNull(result);
        Assert.NotEmpty(result.Message);
    }

    [Fact]
    public async Task Login_FailsInvalidCredentials_UnknownAccount()
    {
        var model = new LoginModel("test@account.com", "testing");

        var response = await HttpClient.PostAsJsonAsync("api/authentication/login", model);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadAsAsync<ApiErrorResponse>();

        Assert.NotNull(result);
        Assert.NotEmpty(result.Message);
    }

    [Fact]
    public async Task Login_FailsInvalidCredentials_InvalidPassword()
    {
        await RegisterDummyUser();
        var model = new LoginModel(_dummyUserCredentials.Email, "NOT GOOD PASSWORD");

        var response = await HttpClient.PostAsJsonAsync("api/authentication/login", model);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadAsAsync<ApiErrorResponse>();

        Assert.NotNull(result);
        Assert.NotEmpty(result.Message);
    }

    [Fact]
    public async Task Login_ReturnsSuccess()
    {
        await RegisterDummyUser();
        var model = new LoginModel(_dummyUserCredentials.Email, _dummyUserCredentials.Password);

        var response = await HttpClient.PostAsJsonAsync("api/authentication/login", model);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsAsync<AuthenticationResult>();

        Assert.NotNull(result?.Token);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task Authenticate_FailsInvalidToken()
    {
        var response = await HttpClient.PostAsJsonAsync("api/authentication/authenticate", "test");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadAsAsync<ApiErrorResponse>();

        Assert.NotNull(result?.Message);
        Assert.NotEmpty(result.Message);
    }

    [Fact]
    public async Task Authenticate_ReturnsUser()
    {
        var user = await RegisterDummyUser();
        var jwtService = await GetJwtService();

        var response =
            await HttpClient.PostAsJsonAsync("api/authentication/authenticate",
                await jwtService.CreateTokenAsync(user));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsAsync<AviaskUserDetails>();

        Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    public async Task Refresh_ReturnsUnauthorized()
    {
        var result = await HttpClient.GetAsync("api/authentication/refresh");

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task Refresh_ReturnsNewToken()
    {
        await Authenticate();

        var result = await HttpClient.GetAsync("api/authentication/refresh");
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var response = await result.Content.ReadAsAsync<AuthenticationResult>();

        Assert.NotNull(response?.Token);
        Assert.NotEmpty(response.Token);
    }

    [Fact]
    public async Task Recovery_ReturnsNotFound()
    {
        var email = "test@example.com";

        var response = await HttpClient.PostAsJsonAsync("api/authentication/recovery", email);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PasswordReset_FailsInvalidModel()
    {
        var model = new PasswordResetModel("abcd", "testing", "testinge");

        var response = await HttpClient.PostAsJsonAsync("api/authentication/passwordReset", model);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PasswordReset_FailsTokenNotFound()
    {
        var model = new PasswordResetModel("abcd", "testing", "testing");

        var response = await HttpClient.PostAsJsonAsync("api/authentication/passwordReset", model);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PasswordReset_FailsExpiredToken()
    {
        var user = await RegisterDummyUser();
        var token = await CreateRecoveryToken(user, Expired(user));

        var model = new PasswordResetModel(token.Id.ToString(), "testing", "testing");

        var response = await HttpClient.PostAsJsonAsync("api/authentication/passwordReset", model);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PasswordReset_UpdatesPassword()
    {
        var user = await RegisterDummyUser();
        var token = await CreateRecoveryToken(user, Pending(user));

        var model = new PasswordResetModel(token.Id.ToString(), "new_password", "new_password");
        var response = await HttpClient.PostAsJsonAsync("api/authentication/passwordReset", model);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        //  Try to login with new password
        var loginModel = new LoginModel(user.Email, "new_password");
        var loginResponse = await HttpClient.PostAsJsonAsync("api/authentication/login", loginModel);

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginResult = await loginResponse.Content.ReadAsAsync<AuthenticationResult>();

        Assert.NotNull(loginResult?.Token);
        Assert.NotEmpty(loginResult.Token);
        Assert.Empty(await DbContext.RecoveryTokens.ToArrayAsync());
    }

    private async Task<JwtService> GetJwtService()
    {
        return new JwtService(ServiceScope.ServiceProvider.GetRequiredService<UserManager<AviaskUser>>(),
            ServiceScope.ServiceProvider.GetRequiredService<IAviaskRepository<AviaskUser, Guid>>());
    }

    private async Task<AviaskUser> RegisterDummyUser()
    {
        var response = await HttpClient.PostAsJsonAsync("api/users", _dummyUserCredentials);
        response.EnsureSuccessStatusCode();

        return await DbContext.Users.FirstAsync(u => _dummyUserCredentials.Email == u.Email);
    }

    private async Task<RecoveryToken> CreateRecoveryToken(AviaskUser user, RecoveryToken recoveryToken)
    {
        await DbContext.RecoveryTokens.AddAsync(recoveryToken);
        await DbContext.SaveChangesAsync();

        return recoveryToken;
    }

    private RecoveryToken Expired(AviaskUser user)
    {
        //  Expired 5 minutes ago
        return new RecoveryToken
        {
            Id = new Guid(),
            User = user,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-5)
        };
    }

    private RecoveryToken Pending(AviaskUser user)
    {
        //  Expires in 5 minutes
        return new RecoveryToken
        {
            Id = new Guid(),
            User = user,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };
    }
}