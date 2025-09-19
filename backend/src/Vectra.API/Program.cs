using Microsoft.OpenApi.Models;
using Vectra.Shared.Configuration;

namespace Vectra.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "..", ".configs");
            builder.Configuration
                .SetBasePath(configPath)
                .AddJsonFile("appsettings.API.json", optional: false)
                .AddJsonFile($"appsettings.API.{builder.Environment.EnvironmentName}.json", optional: true)
                .AddJsonFile("appsettings.Secrets.json", optional: true)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

            builder.Services.AddVectraConfiguration(builder.Configuration);
            var appSettings = builder.Configuration.GetSection(AppSettings.SectionName).Get<AppSettings>();

            // temp settings log
            Console.WriteLine($"Environment: {appSettings?.Environment}");
            Console.WriteLine($"Database Host: {appSettings?.Database?.Host}");
            Console.WriteLine($"JWT Issuer: {appSettings?.Jwt?.Issuer}");
            Console.WriteLine($"CORS Origins: {string.Join(", ", appSettings?.Cors?.AllowedOrigins ?? Array.Empty<string>())}");
            Console.WriteLine($"Connection String: {appSettings?.Database?.GetConnectionString()}");

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
                    Description = "API    "
                });
            });

            //  CORS  
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
                app.UseHttpsRedirection(); //  Redirect  Production
            }

            app.UseCors("AllowVueFrontend");
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}