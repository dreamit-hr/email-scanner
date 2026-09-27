using EmailScanner.Domain;

namespace EmailScanner.Application.MailboxConnections;

public sealed record MailboxConnectionResponse(Guid Id, Guid TenantId, MailboxProvider Provider, string EmailAddress, string DisplayName, string Folder, MailboxConnectionStatus Status, MailboxSyncMode SyncMode, DateTime? LastSyncUtc, DateTime? LastSuccessfulSyncUtc, DateTime? LastFailedSyncUtc, string? ImapHost, int? ImapPort, string? ImapUsername)
{
    public static MailboxConnectionResponse From(MailboxConnection mailbox) => new(mailbox.Id, mailbox.TenantId, mailbox.Provider, mailbox.EmailAddress, mailbox.DisplayName, mailbox.Folder, mailbox.Status, mailbox.SyncMode, mailbox.LastSyncUtc, mailbox.LastSuccessfulSyncUtc, mailbox.LastFailedSyncUtc, mailbox.ImapHost, mailbox.ImapPort, mailbox.ImapUsername);
}
