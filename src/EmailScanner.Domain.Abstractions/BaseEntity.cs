namespace EmailScanner.Domain.Abstractions;

public abstract class BaseEntity : IEntity
{
    private readonly List<DomainEvent> _domainEvents = [];

    public Guid Id { get; protected set; } = Guid.NewGuid();
    public EntityState State { get; private set; } = EntityState.Added;
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void RemoveDomainEvent(DomainEvent domainEvent) => _domainEvents.Remove(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();

    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    protected void MarkModified() => State = EntityState.Modified;
    protected void MarkDeleted() => State = EntityState.Deleted;
}
