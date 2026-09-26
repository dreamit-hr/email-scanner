using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmailScanner.Infrastructure.BlobStorage;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmailScannerBlobStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<BlobStorageOptions>().Bind(configuration.GetSection(BlobStorageOptions.SectionName)).ValidateOnStart();
        services.AddSingleton<IBlobStorageClient, BlobStorageClient>();
        services.AddSingleton<BlobStoragePathProvider>();
        return services;
    }
}
