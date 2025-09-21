using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vectra.Modules.Identity.Domain.Entities;

namespace Vectra.Modules.Identity.Domain.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
        Task<RefreshToken?> GetByTokenWithUserAsync(string token, CancellationToken cancellationToken = default);
        Task<int> CountActiveTokensAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<RefreshToken?> GetOldestActiveTokenAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<List<RefreshToken>> GetActiveTokensByUserAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<List<RefreshToken>> GetExpiredTokensAsync(CancellationToken cancellationToken = default);
        Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
        void Remove(RefreshToken refreshToken);
        void RemoveRange(IEnumerable<RefreshToken> refreshTokens);
    }
}
