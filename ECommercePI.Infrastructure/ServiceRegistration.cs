using System.Text.Json;
using ECommercePI.Application.Interfaces.Repositories;
using ECommercePI.Application.Interfaces.Services;
using ECommercePI.Infrastructure.ExternalServices;
using ECommercePI.Infrastructure.Repositories;
using ECommercePI.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Refit;

namespace ECommercePI.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string? connectionString)
    {
        // 🔁 Retry Policy: 3 kez dene, aralarda 2s bekle
        // Refit + Polly Retry/Timeout with logging
        services.AddRefitClient<IBalanceApi>(new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                })
            })
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri("https://balance-management-pi44.onrender.com");
                c.Timeout = TimeSpan.FromSeconds(90);
            })
            .AddPolicyHandler((serviceProvider, request) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<IBalanceApi>>();
                var retryPolicy = Policy<HttpResponseMessage>
                    .Handle<HttpRequestException>()
                    .OrResult(msg => (int)msg.StatusCode >= 500)
                    .WaitAndRetryAsync(
                        retryCount: 3,
                        sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                        onRetry: (outcome, timespan, retryAttempt, context) =>
                        {
                            var error = outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString();
                            logger.LogWarning(
                                "Retry {RetryAttempt} for {PolicyKey} due to {Reason}. Next try in {Delay}s.",
                                retryAttempt, context.PolicyKey, error, timespan.TotalSeconds);
                        });

                var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(60);

                return Policy.WrapAsync(retryPolicy, timeoutPolicy);
            });

        // 🛢 DbContext
        services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(connectionString));

        // 🧩 Uygulama servisleri
        services.AddScoped<IBalanceService, BalanceService>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        // 🧠 Memory Cache
        services.AddMemoryCache();

        return services;
    }
}