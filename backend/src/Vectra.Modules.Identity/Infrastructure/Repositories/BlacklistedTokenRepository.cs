using Microsoft.EntityFrameworkCore;
using Vectra.Modules.Identity.Domain.Entities;
using Vectra.Modules.Identity.Domain.Repositories;
using Vectra.Modules.Identity.Infrastructure.Persistence;

namespace Vectra.Modules.Identity.Infrastructure.Repositories
{
    public class BlacklistedTokenRepository : IBlacklistedTokenRepository
    {
        private readonly IdentityDbContext _context;

        public BlacklistedTokenRepository(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsBlacklistedAsync(string jti, CancellationToken cancellationToken = default)
        {
            return await _context.BlacklistedTokens
                .AnyAsync(t => t.Jti == jti, cancellationToken);
        }

        public async Task AddAsync(BlacklistedToken token, CancellationToken cancellationToken = default)
        {
            await _context.BlacklistedTokens.AddAsync(token, cancellationToken);
        }

        public async Task<List<BlacklistedToken>> GetExpiredTokensAsync(CancellationToken cancellationToken = default)
        {
            return await _context.BlacklistedTokens
                .Where(t => t.ExpiresAt < DateTime.UtcNow)
                .ToListAsync(cancellationToken);
        }

        public void RemoveRange(IEnumerable<BlacklistedToken> tokens)
        {
            _context.BlacklistedTokens.RemoveRange(tokens);
        }
    }
}
