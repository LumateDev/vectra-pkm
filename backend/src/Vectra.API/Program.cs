
using Microsoft.OpenApi.Models;

namespace Vectra.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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
                    Description = "API дл€ системы управлени€ персональными знани€ми"
                });
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowVueFrontend",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
                              .AllowAnyHeader()
                              .AllowAnyMethod();
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
                app.UseHttpsRedirection(); // ¬ключаем Redirect только в Production
            }

            app.UseCors("AllowVueFrontend");
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
