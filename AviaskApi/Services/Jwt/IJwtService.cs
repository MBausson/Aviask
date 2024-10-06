using System.Security.Claims;
using AviaskApi.Entities;

namespace AviaskApi.Services.Jwt;

public interface IJwtService
{
    /// <summary>
    ///     Returns the user associated with the request
    /// </summary>
    /// <param name="claimsPrincipal">Request claims</param>
    /// <returns>User associated with the JWT claim</returns>
    public Task<AviaskUser?> CurrentUserAuthenticatedAsync(ClaimsPrincipal claimsPrincipal);

    /// <summary>
    ///     Returns the JWT token for the associated user
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public Task<string> CreateTokenAsync(AviaskUser user);

    /// <summary>
    ///     Verifies the validity of the JWT
    /// </summary>
    /// <param name="token">The JWT</param>
    /// <returns>A boolean indicating whether the JWT is valid, and if so, the user's id</returns>
    public Task<(bool, Guid?)> IsTokenValidAsync(string token);
}