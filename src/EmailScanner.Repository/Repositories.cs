using System.Linq.Expressions;
using EmailScanner.Domain;
using EmailScanner.Domain.Abstractions;
using EmailScanner.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace EmailScanner.Repository;

public sealed class Repository<TEntity>(EmailScannerDbContext context) : IRepository<TEntity> where TEntity : class, IEntity
{
    private readonly DbSet<TEntity> _entities = context.Set<TEntity>();
    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => await _entities.FindAsync([id], cancellationToken);
    public async Task<IReadOnlyList<TEntity>> ListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) =>
        await _entities.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    public void Add(TEntity entity) => _entities.Add(entity);
    public void Remove(TEntity entity) => _entities.Remove(entity);
}

public sealed class TenantRepository(EmailScannerDbContext context) : ITenantRepository
{
    private readonly Repository<Tenant> _repository = new(context);
    public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => _repository.GetByIdAsync(id, cancellationToken);
    public Task<IReadOnlyList<Tenant>> ListAsync(Expression<Func<Tenant, bool>> predicate, CancellationToken cancellationToken = default) => _repository.ListAsync(predicate, cancellationToken);
    public void Add(Tenant entity) => _repository.Add(entity);
    public void Remove(Tenant entity) => _repository.Remove(entity);
}

public sealed class MailboxConnectionRepository(EmailScannerDbContext context) : IMailboxConnectionRepository
{
    private readonly Repository<MailboxConnection> _repository = new(context);
    public Task<MailboxConnection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => _repository.GetByIdAsync(id, cancellationToken);
    public Task<IReadOnlyList<MailboxConnection>> ListAsync(Expression<Func<MailboxConnection, bool>> predicate, CancellationToken cancellationToken = default) => _repository.ListAsync(predicate, cancellationToken);
    public void Add(MailboxConnection entity) => _repository.Add(entity);
    public void Remove(MailboxConnection entity) => _repository.Remove(entity);
    public async Task<IReadOnlyList<MailboxConnection>> GetEnabledAsync(CancellationToken cancellationToken = default) => await context.MailboxConnection.AsNoTracking().Where(x => x.Status == MailboxConnectionStatus.Enabled).ToListAsync(cancellationToken);
}

public sealed class EmailRepository(EmailScannerDbContext context) : IEmailRepository
{
    private readonly Repository<Email> _repository = new(context);
    public Task<Email?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => _repository.GetByIdAsync(id, cancellationToken);
    public Task<IReadOnlyList<Email>> ListAsync(Expression<Func<Email, bool>> predicate, CancellationToken cancellationToken = default) => _repository.ListAsync(predicate, cancellationToken);
    public void Add(Email entity) => _repository.Add(entity);
    public void Remove(Email entity) => _repository.Remove(entity);
}

public sealed class AttachmentRepository(EmailScannerDbContext context) : IAttachmentRepository
{
    private readonly Repository<Attachment> _repository = new(context);
    public Task<Attachment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => _repository.GetByIdAsync(id, cancellationToken);
    public Task<IReadOnlyList<Attachment>> ListAsync(Expression<Func<Attachment, bool>> predicate, CancellationToken cancellationToken = default) => _repository.ListAsync(predicate, cancellationToken);
    public void Add(Attachment entity) => _repository.Add(entity);
    public void Remove(Attachment entity) => _repository.Remove(entity);
}

public sealed class UnitOfWork(EmailScannerDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => context.SaveChangesAsync(cancellationToken);
}
