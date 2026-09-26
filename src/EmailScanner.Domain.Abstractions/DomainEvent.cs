namespace EmailScanner.Domain.Abstractions;

public abstract record DomainEvent(Guid EventId, DateTime OccurredUtc);
