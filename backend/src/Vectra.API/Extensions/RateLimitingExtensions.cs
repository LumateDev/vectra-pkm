using AspNetCoreRateLimit;

namespace Vectra.API.Extensions
{
    public static class RateLimitingExtensions
    {
        public static IServiceCollection AddApiRateLimiting(this IServiceCollection services)
        {

            services.AddMemoryCache();

            // Limiting rules
            services.Configure<IpRateLimitOptions>(options =>
            {
                options.EnableEndpointRateLimiting = true;
                options.StackBlockedRequests = false;
                options.HttpStatusCode = 429;
                options.RealIpHeader = "X-Real-IP";
                options.ClientIdHeader = "X-ClientId";
                options.GeneralRules = new List<RateLimitRule>
                {
                    new RateLimitRule
                    {
                        Endpoint = "POST:/api/v1/auth/login",
                        Period = "1m",
                        Limit = 5
                    },
                    new RateLimitRule
                    {
                        Endpoint = "POST:/api/v1/auth/register",
                        Period = "1h",
                        Limit = 10
                    },
                    new RateLimitRule
                    {
                        Endpoint = "POST:/api/v1/auth/refresh",
                        Period = "1m",
                        Limit = 10
                    },
                    new RateLimitRule
                    {
                        Endpoint = "*",
                        Period = "1m",
                        Limit = 100
                    }
                };

                options.QuotaExceededResponse = new QuotaExceededResponse
                {
                    Content = "{{\"error\":\"Too many requests\",\"message\":\"Rate limit exceeded. Try again later.\"}}",
                    ContentType = "application/json",
                    StatusCode = 429
                };
            });

            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
            services.AddInMemoryRateLimiting();

            return services;
        }
    }
}