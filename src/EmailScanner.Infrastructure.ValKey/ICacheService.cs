namespace EmailScanner.Infrastructure.ValKey;

public interface ICacheService
{
    Task<string?> GetStringAsync(string key, CancellationToken cancellationToken = default);
    Task SetStringAsync(string key, string value, TimeSpan ttl, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
