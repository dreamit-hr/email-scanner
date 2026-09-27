using EmailScanner.Domain.Abstractions;
using EmailScanner.Domain.Events;

namespace EmailScanner.Domain;

public sealed class Email : AuditableEntity, IAggregateRoot
{
    private readonly List<EmailRecipient> _recipients = [];
    private readonly List<Attachment> _attachments = [];
    private readonly HashSet<Guid> _tags = [];
    private Email() { }
    public Email(Guid mailboxConnectionId, string internetMessageId, string sender, string subject, string body, DateTime receivedUtc, string mimeBlobPath, string mimeHash, Guid? id = null)
    {
        if (mailboxConnectionId == Guid.Empty) throw new ArgumentException("Mailbox id is required.", nameof(mailboxConnectionId));
        ArgumentException.ThrowIfNullOrWhiteSpace(internetMessageId);
        MailboxConnectionId = mailboxConnectionId; InternetMessageId = internetMessageId.Trim();
        if (id.HasValue)
        {
            if (id.Value == Guid.Empty) throw new ArgumentException("Email id cannot be empty.", nameof(id));
            Id = id.Value;
        }
        Sender = EmailAddressValue.Create(sender).Value; Subject = subject ?? string.Empty; Body = body ?? string.Empty;
        ReceivedUtc = receivedUtc.Kind == DateTimeKind.Utc ? receivedUtc : throw new ArgumentException("Timestamp must be UTC.", nameof(receivedUtc));
        MimeBlobPath = mimeBlobPath; MimeHash = mimeHash; ImportedUtc = DateTime.UtcNow; Status = EmailStatus.Queued;
        AddDomainEvent(new EmailReceivedDomainEvent(Id, ImportedUtc));
    }
    public Guid MailboxConnectionId { get; private set; }
    public string InternetMessageId { get; private set; } = string.Empty;
    public string? ThreadId { get; private set; }
    public string? ConversationId { get; private set; }
    public string Sender { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public string BodyPreview => Body.Length <= 240 ? Body : Body[..240];
    public string MimeBlobPath { get; private set; } = string.Empty;
    public string MimeHash { get; private set; } = string.Empty;
    public DateTime ReceivedUtc { get; private set; }
    public DateTime ImportedUtc { get; private set; }
    public EmailStatus Status { get; private set; }
    public bool HasAttachments => _attachments.Count > 0;
    public IReadOnlyCollection<EmailRecipient> Recipients => _recipients.AsReadOnly();
    public IReadOnlyCollection<Attachment> Attachments => _attachments.AsReadOnly();
    public IReadOnlyCollection<Guid> Tags => _tags;
    public void MarkProcessed() { Status = EmailStatus.Processed; Touch(null); }
    public void MarkFailed() { Status = EmailStatus.Failed; Touch(null); }
    public void MarkQueued() { Status = EmailStatus.Queued; Touch(null); }
    public void AddRecipient(EmailRecipient recipient) { _recipients.Add(recipient); Touch(null); }
    public void AddAttachment(Attachment attachment) { _attachments.Add(attachment); Touch(null); }
    public void AddTag(Guid tagId) { if (tagId == Guid.Empty) throw new ArgumentException("Tag id is required.", nameof(tagId)); _tags.Add(tagId); Touch(null); }
    public void RemoveTag(Guid tagId) { _tags.Remove(tagId); Touch(null); }
}
