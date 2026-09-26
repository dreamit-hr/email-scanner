namespace EmailScanner.Infrastructure.ValKey;

public interface IDistributedLockService
{
    Task<IAsyncDisposable?> TryAcquireAsync(string key, TimeSpan lease, CancellationToken cancellationToken = default);
}
