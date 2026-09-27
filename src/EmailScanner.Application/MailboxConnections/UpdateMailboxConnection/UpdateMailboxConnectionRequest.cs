using EmailScanner.Domain;

namespace EmailScanner.Application.MailboxConnections.UpdateMailboxConnection;

public sealed record UpdateMailboxConnectionRequest(
    Guid Id,
    string DisplayName,
    string Folder,
    MailboxSyncMode SyncMode,
    string? ImapHost = null,
    int? ImapPort = null,
    string? ImapUsername = null,
    string? ImapCredentialReference = null);
