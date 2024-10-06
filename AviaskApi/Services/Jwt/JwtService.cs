using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AviaskApi.Configuration;
using AviaskApi.Entities;
using AviaskApi.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace AviaskApi.Services.Jwt;

public class JwtService : IJwtService
{
    private const string UserIdClaim = "userId";

    private static readonly TimeSpan AuthenticationExpirationDuration = new(0, 0, 45, 0);
    private readonly UserManager<AviaskUser> _userManager;
    private readonly UserRepository _userRepository;

    public JwtService(UserManager<AviaskUser> userManager,
        IAviaskRepository<AviaskUser, Guid> userRepository)
    {
        _userManager = userManager;
        _userRepository = (UserRepository)userRepository;
    }

    public virtual async Task<AviaskUser?> CurrentUserAuthenticatedAsync(ClaimsPrincipal claimsPrincipal)
    {
        if (claimsPrincipal.Identity is null || !claimsPrincipal.Identity.IsAuthenticated) return null;

        var userId = claimsPrincipal.FindFirstValue(UserIdClaim)!;

        return (await _userRepository.GetByIdAsync(new Guid(userId)))!;
    }

    public virtual async Task<string> CreateTokenAsync(AviaskUser user)
    {
        var expiration = DateTime.Now + AuthenticationExpirationDuration;

        var token = await GenerateToken(user, expiration);
        var tokenHandler = new JwtSecurityTokenHandler();

        return tokenHandler.WriteToken(token);
    }

    public virtual async Task<(bool, Guid?)> IsTokenValidAsync(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(Env.Get("JWT_KEY"));

        var result = await tokenHandler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            ClockSkew = TimeSpan.Zero,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = false,
            ValidateActor = true,
            ValidIssuer = Env.Get("JWT_ISSUER"),
            ValidAudience = Env.Get("JWT_AUDIENCE"),
            IssuerSigningKey = new SymmetricSecurityKey(key),
            RequireExpirationTime = true
        });

        var validJwt = result.IsValid && result.Claims.ContainsKey(UserIdClaim);

        return (validJwt, validJwt ? new Guid((string)result.Claims[UserIdClaim]) : null);
    }

    private async Task<JwtSecurityToken> GenerateToken(AviaskUser user, DateTime expiration)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, Env.Get("JWT_SUBJECT")),
            new(JwtRegisteredClaimNames.Iss, Env.Get("JWT_ISSUER")),
            new(JwtRegisteredClaimNames.Aud, Env.Get("JWT_AUDIENCE")),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
            new(UserIdClaim, user.Id.ToString()),
            new(ClaimTypes.Role, (await _userManager.GetRolesAsync(user)).First())
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Env.Get("JWT_KEY"))),
            SecurityAlgorithms.HmacSha256
        );

        return new JwtSecurityToken(
            Env.Get("JWT_ISSUER"),
            Env.Get("JWT_AUDIENCE"),
            claims,
            expires: expiration,
            signingCredentials: credentials
        );
    }
}