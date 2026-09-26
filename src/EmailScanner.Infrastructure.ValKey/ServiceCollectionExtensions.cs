using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace EmailScanner.Infrastructure.ValKey;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmailScannerValKey(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ValKeyOptions>().Bind(configuration.GetSection(ValKeyOptions.SectionName));
        services.AddSingleton<IConnectionMultiplexer>(provider => ConnectionMultiplexer.Connect(provider.GetRequiredService<IOptions<ValKeyOptions>>().Value.ConnectionString));
        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<IDistributedLockService, DistributedLockService>();
        return services;
    }
}
