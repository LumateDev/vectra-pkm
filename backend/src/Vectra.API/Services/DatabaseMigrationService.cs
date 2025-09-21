using Microsoft.EntityFrameworkCore;
using Vectra.Modules.Identity.Infrastructure.Persistence;

namespace Vectra.API.Services
{
    public class DatabaseMigrationService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseMigrationService> _logger;

        public DatabaseMigrationService(
            IServiceProvider serviceProvider,
            ILogger<DatabaseMigrationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting database migration...");

            using var scope = _serviceProvider.CreateScope();

            try
            {
                // Identity module
                var identityContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
                await MigrateContextAsync(identityContext, "Identity", cancellationToken);

                // TODO: Добавить другие контексты когда будут готовы
                // var documentsContext = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();
                // await MigrateContextAsync(documentsContext, "Documents", cancellationToken);

                _logger.LogInformation("Database migration completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database migration failed");
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Development")
                {
                    throw;
                }
            }
        }

        private async Task MigrateContextAsync(DbContext context, string contextName, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Migrating {ContextName} database...", contextName);

            var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
            if (pendingMigrations.Any())
            {
                _logger.LogInformation("Applying {Count} pending migrations for {ContextName}...",
                    pendingMigrations.Count(), contextName);

                await context.Database.MigrateAsync(cancellationToken);

                _logger.LogInformation("{ContextName} database migrated successfully", contextName);
            }
            else
            {
                _logger.LogInformation("No pending migrations for {ContextName}", contextName);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
