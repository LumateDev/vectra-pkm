using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using Vectra.API.Extensions;
using Vectra.API.Services;
using Vectra.Modules.Identity.Extensions;
using Vectra.Modules.Identity.Middleware;
using Vectra.Shared.Configuration;

namespace Vectra.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // FAST MODE WORKAROUND FOR VISUAL STUDIO DOCKER
            // In normal Docker mode: files are copied to /app (as per Dockerfile)
            // In VS Fast Mode: bin/Debug/net9.0 is mounted to /app but working directory changes
            // This detects Fast Mode and adjusts config path accordingly
            var fastModePath = Path.Combine(Directory.GetCurrentDirectory(), "bin", "Debug", "net9.0");
            if (Directory.Exists(fastModePath))
            {
                builder.Configuration.SetBasePath(fastModePath);
                builder.Configuration
                    .AddJsonFile("appsettings.API.json", optional: false)
                    .AddJsonFile($"appsettings.API.{builder.Environment.EnvironmentName}.json", optional: true)
                    .AddJsonFile("appsettings.Secrets.json", optional: true);
            }

            builder.Configuration.AddEnvironmentVariables();

            builder.Services.AddVectraConfiguration(builder.Configuration);
            builder.Services.AddDataProtection()
                .SetApplicationName("Vectra");
            var appSettings = builder.Configuration.GetSection(AppSettings.SectionName).Get<AppSettings>();

            builder.Services.AddIdentityInfrastructure(builder.Configuration);

            Console.WriteLine($"Environment: {appSettings?.Environment}");
            Console.WriteLine($"Database Host: {appSettings?.Database?.Host}");
            Console.WriteLine($"JWT Issuer: {appSettings?.Jwt?.Issuer}");
            Console.WriteLine($"CORS Origins: {string.Join(", ", appSettings?.Cors?.AllowedOrigins ?? Array.Empty<string>())}");
            Console.WriteLine($"Connection String: {appSettings?.Database?.GetConnectionString()}");

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Vectra PKM API",
                    Version = "v1",
                    Description = "Personal Knowledge Management System API"
                });

                // JWT Security
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                // XML comments
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            });

            // JWT Authentication
            var jwtSettings = new JwtSettings();
            builder.Configuration.GetSection($"{AppSettings.SectionName}:Jwt").Bind(jwtSettings);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = jwtSettings.ValidateIssuer,
                    ValidateAudience = jwtSettings.ValidateAudience,
                    ValidateLifetime = jwtSettings.ValidateLifetime,
                    ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            builder.Services.AddAuthorization();
            builder.Services.AddApiRateLimiting();

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowVueFrontend", policy =>
                {
                    policy.WithOrigins(appSettings?.Cors?.AllowedOrigins ?? Array.Empty<string>())
                          .WithMethods(appSettings?.Cors?.AllowedMethods ?? new[] { "GET", "POST", "PUT", "DELETE" })
                          .WithHeaders(appSettings?.Cors?.AllowedHeaders ?? new[] { "Content-Type", "Authorization" })
                          .AllowCredentials(); // Добавил для JWT cookies
                });
            });

            builder.Services.AddHostedService<DatabaseMigrationService>();

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

            app.UseRouting();
            app.UseCors("AllowVueFrontend");
            app.UseIpRateLimiting();
            app.UseAuthentication();
            app.UseMiddleware<JwtMiddleware>();
            app.UseAuthorization();

            app.MapControllers();   
            app.Run();
        }
    }
}