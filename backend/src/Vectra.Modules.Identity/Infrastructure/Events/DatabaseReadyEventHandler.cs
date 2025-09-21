using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vectra.Modules.Identity.Infrastructure.Services;
using Vectra.Shared.Domain.Events;
using Vectra.Shared.Domain.Primitives;

namespace Vectra.Modules.Identity.Infrastructure.Events
{
    public class DatabaseReadyEventHandler : IDomainEventHandler<DatabaseReadyEvent>
    {
        private readonly TokenCleanupService _cleanupService;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly ILogger<DatabaseReadyEventHandler> _logger;

        public DatabaseReadyEventHandler(
            TokenCleanupService cleanupService,
            IHostApplicationLifetime lifetime,
            ILogger<DatabaseReadyEventHandler> logger)
        {
            _cleanupService = cleanupService;
            _lifetime = lifetime;
            _logger = logger;
        }

        public async Task HandleAsync(DatabaseReadyEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Получено событие DatabaseReadyEvent – запускаем TokenCleanupService");
            await _cleanupService.StartManualAsync(_lifetime.ApplicationStopping);
        }
    }
}
