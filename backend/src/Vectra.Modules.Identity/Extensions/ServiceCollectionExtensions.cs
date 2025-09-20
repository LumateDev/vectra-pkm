using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vectra.Modules.Identity.Application.Services;
using Vectra.Modules.Identity.Domain.Repositories;
using Vectra.Modules.Identity.Infrastructure.Persistence;
using Vectra.Modules.Identity.Infrastructure.Repositories;
using Vectra.Modules.Identity.Infrastructure.Security;
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


            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();

            // Security services
            services.AddSingleton<IPasswordHasher, PasswordHasher>();

            return services;
        }
    }
}
