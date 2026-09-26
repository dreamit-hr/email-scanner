using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace EmailScanner.Api.Health;

public sealed class ValKeyHealthCheck(IConnectionMultiplexer connection) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var latency = await connection.GetDatabase().PingAsync();
            return HealthCheckResult.Healthy($"ValKey responded in {latency.TotalMilliseconds:F0} ms.");
        }
        catch (Exception exception) { return HealthCheckResult.Unhealthy("ValKey is unavailable.", exception); }
    }
}
