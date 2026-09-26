using EmailScanner.Domain;

namespace EmailScanner.Application.MailboxConnections.UpdateMailboxConnection;

public sealed record UpdateMailboxConnectionRequest(Guid Id, string DisplayName, string Folder, MailboxSyncMode SyncMode);
