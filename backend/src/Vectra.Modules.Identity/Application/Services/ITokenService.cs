using System.Security.Claims;
using Vectra.Modules.Identity.Domain.Entities;

namespace Vectra.Modules.Identity.Application.Services
{
    public interface ITokenService
    {
        Task<string> GenerateAccessTokenAsync(User user);
        Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId, string? ipAddress = null);
        Task<(string AccessToken, RefreshToken RefreshToken)> RefreshTokenAsync(string refreshToken, string? ipAddress = null);
        Task RevokeTokenAsync(string refreshToken, string? ipAddress = null);
        Task RevokeAllTokensForUserAsync(Guid userId, string? ipAddress = null);
        ClaimsPrincipal? ValidateToken(string token);
        Task BlacklistTokenAsync(string jti, DateTime expiresAt, Guid userId, string? reason = null);
    }
}
