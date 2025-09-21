using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vectra.Modules.Identity.Application.DTOs.Requests;
using Vectra.Modules.Identity.Application.Services;
using Vectra.Modules.Identity.Application.Services.Implementation;
using Vectra.Modules.Identity.Application.Validators;
using Vectra.Modules.Identity.Domain.Repositories;
using Vectra.Modules.Identity.Infrastructure.Persistence;
using Vectra.Modules.Identity.Infrastructure.Repositories;
using Vectra.Modules.Identity.Infrastructure.Security;
using Vectra.Modules.Identity.Infrastructure.Services;
using Vectra.Shared.Configuration;

namespace Vectra.Modules.Identity.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIdentityInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Получаем настройки БД
            var databaseSettings = new DatabaseSettings();
            configuration.GetSection($"{AppSettings.SectionName}:Database").Bind(databaseSettings);

            var connectionString = databaseSettings.GetConnectionString();

            services.AddDbContext<IdentityDbContext>(options =>
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "identity");

                    // Добавляем дополнительные настройки из конфига
                    if (databaseSettings.CommandTimeout > 0)
                        npgsqlOptions.CommandTimeout(databaseSettings.CommandTimeout);
                })
                .EnableSensitiveDataLogging(databaseSettings.EnableSensitiveDataLogging)
                .EnableDetailedErrors(databaseSettings.EnableDetailedErrors));

           
            // Validators
            services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
            services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();

            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Services
            services.AddScoped<IAuthService, AuthService>();

            // Security services
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ITokenService, TokenService>();

            // Background services
            services.AddHostedService<TokenCleanupService>();

            return services;
        }
    }
}
