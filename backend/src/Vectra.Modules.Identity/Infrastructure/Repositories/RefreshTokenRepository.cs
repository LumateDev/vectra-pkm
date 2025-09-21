using Microsoft.EntityFrameworkCore;
using Vectra.Modules.Identity.Domain.Entities;
using Vectra.Modules.Identity.Domain.Repositories;
using Vectra.Modules.Identity.Infrastructure.Persistence;

namespace Vectra.Modules.Identity.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly IdentityDbContext _context;

        public RefreshTokenRepository(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
        }

        public async Task<RefreshToken?> GetByTokenWithUserAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
        }

        public async Task<int> CountActiveTokensAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            // Вместо IsActive используем базовые свойства
            return await _context.RefreshTokens
                .CountAsync(rt =>
                    rt.UserId == userId &&
                    rt.RevokedAt == null &&
                    rt.ExpiresAt > DateTime.UtcNow,
                    cancellationToken);
        }

        public async Task<RefreshToken?> GetOldestActiveTokenAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.IsActive)
                .OrderBy(rt => rt.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<RefreshToken>> GetActiveTokensByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<RefreshToken>> GetExpiredTokensAsync(CancellationToken cancellationToken = default)
        {
            return await _context.RefreshTokens
                .Where(rt => rt.ExpiresAt < DateTime.UtcNow)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        }

        public void Remove(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Remove(refreshToken);
        }

        public void RemoveRange(IEnumerable<RefreshToken> refreshTokens)
        {
            _context.RefreshTokens.RemoveRange(refreshTokens);
        }
    }
}