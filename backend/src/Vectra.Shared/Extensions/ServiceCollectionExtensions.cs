using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vectra.Shared.Configuration;

namespace Microsoft.Extensions.DependencyInjection // ✅ Правильное пространство имен
{
    public static class ServiceCollectionExtensions // ✅ Static класс
    {
        public static IServiceCollection AddVectraConfiguration( // ✅ Static метод
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