using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Vectra.Modules.Identity.Application.Exceptions;
using Vectra.Modules.Identity.Application.Services;
using Vectra.Modules.Identity.Domain.Entities;
using Vectra.Modules.Identity.Domain.Repositories;
using Vectra.Shared.Configuration;

namespace Vectra.Modules.Identity.Infrastructure.Security
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IBlacklistedTokenRepository _blacklistedTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TokenService> _logger;
        private readonly JwtSecurityTokenHandler _tokenHandler;

        private const int MaxActiveTokensPerUser = 5;

        public TokenService(
            IOptions<JwtSettings> jwtSettings,
            IRefreshTokenRepository refreshTokenRepository,
            IUnitOfWork unitOfWork,
            ILogger<TokenService> logger,
            IBlacklistedTokenRepository blacklistedTokenRepository)
        {
            _jwtSettings = jwtSettings.Value;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _tokenHandler = new JwtSecurityTokenHandler();
            _blacklistedTokenRepository = blacklistedTokenRepository;
        }

        public Task<string> GenerateAccessTokenAsync(User user)
        {
            var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            var tokenString = _tokenHandler.WriteToken(token);

            _logger.LogDebug("Generated access token for user {UserId}", user.Id);

            return Task.FromResult(tokenString);
        }

        public async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId, string? ipAddress = null)
        {
            // Проверяем количество активных токенов
            var activeTokensCount = await _refreshTokenRepository.CountActiveTokensAsync(userId);

            if (activeTokensCount >= MaxActiveTokensPerUser)
            {
                // Удаляем самый старый активный токен
                var oldestToken = await _refreshTokenRepository.GetOldestActiveTokenAsync(userId);

                if (oldestToken != null)
                {
                    oldestToken.Revoke(ipAddress, "Token rotation limit exceeded");
                    _logger.LogInformation("Revoked oldest token for user {UserId} due to limit", userId);
                }
            }

            var refreshToken = new RefreshToken(
                token: GenerateRefreshTokenString(),
                userId: userId,
                daysToExpire: _jwtSettings.RefreshTokenExpirationDays,
                createdByIp: ipAddress
            );

            await _refreshTokenRepository.AddAsync(refreshToken);

            _logger.LogDebug("Generated refresh token for user {UserId}", userId);

            return refreshToken;
        }

        public async Task<(string AccessToken, RefreshToken RefreshToken)> RefreshTokenAsync(
            string refreshToken,
            string? ipAddress = null)
        {
            var token = await _refreshTokenRepository.GetByTokenWithUserAsync(refreshToken);

            if (token == null)
            {
                _logger.LogWarning("Refresh token not found: {Token}", refreshToken);
                throw new InvalidTokenException();
            }

            if (!token.IsActive)
            {
                if (token.IsRevoked)
                {
                    // Token reuse detection - revoke all descendant tokens
                    _logger.LogWarning("Token reuse detected for user {UserId}", token.UserId);
                    await RevokeDescendantTokensAsync(token, ipAddress);
                }
                throw new InvalidTokenException();
            }

            // Replace old refresh token with a new one
            var newRefreshToken = await RotateRefreshTokenAsync(token, ipAddress);

            // Generate new access token
            var accessToken = await GenerateAccessTokenAsync(token.User);

            // Save all changes
            await _unitOfWork.SaveChangesAsync();

            return (accessToken, newRefreshToken);
        }

        public async Task RevokeTokenAsync(string refreshToken, string? ipAddress = null)
        {
            var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

            if (token == null || !token.IsActive)
                return;

            token.Revoke(ipAddress);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Revoked refresh token for user {UserId}", token.UserId);
        }

        public async Task RevokeAllTokensForUserAsync(Guid userId, string? ipAddress = null)
        {
            var activeTokens = await _refreshTokenRepository.GetActiveTokensByUserAsync(userId);

            foreach (var token in activeTokens)
            {
                token.Revoke(ipAddress, "All tokens revoked by system");
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Revoked all {Count} tokens for user {UserId}", activeTokens.Count, userId);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = _jwtSettings.ValidateIssuer,
                    ValidateAudience = _jwtSettings.ValidateAudience,
                    ValidateLifetime = _jwtSettings.ValidateLifetime,
                    ValidateIssuerSigningKey = _jwtSettings.ValidateIssuerSigningKey,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidAudience = _jwtSettings.Audience,
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = _tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                if (!IsJwtWithValidSecurityAlgorithm(validatedToken))
                    return null;

                var jti = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                if (!string.IsNullOrEmpty(jti) &&
                    _blacklistedTokenRepository.IsBlacklistedAsync(jti).GetAwaiter().GetResult())
                {
                    _logger.LogDebug("Token {Jti} is blacklisted", jti);
                    return null;
                }

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Token validation failed");
                return null;
            }
        }

        private async Task<RefreshToken> RotateRefreshTokenAsync(RefreshToken refreshToken, string? ipAddress)
        {
            var newRefreshToken = new RefreshToken(
                token: GenerateRefreshTokenString(),
                userId: refreshToken.UserId,
                daysToExpire: _jwtSettings.RefreshTokenExpirationDays,
                createdByIp: ipAddress
            );

            refreshToken.Revoke(ipAddress, newRefreshToken.Token);

            await _refreshTokenRepository.AddAsync(newRefreshToken);

            return newRefreshToken;
        }

        private async Task RevokeDescendantTokensAsync(RefreshToken refreshToken, string? ipAddress)
        {
            if (!string.IsNullOrEmpty(refreshToken.ReplacedByToken))
            {
                var descendantToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken.ReplacedByToken);

                if (descendantToken != null)
                {
                    if (descendantToken.IsActive)
                        descendantToken.Revoke(ipAddress);
                    else
                        await RevokeDescendantTokensAsync(descendantToken, ipAddress);
                }
            }
        }

        private static string GenerateRefreshTokenString()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private static bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
        {
            return validatedToken is JwtSecurityToken jwtSecurityToken &&
                   jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                       StringComparison.InvariantCultureIgnoreCase);
        }

        public async Task BlacklistTokenAsync(string jti, DateTime expiresAt, Guid userId, string? reason = null)
        {
            var blacklistedToken = new BlacklistedToken(jti, expiresAt, userId, reason);
            await _blacklistedTokenRepository.AddAsync(blacklistedToken);
            _logger.LogInformation("Token {Jti} blacklisted for user {UserId}. Reason: {Reason}",
                jti, userId, reason ?? "Not specified");
        }
    }
}