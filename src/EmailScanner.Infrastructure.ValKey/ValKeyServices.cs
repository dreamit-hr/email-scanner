using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace EmailScanner.Infrastructure.ValKey;

public sealed class CacheService(IConnectionMultiplexer connection, IOptions<ValKeyOptions> options) : ICacheService
{
    private readonly IDatabase _database = connection.GetDatabase();
    private string Key(string key) => options.Value.InstanceName + key;
    public async Task<string?> GetStringAsync(string key, CancellationToken cancellationToken = default) => await _database.StringGetAsync(Key(key));
    public async Task SetStringAsync(string key, string value, TimeSpan ttl, CancellationToken cancellationToken = default) => await _database.StringSetAsync(Key(key), value, ttl);
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default) => await _database.KeyDeleteAsync(Key(key));
}

public sealed class DistributedLockService(IConnectionMultiplexer connection, IOptions<ValKeyOptions> options) : IDistributedLockService
{
    private readonly IDatabase _database = connection.GetDatabase();
    public async Task<IAsyncDisposable?> TryAcquireAsync(string key, TimeSpan lease, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        if (lease <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(lease));
        var lockKey = options.Value.InstanceName + key;
        var token = Guid.NewGuid().ToString("N");
        var acquired = await _database.StringSetAsync(lockKey, token, lease, When.NotExists);
        return acquired ? new RedisLease(_database, lockKey, token) : null;
    }

    private sealed class RedisLease(IDatabase database, string key, string token) : IAsyncDisposable
    {
        private const string ReleaseScript = "if redis.call('get', KEYS[1]) == ARGV[1] then return redis.call('del', KEYS[1]) else return 0 end";
        public async ValueTask DisposeAsync() => await database.ScriptEvaluateAsync(ReleaseScript, [key], [token]);
    }
}
