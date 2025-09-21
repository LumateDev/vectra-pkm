using Vectra.Modules.Identity.Domain.Entities;

namespace Vectra.Modules.Identity.Domain.Repositories
{
    public interface IBlacklistedTokenRepository
    {
        Task<bool> IsBlacklistedAsync(string jti, CancellationToken cancellationToken = default);
        Task AddAsync(BlacklistedToken token, CancellationToken cancellationToken = default);
        Task<List<BlacklistedToken>> GetExpiredTokensAsync(CancellationToken cancellationToken = default);
        void RemoveRange(IEnumerable<BlacklistedToken> tokens);
    }
}
