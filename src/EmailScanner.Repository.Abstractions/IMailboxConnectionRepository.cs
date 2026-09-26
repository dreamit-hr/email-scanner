using EmailScanner.Domain;

namespace EmailScanner.Repository.Abstractions;

public interface IMailboxConnectionRepository : IRepository<MailboxConnection>
{
    Task<IReadOnlyList<MailboxConnection>> GetEnabledAsync(CancellationToken cancellationToken = default);
}
