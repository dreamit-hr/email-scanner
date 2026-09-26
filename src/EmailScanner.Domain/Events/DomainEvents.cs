using EmailScanner.Domain.Abstractions;

namespace EmailScanner.Domain.Events;

public sealed record MailboxConnectionCreatedDomainEvent(Guid MailboxConnectionId, DateTime OccurredUtc) : DomainEvent(Guid.NewGuid(), OccurredUtc);
public sealed record EmailReceivedDomainEvent(Guid EmailId, DateTime OccurredUtc) : DomainEvent(Guid.NewGuid(), OccurredUtc);
public sealed record AttachmentUploadedDomainEvent(Guid AttachmentId, DateTime OccurredUtc) : DomainEvent(Guid.NewGuid(), OccurredUtc);
public sealed record EmailRuleMatchedDomainEvent(Guid EmailId, Guid EmailRuleId, DateTime OccurredUtc) : DomainEvent(Guid.NewGuid(), OccurredUtc);
public sealed record WebhookDeliveryRequestedDomainEvent(Guid WebhookDeliveryId, DateTime OccurredUtc) : DomainEvent(Guid.NewGuid(), OccurredUtc);
