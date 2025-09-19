using Microsoft.Extensions.Configuration;
using Vectra.Shared.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddVectraConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Регистрируем главную конфигурацию
            services.Configure<AppSettings>(configuration.GetSection(AppSettings.SectionName));

            // Регистрируем отдельные настройки для удобства инъекции
            services.Configure<DatabaseSettings>(configuration.GetSection($"{AppSettings.SectionName}:Database"));
            services.Configure<JwtSettings>(configuration.GetSection($"{AppSettings.SectionName}:Jwt"));
            services.Configure<CorsSettings>(configuration.GetSection($"{AppSettings.SectionName}:Cors"));

            return services;
        }
    }
}