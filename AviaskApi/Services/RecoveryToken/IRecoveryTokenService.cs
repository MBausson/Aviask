namespace AviaskApi.Services.RecoveryToken;

public interface IRecoveryTokenService
{
    public Task<Guid> CreateRecoveryToken(Guid userId);

    public Task CleanUserTokens(Guid userId);

    public Task<Entities.RecoveryToken?> GetValidToken(string tokenId);

    public Task DeleteRecoveryToken(Entities.RecoveryToken token);
}