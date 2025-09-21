using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Vectra.Modules.Identity.Application.Services;
using Vectra.Modules.Identity.Domain.Repositories;

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

            if (!string.IsNullOrEmpty(token))
                await AttachUserToContext(context, token);

            await _next(context);
        }

        private async Task AttachUserToContext(HttpContext context, string token)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
                var blacklistedRepo = scope.ServiceProvider.GetRequiredService<IBlacklistedTokenRepository>();

                var principal = tokenService.ValidateToken(token);
                if (principal == null) return;

                // blacklist check
                var jti = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                if (!string.IsNullOrEmpty(jti) &&
                    await blacklistedRepo.IsBlacklistedAsync(jti))
                {
                    // token revoke
                    return;
                }
                
                context.User = principal;

                if (Guid.TryParse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userGuid))
                    context.Items["UserId"] = userGuid;
            }
            catch
            {
                // Token validation failed
                // Do nothing - user is not attached to context
            }
        }
    }
}