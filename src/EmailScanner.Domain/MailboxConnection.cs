using EmailScanner.Domain.Abstractions;
using EmailScanner.Domain.Events;

namespace EmailScanner.Domain;

public sealed class MailboxConnection : AuditableEntity, IAggregateRoot
{
    private MailboxConnection() { }

    public MailboxConnection(Guid tenantId, MailboxProvider provider, string emailAddress, string displayName, string folder, string? imapHost = null, int? imapPort = null, string? imapUsername = null, string? imapCredentialReference = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant id is required.", nameof(tenantId));
        TenantId = tenantId;
        Provider = provider;
        EmailAddress = EmailAddressValue.Create(emailAddress).Value;
        DisplayName = displayName?.Trim() ?? string.Empty;
        UpdateFolder(folder);
        UpdateImapSettings(imapHost, imapPort, imapUsername, imapCredentialReference);
        AddDomainEvent(new MailboxConnectionCreatedDomainEvent(Id, DateTime.UtcNow));
    }

    public Guid TenantId { get; private set; }
    public MailboxProvider Provider { get; private set; }
    public string EmailAddress { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string Folder { get; private set; } = "INBOX";
    public MailboxConnectionStatus Status { get; private set; } = MailboxConnectionStatus.Pending;
    public MailboxSyncMode SyncMode { get; private set; } = MailboxSyncMode.Delta;
    public Guid? MailboxCredentialId { get; private set; }
    public string? ImapHost { get; private set; }
    public int? ImapPort { get; private set; }
    public string? ImapUsername { get; private set; }
    public string? ImapCredentialReference { get; private set; }
    public string? SyncToken { get; private set; }
    public DateTime? LastSyncUtc { get; private set; }
    public DateTime? LastSuccessfulSyncUtc { get; private set; }
    public DateTime? LastFailedSyncUtc { get; private set; }

    public void Enable() { Status = MailboxConnectionStatus.Enabled; Touch(null); }
    public void Disable() { Status = MailboxConnectionStatus.Disabled; Touch(null); }
    public void UpdateDetails(string displayName, MailboxSyncMode syncMode)
    {
        DisplayName = displayName?.Trim() ?? string.Empty;
        SyncMode = syncMode;
        Touch(null);
    }
    public void UpdateFolder(string folder) { ArgumentException.ThrowIfNullOrWhiteSpace(folder); Folder = folder.Trim(); Touch(null); }
    public void UpdateImapSettings(string? host, int? port, string? username, string? credentialReference)
    {
        if (Provider == MailboxProvider.Imap)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(host);
            ArgumentException.ThrowIfNullOrWhiteSpace(username);
            ArgumentException.ThrowIfNullOrWhiteSpace(credentialReference);
            if (port is < 1 or > 65535) throw new ArgumentOutOfRangeException(nameof(port));
        }
        ImapHost = string.IsNullOrWhiteSpace(host) ? null : host.Trim();
        ImapPort = port;
        ImapUsername = string.IsNullOrWhiteSpace(username) ? null : username.Trim();
        ImapCredentialReference = string.IsNullOrWhiteSpace(credentialReference) ? null : credentialReference.Trim();
        Touch(null);
    }
    public void UpdateSyncToken(string? token) { SyncToken = token; Touch(null); }
    public void MarkSyncStarted(DateTime utcNow) { LastSyncUtc = RequireUtc(utcNow); Status = MailboxConnectionStatus.Syncing; Touch(null); }
    public void MarkSyncCompleted(DateTime utcNow, string? syncToken) { LastSuccessfulSyncUtc = RequireUtc(utcNow); SyncToken = syncToken; Status = MailboxConnectionStatus.Enabled; Touch(null); }
    public void MarkSyncFailed(DateTime utcNow) { LastFailedSyncUtc = RequireUtc(utcNow); Status = MailboxConnectionStatus.Failed; Touch(null); }

    private static DateTime RequireUtc(DateTime value) => value.Kind == DateTimeKind.Utc ? value : throw new ArgumentException("Timestamp must be UTC.", nameof(value));
}
