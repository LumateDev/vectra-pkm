using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Vectra.Modules.Identity.Application.Services;

namespace Vectra.Modules.Identity.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;

        public JwtMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"]
                .FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                await AttachUserToContext(context, token);
            }

            await _next(context);
        }

        private async Task AttachUserToContext(HttpContext context, string token)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

                var principal = tokenService.ValidateToken(token);
                if (principal != null)
                {
                    context.User = principal;

                    // Опционально: можно добавить пользователя в Items для быстрого доступа
                    var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (Guid.TryParse(userId, out var userGuid))
                    {
                        context.Items["UserId"] = userGuid;
                    }
                }
            }
            catch
            {
                // Token validation failed
                // Do nothing - user is not attached to context
            }
        }
    }
}