
using Microsoft.OpenApi.Models;
using Vectra.Shared.Configuration;

namespace Vectra.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // Определяем путь к конфигурационным файлам
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "config");

            // Загрузка конфигурации
            builder.Configuration
                .SetBasePath(configPath) // Устанавливаем базовый путь к config папке
                .AddJsonFile("appsettings.Shared.json", optional: false)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                .AddJsonFile("appsettings.Secrets.json", optional: true)
                .AddJsonFile("appsettings.Local.json", optional: true)
                .SetBasePath(Directory.GetCurrentDirectory()) // Возвращаем базовый путь обратно
                .AddEnvironmentVariables();


            // Регистрация конфигурации из Shared проекта
            builder.Services.AddVectraConfiguration(builder.Configuration);

            // Получаем настройки для использования
            var appSettings = builder.Configuration.GetSection(AppSettings.SectionName).Get<AppSettings>();

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Vectra API",
                    Version = "v1",
                    Description = "API для системы управления персональными знаниями"
                });
            });

            // Настройка CORS из конфигурации
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowVueFrontend", policy =>
                {
                    policy.WithOrigins(appSettings?.Cors.AllowedOrigins ?? Array.Empty<string>())
                          .WithMethods(appSettings?.Cors.AllowedMethods ?? new[] { "GET", "POST", "PUT", "DELETE" })
                          .WithHeaders(appSettings?.Cors.AllowedHeaders ?? new[] { "Content-Type", "Authorization" });
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Vectra API V1");
                });
            }
            else // Production
            {
                app.UseHttpsRedirection(); // Включаем Redirect только в Production
            }

            app.UseCors("AllowVueFrontend");
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
