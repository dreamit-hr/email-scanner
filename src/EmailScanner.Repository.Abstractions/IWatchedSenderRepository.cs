using EmailScanner.Domain;

namespace EmailScanner.Repository.Abstractions;

public interface IWatchedSenderRepository
{
    Task<WatchedSender?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WatchedSender>> ListForTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WatchedSender>> GetEnabledForTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    void Add(WatchedSender sender);
    void Remove(WatchedSender sender);
}
