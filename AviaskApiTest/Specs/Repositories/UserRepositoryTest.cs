using AviaskApi.Entities;
using AviaskApiTest.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace AviaskApiTest.Specs.Repositories;

public class UserRepositoryTest : FunctionalTest
{
    private readonly Mock<UserManager<AviaskUser>> _mockUserManager;
    private readonly QuestionFactory _questionFactory = new();
    private readonly UserRepository _repository;
    private readonly UserFactory _userFactory = new();

    public UserRepositoryTest(FunctionalTestWebApplicationFactory factory, ITestOutputHelper output) : base(factory,
        output)
    {
        _mockUserManager = new Mock<UserManager<AviaskUser>>(Mock.Of<IUserStore<AviaskUser>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);

        _repository = new UserRepository(DbContext, _mockUserManager.Object);
    }

    [Fact]
    public async Task GetQuery()
    {
        var users = _userFactory.GetMany(5);

        foreach (var aviaskUser in users) await DbContext.AviaskUsers.AddAsync(aviaskUser);

        await DbContext.SaveChangesAsync();

        var result = _repository.GetQuery();

        Assert.Equivalent(users, result);
    }

    [Fact]
    public async Task CreateAsync_ThrowsNotImplementedException()
    {
        await Assert.ThrowsAsync<NotImplementedException>(async () =>
        {
            await _repository.CreateAsync(new AviaskUser());
        });
    }

    [Fact]
    public async Task ExistsByIdAsync_Exists()
    {
        var user = _userFactory.GetOne();

        await DbContext.AviaskUsers.AddAsync(user);
        await DbContext.SaveChangesAsync();

        _mockUserManager.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);

        Assert.True(await _repository.ExistsByIdAsync(user.Id));
    }

    [Fact]
    public async Task ExistsByIdAsync_NotExists()
    {
        Assert.False(await _repository.ExistsByIdAsync(new Guid()));
    }

    [Fact]
    public async Task GetAllAsync()
    {
        var persistedUsers = _userFactory.GetMany(5).ToArray();

        await DbContext.AviaskUsers.ExecuteDeleteAsync();
        await DbContext.AviaskUsers.AddRangeAsync(persistedUsers);
        await DbContext.SaveChangesAsync();

        var result = await _repository.GetAllAsync();

        Assert.Equal(persistedUsers.Length, result.Length);
        Assert.Equivalent(persistedUsers.Select(u => u.Id), result.Select(u => u.Id));
    }

    [Fact]
    public async Task GetByIdAsync_NotExists()
    {
        _mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((AviaskUser?)null);

        var result = await _repository.GetByIdAsync(new Guid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_Exists()
    {
        var persistedUser = _userFactory.GetOne();

        await DbContext.AviaskUsers.AddAsync(persistedUser);
        await DbContext.SaveChangesAsync();

        _mockUserManager.Setup(m => m.FindByIdAsync(persistedUser.Id.ToString())).ReturnsAsync(persistedUser);
        _mockUserManager.Setup(m => m.GetRolesAsync(persistedUser))
            .ReturnsAsync(new List<string>
                { persistedUser.Role! });

        var result = await _repository.GetByIdAsync(persistedUser.Id);

        Assert.NotNull(result);
        Assert.Equal(persistedUser, result);
    }

    [Fact]
    public async Task ExistsByEmailAsync_NotExists()
    {
        _mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((AviaskUser?)null);

        Assert.False(await _repository.ExistsByEmailAsync("email@example.com"));
    }

    [Fact]
    public async Task ExistsByEmailAsync_Exists()
    {
        var persistedUser = _userFactory.GetOne();

        _mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(persistedUser);

        Assert.True(await _repository.ExistsByEmailAsync(persistedUser.Email!));
    }

    [Fact]
    public async Task ExistsByUsernameAsync_NotExists()
    {
        _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((AviaskUser?)null);

        Assert.False(await _repository.ExistsByUsernameAsync("username"));
    }

    [Fact]
    public async Task ExistsByUsernameAsync_Exists()
    {
        var persistedUser = _userFactory.GetOne();

        _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(persistedUser);

        Assert.True(await _repository.ExistsByUsernameAsync(persistedUser.Email!));
    }

    [Fact]
    public async Task GetByUsernameAsync_NotExists()
    {
        _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((AviaskUser?)null);

        Assert.Null(await _repository.GetByUsernameAsync("username"));
    }

    [Fact]
    public async Task GetByUsernameAsync_Exists()
    {
        var persistedUser = _userFactory.GetOne();

        await DbContext.AviaskUsers.AddAsync(persistedUser);
        await DbContext.SaveChangesAsync();

        var result = await _repository.GetByUsernameAsync(persistedUser.UserName!);

        Assert.NotNull(result);
        Assert.Equal(persistedUser.Id, result.Id);
    }

    [Fact]
    public async Task GetByEmailAsync_NotExists()
    {
        Assert.Null(await _repository.GetByEmailAsync("email@example.com"));
    }

    [Fact]
    public async Task GetByEmailAsync_Exists()
    {
        var persistedUser = _userFactory.GetOne();

        _mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(persistedUser);

        var result = await _repository.GetByEmailAsync(persistedUser.Email!);

        Assert.NotNull(result);
        Assert.Equal(persistedUser, result);
    }

    [Fact]
    public async Task GetMostPublicationUsers()
    {
        var firstUser = _userFactory.GetOne();
        var firstUserQuestions = _questionFactory.GetMany(5).ToList();

        firstUserQuestions.ForEach(q => { q.Publisher = firstUser; });

        var secondUser = _userFactory.GetOne();
        var secondUserQuestions = _questionFactory.GetMany(3).ToList();

        secondUserQuestions.ForEach(q => { q.Publisher = secondUser; });

        var thirdUser = _userFactory.GetOne();
        var thirdUserQuestions = _questionFactory.GetMany(2).ToList();
        thirdUserQuestions[0].Status = QuestionStatus.PENDING;

        thirdUserQuestions.ForEach(q => { q.Publisher = thirdUser; });

        foreach (var aviaskUser in new[] { secondUser, firstUser, thirdUser })
            await CreateUser(aviaskUser);

        await CreateQuestions([..firstUserQuestions, ..secondUserQuestions, ..thirdUserQuestions]);

        var result = await _repository.GetMostPublicationUsers(20);

        Assert.Equal(firstUser.Email, result[0].Email);
        Assert.Equal(secondUser.Email, result[1].Email);
        Assert.Equal(thirdUser.Email, result[2].Email);
    }
}
