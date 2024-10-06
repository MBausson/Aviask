using System.Net;
using AviaskApi.Entities;
using AviaskApi.Models;
using AviaskApi.Models.Result;
using AviaskApiTest.Abstractions;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace AviaskApiTest.Specs.Controllers;

[CollectionDefinition("Integration")]
public class UsersControllerTest : FunctionalTest
{
    private readonly QuestionFactory _questionFactory = new();
    private readonly UserFactory _userFactory = new();

    public UsersControllerTest(FunctionalTestWebApplicationFactory factory, ITestOutputHelper output) : base(factory,
        output)
    {
    }

    [Fact]
    public async Task Index_ReturnsUnauthorized()
    {
        var users = _userFactory.GetMany(5);

        foreach (var aviaskUser in users) await CreateUser(aviaskUser);

        var response = await HttpClient.GetAsync("api/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Index_Returns_Page_1()
    {
        await Authenticate();

        var users = _userFactory.GetMany(30);

        foreach (var aviaskUser in users) await CreateUser(aviaskUser);

        var response = await HttpClient.GetAsync("api/users");
        var result = await response.Content.ReadAsAsync<FilteredResult<AviaskUserExtended>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(25, result.Elements.Count());

        var expectedUserIds = users.Select(u => u.Id).Take(25);
        var resultUserIds = result.Elements.Select(u => u.Id);

        Assert.Equivalent(expectedUserIds, resultUserIds);
    }

    [Fact]
    public async Task Index_Returns_Page_2()
    {
        await Authenticate();

        var users = _userFactory.GetMany(30);

        foreach (var aviaskUser in users) await CreateUser(aviaskUser);

        var response = await HttpClient.GetAsync("api/users?page=2");
        var result = await response.Content.ReadAsAsync<FilteredResult<AviaskUserExtended>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(6, result.Elements.Count()); // 5 + connected user via Authenticate

        var expectedUserIds = users.Select(u => u.Id).Skip(25);
        var resultUserIds = result.Elements.Select(u => u.Id);

        Assert.Equivalent(expectedUserIds, resultUserIds);
    }

    [Fact]
    public async Task Show_ReturnsNotFound()
    {
        var response = await HttpClient.GetAsync($"api/user/{new Guid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Show_ReturnsUser()
    {
        var user = _userFactory.GetOne();
        await CreateUser(user);

        var response = await HttpClient.GetAsync($"api/user/{user.Id}");
        var result = await response.Content.ReadAsAsync<UserProfileResult>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(user.Id, result.UserDetails.Id);
    }

    [Fact]
    public async Task Show_NoTarget_ReturnsNotFound()
    {
        var response = await HttpClient.GetAsync("api/user");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ShowExtended_ReturnsNotFound()
    {
        await Authenticate();

        var response = await HttpClient.GetAsync($"api/user/extended/{new Guid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ShowExtended_ReturnsUser()
    {
        await Authenticate();

        var user = _userFactory.GetOne();
        await CreateUser(user);

        var response = await HttpClient.GetAsync($"api/user/extended/{user.Id}");
        var result = await response.Content.ReadAsAsync<AviaskUserExtended>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    public async Task Create_InvalidData()
    {
        var createUserModel = new CreateUserModel("abc", "mail@mail.com", "pwd");

        var response = await HttpClient.PostAsJsonAsync("api/users", createUserModel);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadAsAsync<ApiErrorResponse>();

        Assert.NotNull(result?.Message);
        Assert.NotEmpty(result.Message);
        Assert.Equal(0, await DbContext.Users.CountAsync());
    }

    [Fact]
    public async Task Create_ReturnsAlreadyExistsUsername()
    {
        var user = _userFactory.GetOne();
        await CreateUser(user);

        var createUserModel = new CreateUserModel("abc", user.Email!, "pwd");

        var response = await HttpClient.PostAsJsonAsync("api/users", createUserModel);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadAsAsync<ApiErrorResponse>();

        Assert.NotNull(result?.Message);
        Assert.NotEmpty(result.Message);
        Assert.Equal(1, await DbContext.Users.CountAsync());
    }

    [Fact]
    public async Task Create_ReturnsAlreadyExistsEmail()
    {
        var user = _userFactory.GetOne();
        await CreateUser(user);

        var createUserModel = new CreateUserModel(user.UserName!, "email@example.com", "pwd");

        var response = await HttpClient.PostAsJsonAsync("api/users", createUserModel);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadAsAsync<ApiErrorResponse>();

        Assert.NotNull(result?.Message);
        Assert.NotEmpty(result.Message);
        Assert.Equal(1, await DbContext.Users.CountAsync());
    }

    [Fact]
    public async Task New_CreatesUser()
    {
        var user = _userFactory.GetOne();
        await CreateUser(user);

        var createUserModel = new CreateUserModel(user.UserName, user.Email, "testing");

        var response = await HttpClient.PostAsJsonAsync("api/users", createUserModel);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsAsync<AuthenticationResult>();
        Assert.NotNull(result?.Token);
        Assert.NotEmpty(result.Token);
        Assert.NotNull(await DbContext.Users.Where(u => u.Id == user.Id).FirstOrDefaultAsync());
    }

    [Fact]
    public async Task Update_ReturnsUnauthorized()
    {
        var response = await HttpClient.GetAsync($"api/user/{new Guid()}/update");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsNotFound()
    {
        await Authenticate();

        var response = await HttpClient.GetAsync($"api/user/{new Guid()}/update");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsEditUserModel()
    {
        var user = _userFactory.GetOne();
        await CreateUser(user);

        await Authenticate();

        var response = await HttpClient.GetAsync($"api/user/{user.Id}/update");
        var result = await response.Content.ReadAsAsync<EditUserModel>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(user.Email, result.Email);
    }

    [Fact]
    public async Task Edit_ReturnsUnauthorized()
    {
        var response = await HttpClient.PutAsJsonAsync($"api/user/{new Guid()}", new { });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Edit_ReturnsUserNotFound()
    {
        await Authenticate();

        var response = await HttpClient.PutAsJsonAsync($"api/user/{new Guid()}",
            new EditUserModel("user", "user@email.com", "member", false));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var result = await response.Content.ReadAsAsync<ApiErrorResponse>();

        Assert.NotNull(result?.Message);
        Assert.NotEmpty(result.Message);
    }

    [Fact]
    public async Task Edit_InvalidData()
    {
        await Authenticate();

        var user = _userFactory.GetOne();
        await CreateUser(user);

        var editModel = new EditUserModel("", user.Email, user.Role, user.IsPremium);

        var response = await HttpClient.PutAsJsonAsync($"api/user/{user.Id}", editModel);
        var result = await response.Content.ReadAsAsync<ApiErrorResponse>();

        Assert.NotNull(result?.Message);
        Assert.NotEmpty(result.Message);

        var resultUser = await DbContext.Users.FirstAsync(u => u.Id == user.Id);

        Assert.Equal(user.UserName, resultUser.UserName);
    }

    [Fact]
    public async Task Edit_UsernameAlreadyExists()
    {
        await Authenticate();

        var persistedUser = _userFactory.GetOne();
        await CreateUser(persistedUser);

        var user = _userFactory.GetOne();
        await CreateUser(user);

        var editModel = new EditUserModel(persistedUser.UserName, user.Email, user.Role, user.IsPremium);

        var response = await HttpClient.PutAsJsonAsync($"api/user/{user.Id}", editModel);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadAsAsync<ApiErrorResponse>();

        Assert.NotNull(result?.Message);
        Assert.NotEmpty(result.Message);

        var resultUser = await DbContext.Users.FirstAsync(u => u.Id == user.Id);

        Assert.Equal(user.UserName, resultUser.UserName);
    }

    [Fact]
    public async Task Edit_EmailAlreadyExists()
    {
        await Authenticate();

        var persistedUser = _userFactory.GetOne();
        await CreateUser(persistedUser);

        var user = _userFactory.GetOne();
        await CreateUser(user);

        var editModel = new EditUserModel(user.UserName, persistedUser.Email, user.Role, user.IsPremium);

        var response = await HttpClient.PutAsJsonAsync($"api/user/{user.Id}", editModel);
        var result = await response.Content.ReadAsAsync<ApiErrorResponse>();

        Assert.NotNull(result?.Message);
        Assert.NotEmpty(result.Message);

        var resultUser = await DbContext.Users.FirstAsync(u => u.Id == user.Id);
        Assert.Equal(user.UserName, resultUser.UserName);
    }

    [Fact]
    public async Task Edit_UpdatesUser()
    {
        await Authenticate();

        var user = _userFactory.GetOne();
        await CreateUser(user);

        var editModel = new EditUserModel("newUsername123", user.Email, user.Role!, user.IsPremium);

        var response = await HttpClient.PutAsJsonAsync($"api/user/{user.Id}", editModel);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Destroy_ReturnsUnauthorized()
    {
        var response = await HttpClient.DeleteAsync($"api/user/{new Guid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Destroy_ReturnsNotFound()
    {
        await Authenticate();

        var response = await HttpClient.DeleteAsync($"api/user/{new Guid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Destroy_DeletesUser()
    {
        await Authenticate();

        var user = _userFactory.GetOne();
        await CreateUser(user);

        var response = await HttpClient.DeleteAsync($"api/user/{user.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single(await DbContext.Users.ToArrayAsync());
        Assert.DoesNotContain(user.Id, await DbContext.Users.Select(u => u.Id).ToArrayAsync());
    }

    [Fact]
    public async Task Statistics_ReturnsUnauthorized()
    {
        var response = await HttpClient.GetAsync("api/user/statistics");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Statistics_Member_Target_ReturnsNotFound()
    {
        await Authenticate("member");

        var user = _userFactory.GetOne();
        await CreateUser(user);

        var response = await HttpClient.GetAsync($"api/user/statistics?id={user.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Statistics_Target_ReturnsNotFound()
    {
        await Authenticate();

        var response = await HttpClient.GetAsync($"api/user/statistics?id={new Guid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Statistics_Target_ReturnsStatistics()
    {
        await Authenticate();

        var user = _userFactory.GetOne();
        await CreateUser(user);

        var response = await HttpClient.GetAsync($"api/user/statistics?id={user.Id}");
        var result = await response.Content.ReadAsAsync<UserStatistics>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Statistics_Self_ReturnsStatistics()
    {
        await Authenticate();

        var response = await HttpClient.GetAsync("api/user/statistics");
        var result = await response.Content.ReadAsAsync<UserStatistics>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Publications_ReturnsUnauthorized()
    {
        var response = await HttpClient.GetAsync("api/user/publications");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Publications_ReturnsPublications()
    {
        var authenticatedUser = await Authenticate();

        var otherUser = _userFactory.GetOne();
        await CreateUser(otherUser);

        var questions = _questionFactory.GetMany(10).ToList();
        questions[..7].ForEach(q => q.Publisher = authenticatedUser);
        questions[7..].ForEach(q => q.Publisher = otherUser);

        foreach (var question in questions) await CreateQuestion(question);

        var response = await HttpClient.GetAsync("api/user/publications");
        var result = await response.Content.ReadAsAsync<FilteredResult<QuestionDetails>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.Equal(7, result.Elements.Count());
        Assert.Equal(7, result.TotalCount);
        Assert.All(result.Elements, q => Assert.Equal(authenticatedUser.Id, q.PublisherId));
    }

    [Fact]
    public async Task CurrentLeaderboard_Unauthenticated_ReturnsLeaderboard()
    {
        var firstUser = _userFactory.GetOne();
        var firstUserQuestions = _questionFactory.GetMany(5).ToList();

        firstUserQuestions.ForEach(q => { q.Publisher = firstUser; });

        var secondUser = _userFactory.GetOne();
        var secondUserQuestions = _questionFactory.GetMany(3).ToList();

        secondUserQuestions.ForEach(q => { q.Publisher = secondUser; });

        var thirdUser = _userFactory.GetOne();
        var thirdUserQuestions = _questionFactory.GetMany(2).ToList();

        thirdUserQuestions.ForEach(q => { q.Publisher = thirdUser; });

        foreach (var aviaskUser in new[] { secondUser, firstUser, thirdUser })
            await CreateUser(aviaskUser);

        await CreateQuestions([..firstUserQuestions, ..secondUserQuestions, ..thirdUserQuestions]);

        var response = await HttpClient.GetAsync("api/users/leaderboard");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsAsync<CurrentLeaderboard>();

        Assert.NotNull(result);

        Assert.Equal(0, result.CurrentUserCount);
        Assert.Equal(firstUser.UserName, result.Users[0].User.UserName);
        Assert.Equal(secondUser.UserName, result.Users[1].User.UserName);
        Assert.Equal(thirdUser.UserName, result.Users[2].User.UserName);
    }

    [Fact]
    public async Task CurrentLeaderboard_Authenticated_ReturnsLeaderboard_AndCurrentUser()
    {
        var firstUser = _userFactory.GetOne();
        var firstUserQuestions = _questionFactory.GetMany(5).ToList();

        firstUserQuestions.ForEach(q => { q.Publisher = firstUser; });

        var secondUser = await Authenticate("member");
        var secondUserQuestions = _questionFactory.GetMany(3).ToList();

        secondUserQuestions.ForEach(q => { q.Publisher = secondUser; });

        var thirdUser = _userFactory.GetOne();
        var thirdUserQuestions = _questionFactory.GetMany(2).ToList();
        thirdUserQuestions[0].Status = QuestionStatus.PENDING;

        thirdUserQuestions.ForEach(q => { q.Publisher = thirdUser; });

        foreach (var aviaskUser in new[] { secondUser, firstUser, thirdUser })
            await CreateUser(aviaskUser);

        await CreateQuestions([..firstUserQuestions, ..secondUserQuestions, ..thirdUserQuestions]);

        var response = await HttpClient.GetAsync("api/users/leaderboard");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsAsync<CurrentLeaderboard>();

        Assert.NotNull(result);

        Assert.Equal(secondUser.Publications.Count, result.CurrentUserCount); // Current user

        Assert.Equal(5, result.Users[0].QuestionsCount);
        Assert.Equal(firstUser.UserName, result.Users[0].User.UserName);

        Assert.Equal(3, result.Users[1].QuestionsCount);
        Assert.Equal(secondUser.UserName, result.Users[1].User.UserName);

        Assert.Equal(thirdUser.UserName, result.Users[2].User.UserName);
        //  Third user has 2 publications, 1 of which is pending and not accepted
        Assert.Equal(1, result.Users[2].QuestionsCount);
    }
}
