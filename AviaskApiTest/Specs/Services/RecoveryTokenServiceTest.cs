using AviaskApi.Entities;
using AviaskApi.Services.RecoveryToken;

namespace AviaskApiTest.Specs.Services;

public class RecoveryTokenServiceTest
{
    private readonly RecoveryTokenFactory _factory = new();
    private readonly Mock<RecoveryTokenRepository> _repositoryMock;
    private readonly IRecoveryTokenService _service;

    public RecoveryTokenServiceTest()
    {
        _repositoryMock = new Mock<RecoveryTokenRepository>(null!);

        _service = new RecoveryTokenService(_repositoryMock.Object);
    }

    [Fact]
    public async Task IsExpired_NotExpired()
    {
        var recoveryToken = _factory.GetOne();
        recoveryToken.ExpiresAt = DateTime.UtcNow + new TimeSpan(0, 30, 0);

        Assert.False(recoveryToken.IsExpired());
    }

    [Fact]
    public async Task IsExpired_Expired()
    {
        var recoveryToken = _factory.GetOne();
        recoveryToken.ExpiresAt = DateTime.UtcNow - new TimeSpan(0, 30, 0);

        Assert.True(recoveryToken.IsExpired());
    }

    [Fact]
    public async Task CreateRecoveryToken_CreatesRecoveryToken()
    {
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<RecoveryToken>())).Verifiable();

        await _service.CreateRecoveryToken(new Guid());

        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<RecoveryToken>()), Times.Once);
    }

    [Fact]
    public async Task CleanUserTokens_RemovesExpiredToken()
    {
        var user = new UserFactory().GetOne();

        var expired = Expired(user);
        var pending = Pending(user);

        _repositoryMock.Setup(m => m.GetByUser(user.Id)).ReturnsAsync([expired, pending]);
        _repositoryMock.Setup(m => m.DeleteAsync(expired)).Verifiable();

        await _service.CleanUserTokens(user.Id);

        _repositoryMock.Verify(m => m.DeleteAsync(expired), Times.Once);
        _repositoryMock.Verify(m => m.DeleteAsync(pending), Times.Never);
    }

    [Fact]
    public async Task IsTokenValid_Invalid_TokenNotExists()
    {
        var result = await _service.GetValidToken(new Guid().ToString());

        Assert.Null(result);
    }

    [Fact]
    public async Task IsTokenValid_Invalid_DifferentUser()
    {
        var user = new UserFactory().GetOne();
        var expired = Expired(user);
        expired.UserId = new Guid();

        _repositoryMock.Setup(m => m.GetByIdAsync(expired.Id)).ReturnsAsync(expired);

        var result = await _service.GetValidToken(expired.Id.ToString());

        Assert.Null(result);
    }

    [Fact]
    public async Task IsTokenValid_Invalid_Expired()
    {
        var user = new UserFactory().GetOne();
        var expired = Expired(user);

        _repositoryMock.Setup(m => m.GetByIdAsync(expired.Id)).ReturnsAsync(expired);

        var result = await _service.GetValidToken(expired.Id.ToString());

        Assert.Null(result);
    }

    [Fact]
    public async Task IsTokenValid_Valid()
    {
        var user = new UserFactory().GetOne();
        var pending = Pending(user);

        _repositoryMock.Setup(m => m.GetByIdAsync(pending.Id)).ReturnsAsync(pending);

        var result = await _service.GetValidToken(pending.Id.ToString());

        Assert.NotNull(result);
    }

    [Fact]
    public async Task DeleteRecoveryToken_Deletes()
    {
        var user = new UserFactory().GetOne();
        var pending = Pending(user);

        _repositoryMock.Setup(m => m.DeleteAsync(pending)).Verifiable();

        await _service.DeleteRecoveryToken(pending);

        _repositoryMock.Verify(m => m.DeleteAsync(pending), Times.Once);
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