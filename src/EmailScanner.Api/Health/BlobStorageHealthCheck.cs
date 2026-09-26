using EmailScanner.Infrastructure.BlobStorage;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EmailScanner.Api.Health;

public sealed class BlobStorageHealthCheck(IBlobStorageClient storage) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await storage.CheckHealthAsync(cancellationToken);
            return HealthCheckResult.Healthy("Blob storage is available.");
        }
        catch (Exception exception) { return HealthCheckResult.Unhealthy("Blob storage is unavailable.", exception); }
    }
}
