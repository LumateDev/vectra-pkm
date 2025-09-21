using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vectra.Modules.Identity.Domain.Repositories;

namespace Vectra.Modules.Identity.Infrastructure.Services
{
    public class TokenCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TokenCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(1);

        public TokenCleanupService(IServiceProvider serviceProvider, ILogger<TokenCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Token Cleanup Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredTokensAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up expired tokens");
                }

                await Task.Delay(_cleanupInterval, stoppingToken);
            }

            _logger.LogInformation("Token Cleanup Service stopped");
        }

        private async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var refreshTokenRepository = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
            var blacklistedTokenRepository = scope.ServiceProvider.GetRequiredService<IBlacklistedTokenRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var expiredRefreshTokens = await refreshTokenRepository.GetExpiredTokensAsync(cancellationToken);
            if (expiredRefreshTokens.Any())
            {
                refreshTokenRepository.RemoveRange(expiredRefreshTokens);
                _logger.LogInformation("Removing {Count} expired refresh tokens", expiredRefreshTokens.Count);
            }

            var expiredBlacklistedTokens = await blacklistedTokenRepository.GetExpiredTokensAsync(cancellationToken);
            if (expiredBlacklistedTokens.Any())
            {
                blacklistedTokenRepository.RemoveRange(expiredBlacklistedTokens);
                _logger.LogInformation("Removing {Count} expired blacklisted tokens", expiredBlacklistedTokens.Count);
            }

            var totalRemoved = await unitOfWork.SaveChangesAsync(cancellationToken);
            if (totalRemoved > 0)
            {
                _logger.LogInformation("Total {Count} expired tokens removed", totalRemoved);
            }
        }
    }
}