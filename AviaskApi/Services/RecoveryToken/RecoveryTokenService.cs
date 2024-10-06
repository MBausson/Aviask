using AviaskApi.Repositories;

namespace AviaskApi.Services.RecoveryToken;

public class RecoveryTokenService : IRecoveryTokenService
{
    private readonly RecoveryTokenRepository _repository;

    public RecoveryTokenService(IAviaskRepository<Entities.RecoveryToken, Guid> repository)
    {
        _repository = (RecoveryTokenRepository)repository;
    }

    public static TimeSpan ExpirationDelay { get; } = new(12, 0, 0);

    public async Task<Guid> CreateRecoveryToken(Guid userId)
    {
        await CleanUserTokens(userId);

        var token = new Entities.RecoveryToken
        {
            Id = new Guid(),
            ExpiresAt = DateTime.UtcNow + ExpirationDelay,
            UserId = userId
        };

        await _repository.CreateAsync(token);

        return token.Id;
    }

    public async Task CleanUserTokens(Guid userId)
    {
        var pendingToken = (await _repository.GetByUser(userId)).OrderBy(r => r.ExpiresAt);
        var expiredTokens = pendingToken.Where(t => t.IsExpired());

        foreach (var recoveryToken in expiredTokens) await _repository.DeleteAsync(recoveryToken);
    }

    public async Task<Entities.RecoveryToken?> GetValidToken(string tokenId)
    {
        if (!Guid.TryParse(tokenId, out var tokenGuid)) return null;

        var token = await _repository.GetByIdAsync(tokenGuid);

        if (token is null) return null;
        if (!token.IsExpired()) return token;

        await CleanUserTokens(token.UserId);
        return null;
    }

    public async Task DeleteRecoveryToken(Entities.RecoveryToken token)
    {
        await _repository.DeleteAsync(token);
    }

    public async Task<bool> TokenExists(Guid tokenId)
    {
        return await _repository.ExistsByIdAsync(tokenId);
    }
}